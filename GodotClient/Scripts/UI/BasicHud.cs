using System.Text;
using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.UI;

public partial class BasicHud : CanvasLayer
{
    private static readonly string[] ProductionKeys =
    {
        "unit.rock_raiders.crew", "unit.rock_raiders.hover_scout", "unit.rock_raiders.rapid_rider", "unit.rock_raiders.loader_dozer"
    };

    private readonly StringBuilder _builder = new(1024);
    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private RtsInputController? _input;
    private RtsCameraController? _camera;
    private MinimapPresentationSource? _minimapSource;
    private HudView? _view;
    private EntityId _alertFocusEntity = EntityId.None;
    private bool _energyPopoverVisible;
    private double _nextRefresh;

    public HudView? View => _view;

    public void Configure(GodotSimBridge bridge, SelectionController selection, RtsInputController input, RtsCameraController camera)
    {
        _bridge = bridge;
        _selection = selection;
        _input = input;
        _camera = camera;
        _minimapSource = new MinimapPresentationSource();
        Name = "BasicHUD";
        Layer = 10;
        ProcessPriority = 200;
        _view = new HudView();
        AddChild(_view);
        _view.Configure(M7HudProfile.CreateDefault());
        _view.CommandRequested += HandleCommand;
        _view.PowerPriorityRequested += priority => _input.SetSelectedEnergyPriority((EnergyPriority)priority);
        _view.EnergyDetailsRequested += () => { _energyPopoverVisible = !_energyPopoverVisible; RefreshNow(); };
        _view.AlertRequested += FocusCurrentAlert;
        _view.SelectionGroupRequested += NarrowSelectionToGroup;
        _view.MinimapCameraRequested += HandleMinimapCamera;
        _view.MinimapGroundCommandRequested += _input.IssueMinimapGroundCommand;
        RefreshNow();
    }

    public override void _Process(double delta)
    {
        if (_bridge is null || _selection is null || _view is null) return;
        if (_camera is not null) _view.UpdateMinimapCamera(_camera.GetGroundViewportPolygon());
        double now = Time.GetTicksMsec() / 1000.0;
        if (now < _nextRefresh) return;
        _nextRefresh = now + 0.10;
        _view.ApplyFrame(BuildFrame());
    }

    private void RefreshNow()
    {
        _nextRefresh = 0;
        if (_view is not null && _bridge is not null && _selection is not null) _view.ApplyFrame(BuildFrame(), true);
    }

    private HudFrame BuildFrame()
    {
        HudFrame frame = new() { Faction = HudFaction.RockRaiders };
        BuildResourceFrame(frame);
        BuildSelectionFrame(frame);
        BuildAlertFrame(frame);
        frame.MatchState = $"{FormatMatchTime(_bridge!.World.Tick.Value)}  •  PLAYER 1";
        frame.Events = BuildEventFeed(frame);
        frame.Commands = BuildCommands();
        frame.EnergyPopoverVisible = _energyPopoverVisible;
        if (_minimapSource is not null && _bridge.Current is not null)
            frame.Minimap = _minimapSource.Capture(_bridge.World, _bridge.Current, _selection!.Selected);
        return frame;
    }

    private void HandleMinimapCamera(Vector2I cell)
    {
        if (_camera is null) return;
        float scale = GodotConversions.WorldUnitsPerBuildCell;
        _camera.CenterOn(new Vector3((cell.X + 0.5f) * scale, 0f, (cell.Y + 0.5f) * scale));
    }

    private void BuildResourceFrame(HudFrame frame)
    {
        EntityId[] components = WorksiteGraphSystem.GetPlayerComponents(_bridge!.World, 0);
        EntityId activeRoot = ActiveWorksiteRoot(components);
        int ore = activeRoot == EntityId.None ? 0 : WorksiteGraphSystem.GetProcessedResourceTotal(_bridge.World, activeRoot, ResourceType.Ore);
        int pending = activeRoot == EntityId.None ? 0 : WorksiteGraphSystem.GetPendingHauledResourceTotal(_bridge.World, activeRoot, ResourceType.Ore);
        frame.Ore = pending > 0 ? $"{ore}  +{pending} incoming" : ore.ToString();
        if (components.Length > 1) frame.Ore += $"  •  {components.Length} pools";
        int spendableCrystals = 0, committedCrystals = 0;
        IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!_bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != 0) continue;
            if (_bridge.World.Entities.ResourceBank.TryGet(id, out ResourceBank bank) && bank.Type == ResourceType.Crystal) spendableCrystals += bank.ProcessedAmount;
            if (_bridge.World.Entities.ResonanceCore.TryGet(id, out ResonanceCore resonance)) committedCrystals += ResonanceCoreSystem.CountCommitted(resonance);
        }
        frame.Crystals = committedCrystals > 0 ? $"{spendableCrystals}  •  {committedCrystals} committed" : spendableCrystals.ToString();
        OperationsCapacityState capacity = _bridge.World.GetOperationsCapacity(0);
        frame.Operations = capacity.Reserved > 0 ? $"{capacity.Used} / {capacity.Maximum}  +{capacity.Reserved} queued" : $"{capacity.Used} / {capacity.Maximum}";
        frame.ResourcePriority = capacity.IsOverCapacity ? HudAlertPriority.Critical : capacity.IsAdvanceWarning ? HudAlertPriority.High : HudAlertPriority.None;
        if (activeRoot == EntityId.None || !_bridge.World.Entities.EnergyDomain.TryGet(activeRoot, out EnergyDomain energy))
        {
            frame.Energy = "NO DOMAIN";
            frame.EnergyPopover = "No active Energy Domain is connected to the selected Worksite.";
        }
        else
        {
            int net = energy.GenerationPerSecond - energy.ContinuousDemandPerSecond;
            frame.Energy = $"{EnergyText(energy.Reserve)} / {EnergyText(energy.ReserveCapacity)}  |  {energy.GenerationPerSecond}↑  {energy.ContinuousDemandPerSecond}↓  |  {(net >= 0 ? "+" : string.Empty)}{net}/s";
            frame.EnergyPopover = $"Worksite #{activeRoot.Value}\nReserve  {EnergyText(energy.Reserve)} / {EnergyText(energy.ReserveCapacity)}\nGeneration  +{energy.GenerationPerSecond} E/s\nDemand  -{energy.ContinuousDemandPerSecond} E/s\nNet  {(net >= 0 ? "+" : string.Empty)}{net} E/s\nState  {(energy.IsBrownout ? "BROWNOUT" : energy.IsDeficit ? "RESERVE DRAINING" : "STABLE")}";
        }
        frame.FactionMechanic = components.Length switch { 0 => "NO WORKSITE", 1 => "WORKSITE CONNECTED", _ => $"{components.Length} WORKSITE POOLS" };
    }

    private void BuildSelectionFrame(HudFrame frame)
    {
        IReadOnlyList<EntityId> selected = _selection!.Selected;
        if (selected.Count == 0)
        {
            frame.Selection = new HudSelectionFrame
            {
                Title = _input?.BuildModeActive == true ? "BUILD MODE" : "NO SELECTION",
                Subtitle = _input?.BuildModeActive == true ? _input.BuildStatus : "Select an entity or issue a command.",
                PortraitCaption = _input?.BuildModeActive == true ? "PLACEMENT\nPREVIEW" : "NO SELECTION",
                StatusText = "RMB context  •  Shift queues  •  B build  •  F8 developer tools"
            };
            return;
        }
        if (selected.Count > 1)
        {
            frame.Selection = BuildGroupedSelection(selected);
            return;
        }
        EntityId id = selected[0];
        HudSelectionFrame selection = new()
        {
            Title = EntityName(id), Subtitle = SelectionSubtitle(id), PortraitCaption = PortraitPlaceholder(id), Count = 1
        };
        if (_bridge!.World.Entities.Health.TryGet(id, out Health health))
        {
            int percent = HealthPercent(health);
            selection.HealthPercent = percent;
            selection.HealthText = $"HP {health.Current.RoundToInt()} / {health.Maximum.RoundToInt()}  •  A{health.ArmorRating}  •  {DamageState(percent)}";
        }
        selection.StatusText = BuildEntityStatus(id);
        if (_bridge.World.Entities.PowerState.TryGet(id, out PowerState power))
        {
            selection.ShowPowerPriority = true;
            selection.PowerPriority = (int)power.Priority;
        }
        frame.Selection = selection;
        if (_bridge.World.Entities.Production.TryGet(id, out Production production)) frame.Queue = BuildQueue(production, id);
    }

    private HudSelectionFrame BuildGroupedSelection(IReadOnlyList<EntityId> selected)
    {
        Dictionary<uint, SelectionAccumulator> groups = new();
        bool hasPriority = false, mixedPriority = false;
        EnergyPriority commonPriority = EnergyPriority.Normal;
        for (int i = 0; i < selected.Count; i++)
        {
            EntityId id = selected[i];
            ContentId type = _bridge!.World.Entities.Selectable.TryGet(id, out Selectable selectable) ? selectable.ContentType : default;
            if (!groups.TryGetValue(type.Value, out SelectionAccumulator? accumulator))
            {
                accumulator = new SelectionAccumulator(type, EntityName(id));
                groups.Add(type.Value, accumulator);
            }
            accumulator.Count++;
            if (_bridge.World.Entities.Health.TryGet(id, out Health health))
            {
                int percent = HealthPercent(health);
                accumulator.HealthTotal += percent;
                accumulator.HealthCount++;
                if (percent < 70) accumulator.DamagedCount++;
            }
            if (_bridge.World.Entities.Transformation.TryGet(id, out Transformation transformation) && transformation.Phase != TransformationPhase.Idle) accumulator.ActiveStateCount++;
            if (_bridge.World.Entities.PowerState.TryGet(id, out PowerState power))
            {
                if (!hasPriority) { hasPriority = true; commonPriority = power.Priority; }
                else if (commonPriority != power.Priority) mixedPriority = true;
            }
        }
        List<SelectionAccumulator> ordered = groups.Values.OrderByDescending(group => group.Count).ThenBy(group => group.Type.Value).ToList();
        HudSelectionFrame result = new()
        {
            Title = $"{selected.Count} OBJECTS SELECTED", Subtitle = $"{ordered.Count} gameplay types  •  grouped for RTS-scale readability",
            PortraitCaption = $"GROUP\n{selected.Count} SELECTED", Count = selected.Count,
            StatusText = "Click a type card to narrow selection. Individual cards appear after narrowing.", ShowPowerPriority = hasPriority,
            PowerPriority = (int)commonPriority, MixedPowerPriority = mixedPriority
        };
        for (int i = 0; i < ordered.Count && i < 8; i++)
        {
            SelectionAccumulator group = ordered[i];
            result.Groups.Add(new HudSelectionGroupFrame
            {
                Id = group.Type.Value.ToString(), Name = group.Name, Count = group.Count,
                AverageHealthPercent = group.HealthCount == 0 ? 100 : group.HealthTotal / group.HealthCount,
                StateSummary = $"{(group.DamagedCount > 0 ? $"{group.DamagedCount} damaged" : "healthy")}{(group.ActiveStateCount > 0 ? $"  •  {group.ActiveStateCount} active" : string.Empty)}"
            });
        }
        return result;
    }

    private string BuildEntityStatus(EntityId id)
    {
        _builder.Clear();
        SimulationWorld world = _bridge!.World;
        if (world.Entities.Targetable.TryGet(id, out Targetable targetable))
            _builder.Append("Target ").Append(targetable.Class).Append("  •  ").Append(targetable.Layer == CombatTargetLayer.TrueAir ? "TRUE AIR" : "GROUND").Append('\n');
        if (world.Entities.Worker.TryGet(id, out Worker worker) && world.Entities.ResourceCarrier.TryGet(id, out ResourceCarrier carrier))
            _builder.Append("Task ").Append(worker.TaskState).Append("  •  Cargo ").Append(carrier.Amount).Append('/').Append(carrier.Capacity).Append(" Ore\n");
        if (world.Entities.ConstructionSite.TryGet(id, out ConstructionSite site))
            _builder.Append("Construction ").Append(site.ProgressTicks * 100 / site.RequiredTicks).Append("%  •  Reserved ").Append(site.ReservedOre).Append(" Ore / ").Append(site.ReservedEnergy).Append(" Energy\n");
        if (world.Entities.Transport.TryGet(id, out Transport transport))
            _builder.Append("Passengers ").Append(transport.OccupiedPoints).Append(" / ").Append(transport.CapacityPoints).Append("  •  Crew ×").Append(transport.PassengerCount).Append('\n');
        if (world.Entities.Transformation.TryGet(id, out Transformation transformation) && world.Content.TryGetTransformation(world.Entities.Selectable.Get(id).ContentType, out TransformationDefinition definition))
        {
            TransformationModeDefinition current = definition.GetMode(transformation.CurrentState);
            TransformationModeDefinition destination = definition.GetMode(transformation.DestinationState);
            if (transformation.Phase == TransformationPhase.Idle)
                _builder.Append("State ").Append(current.DisplayName.ToUpperInvariant()).Append("  •  Q → ").Append(definition.GetDestination(transformation.CurrentState).DisplayName.ToUpperInvariant()).Append('\n');
            else _builder.Append(current.DisplayName.ToUpperInvariant()).Append(" → ").Append(destination.DisplayName.ToUpperInvariant()).Append("  •  ").Append(TransformationSystem.ProgressBasisPoints(transformation) / 100).Append("%\n");
        }
        if (world.Entities.WorksiteMember.TryGet(id, out WorksiteMember worksite)) _builder.Append("Worksite #").Append(worksite.ComponentRoot.Value).Append("  •  SERVICED\n");
        if (world.Entities.ForwardServiceMember.Has(id)) _builder.Append(ForwardServiceSystem.TryGetProviderForMember(world, id, out EntityId provider) ? $"Service available  •  source #{provider.Value}\n" : "No Forward Service\n");
        if (world.Entities.ResonanceCore.TryGet(id, out ResonanceCore resonance))
            _builder.Append("Committed Crystals ").Append(ResonanceCoreSystem.CountCommitted(resonance)).Append(" / ").Append(ResonanceCoreSystem.MaximumSlots(resonance)).Append("  •  ").Append(BrownoutSystem.IsOperational(world, id) ? "CHARGE ONLINE" : "BROWNOUT — CRYSTALS RETAINED").Append('\n');
        if (world.Entities.TubeStation.TryGet(id, out TubeStation station))
            _builder.Append("Aero Tube ").Append(station.ConnectionCount).Append(" / ").Append(station.ConnectionLimit).Append(" links  •  throughput ").Append(station.HypersledThroughputUnlocked ? 3 : 2).Append('\n');
        if (world.Entities.PowerState.TryGet(id, out PowerState power)) _builder.Append("Power ").Append(power.IsPowered ? "ONLINE" : "BROWNOUT DISABLED").Append("  •  ").Append(power.Priority).Append(" priority\n");
        if (world.Entities.Production.TryGet(id, out Production production)) _builder.Append("Production ").Append(production.Count).Append('/').Append(Production.Capacity).Append(production.SpawnBlocked ? "  •  EXIT BLOCKED" : string.Empty).Append('\n');
        int stabilityTicks = DisplacementSystem.RemainingStabilityTicks(world, id);
        if (stabilityTicks > 0) _builder.Append("Stability protection ").Append((stabilityTicks + 19) / 20).Append("s\n");
        if (_input?.BuildModeActive == true) _builder.Append("Build mode  •  ").Append(_input.BuildStatus).Append('\n');
        if (_builder.Length == 0) _builder.Append("Ready for orders.");
        return _builder.ToString().TrimEnd();
    }

    private void BuildAlertFrame(HudFrame frame)
    {
        _alertFocusEntity = EntityId.None;
        EntityId[] roots = WorksiteGraphSystem.GetPlayerComponents(_bridge!.World, 0);
        for (int i = 0; i < roots.Length; i++)
        {
            if (!_bridge.World.Entities.EnergyDomain.TryGet(roots[i], out EnergyDomain energy) || !energy.IsBrownout) continue;
            int deficit = energy.ContinuousDemandPerSecond - energy.GenerationPerSecond;
            _alertFocusEntity = roots[i];
            frame.Alert = new HudAlertFrame
            {
                Priority = HudAlertPriority.High,
                Text = $"BROWNOUT — Worksite #{roots[i].Value} deficit {deficit} E/s",
                Actionable = _bridge.World.Entities.Transform.Has(_alertFocusEntity)
            };
            return;
        }
        OperationsCapacityState capacity = _bridge.World.GetOperationsCapacity(0);
        if (capacity.IsOverCapacity)
        {
            _alertFocusEntity = ActiveWorksiteRoot(roots);
            frame.Alert = new HudAlertFrame
            {
                Priority = HudAlertPriority.Critical,
                Text = "OPERATIONS CAPACITY EXCEEDED",
                Actionable = _alertFocusEntity != EntityId.None && _bridge.World.Entities.Transform.Has(_alertFocusEntity)
            };
        }
    }

    private void FocusCurrentAlert()
    {
        if (_camera is null || _bridge is null || _alertFocusEntity == EntityId.None ||
            !_bridge.World.Entities.Transform.TryGet(_alertFocusEntity, out SimTransform transform)) return;
        _camera.CenterOn(transform.Position.ToWorld());
    }

    private static List<HudEventFrame> BuildEventFeed(HudFrame frame)
    {
        List<HudEventFrame> events = new();
        if (frame.Alert.Priority != HudAlertPriority.None) events.Add(new HudEventFrame { Priority = frame.Alert.Priority, Time = "NOW", Text = frame.Alert.Text });
        return events;
    }

    private List<HudCommandFrame> BuildCommands()
    {
        List<HudCommandFrame> commands = new();
        if (_selection!.Selected.Count == 0)
        {
            commands.Add(Command("build", "BUILD", "B", "", "Open categorized Build Mode.", false));
            return commands;
        }
        bool hasProducer = _selection.Selected.Any(id => _bridge!.World.Entities.Production.Has(id));
        if (hasProducer)
        {
            for (int i = 0; i < ProductionKeys.Length; i++)
            {
                string key = ProductionKeys[i];
                commands.Add(new HudCommandFrame { Id = key, Name = ProductionName(key), Cost = ProductionCost(key), Tooltip = ProductionTooltip(key), Enabled = _input!.CanQueueProduction(key), QueueFive = true });
            }
        }
        else
        {
            commands.Add(Command("move", "MOVE", "M", "", "Target terrain in the world; RMB uses contextual Move.", false));
            commands.Add(Command("attack", "ATTACK", "A", "", "Target a visible enemy in the world; RMB uses contextual Attack.", false));
            commands.Add(Command("stop", "STOP", "S", "", "Immediately stop the selected entities.", true));
            commands.Add(Command("hold", "HOLD", "H", "", "Command slot reserved for T073 complete catalog.", false));
            commands.Add(Command("patrol", "PATROL", "P", "", "Command slot reserved for T073 complete catalog.", false));
            commands.Add(Command("spread", "SPREAD", "V", "", "Command slot reserved for T073 complete catalog.", false));
            if (_selection.Selected.Any(id => _bridge!.World.Entities.Transformation.Has(id))) commands.Add(Command("state", "STATE CHANGE", "Q", "", "Toggle the selected tactical state.", true));
        }
        return commands;
    }

    private List<HudQueueFrame> BuildQueue(Production production, EntityId facility)
    {
        List<HudQueueFrame> queue = new();
        bool paused = !BrownoutSystem.IsOperational(_bridge!.World, facility);
        for (int i = 0; i < production.Count; i++)
        {
            ProductionQueueItem item = production.Get(i);
            int percent = item.TotalTicks == 0 ? 0 : (item.TotalTicks - item.RemainingTicks) * 100 / item.TotalTicks;
            queue.Add(new HudQueueFrame { Name = UnitName(item.UnitType), ProgressPercent = percent, Paused = paused });
        }
        return queue;
    }

    private void HandleCommand(string id, bool fiveCopies)
    {
        if (_input is null) return;
        if (id.StartsWith("unit.", StringComparison.Ordinal)) _input.QueueProduction(id, fiveCopies);
        else if (id == "stop") _input.StopSelected();
        else if (id == "state") _input.StateChangeSelected();
        RefreshNow();
    }

    private void NarrowSelectionToGroup(string groupId)
    {
        if (_selection is null || !uint.TryParse(groupId, out uint typeValue)) return;
        List<EntityId> narrowed = new();
        for (int i = 0; i < _selection.Selected.Count; i++)
        {
            EntityId id = _selection.Selected[i];
            if (_bridge!.World.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType.Value == typeValue) narrowed.Add(id);
        }
        if (narrowed.Count > 0) _selection.SetSelection(narrowed);
        RefreshNow();
    }

    private EntityId ActiveWorksiteRoot(EntityId[] components)
    {
        if (_selection!.Selected.Count > 0 && _bridge!.World.Entities.WorksiteMember.TryGet(_selection.Selected[0], out WorksiteMember selected)) return selected.ComponentRoot;
        return components.Length > 0 ? components[0] : EntityId.None;
    }

    private string SelectionSubtitle(EntityId id)
    {
        if (!_bridge!.World.Entities.Selectable.TryGet(id, out Selectable selectable)) return "Unknown entity";
        return selectable.Kind switch
        {
            SelectableKind.Building => "STRUCTURE  •  local systems and queue", SelectableKind.ResourceNode => "RESOURCE DEPOSIT",
            SelectableKind.Worker => "WORKER  •  economy / construction / repair", _ => "COMBAT / SUPPORT UNIT"
        };
    }

    private string PortraitPlaceholder(EntityId id)
    {
        if (!_bridge!.World.Entities.Selectable.TryGet(id, out Selectable selectable)) return "ENTITY VIEW\nART PENDING";
        return selectable.Kind switch { SelectableKind.Building => "STRUCTURE VIEW\nART PENDING", SelectableKind.ResourceNode => "RESOURCE VIEW\nART PENDING", _ => "UNIT VIEW\nART PENDING" };
    }

    private string EntityName(EntityId id)
    {
        if (!_bridge!.World.Entities.Selectable.TryGet(id, out Selectable selectable)) return $"Object #{id.Value}";
        if (_bridge.World.Content.TryGetBuilding(selectable.ContentType, out BuildingDefinition building)) return DisplayName(building.StableKey);
        if (_bridge.World.Content.TryGetEntity(selectable.ContentType, out PrototypeEntityDefinition entity)) return DisplayName(entity.StableKey);
        for (int i = 0; i < _bridge.World.Content.ResourceNodes.Length; i++) if (_bridge.World.Content.ResourceNodes[i].Id == selectable.ContentType) return DisplayName(_bridge.World.Content.ResourceNodes[i].StableKey);
        return DisplayNameById(selectable.ContentType) ?? $"Object #{id.Value}";
    }

    private static string? DisplayNameById(ContentId id)
    {
        string[] known = { "building.ast.service_refit_hub", "unit.ast.t3_trike", "building.ali.etx_command_core", "building.ali.resonance_core", "unit.ali.razor_skimmer", "building.mar.aero_tube_hangar", "building.mar.settlement_station", "unit.mar.worker_robot", "unit.mar.double_hover", "unit.mar.jet_scooter", "unit.mar.excavation_searcher", "unit.ast.t3_trike.displacement_target", "unit.rock_raiders.hover_scout.m5_excavation_runner" };
        for (int i = 0; i < known.Length; i++) if (StableId.FromKey(known[i]) == id) return DisplayName(known[i]);
        return null;
    }

    private static HudCommandFrame Command(string id, string name, string hotkey, string cost, string tooltip, bool enabled)
        => new() { Id = id, Name = name, Hotkey = hotkey, Cost = cost, Tooltip = tooltip, Enabled = enabled };
    private static int HealthPercent(Health health) => health.Maximum.Raw == 0 ? 0 : checked((int)((long)health.Current.Raw * 100 / health.Maximum.Raw));
    private static string DamageState(int percent) => percent >= 70 ? "HEALTHY" : percent >= 35 ? "DAMAGED" : percent >= 20 ? "HEAVILY DAMAGED" : "CRITICAL";
    private static string FormatMatchTime(int tick) => $"{tick / SimClock.TicksPerSecond / 60:00}:{tick / SimClock.TicksPerSecond % 60:00}";
    private static string EnergyText(Fix32 value) { int tenths = (int)(((long)value.Raw * 10 + Fix32.OneRaw / 2) / Fix32.OneRaw); return $"{tenths / 10}.{tenths % 10}"; }
    private static string UnitName(ContentId id) { for (int i = 0; i < ProductionKeys.Length; i++) if (StableId.FromKey(ProductionKeys[i]) == id) return ProductionName(ProductionKeys[i]); return id.Value.ToString(); }
    private static string ProductionName(string key) => key switch { "unit.rock_raiders.crew" => "CREW", "unit.rock_raiders.hover_scout" => "HOVER SCOUT", "unit.rock_raiders.rapid_rider" => "RAPID RIDER", "unit.rock_raiders.loader_dozer" => "LOADER DOZER", _ => key };
    private static string ProductionCost(string key) => key switch { "unit.rock_raiders.crew" => "50 ORE • 1 OC", "unit.rock_raiders.hover_scout" => "75 ORE • 10 E • 1 OC", "unit.rock_raiders.rapid_rider" => "90 ORE • 10 E • 2 OC", "unit.rock_raiders.loader_dozer" => "125 ORE • 15 E • 3 OC", _ => string.Empty };
    private static string ProductionTooltip(string key) => key switch { "unit.rock_raiders.crew" => "16 seconds • Worker, builder and repair Crew", "unit.rock_raiders.hover_scout" => "20 seconds • Fast reconnaissance vehicle", "unit.rock_raiders.rapid_rider" => "28 seconds • Mobile logistics and support vehicle", "unit.rock_raiders.loader_dozer" => "36 seconds • Heavy industrial utility vehicle", _ => key };
    private static string DisplayName(string key) => key switch
    {
        "building.rock_raiders.hq" => "Rock Raiders HQ", "building.rock_raiders.ore_processing_plant" => "Ore Processing Plant", "building.rock_raiders.power_station" => "Power Station", "building.rock_raiders.vehicle_service_bay" => "Vehicle Service Bay",
        "unit.rock_raiders.crew" => "Crew", "unit.rock_raiders.hover_scout" => "Hover Scout", "unit.rock_raiders.rapid_rider" => "Rapid Rider", "unit.rock_raiders.loader_dozer" => "Loader Dozer", "unit.rock_raiders.chrome_crusher" => "Chrome Crusher",
        "unit.astronauts.mx41_switch_fighter" => "MX-41 Switch Fighter", "resource.ore.standard" => "Standard Ore Deposit", "resource.ore.small" => "Small Ore Deposit", "resource.ore.rich" => "Rich Ore Deposit", "resource.ore.deep_contested_seam" => "Deep Contested Ore Seam",
        "building.ast.service_refit_hub" => "Service & Refit Hub", "unit.ast.t3_trike" => "T3-Trike", "building.ali.etx_command_core" => "ETX Command Core", "building.ali.resonance_core" => "Resonance Core", "unit.ali.razor_skimmer" => "Razor Skimmer",
        "building.mar.aero_tube_hangar" => "Aero Tube Hangar", "building.mar.settlement_station" => "Settlement Station", "unit.mar.worker_robot" => "Worker Robot", "unit.mar.double_hover" => "Double Hover", "unit.mar.jet_scooter" => "Jet Scooter", "unit.mar.excavation_searcher" => "Excavation Searcher",
        "unit.ast.t3_trike.displacement_target" => "Enemy T3-Trike", "unit.rock_raiders.hover_scout.m5_excavation_runner" => "Hover Scout", _ => key
    };

    private sealed class SelectionAccumulator(ContentId type, string name)
    {
        public ContentId Type { get; } = type;
        public string Name { get; } = name;
        public int Count { get; set; }
        public int HealthTotal { get; set; }
        public int HealthCount { get; set; }
        public int DamagedCount { get; set; }
        public int ActiveStateCount { get; set; }
    }
}
