using System.Text;
using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.UI;

public partial class BasicHud : CanvasLayer
{
    private static readonly Color Graphite = new("111820");
    private static readonly Color GraphiteRaised = new("18232d");
    private static readonly Color RaiderAccent = new("e6ad28");
    private static readonly Color TextPrimary = new("f2eee3");
    private static readonly Color TextMuted = new("aab4b8");
    private static readonly Color Good = new("65c987");
    private static readonly Color Warning = new("f2b84b");
    private static readonly Color Danger = new("ff6b45");

    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private RtsInputController? _input;
    private Label? _oreValue;
    private Label? _energyValue;
    private Label? _crystalValue;
    private Label? _ocValue;
    private Label? _alertLabel;
    private PanelContainer? _alertPanel;
    private PanelContainer? _energyPopover;
    private Label? _energyPopoverLabel;
    private Label? _selectionTitle;
    private Label? _selectionDetails;
    private Label? _portraitLabel;
    private HBoxContainer? _priorityRow;
    private VBoxContainer? _contextualActions;
    private readonly Dictionary<EnergyPriority, Button> _priorityButtons = new();
    private readonly Dictionary<string, Button> _productionButtons = new();
    private readonly StringBuilder _builder = new(512);
    private static readonly string[] ProductionKeys =
    {
        "unit.rock_raiders.crew", "unit.rock_raiders.hover_scout", "unit.rock_raiders.rapid_rider", "unit.rock_raiders.loader_dozer"
    };

    public void Configure(GodotSimBridge bridge, SelectionController selection, RtsInputController input)
    {
        _bridge = bridge; _selection = selection; _input = input;
        Name = "BasicHUD"; Layer = 10; ProcessPriority = 200;

        Control root = new() { Name = "HUDRoot", MouseFilter = Control.MouseFilterEnum.Ignore };
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(root);
        BuildResourceStrip(root);
        BuildAlert(root);
        BuildSelectionPanel(root);
    }

    public override void _Process(double delta)
    {
        if (_bridge is null || _selection is null) return;
        UpdateResources();
        UpdateAlerts();
        UpdateSelection();
        UpdateProduction();
    }

    private void BuildResourceStrip(Control root)
    {
        PanelContainer panel = AnchoredPanel("ResourceStrip", 0.16f, 0.84f, 0f, 0f, 10f, 60f);
        panel.AddThemeStyleboxOverride("panel", PanelStyle(Graphite, RaiderAccent, 2));
        HBoxContainer row = new() { Name = "ResourceRow", Alignment = BoxContainer.AlignmentMode.Center };
        row.AddThemeConstantOverride("separation", 6);
        panel.AddChild(row);
        _oreValue = AddResourceBlock(row, "ORE", "500", "Processed Ore available to the active Worksite");
        Button energyButton = new() { Name = "EnergyButton", Flat = true, CustomMinimumSize = new Vector2(330, 48), TooltipText = "Open Energy Domain details" };
        VBoxContainer energyBox = ResourceBox("ENERGY", out _energyValue);
        energyButton.AddChild(energyBox);
        energyButton.Pressed += () => { if (_energyPopover is not null) _energyPopover.Visible = !_energyPopover.Visible; };
        row.AddChild(energyButton);
        _crystalValue = AddResourceBlock(row, "CRYSTALS", "0", "Spendable Crystals");
        _ocValue = AddResourceBlock(row, "OPERATIONS", "6 / 16", "Active and maximum Operations Capacity");
        Label phase = new() { Text = "M5", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, CustomMinimumSize = new Vector2(54, 48), TooltipText = "Four-Faction System Proof — Worksite graph" };
        phase.AddThemeColorOverride("font_color", RaiderAccent); phase.AddThemeFontSizeOverride("font_size", 15); row.AddChild(phase);
        root.AddChild(panel);

        _energyPopover = AnchoredPanel("EnergyDomainPopover", 0.36f, 0.64f, 0f, 0f, 76f, 196f);
        _energyPopover.Visible = false;
        _energyPopover.AddThemeStyleboxOverride("panel", PanelStyle(GraphiteRaised, RaiderAccent, 1));
        VBoxContainer popoverBox = new(); popoverBox.AddThemeConstantOverride("separation", 5); _energyPopover.AddChild(popoverBox);
        Label heading = HudLabel("ENERGY DOMAIN", 16, RaiderAccent); popoverBox.AddChild(heading);
        _energyPopoverLabel = HudLabel(string.Empty, 15, TextPrimary); _energyPopoverLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart; popoverBox.AddChild(_energyPopoverLabel);
        root.AddChild(_energyPopover);
    }

    private void BuildAlert(Control root)
    {
        _alertPanel = AnchoredPanel("AlertBanner", 0.30f, 0.70f, 0f, 0f, 78f, 120f);
        _alertPanel.AddThemeStyleboxOverride("panel", PanelStyle(new Color(0.24f, 0.11f, 0.07f, 0.96f), Danger, 2));
        _alertLabel = HudLabel(string.Empty, 17, TextPrimary);
        _alertLabel.HorizontalAlignment = HorizontalAlignment.Center; _alertLabel.VerticalAlignment = VerticalAlignment.Center;
        _alertPanel.AddChild(_alertLabel); _alertPanel.Visible = false; root.AddChild(_alertPanel);
    }

    private void BuildSelectionPanel(Control root)
    {
        PanelContainer panel = AnchoredPanel("SelectionPanel", 0.10f, 0.90f, 1f, 1f, -218f, -16f);
        panel.AddThemeStyleboxOverride("panel", PanelStyle(Graphite, new Color(0.30f, 0.38f, 0.42f), 1));
        HBoxContainer layout = new(); layout.AddThemeConstantOverride("separation", 14); panel.AddChild(layout);

        PanelContainer portrait = new() { Name = "PortraitSlot", CustomMinimumSize = new Vector2(150, 0), SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        portrait.AddThemeStyleboxOverride("panel", PanelStyle(GraphiteRaised, new Color(0.28f, 0.35f, 0.38f), 1));
        _portraitLabel = HudLabel("PORTRAIT\nRESERVED", 14, TextMuted);
        _portraitLabel.HorizontalAlignment = HorizontalAlignment.Center; _portraitLabel.VerticalAlignment = VerticalAlignment.Center;
        portrait.AddChild(_portraitLabel); layout.AddChild(portrait);

        VBoxContainer box = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; box.AddThemeConstantOverride("separation", 7); layout.AddChild(box);
        _selectionTitle = HudLabel("NO SELECTION", 19, RaiderAccent); box.AddChild(_selectionTitle);
        _selectionDetails = HudLabel("Select Crew, a structure, or an Ore deposit.", 15, TextMuted);
        _selectionDetails.AutowrapMode = TextServer.AutowrapMode.WordSmart; _selectionDetails.SizeFlagsVertical = Control.SizeFlags.ExpandFill; box.AddChild(_selectionDetails);
        _priorityRow = new HBoxContainer { Name = "ContextualEnergyPriority", Visible = false };
        _priorityRow.AddThemeConstantOverride("separation", 5); _priorityRow.AddChild(HudLabel("POWER PRIORITY", 13, TextMuted));
        AddPriorityButton(EnergyPriority.High); AddPriorityButton(EnergyPriority.Normal); AddPriorityButton(EnergyPriority.Low);
        box.AddChild(_priorityRow);
        Label help = HudLabel("RMB context command  •  Shift queues  •  B build  •  F8 developer tools", 13, TextMuted); box.AddChild(help);

        MarginContainer contextualSlot = new() { Name = "ContextualSlot", CustomMinimumSize = new Vector2(340, 0), SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        layout.AddChild(contextualSlot);
        _contextualActions = new VBoxContainer { Name = "ContextualActions", Visible = false };
        _contextualActions.AddThemeConstantOverride("separation", 7); contextualSlot.AddChild(_contextualActions);
        _contextualActions.AddChild(HudLabel("AVAILABLE UNITS", 16, RaiderAccent));
        Label instruction = HudLabel("Selected facility queue — maximum 8", 13, TextMuted); instruction.Name = "ProductionHint"; _contextualActions.AddChild(instruction);
        GridContainer grid = new() { Name = "ProductionGrid", Columns = 2 }; grid.AddThemeConstantOverride("h_separation", 6); grid.AddThemeConstantOverride("v_separation", 6);
        _contextualActions.AddChild(grid);
        for (int i = 0; i < ProductionKeys.Length; i++) AddProductionButton(grid, ProductionKeys[i]);
        Label queueHint = HudLabel("Click +1  •  Shift-click +5", 12, TextMuted); _contextualActions.AddChild(queueHint);
        root.AddChild(panel);
    }

    private void UpdateResources()
    {
        if (_bridge is null || _oreValue is null || _energyValue is null || _crystalValue is null || _ocValue is null) return;
        EntityId[] components = WorksiteGraphSystem.GetPlayerComponents(_bridge.World, 0);
        EntityId activeRoot = ActiveWorksiteRoot(components);
        int ore = activeRoot == EntityId.None ? 0 : WorksiteGraphSystem.GetProcessedResourceTotal(_bridge.World, activeRoot, ResourceType.Ore);
        int pending = activeRoot == EntityId.None ? 0 : WorksiteGraphSystem.GetPendingHauledResourceTotal(_bridge.World, activeRoot, ResourceType.Ore);
        _oreValue.Text = pending > 0 ? $"{ore}  (+{pending} receiving)" : ore.ToString();
        if (components.Length > 1) _oreValue.Text += $"  •  {components.Length} SITES";
        int spendableCrystals = 0, committedCrystals = 0, transitioningCrystals = 0;
        var alive = _bridge.World.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!_bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != 0) continue;
            if (_bridge.World.Entities.ResourceBank.TryGet(id, out ResourceBank crystalBank) && crystalBank.Type == ResourceType.Crystal) spendableCrystals += crystalBank.ProcessedAmount;
            if (_bridge.World.Entities.ResonanceCore.TryGet(id, out ResonanceCore resonance))
            {
                committedCrystals += ResonanceCoreSystem.CountCommitted(resonance);
                if (resonance.TransitionKind != ResonanceTransitionKind.None) transitioningCrystals++;
            }
        }
        AlienChargeState charge = _bridge.World.GetAlienCharge(0);
        _crystalValue.Text = $"{spendableCrystals} spendable  •  Charge {ChargeText(charge.CurrentMillicharge)} / {ChargeText(charge.MaximumMillicharge)}  +{ChargeText(charge.GenerationMillichargePerSecond)}/s";
        if (committedCrystals > 0 || transitioningCrystals > 0)
            _crystalValue.Text += $"  •  {committedCrystals} committed{(transitioningCrystals > 0 ? $"  •  {transitioningCrystals} changing" : string.Empty)}";
        OperationsCapacityState capacity = _bridge.World.GetOperationsCapacity(0);
        _ocValue.Text = capacity.Reserved > 0 ? $"{capacity.Used} / {capacity.Maximum}  (+{capacity.Reserved} queued)" : $"{capacity.Used} / {capacity.Maximum}";
        if (capacity.IsOverCapacity) _ocValue.Text += "  OVER CAPACITY";
        else if (capacity.IsAdvanceWarning) _ocValue.Text += "  WARNING";
        _ocValue.AddThemeColorOverride("font_color", capacity.IsOverCapacity ? Danger : capacity.IsAdvanceWarning ? Warning : TextPrimary);

        EntityId root = activeRoot;
        if (root == EntityId.None || !_bridge.World.Entities.EnergyDomain.Has(root)) { _energyValue.Text = "NO DOMAIN"; return; }
        EnergyDomain energy = _bridge.World.Entities.EnergyDomain.Get(root);
        int net = energy.GenerationPerSecond - energy.ContinuousDemandPerSecond;
        _energyValue.Text = $"{EnergyText(energy.Reserve)} / {EnergyText(energy.ReserveCapacity)}  |  {energy.GenerationPerSecond}↑  {energy.ContinuousDemandPerSecond}↓  |  {(net >= 0 ? "+" : string.Empty)}{net}/s";
        _energyValue.AddThemeColorOverride("font_color", energy.IsBrownout ? Danger : energy.IsDeficit ? Warning : TextPrimary);
        if (_energyPopoverLabel is not null)
        {
            _energyPopoverLabel.Text = $"Worksite #{root.Value}  •  {components.Length} component{(components.Length == 1 ? string.Empty : "s")}\nReserve  {EnergyText(energy.Reserve)} / {EnergyText(energy.ReserveCapacity)}\nGeneration  +{energy.GenerationPerSecond} E/s\nDemand  -{energy.ContinuousDemandPerSecond} E/s\nNet  {(net >= 0 ? "+" : string.Empty)}{net} E/s\nState  {(energy.IsBrownout ? $"BROWNOUT — {energy.PoweredDemandPerSecond}/{energy.ContinuousDemandPerSecond} E/s powered" : energy.IsDeficit ? "Reserve draining" : "Stable")}";
        }
    }

    private void UpdateAlerts()
    {
        if (_bridge is null || _alertPanel is null || _alertLabel is null) return;
        OperationsCapacityState capacity = _bridge.World.GetOperationsCapacity(0);
        EntityId[] roots = WorksiteGraphSystem.GetPlayerComponents(_bridge.World, 0);
        for (int i = 0; i < roots.Length; i++)
        {
            EntityId root = roots[i];
            if (!_bridge.World.Entities.EnergyDomain.TryGet(root, out EnergyDomain energy) || !energy.IsBrownout) continue;
            int deficit = energy.ContinuousDemandPerSecond - energy.GenerationPerSecond;
            _alertPanel.Visible = true; _alertLabel.Text = $"⚡ BROWNOUT — Worksite #{root.Value} demand exceeds generation by {deficit} E/s"; return;
        }
        if (capacity.IsOverCapacity) { _alertPanel.Visible = true; _alertLabel.Text = "OPERATIONS CAPACITY EXCEEDED — increase operational infrastructure"; return; }
        _alertPanel.Visible = false;
    }

    private void UpdateSelection()
    {
        if (_bridge is null || _selection is null || _selectionTitle is null || _selectionDetails is null || _portraitLabel is null || _priorityRow is null) return;
        if (_selection.Selected.Count == 0)
        {
            _selectionTitle.Text = "NO SELECTION"; _selectionDetails.Text = _input?.BuildModeActive == true ? $"BUILD MODE\n{_input.BuildStatus}" : "Select Crew, a structure, or an Ore deposit.";
            _portraitLabel.Text = "PORTRAIT\nRESERVED";
            _priorityRow.Visible = false; return;
        }
        EntityId first = _selection.Selected[0];
        _selectionTitle.Text = _selection.Selected.Count == 1 ? EntityName(first) : $"{_selection.Selected.Count} OBJECTS SELECTED";
        _portraitLabel.Text = PortraitPlaceholder(first, _selection.Selected.Count);
        _builder.Clear();
        if (_selection.Selected.Count > 1) _builder.Append("Primary: ").Append(EntityName(first)).Append('\n');
        if (_bridge.World.Entities.Worker.TryGet(first, out Worker worker) && _bridge.World.Entities.ResourceCarrier.TryGet(first, out ResourceCarrier carrier))
            _builder.Append("Task  ").Append(worker.TaskState).Append("   Cargo  ").Append(carrier.Amount).Append('/').Append(carrier.Capacity).Append(" Ore\n");
        if (_bridge.World.Entities.ConstructionSite.TryGet(first, out ConstructionSite site))
            _builder.Append("Construction  ").Append(site.ProgressTicks * 100 / site.RequiredTicks).Append("%   Reserved  ").Append(site.ReservedOre).Append(" Ore / ").Append(site.ReservedEnergy).Append(" Energy\n");
        if (_bridge.World.Entities.Building.Has(first) && IsRockRaiderWorksiteObject(first))
        {
            if (_bridge.World.Entities.WorksiteMember.TryGet(first, out WorksiteMember worksite)) _builder.Append("Worksite  #").Append(worksite.ComponentRoot.Value).Append("   SERVICED\n");
            else _builder.Append("Worksite  DISCONNECTED — local supplied work may continue\n");
        }
        if (_bridge.World.Entities.ForwardServiceMember.Has(first))
            _builder.Append(ForwardServiceSystem.TryGetProviderForMember(_bridge.World, first, out EntityId serviceProvider)
                ? $"Service Available  •  source #{serviceProvider.Value}\n"
                : "No Forward Service.\n");
        if (_bridge.World.Entities.ForwardServiceProvider.TryGet(first, out ForwardServiceProvider forwardService))
            _builder.Append("Forward Service  ").Append(forwardService.IsActive ? "ACTIVE" : "INACTIVE").Append("  •  ").Append(forwardService.RadiusBuildCells).Append(" cells\n");
        if (_bridge.World.Entities.Deployment.TryGet(first, out Deployment deployment))
            _builder.Append("Deployment  ").Append(deployment.State).Append('\n');
        if (_bridge.World.Entities.MissionRefitState.TryGet(first, out MissionRefitState refitState))
        {
            _builder.Append("Configuration  ").Append(refitState.CurrentConfiguration == MissionConfiguration.T3Survey ? "Survey" : "Escort")
                .Append("  •  Survey module ").Append((refitState.OwnedConfigurationMask & 2) != 0 ? "OWNED" : refitState.SurveyUnlocked ? "AVAILABLE" : "LOCKED").Append('\n');
            if (refitState.ConfigurationLockTicks > 0)
                _builder.Append("Configuration Lock  ").Append((refitState.ConfigurationLockTicks + 19) / 20).Append("s\n");
        }
        if (_bridge.World.Entities.MissionRefitJob.TryGet(first, out MissionRefitJob refitJob))
            _builder.Append("Mission Refit  ").Append((refitJob.TotalTicks - refitJob.RemainingTicks) * 100 / refitJob.TotalTicks).Append("%  •  committed ")
                .Append(refitJob.CommittedOre).Append(" Ore / ").Append(refitJob.CommittedEnergy).Append(" Energy\n");
        if (_bridge.World.Entities.ResonanceCore.TryGet(first, out ResonanceCore resonance))
        {
            int committed = ResonanceCoreSystem.CountCommitted(resonance);
            _builder.Append("Resonance  ").Append(committed).Append(" / ").Append(ResonanceCoreSystem.MaximumSlots(resonance)).Append(" slots  •  desired ").Append(resonance.DesiredCommittedCrystals).Append('\n');
            _builder.Append("Core Demand  ").Append(ResonanceCoreSystem.ContinuousEnergyDemand(resonance)).Append(" E/s  •  ")
                .Append(BrownoutSystem.IsOperational(_bridge.World, first) ? "OPERATIONAL" : "BROWNOUT — commitments retained").Append('\n');
            int coreCommitted = ResonanceCoreSystem.CountCommitted(resonance);
            _builder.Append("Charge Contribution  ").Append(20 * (1 + coreCommitted)).Append(" max  •  +")
                .Append(BrownoutSystem.IsOperational(_bridge.World, first) ? ChargeText(coreCommitted * AlienChargeSystem.GenerationPerCrystalMillichargePerSecond) : "0.0").Append("/s\n");
            if (resonance.TransitionKind != ResonanceTransitionKind.None)
                _builder.Append(resonance.TransitionKind == ResonanceTransitionKind.Commit ? "Committing Crystal  " : "Withdrawing Crystal  ")
                    .Append((resonance.TransitionTotalTicks - resonance.TransitionRemainingTicks) * 100 / resonance.TransitionTotalTicks).Append("%\n");
        }
        if (_bridge.World.Entities.SurgeZone.TryGet(first, out SurgeZone surgeZone))
            _builder.Append(surgeZone.BuildupRemainingTicks > 0 ? "Surge Buildup  " : "Surge Active  ")
                .Append((surgeZone.BuildupRemainingTicks > 0 ? surgeZone.BuildupRemainingTicks : surgeZone.ActiveRemainingTicks) / 20.0f).Append("s  •  ").Append(surgeZone.RadiusBuildCells).Append(" cells\n");
        if (AlienChargeSystem.IsSurged(_bridge.World, first)) _builder.Append("SURGED  •  cadence ×0.80  •  ETX reconfiguration ×0.70\n");
        if (_bridge.World.Entities.TubeStation.TryGet(first, out TubeStation tubeStation))
        {
            _builder.Append("Aero Tube  ").Append(tubeStation.ConnectionCount).Append(" / ").Append(tubeStation.ConnectionLimit).Append(" connections  •  component #").Append(tubeStation.ComponentRoot.Value).Append('\n');
            _builder.Append("Transfer channels  ").Append(tubeStation.HypersledThroughputUnlocked ? 3 : 2).Append(tubeStation.HypersledThroughputUnlocked ? "  •  Hypersled Throughput" : string.Empty).Append('\n');
            if (_bridge.World.Entities.TubeComponent.TryGet(tubeStation.ComponentRoot, out TubeComponent tubeComponent))
                _builder.Append("Network  ").Append(tubeComponent.StationCount).Append(" Stations  •  ").Append(tubeComponent.OperationalLinkCount).Append(" active Links  •  topology ").Append(tubeComponent.TopologyRevision).Append('\n');
        }
        if (_bridge.World.Entities.TubeLink.TryGet(first, out TubeLink tubeLink))
            _builder.Append("Tube Link  #").Append(tubeLink.EndpointA.Value).Append(" → #").Append(tubeLink.EndpointB.Value).Append("  •  ")
                .Append(tubeLink.LengthBuildCells).Append(" cells  •  ").Append(tubeLink.IsOperational ? "ACTIVE  •  1 E/s" : "DISABLED").Append('\n');
        if (_bridge.World.Entities.PowerState.TryGet(first, out PowerState power))
            _builder.Append("Power  ").Append(power.IsPowered ? "ONLINE" : "DISABLED — Energy Domain Brownout").Append("   Priority  ").Append(power.Priority).Append('\n');
        if (_bridge.World.Entities.Production.TryGet(first, out Production production)) AppendProductionQueue(production, first);
        if (_input?.BuildModeActive == true) _builder.Append("BUILD MODE  ").Append(_input.BuildStatus).Append('\n');
        int stabilityTicks = DisplacementSystem.RemainingStabilityTicks(_bridge.World, first);
        if (stabilityTicks > 0) _builder.Append("Stability  ").Append((stabilityTicks + SimClock.TicksPerSecond - 1) / SimClock.TicksPerSecond).Append("s\n");
        if (_builder.Length == 0) _builder.Append("Ready for orders.");
        _selectionDetails.Text = _builder.ToString().TrimEnd();

        bool compatible = false; EnergyPriority common = EnergyPriority.Normal; bool commonSet = false, mixed = false;
        for (int i = 0; i < _selection.Selected.Count; i++)
        {
            if (!_bridge.World.Entities.PowerState.TryGet(_selection.Selected[i], out PowerState state)) continue;
            compatible = true;
            if (!commonSet) { common = state.Priority; commonSet = true; }
            else if (common != state.Priority) mixed = true;
        }
        _priorityRow.Visible = compatible;
        foreach ((EnergyPriority priority, Button button) in _priorityButtons)
            button.Modulate = !mixed && priority == common ? RaiderAccent : Colors.White;
    }

    private void UpdateProduction()
    {
        if (_bridge is null || _input is null || _selection is null || _contextualActions is null) return;
        bool hasProducer = false;
        for (int i = 0; i < _selection.Selected.Count; i++) if (_bridge.World.Entities.Production.Has(_selection.Selected[i])) { hasProducer = true; break; }
        _contextualActions.Visible = hasProducer;
        foreach ((string key, Button button) in _productionButtons) button.Disabled = !hasProducer || !_input.CanQueueProduction(key);
    }

    private void AppendProductionQueue(Production production, EntityId facility)
    {
        _builder.Append("Production  ").Append(production.Count).Append('/').Append(Production.Capacity);
        if (!BrownoutSystem.IsOperational(_bridge!.World, facility)) _builder.Append(" — PAUSED: BROWNOUT");
        else if (production.SpawnBlocked) _builder.Append(" — EXIT BLOCKED");
        _builder.Append('\n');
        for (int i = 0; i < production.Count; i++)
        {
            ProductionQueueItem item = production.Get(i);
            int percent = item.TotalTicks == 0 ? 0 : (item.TotalTicks - item.RemainingTicks) * 100 / item.TotalTicks;
            _builder.Append(i == 0 ? "Active  " : "Queued  ").Append(UnitName(item.UnitType)).Append("  ").Append(percent).Append("%\n");
        }
    }

    private string EntityName(EntityId id)
    {
        if (_bridge is null) return $"Object #{id.Value}";
        if (_bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable))
        {
            if (_bridge.World.Content.TryGetBuilding(selectable.ContentType, out BuildingDefinition building)) return DisplayName(building.StableKey);
            if (_bridge.World.Content.TryGetEntity(selectable.ContentType, out PrototypeEntityDefinition entity)) return DisplayName(entity.StableKey);
            for (int i = 0; i < _bridge.World.Content.ResourceNodes.Length; i++)
                if (_bridge.World.Content.ResourceNodes[i].Id == selectable.ContentType) return DisplayName(_bridge.World.Content.ResourceNodes[i].StableKey);
            string m5Name = M5ProofName(selectable.ContentType);
            if (m5Name.Length > 0) return m5Name;
        }
        return $"Object #{id.Value}";
    }

    private string PortraitPlaceholder(EntityId id, int selectionCount)
    {
        if (selectionCount > 1) return $"GROUP\n{selectionCount} SELECTED";
        if (_bridge is not null && _bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable))
            return selectable.Kind == SelectableKind.Building ? "STRUCTURE VIEW\nART PENDING" : selectable.Kind == SelectableKind.ResourceNode ? "RESOURCE VIEW\nART PENDING" : "UNIT PORTRAIT\nART PENDING";
        return "PORTRAIT\nART PENDING";
    }

    private void AddPriorityButton(EnergyPriority priority)
    {
        Button button = SmallButton(priority.ToString()); button.Name = $"Priority{priority}";
        button.Pressed += () => _input?.SetSelectedEnergyPriority(priority);
        _priorityRow!.AddChild(button); _priorityButtons.Add(priority, button);
    }

    private void AddProductionButton(Container parent, string key)
    {
        Button button = SmallButton(ProductionButtonText(key)); button.Name = key.Split('.')[^1]; button.CustomMinimumSize = new Vector2(0, 58); button.TooltipText = ProductionTooltip(key);
        button.Pressed += () => _input?.QueueProduction(key, Input.IsKeyPressed(Key.Shift));
        parent.AddChild(button); _productionButtons.Add(key, button);
    }

    private static Label AddResourceBlock(Container parent, string title, string value, string tooltip)
    {
        VBoxContainer box = ResourceBox(title, out Label label); box.TooltipText = tooltip; label.Text = value; parent.AddChild(box); return label;
    }

    private static VBoxContainer ResourceBox(string title, out Label value)
    {
        VBoxContainer box = new() { CustomMinimumSize = new Vector2(185, 48) };
        Label heading = HudLabel(title, 12, TextMuted); heading.HorizontalAlignment = HorizontalAlignment.Center; box.AddChild(heading);
        value = HudLabel(string.Empty, 17, TextPrimary); value.HorizontalAlignment = HorizontalAlignment.Center; box.AddChild(value);
        return box;
    }

    private static PanelContainer AnchoredPanel(string name, float left, float right, float top, float bottom, float topOffset, float bottomOffset)
        => new() { Name = name, AnchorLeft = left, AnchorRight = right, AnchorTop = top, AnchorBottom = bottom, OffsetLeft = 0, OffsetRight = 0, OffsetTop = topOffset, OffsetBottom = bottomOffset, MouseFilter = Control.MouseFilterEnum.Stop };

    private static StyleBoxFlat PanelStyle(Color background, Color border, int borderWidth)
        => new() { BgColor = new Color(background, 0.96f), BorderColor = border, BorderWidthLeft = borderWidth, BorderWidthTop = borderWidth, BorderWidthRight = borderWidth, BorderWidthBottom = borderWidth, CornerRadiusTopLeft = 7, CornerRadiusTopRight = 7, CornerRadiusBottomLeft = 7, CornerRadiusBottomRight = 7, ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 8, ContentMarginBottom = 8 };

    private static Label HudLabel(string text, int size, Color color)
    {
        Label label = new() { Text = text }; label.AddThemeFontSizeOverride("font_size", size); label.AddThemeColorOverride("font_color", color); return label;
    }

    private static Button SmallButton(string text)
    {
        Button button = new() { Text = text, CustomMinimumSize = new Vector2(86, 34) }; button.AddThemeFontSizeOverride("font_size", 14); return button;
    }

    private static string EnergyText(Fix32 value)
    {
        int tenths = (int)(((long)value.Raw * 10 + Fix32.OneRaw / 2) / Fix32.OneRaw);
        return $"{tenths / 10}.{tenths % 10}";
    }

    private static string ChargeText(int millicharge)
    {
        int tenths = (millicharge + 50) / 100;
        return $"{tenths / 10}.{tenths % 10}";
    }

    private EntityId ActiveWorksiteRoot(EntityId[] components)
    {
        if (_bridge is not null && _selection is not null && _selection.Selected.Count > 0 &&
            _bridge.World.Entities.WorksiteMember.TryGet(_selection.Selected[0], out WorksiteMember selected)) return selected.ComponentRoot;
        return components.Length > 0 ? components[0] : EntityId.None;
    }

    private bool IsRockRaiderWorksiteObject(EntityId id)
    {
        if (_bridge is null || !_bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable)) return false;
        string[] keys =
        {
            "building.rock_raiders.hq", "building.rock_raiders.ore_processing_plant",
            "building.rock_raiders.power_station", "building.rock_raiders.vehicle_service_bay"
        };
        for (int i = 0; i < keys.Length; i++) if (StableId.FromKey(keys[i]) == selectable.ContentType) return true;
        return false;
    }

    private static string UnitName(ContentId id)
    {
        for (int i = 0; i < ProductionKeys.Length; i++) if (StableId.FromKey(ProductionKeys[i]) == id) return DisplayName(ProductionKeys[i]);
        return id.Value.ToString();
    }

    private static string ProductionButtonText(string key) => key switch
    {
        "unit.rock_raiders.crew" => "CREW\n50 ORE • 1 OC",
        "unit.rock_raiders.hover_scout" => "HOVER SCOUT\n75 ORE • 10 E • 1 OC",
        "unit.rock_raiders.rapid_rider" => "RAPID RIDER\n90 ORE • 10 E • 2 OC",
        "unit.rock_raiders.loader_dozer" => "LOADER DOZER\n125 ORE • 15 E • 3 OC",
        _ => key
    };

    private static string ProductionTooltip(string key) => key switch
    {
        "unit.rock_raiders.crew" => "16 seconds • Worker, builder and repair Crew",
        "unit.rock_raiders.hover_scout" => "20 seconds • Fast reconnaissance vehicle",
        "unit.rock_raiders.rapid_rider" => "28 seconds • Mobile logistics and support vehicle",
        "unit.rock_raiders.loader_dozer" => "36 seconds • Heavy industrial utility vehicle",
        _ => key
    };

    private static string DisplayName(string key) => key switch
    {
        "building.rock_raiders.hq" => "Rock Raiders HQ",
        "building.rock_raiders.ore_processing_plant" => "Ore Processing Plant",
        "building.rock_raiders.power_station" => "Power Station",
        "building.rock_raiders.vehicle_service_bay" => "Vehicle Service Bay",
        "unit.rock_raiders.crew" => "Crew",
        "unit.rock_raiders.hover_scout" => "Hover Scout",
        "unit.rock_raiders.rapid_rider" => "Rapid Rider",
        "unit.rock_raiders.loader_dozer" => "Loader Dozer",
        "resource.ore.standard" => "Standard Ore Deposit",
        "resource.ore.small" => "Small Ore Deposit",
        "resource.ore.rich" => "Rich Ore Deposit",
        "resource.ore.deep_contested_seam" => "Deep Contested Ore Seam",
        "building.ast.service_refit_hub" => "Service & Refit Hub",
        "unit.ast.t3_trike" => "T3-Trike",
        "building.ali.etx_command_core" => "ETX Command Core",
        "building.ali.resonance_core" => "Resonance Core",
        "unit.ali.razor_skimmer" => "Razor Skimmer",
        "building.mar.aero_tube_hangar" => "Aero Tube Hangar",
        "building.mar.settlement_station" => "Settlement Station",
        "unit.mar.worker_robot" => "Worker Robot",
        "unit.mar.double_hover" => "Double Hover",
        "unit.mar.jet_scooter" => "Jet Scooter",
        "unit.mar.excavation_searcher" => "Excavation Searcher",
        "unit.ast.t3_trike.displacement_target" => "Clamp Test Target",
        _ => key
    };

    private static string M5ProofName(ContentId id)
    {
        string[] keys =
        {
            "building.ast.service_refit_hub", "unit.ast.t3_trike", "building.ali.etx_command_core", "building.ali.resonance_core",
            "unit.ali.razor_skimmer", "building.mar.aero_tube_hangar", "building.mar.settlement_station", "unit.mar.worker_robot",
            "unit.mar.double_hover", "unit.mar.jet_scooter", "unit.mar.excavation_searcher", "unit.ast.t3_trike.displacement_target"
        };
        for (int i = 0; i < keys.Length; i++) if (StableId.FromKey(keys[i]) == id) return DisplayName(keys[i]);
        return string.Empty;
    }
}
