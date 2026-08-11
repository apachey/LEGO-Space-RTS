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
        _oreValue = AddResourceBlock(row, "ORE", "500", "Processed Ore available to spend");
        Button energyButton = new() { Name = "EnergyButton", Flat = true, CustomMinimumSize = new Vector2(330, 48), TooltipText = "Open Energy Domain details" };
        VBoxContainer energyBox = ResourceBox("ENERGY", out _energyValue);
        energyButton.AddChild(energyBox);
        energyButton.Pressed += () => { if (_energyPopover is not null) _energyPopover.Visible = !_energyPopover.Visible; };
        row.AddChild(energyButton);
        _crystalValue = AddResourceBlock(row, "CRYSTALS", "0", "Spendable Crystals");
        _ocValue = AddResourceBlock(row, "OPERATIONS", "6 / 16", "Active and maximum Operations Capacity");
        Label phase = new() { Text = "M3", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, CustomMinimumSize = new Vector2(54, 48), TooltipText = "Economy & Base Building prototype" };
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
        _selectionTitle = HudLabel("NO SELECTION", 19, RaiderAccent); _selectionTitle.Name = "SelectionTitle"; box.AddChild(_selectionTitle);
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
        int ore = _bridge.World.GetProcessedResourceTotal(0, ResourceType.Ore);
        int pending = _bridge.World.GetPendingHauledResourceTotal(0, ResourceType.Ore);
        _oreValue.Text = pending > 0 ? $"{ore}  (+{pending} receiving)" : ore.ToString();
        _crystalValue.Text = "0";
        OperationsCapacityState capacity = _bridge.World.GetOperationsCapacity(0);
        _ocValue.Text = capacity.Reserved > 0 ? $"{capacity.Used} / {capacity.Maximum}  (+{capacity.Reserved} queued)" : $"{capacity.Used} / {capacity.Maximum}";
        if (capacity.IsOverCapacity) _ocValue.Text += "  OVER CAPACITY";
        else if (capacity.IsAdvanceWarning) _ocValue.Text += "  WARNING";
        _ocValue.AddThemeColorOverride("font_color", capacity.IsOverCapacity ? Danger : capacity.IsAdvanceWarning ? Warning : TextPrimary);

        if (!EnergyDomainSystem.TryGetPlayerDomain(_bridge.World, 0, out EntityId root)) { _energyValue.Text = "NO DOMAIN"; return; }
        EnergyDomain energy = _bridge.World.Entities.EnergyDomain.Get(root);
        int net = energy.GenerationPerSecond - energy.ContinuousDemandPerSecond;
        _energyValue.Text = $"{EnergyText(energy.Reserve)} / {EnergyText(energy.ReserveCapacity)}  |  {energy.GenerationPerSecond}↑  {energy.ContinuousDemandPerSecond}↓  |  {(net >= 0 ? "+" : string.Empty)}{net}/s";
        _energyValue.AddThemeColorOverride("font_color", energy.IsBrownout ? Danger : energy.IsDeficit ? Warning : TextPrimary);
        if (_energyPopoverLabel is not null)
        {
            _energyPopoverLabel.Text = $"HQ Domain #{root.Value}\nReserve  {EnergyText(energy.Reserve)} / {EnergyText(energy.ReserveCapacity)}\nGeneration  +{energy.GenerationPerSecond} E/s\nDemand  -{energy.ContinuousDemandPerSecond} E/s\nNet  {(net >= 0 ? "+" : string.Empty)}{net} E/s\nState  {(energy.IsBrownout ? $"BROWNOUT — {energy.PoweredDemandPerSecond}/{energy.ContinuousDemandPerSecond} E/s powered" : energy.IsDeficit ? "Reserve draining" : "Stable")}";
        }
    }

    private void UpdateAlerts()
    {
        if (_bridge is null || _alertPanel is null || _alertLabel is null) return;
        OperationsCapacityState capacity = _bridge.World.GetOperationsCapacity(0);
        if (EnergyDomainSystem.TryGetPlayerDomain(_bridge.World, 0, out EntityId root) && _bridge.World.Entities.EnergyDomain.TryGet(root, out EnergyDomain energy) && energy.IsBrownout)
        {
            int deficit = energy.ContinuousDemandPerSecond - energy.GenerationPerSecond;
            _alertPanel.Visible = true; _alertLabel.Text = $"⚡ BROWNOUT — Energy Domain demand exceeds generation by {deficit} E/s"; return;
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
        if (_bridge.World.Entities.Health.TryGet(first, out Health health))
        {
            int percent = health.Maximum.Raw == 0 ? 0 : checked((int)((long)health.Current.Raw * 100 / health.Maximum.Raw));
            _builder.Append("HP  ").Append(health.Current.RoundToInt()).Append(" / ").Append(health.Maximum.RoundToInt())
                .Append("   Armor  A").Append(health.ArmorRating).Append("   ")
                .Append(percent >= 70 ? "HEALTHY" : percent >= 35 ? "DAMAGED" : percent > 0 ? "HEAVILY DAMAGED" : "DEPLETED").Append('\n');
        }
        if (_bridge.World.Entities.Targetable.TryGet(first, out Targetable targetable))
            _builder.Append("Target class  ").Append(targetable.Class).Append('\n');
        if (_bridge.World.Entities.Worker.TryGet(first, out Worker worker) && _bridge.World.Entities.ResourceCarrier.TryGet(first, out ResourceCarrier carrier))
            _builder.Append("Task  ").Append(worker.TaskState).Append("   Cargo  ").Append(carrier.Amount).Append('/').Append(carrier.Capacity).Append(" Ore\n");
        if (_bridge.World.Entities.Builder.TryGet(first, out Builder builder) &&
            (builder.JobState == BuilderJobState.MovingToRepair || builder.JobState == BuilderJobState.Repairing))
            _builder.Append("Repair  ").Append(builder.JobState == BuilderJobState.Repairing ? "ACTIVE" : "APPROACHING")
                .Append("   Target #").Append(builder.RepairTarget.Value).Append('\n');
        if (_bridge.World.Entities.ConstructionSite.TryGet(first, out ConstructionSite site))
            _builder.Append("Construction  ").Append(site.ProgressTicks * 100 / site.RequiredTicks).Append("%   Reserved  ").Append(site.ReservedOre).Append(" Ore / ").Append(site.ReservedEnergy).Append(" Energy\n");
        if (_bridge.World.Entities.PowerState.TryGet(first, out PowerState power))
            _builder.Append("Power  ").Append(power.IsPowered ? "ONLINE" : "DISABLED — Energy Domain Brownout").Append("   Priority  ").Append(power.Priority).Append('\n');
        if (_bridge.World.Entities.Production.TryGet(first, out Production production)) AppendProductionQueue(production, first);
        if (_input?.BuildModeActive == true) _builder.Append("BUILD MODE  ").Append(_input.BuildStatus).Append('\n');
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
        "unit.rock_raiders.chrome_crusher" => "Chrome Crusher",
        "resource.ore.standard" => "Standard Ore Deposit",
        "resource.ore.small" => "Small Ore Deposit",
        "resource.ore.rich" => "Rich Ore Deposit",
        "resource.ore.deep_contested_seam" => "Deep Contested Ore Seam",
        _ => key
    };
}
