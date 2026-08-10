using System.Text;
using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.UI;

public partial class DebugHud : CanvasLayer
{
    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private DebugRenderer? _debug;
    private RtsInputController? _input;
    private FogPresenter? _fog;
    private Label? _label;
    private double _nextHashUpdate;
    private string _cachedHash = "-";
    private readonly StringBuilder _builder = new(1024);
    private readonly Dictionary<string, Button> _productionButtons = new();
    private readonly Dictionary<EnergyPriority, Button> _priorityButtons = new();
    private static readonly string[] ProductionKeys =
    {
        "unit.rock_raiders.crew", "unit.rock_raiders.hover_scout", "unit.rock_raiders.rapid_rider", "unit.rock_raiders.loader_dozer"
    };

    public void Configure(GodotSimBridge bridge, SelectionController selection, RtsInputController input, DebugRenderer debug, FogPresenter fog)
    {
        _bridge = bridge; _selection = selection; _input = input; _debug = debug; _fog = fog;
        Layer = 10;
        PanelContainer panel = new() { Name = "DebugPanel", Position = new Vector2(12, 12), CustomMinimumSize = new Vector2(760, 0) };
        VBoxContainer box = new(); panel.AddChild(box);
        _label = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _label.AddThemeFontSizeOverride("font_size", 18);
        box.AddChild(_label);
        HFlowContainer toggles = new(); box.AddChild(toggles);
        AddToggle(toggles, "Nav", () => _debug.DrawNavigation, v => _debug.DrawNavigation = v);
        AddToggle(toggles, "HPA", () => _debug.DrawClusters, v => _debug.DrawClusters = v);
        AddToggle(toggles, "Portals", () => _debug.DrawPortals, v => _debug.DrawPortals = v);
        AddToggle(toggles, "Paths", () => _debug.DrawPaths, v => _debug.DrawPaths = v);
        AddToggle(toggles, "Separation", () => _debug.DrawLocalSeparation, v => _debug.DrawLocalSeparation = v);
        AddToggle(toggles, "Buckets", () => _debug.DrawSpatialBuckets, v => _debug.DrawSpatialBuckets = v);
        AddToggle(toggles, "Vision", () => _debug.DrawVision, v => _debug.DrawVision = v);
        AddToggle(toggles, "Excavatable", () => _debug.DrawExcavatable, v => _debug.DrawExcavatable = v);
        AddToggle(toggles, "Fog", () => _fog.FogVisible, _fog.SetFogVisible);
        HFlowContainer production = new() { Name = "ProductionButtons" }; box.AddChild(production);
        for (int i = 0; i < ProductionKeys.Length; i++) AddProductionButton(production, ProductionKeys[i]);
        HFlowContainer power = new() { Name = "EnergyPriorityButtons" }; box.AddChild(power);
        AddPriorityButton(power, EnergyPriority.High); AddPriorityButton(power, EnergyPriority.Normal); AddPriorityButton(power, EnergyPriority.Low);
        Button drain = new() { Text = "DEV: Drain Energy", CustomMinimumSize = new Vector2(0f, 34f) };
        drain.Pressed += () => _input?.DebugDrainEnergy(); power.AddChild(drain);
        AddChild(panel);
        ProcessPriority = 200;
    }

    public override void _Process(double delta)
    {
        if (_bridge is null || _selection is null || _label is null) return;
        double now = Time.GetTicksMsec() / 1000.0;
        if (now >= _nextHashUpdate) { _cachedHash = _bridge.StateHashHex(); _nextHashUpdate = now + 0.25; }
        _builder.Clear();
        OperationsCapacityState operationsCapacity = _bridge.World.GetOperationsCapacity(0);
        bool hasEnergy = EnergyDomainSystem.TryGetPlayerDomain(_bridge.World, 0, out EntityId energyRoot);
        EnergyDomain energy = hasEnergy ? _bridge.World.Entities.EnergyDomain.Get(energyRoot) : default;
        _builder.Append("LEGO Space RTS — M3 Economy Prototype\nTick: ").Append(_bridge.World.Tick.Value).Append("   Hash: ").Append(_cachedHash)
            .Append("   Content: ").Append(_bridge.GameplayContentHash.ToString("X16")).Append('\n')
            .Append("Sim: ").Append(_bridge.LastSimulationMs.ToString("F3")).Append(" ms   Path: ").Append(_bridge.LastPathfindingMs.ToString("F3")).Append(" ms   Entities: ").Append(_bridge.World.Entities.Alive.Count).Append('\n')
            .Append("Ore: ").Append(_bridge.World.GetProcessedResourceTotal(0, ResourceType.Ore)).Append(" processed   ")
            .Append(_bridge.World.GetPendingHauledResourceTotal(0, ResourceType.Ore)).Append(" hauled at HQ   OC: ")
            .Append(operationsCapacity.Used).Append(" / ").Append(operationsCapacity.Maximum);
        if (operationsCapacity.Reserved > 0) _builder.Append(" (").Append(operationsCapacity.Reserved).Append(" reserved)");
        if (operationsCapacity.IsOverCapacity) _builder.Append(" — OVER CAPACITY");
        else if (operationsCapacity.IsAdvanceWarning) _builder.Append(" — CAPACITY WARNING");
        if (hasEnergy)
        {
            _builder.Append('\n').Append("Energy: ").Append(EnergyText(energy.Reserve)).Append(" / ").Append(EnergyText(energy.ReserveCapacity))
                .Append("   Generation: +").Append(energy.GenerationPerSecond).Append(" E/s   Demand: -").Append(energy.ContinuousDemandPerSecond).Append(" E/s");
            if (energy.IsBrownout) _builder.Append(" — BROWNOUT: ").Append(energy.PoweredDemandPerSecond).Append('/').Append(energy.ContinuousDemandPerSecond).Append(" E/s powered");
            else if (energy.IsDeficit) _builder.Append(" — RESERVE DRAINING");
        }
        _builder.Append('\n')
            .Append("Selected: ").Append(_selection.Selected.Count).Append(" / 128");
        if (_selection.Selected.Count > 0)
        {
            EntityId first = _selection.Selected[0];
            _builder.Append("   IDs:");
            for (int i = 0; i < _selection.Selected.Count && i < 12; i++) _builder.Append(' ').Append(_selection.Selected[i].Value);
            if (_selection.Selected.Count > 12) _builder.Append(" …");
            if (_bridge.World.Entities.Navigation.TryGet(first, out NavigationAgent n) && _bridge.World.Entities.Movement.TryGet(first, out Movement m) && _bridge.World.Entities.Transform.TryGet(first, out SimTransform t))
            {
                RouteCorridor? corridor = _bridge.World.GetCorridor(first);
                VisibilityState fog = _bridge.World.Fog.Get(0, t.Position.X.FloorToInt(), t.Position.Y.FloorToInt());
                _builder.Append("\nFirst: footprint=").Append(n.Footprint).Append(" pathIndex=").Append(m.PathIndex).Append('/').Append(corridor?.Cells.Count ?? 0)
                    .Append(" local=").Append(m.CompressionTicks > 0 ? "compressed" : "clear").Append(" fog=").Append(fog).Append(" compressTicks=").Append(m.CompressionTicks);
            }
            if (_bridge.World.Entities.Worker.TryGet(first, out Worker worker) && _bridge.World.Entities.ResourceCarrier.TryGet(first, out ResourceCarrier carrier))
                _builder.Append("\nWorker: ").Append(worker.TaskState).Append(" cargo=").Append(carrier.Amount).Append('/').Append(carrier.Capacity)
                    .Append(" resource=").Append(worker.ResourceTarget.Value).Append(" receiver=").Append(worker.ReceiverTarget.Value);
            if (_bridge.World.Entities.Builder.TryGet(first, out Builder construction))
                _builder.Append("\nBuilder: ").Append(construction.JobState).Append(" site=").Append(construction.ConstructionTarget.Value);
            if (_bridge.World.Entities.PowerState.TryGet(first, out PowerState powerState))
                _builder.Append("\nPower: ").Append(powerState.IsPowered ? "ONLINE" : "DISABLED — Energy Domain Brownout")
                    .Append("   Priority: ").Append(powerState.Priority);
        }
        if (_selection.LastFilteredWorkerCount > 0)
            _builder.Append("\nBox-select priority filtered ").Append(_selection.LastFilteredWorkerCount).Append(" worker(s); Ctrl+drag includes workers.");
        if (_input?.BuildModeActive == true) _builder.Append("\nBUILD: ").Append(_input.BuildStatus);
        UpdateProductionButtons();
        UpdatePriorityButtons();
        AppendProductionStatus();
        _builder.Append("\nRMB Move / Harvest / Assist Site | Shift+RMB Queue | S Stop | H Hold");
        _builder.Append("\nB Build Mode | Tab building | R rotate | LMB place | Esc/RMB cancel | Ctrl+Z refund latest unstarted site");
        _builder.Append("\nM2 note: Hold and Stop both halt movement now; Hold differs once combat exists (fires without chasing).");
        _builder.Append("\nCtrl+0–9 assign | 0–9 recall | F9 topology-open | ,/. rotate | wheel zoom");
        _label.Text = _builder.ToString();
    }

    private void AddProductionButton(Container parent, string unitKey)
    {
        Button button = new() { Text = ProductionButtonText(unitKey), Disabled = true, CustomMinimumSize = new Vector2(0f, 34f) };
        button.AddThemeFontSizeOverride("font_size", 15);
        button.Pressed += () => _input?.QueueProduction(unitKey, Input.IsKeyPressed(Key.Shift));
        parent.AddChild(button); _productionButtons.Add(unitKey, button);
    }

    private void UpdateProductionButtons()
    {
        if (_input is null) return;
        foreach ((string key, Button button) in _productionButtons) button.Disabled = !_input.CanQueueProduction(key);
    }

    private void AddPriorityButton(Container parent, EnergyPriority priority)
    {
        Button button = new() { Text = $"Power: {priority}", Disabled = true, CustomMinimumSize = new Vector2(0f, 34f) };
        button.Pressed += () => _input?.SetSelectedEnergyPriority(priority);
        parent.AddChild(button); _priorityButtons.Add(priority, button);
    }

    private void UpdatePriorityButtons()
    {
        if (_bridge is null || _selection is null) return;
        bool compatible = false;
        for (int i = 0; i < _selection.Selected.Count; i++) if (_bridge.World.Entities.PowerState.Has(_selection.Selected[i])) { compatible = true; break; }
        foreach ((EnergyPriority _, Button button) in _priorityButtons) button.Disabled = !compatible;
    }

    private void AppendProductionStatus()
    {
        if (_bridge is null || _selection is null) return;
        for (int i = 0; i < _selection.Selected.Count; i++)
        {
            EntityId id = _selection.Selected[i];
            if (!_bridge.World.Entities.Production.TryGet(id, out Production production)) continue;
            _builder.Append("\nProduction: ").Append(production.Count).Append('/').Append(Production.Capacity);
            if (!BrownoutSystem.IsOperational(_bridge.World, id)) _builder.Append(" — PAUSED: BROWNOUT");
            if (production.SpawnBlocked) _builder.Append(" — EXIT BLOCKED");
            if (production.HasRallyPoint) _builder.Append(" — rally set");
            for (int q = 0; q < production.Count; q++)
            {
                ProductionQueueItem item = production.Get(q);
                int percent = item.TotalTicks == 0 ? 0 : (item.TotalTicks - item.RemainingTicks) * 100 / item.TotalTicks;
                _builder.Append(" | ").Append(q + 1).Append(':').Append(UnitName(item.UnitType)).Append(' ').Append(percent).Append('%');
            }
            return;
        }
    }

    private static string UnitName(ContentId id)
    {
        for (int i = 0; i < ProductionKeys.Length; i++) if (StableId.FromKey(ProductionKeys[i]) == id) return ProductionButtonText(ProductionKeys[i]).Split(' ')[0];
        return id.Value.ToString();
    }

    private static string ProductionButtonText(string key) => key switch
    {
        "unit.rock_raiders.crew" => "Crew 50O 1OC 16s",
        "unit.rock_raiders.hover_scout" => "Scout 75O 10E 1OC 20s",
        "unit.rock_raiders.rapid_rider" => "Rider 90O 10E 2OC 28s",
        "unit.rock_raiders.loader_dozer" => "Dozer 125O 15E 3OC 36s",
        _ => key
    };

    private static string EnergyText(Fix32 value)
    {
        int tenths = (int)(((long)value.Raw * 10 + Fix32.OneRaw / 2) / Fix32.OneRaw);
        return $"{tenths / 10}.{tenths % 10}";
    }

    private static void AddToggle(Container parent, string name, Func<bool> getter, Action<bool> setter)
    {
        Button button = new() { Text = name, ToggleMode = true, ButtonPressed = getter(), CustomMinimumSize = new Vector2(0f, 32f) };
        button.AddThemeFontSizeOverride("font_size", 16);
        button.Pressed += () => setter(button.ButtonPressed);
        parent.AddChild(button);
    }
}
