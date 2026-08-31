using Godot;

namespace LegoSpaceRTS.UI;

public partial class HudView : Control
{
    private const int CommandCapacity = 12;
    private const int GroupCapacity = 8;
    private const int QueueCapacity = 8;
    private const int EventCapacity = 4;

    private readonly List<Label> _titleLabels = new();
    private readonly List<Label> _sectionLabels = new();
    private readonly List<Label> _bodyLabels = new();
    private readonly List<Label> _valueLabels = new();
    private readonly List<Label> _metaLabels = new();
    private readonly List<Label> _iconLabels = new();
    private readonly List<Label> _topStripLabels = new();
    private readonly List<Label> _topStripIconLabels = new();
    private readonly List<Label> _raisedSurfaceLabels = new();
    private readonly List<PanelContainer> _surfacePanels = new();
    private readonly List<PanelContainer> _raisedPanels = new();
    private readonly List<HudFactionChrome> _factionChrome = new();
    private readonly List<HudFactionSurfaceMask> _factionSurfaceMasks = new();
    private readonly List<HudStructuralChrome> _structuralChrome = new();
    private readonly List<HudRasterSurfaceOverlay> _rasterSurfaceOverlays = new();
    private readonly List<Button> _allButtons = new();
    private readonly List<ColorRect> _topDividers = new();
    private readonly Button[] _commandButtons = new Button[CommandCapacity];
    private readonly Label[] _commandIconLabels = new Label[CommandCapacity];
    private readonly Label[] _commandHotkeyLabels = new Label[CommandCapacity];
    private readonly Label[] _commandCostLabels = new Label[CommandCapacity];
    private readonly Button[] _groupButtons = new Button[GroupCapacity];
    private readonly ProgressBar[] _groupHealth = new ProgressBar[GroupCapacity];
    private readonly Control[] _queueRows = new Control[QueueCapacity];
    private readonly Label[] _queueLabels = new Label[QueueCapacity];
    private readonly ProgressBar[] _queueProgress = new ProgressBar[QueueCapacity];
    private readonly Label[] _eventLabels = new Label[EventCapacity];
    private readonly Button[] _priorityButtons = new Button[3];

    private M7HudProfile _profile = M7HudProfile.CreateDefault();
    private HudFrame _frame = new();
    private string _lastSignature = string.Empty;
    private Control? _safeArea;
    private PanelContainer? _topPanel;
    private HudFactionSurfaceMask? _topSurfaceMask;
    private HBoxContainer? _resourceRow;
    private PanelContainer? _bottomDeck;
    private HudFactionSurfaceMask? _bottomSurfaceMask;
    private Control? _bottomDeckContent;
    private HudFactionChrome? _bottomDeckChrome;
    private HudStructuralChrome? _bottomDeckStructuralChrome;
    private PanelContainer? _minimapPanel;
    private PanelContainer? _selectionPanel;
    private PanelContainer? _commandPanel;
    private PanelContainer? _alertPanel;
    private PanelContainer? _eventPanel;
    private PanelContainer? _objectivePanel;
    private PanelContainer? _tooltipPanel;
    private PanelContainer? _energyPopover;
    private Label? _oreHeading;
    private Label? _energyHeading;
    private Label? _crystalHeading;
    private Label? _operationsHeading;
    private Label? _mechanicHeading;
    private Label? _oreValue;
    private Label? _energyValue;
    private Label? _crystalValue;
    private Label? _operationsValue;
    private Label? _mechanicValue;
    private Label? _matchState;
    private Label? _selectionTitle;
    private Label? _selectionSubtitle;
    private Label? _portraitLabel;
    private HudPortraitView? _portraitView;
    private PanelContainer? _portraitPanel;
    private ProgressBar? _selectionHealth;
    private Label? _selectionHealthText;
    private Label? _selectionStatus;
    private GridContainer? _groupList;
    private ScrollContainer? _groupScroll;
    private HBoxContainer? _priorityRow;
    private Label? _priorityHeading;
    private Label? _commandTitle;
    private GridContainer? _commandGrid;
    private Label? _queueHeading;
    private ScrollContainer? _queueScroll;
    private HBoxContainer? _queueList;
    private Button? _alertButton;
    private Label? _objectiveLabel;
    private Label? _tooltipTitle;
    private Label? _tooltipBody;
    private Label? _energyPopoverLabel;
    private Label? _energyPopoverTitle;
    private HudMinimapView? _minimap;
    private bool _treeBuilt;
    private bool _layoutQueued;

    public event Action<string, bool>? CommandRequested;
    public event Action<string>? SelectionGroupRequested;
    public event Action<int>? PowerPriorityRequested;
    public event Action? EnergyDetailsRequested;
    public event Action? AlertRequested;
    public event Action<Vector2I>? MinimapCameraRequested;
    public event Action<Vector2I, bool>? MinimapGroundCommandRequested;

    public M7HudProfile Profile => _profile;
    public HudFrame Frame => _frame;

    public void UpdateMinimapCamera(IReadOnlyList<Vector2> buildPoints) => _minimap?.SetCameraPolygon(buildPoints);

    public void Configure(M7HudProfile profile)
    {
        Name = "HudView";
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        if (!_treeBuilt) BuildTree();
        ApplyProfile(profile);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized && _treeBuilt) LayoutPanels();
    }

    public void ApplyProfile(M7HudProfile profile)
    {
        profile.Normalize();
        _profile = profile;
        if (!_treeBuilt) return;
        ApplyTypography();
        ApplySurfaces();
        _minimap?.ApplyProfile(profile);
        // Visibility changes alter container minimums. Apply the current
        // content before measuring the responsive panel rectangles.
        ApplyFrame(_frame, true);
        LayoutPanels();
        QueueDeferredLayout();
    }

    public void ApplyFrame(HudFrame frame, bool force = false)
    {
        if (!_treeBuilt) return;
        int frameFaction = HudFactionSkinLibrary.IndexFor(frame.Faction);
        if (_profile.ArtSkin.SurfacePalette == HudSurfacePalette.FactionBound &&
            _profile.ArtSkin.Faction != frameFaction)
        {
            HudFactionSkinLibrary.Apply(_profile, frameFaction);
            ApplySurfaces();
            _portraitView?.ApplyProfile(_profile);
        }
        string signature = frame.ContentSignature();
        if (!force && signature == _lastSignature) return;
        bool layoutVisibilityChanged = frame.Selection.Groups.Count != _frame.Selection.Groups.Count ||
            frame.Commands.Count != _frame.Commands.Count || frame.Queue.Count != _frame.Queue.Count ||
            frame.Selection.ShowPowerPriority != _frame.Selection.ShowPowerPriority ||
            (frame.Selection.HealthPercent >= 0) != (_frame.Selection.HealthPercent >= 0) ||
            (frame.Alert.Priority != HudAlertPriority.None && frame.Alert.Text.Length > 0) !=
                (_frame.Alert.Priority != HudAlertPriority.None && _frame.Alert.Text.Length > 0) ||
            (frame.Events.Count > 0) != (_frame.Events.Count > 0) ||
            (frame.Objective.Length > 0) != (_frame.Objective.Length > 0) ||
            (frame.TooltipTitle.Length > 0 && frame.TooltipQuick.Length > 0) !=
                (_frame.TooltipTitle.Length > 0 && _frame.TooltipQuick.Length > 0) ||
            (frame.EnergyPopoverVisible && frame.EnergyPopover.Length > 0) !=
                (_frame.EnergyPopoverVisible && _frame.EnergyPopover.Length > 0);
        _lastSignature = signature;
        _frame = frame;

        SetText(_oreValue, frame.Ore);
        SetText(_energyValue, frame.Energy);
        SetText(_crystalValue, frame.Crystals);
        SetText(_operationsValue, frame.Operations);
        SetText(_mechanicValue, frame.FactionMechanic);
        SetText(_matchState, frame.MatchState);
        SetText(_selectionTitle, frame.Selection.Title);
        SetText(_selectionSubtitle, frame.Selection.Subtitle);
        SetText(_portraitLabel, HudPortraitView.ConciseCaption(frame.Selection));
        _portraitView?.SetSelection(frame.Selection);
        SetText(_selectionHealthText, frame.Selection.HealthText);
        SetText(_selectionStatus, frame.Selection.StatusText);
        if (_alertButton is not null)
        {
            _alertButton.Text = frame.Alert.Text;
            _alertButton.TooltipText = frame.Alert.Text;
            _alertButton.Disabled = !frame.Alert.Actionable;
            _alertButton.MouseDefaultCursorShape = frame.Alert.Actionable
                ? Control.CursorShape.PointingHand
                : Control.CursorShape.Arrow;
        }
        SetText(_objectiveLabel, frame.Objective);
        SetText(_tooltipTitle, frame.TooltipTitle);
        SetText(_tooltipBody, frame.ExpandedTooltip && frame.TooltipExpanded.Length > 0
            ? $"{frame.TooltipQuick}\n\n{frame.TooltipExpanded}" : frame.TooltipQuick);
        SetText(_energyPopoverLabel, frame.EnergyPopover);

        bool groupedSelection = frame.Selection.Groups.Count > 0;
        if (_selectionHealth is not null)
        {
            _selectionHealth.Visible = !groupedSelection && frame.Selection.HealthPercent >= 0;
            _selectionHealth.Value = Math.Clamp(frame.Selection.HealthPercent, 0, 100);
            ApplyHealthFill(_selectionHealth, frame.Selection.HealthPercent);
        }
        if (_selectionHealthText is not null) _selectionHealthText.Visible = !groupedSelection && frame.Selection.HealthPercent >= 0;
        if (_selectionSubtitle is not null) _selectionSubtitle.Visible = !groupedSelection;
        if (_selectionStatus is not null) _selectionStatus.Visible = !groupedSelection;
        if (_priorityRow is not null) _priorityRow.Visible = !groupedSelection && frame.Selection.ShowPowerPriority;
        for (int i = 0; i < _priorityButtons.Length; i++)
            _priorityButtons[i].ButtonPressed = !frame.Selection.MixedPowerPriority && frame.Selection.PowerPriority == i;
        if (_portraitPanel is not null) _portraitPanel.Visible = _profile.Content.ShowPortrait;
        if (_alertPanel is not null) _alertPanel.Visible = frame.Alert.Priority != HudAlertPriority.None && frame.Alert.Text.Length > 0;
        if (_eventPanel is not null) _eventPanel.Visible = _profile.Content.ShowEventFeed && frame.Events.Count > 0;
        if (_objectivePanel is not null) _objectivePanel.Visible = _profile.Content.ShowObjectiveTracker && frame.Objective.Length > 0;
        if (_tooltipPanel is not null) _tooltipPanel.Visible = frame.TooltipTitle.Length > 0 && frame.TooltipQuick.Length > 0;
        if (_energyPopover is not null) _energyPopover.Visible = frame.EnergyPopoverVisible && frame.EnergyPopover.Length > 0;

        ApplyPriorityColor(_operationsValue, frame.ResourcePriority);
        ApplyPriorityPanel(_alertPanel, frame.Alert.Priority);
        ApplySelectionGroups(frame.Selection.Groups);
        ApplyCommands(frame.Commands);
        ApplyQueue(frame.Queue);
        ApplyEvents(frame.Events);
        _minimap?.SetFrame(frame.Minimap);
        if (layoutVisibilityChanged)
        {
            LayoutPanels();
            QueueDeferredLayout();
        }
    }

    private void QueueDeferredLayout()
    {
        if (_layoutQueued) return;
        _layoutQueued = true;
        Callable.From(() =>
        {
            _layoutQueued = false;
            if (IsInsideTree()) LayoutPanels();
        }).CallDeferred();
    }

    private void BuildTree()
    {
        _treeBuilt = true;
        _safeArea = new Control { Name = "HudSafeArea", MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_safeArea);

        BuildTopStrip();
        BuildBottomDeck();
        BuildMinimapRegion();
        BuildSelectionRegion();
        BuildPortraitRegion();
        BuildCommandRegion();
        BuildTransientRegions();
    }

    private void BuildBottomDeck()
    {
        _bottomDeck = SurfacePanel("BottomDeck", false);
        _safeArea!.AddChild(_bottomDeck);
        _bottomSurfaceMask = DirectSurfaceMask(_bottomDeck);
        if (_bottomSurfaceMask is not null &&
            _bottomSurfaceMask.FindChild("BottomDeckRasterSurface", false, false) is CanvasItem rasterSurface)
            _bottomSurfaceMask.RegisterMaskedSurface(rasterSurface);
        _bottomDeckContent = new Control
        {
            Name = "BottomDeckContent",
            MouseFilter = MouseFilterEnum.Ignore,
            // Above structural backing, below the visible faction frame. The
            // inherited shader clips independently of CanvasItem Z ordering.
            ZIndex = 11
        };
        if (_bottomSurfaceMask is not null) _bottomSurfaceMask.AddChild(_bottomDeckContent);
        else _bottomDeck.AddChild(_bottomDeckContent);
        _bottomDeckChrome = DirectChrome(_bottomDeck);
        _bottomDeckStructuralChrome = DirectStructuralChrome(_bottomDeck);
    }

    private void BuildTopStrip()
    {
        _topPanel = SurfacePanel("ResourceStrip", false);
        _safeArea!.AddChild(_topPanel);
        _topSurfaceMask = DirectSurfaceMask(_topPanel);
        _resourceRow = new HBoxContainer
        {
            Name = "ResourceRow",
            Alignment = BoxContainer.AlignmentMode.End,
            // Above structural backing, below the visible faction frame.
            ZIndex = 11
        };
        if (_topSurfaceMask is not null) _topSurfaceMask.AddChild(_resourceRow);
        else _topPanel.AddChild(_resourceRow);
        HBoxContainer row = _resourceRow;
        AddResourceChip(row, "◆", "ORE", out _oreHeading, out _oreValue);
        AddTopDivider(row);
        Button energyButton = Button("", "EnergyButton");
        energyButton.Flat = true;
        energyButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        energyButton.SizeFlagsStretchRatio = 1.55f;
        HBoxContainer energy = ResourceChip("ϟ", "ENERGY", out _energyHeading, out _energyValue);
        energyButton.AddChild(energy);
        energyButton.Pressed += () => EnergyDetailsRequested?.Invoke();
        row.AddChild(energyButton);
        AddTopDivider(row);
        AddResourceChip(row, "◇", "CRYSTALS", out _crystalHeading, out _crystalValue);
        AddTopDivider(row);
        AddResourceChip(row, "▦", "OPERATIONS", out _operationsHeading, out _operationsValue);
        AddTopDivider(row);
        HBoxContainer mechanic = ResourceChip("◈", "SYSTEM", out _mechanicHeading, out _mechanicValue);
        mechanic.SizeFlagsStretchRatio = 2.8f;
        row.AddChild(mechanic);
        AddTopDivider(row);
        _matchState = Label("MATCH", TextRole.Meta);
        _matchState.HorizontalAlignment = HorizontalAlignment.Center;
        _matchState.VerticalAlignment = VerticalAlignment.Center;
        _matchState.CustomMinimumSize = new Vector2(132, 0);
        row.AddChild(_matchState);
        _topStripLabels.Add(_matchState);
    }

    private void BuildMinimapRegion()
    {
        _minimapPanel = SurfacePanel("MinimapRegion", false);
        _bottomDeckContent!.AddChild(_minimapPanel);
        _bottomSurfaceMask?.RegisterMaskedSurface(_minimapPanel);
        _minimap = new HudMinimapView
        {
            Name = "MinimapSlot", SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _minimap.Configure(_profile);
        _bottomSurfaceMask?.RegisterMaskedSurfaceTree(_minimap);
        _minimap.CameraRequested += cell => MinimapCameraRequested?.Invoke(cell);
        _minimap.GroundCommandRequested += (cell, queued) => MinimapGroundCommandRequested?.Invoke(cell, queued);
        _minimap.TooltipText = "North-up tactical map. Left-click or drag: camera. Right-click: move/rally. Shift + right-click: queue.";
        _minimapPanel.AddChild(_minimap);
        Label north = Label("N", TextRole.Section);
        north.Name = "MinimapLegend";
        north.MouseFilter = MouseFilterEnum.Ignore;
        north.AnchorLeft = 0.82f;
        north.AnchorRight = 1f;
        north.AnchorBottom = 0.18f;
        north.HorizontalAlignment = HorizontalAlignment.Center;
        north.VerticalAlignment = VerticalAlignment.Center;
        _minimap.AddChild(north);
    }

    private void BuildSelectionRegion()
    {
        _selectionPanel = SurfacePanel("SelectionPanel", false);
        _bottomDeckContent!.AddChild(_selectionPanel);
        VBoxContainer information = new() { Name = "SelectionInformation", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _selectionPanel.AddChild(information);
        HBoxContainer titleRow = new() { Name = "SelectionHeader" };
        information.AddChild(titleRow);
        _selectionTitle = Label("NO SELECTION", TextRole.Title);
        _selectionTitle.Name = "SelectionTitle";
        _selectionTitle.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        titleRow.AddChild(_selectionTitle);
        _selectionHealthText = Label(string.Empty, TextRole.Meta);
        _selectionHealthText.HorizontalAlignment = HorizontalAlignment.Right;
        titleRow.AddChild(_selectionHealthText);
        _selectionSubtitle = Label("Select an entity or issue a command.", TextRole.Meta);
        _selectionSubtitle.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _selectionSubtitle.ClipText = true;
        information.AddChild(_selectionSubtitle);
        _selectionHealth = new ProgressBar
        {
            Name = "SelectionHealth", MinValue = 0, MaxValue = 100, ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 8)
        };
        information.AddChild(_selectionHealth);
        _selectionStatus = Label(string.Empty, TextRole.Body);
        _selectionStatus.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _selectionStatus.MaxLinesVisible = 2;
        _selectionStatus.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _selectionStatus.ClipText = true;
        information.AddChild(_selectionStatus);
        _priorityRow = new HBoxContainer { Name = "ContextualEnergyPriority" };
        _priorityHeading = Label("POWER", TextRole.Section);
        _priorityRow.AddChild(_priorityHeading);
        string[] priorities = { "HIGH", "NORMAL", "LOW" };
        for (int i = 0; i < priorities.Length; i++)
        {
            int priority = i;
            _priorityButtons[i] = Button(priorities[i], $"Priority{priorities[i]}");
            _priorityButtons[i].ToggleMode = true;
            _priorityButtons[i].Pressed += () => PowerPriorityRequested?.Invoke(priority);
            _priorityRow.AddChild(_priorityButtons[i]);
        }
        information.AddChild(_priorityRow);

        _groupScroll = new ScrollContainer
        {
            Name = "SelectionGroupScroll", SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill, HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        information.AddChild(_groupScroll);
        _groupList = new GridContainer
        {
            Name = "SelectionTypeGroups", Columns = 4, SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _groupScroll.AddChild(_groupList);
        for (int i = 0; i < GroupCapacity; i++)
        {
            int index = i;
            VBoxContainer group = new()
            {
                Name = $"SelectionGroupCard{i}",
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            _groupButtons[i] = Button("GROUP", $"SelectionGroup{i}");
            _groupButtons[i].Alignment = HorizontalAlignment.Center;
            _groupButtons[i].CustomMinimumSize = new Vector2(142, 52);
            _groupButtons[i].TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            _groupButtons[i].ClipText = true;
            _groupButtons[i].Pressed += () =>
            {
                if (index < _frame.Selection.Groups.Count) SelectionGroupRequested?.Invoke(_frame.Selection.Groups[index].Id);
            };
            group.AddChild(_groupButtons[i]);
            _groupHealth[i] = new ProgressBar
            {
                MinValue = 0, MaxValue = 100, ShowPercentage = false,
                CustomMinimumSize = new Vector2(0, 4)
            };
            group.AddChild(_groupHealth[i]);
            _groupList.AddChild(group);
        }
    }

    private void BuildPortraitRegion()
    {
        _portraitPanel = SurfacePanel("PortraitSlot", true);
        _bottomDeckContent!.AddChild(_portraitPanel);
        _portraitPanel.CustomMinimumSize = new Vector2(132, 0);
        VBoxContainer portraitStack = new() { Name = "TacticalPortraitStack" };
        _portraitPanel.AddChild(portraitStack);
        _portraitView = new HudPortraitView
        {
            Name = "TacticalPortrait",
            CustomMinimumSize = new Vector2(128, 112),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _portraitView.ApplyProfile(_profile);
        portraitStack.AddChild(_portraitView);
        _portraitLabel = Label("NO SELECTION", TextRole.Meta);
        _portraitLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _portraitLabel.VerticalAlignment = VerticalAlignment.Center;
        _portraitLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _portraitLabel.ClipText = true;
        _portraitLabel.CustomMinimumSize = new Vector2(0, 18);
        portraitStack.AddChild(_portraitLabel);
    }

    private void BuildCommandRegion()
    {
        _commandPanel = SurfacePanel("CommandPanel", false);
        _bottomDeckContent!.AddChild(_commandPanel);
        _bottomSurfaceMask?.RegisterMaskedSurface(_commandPanel);
        VBoxContainer box = new() { Name = "CommandStack" }; _commandPanel.AddChild(box);
        _commandTitle = Label("COMMANDS", TextRole.Section);
        _commandTitle.Name = "CommandTitle";
        _commandTitle.Visible = true;
        box.AddChild(_commandTitle);
        _queueHeading = Label("QUEUE", TextRole.Section);
        _queueHeading.Visible = false;
        box.AddChild(_queueHeading);
        _queueScroll = new ScrollContainer
        {
            Name = "ProductionQueueScroll", CustomMinimumSize = new Vector2(0, 38),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Auto,
            VerticalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        box.AddChild(_queueScroll);
        _queueList = new HBoxContainer { Name = "ProductionQueue", SizeFlagsVertical = SizeFlags.ExpandFill };
        _queueScroll.AddChild(_queueList);
        for (int i = 0; i < QueueCapacity; i++)
        {
            VBoxContainer queueRow = new() { CustomMinimumSize = new Vector2(104, 0) };
            _queueRows[i] = queueRow;
            HBoxContainer line = new(); queueRow.AddChild(line);
            _queueLabels[i] = Label(string.Empty, TextRole.Meta);
            _queueLabels[i].SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _queueLabels[i].TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            _queueLabels[i].ClipText = true;
            line.AddChild(_queueLabels[i]);
            Button cancel = Button("×", $"CancelQueue{i}");
            cancel.CustomMinimumSize = new Vector2(22, 20);
            cancel.Disabled = true;
            // Queue cancellation is not implemented until T073. Do not spend
            // scarce command-card width on a dead control; the retained node
            // can become visible when the actual interaction exists.
            cancel.Visible = false;
            cancel.TooltipText = "Cancel this queued item.";
            line.AddChild(cancel);
            _queueProgress[i] = new ProgressBar
            {
                MinValue = 0, MaxValue = 100, ShowPercentage = false,
                CustomMinimumSize = new Vector2(0, 4)
            };
            queueRow.AddChild(_queueProgress[i]);
            _queueList.AddChild(queueRow);
        }
        _commandGrid = new GridContainer
        {
            Name = "CommandGrid", Columns = 4, SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        box.AddChild(_commandGrid);
        for (int i = 0; i < CommandCapacity; i++)
        {
            int index = i;
            _commandButtons[i] = Button("—", $"Command{i}");
            _commandButtons[i].CustomMinimumSize = new Vector2(58, 40);
            _commandButtons[i].SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _commandButtons[i].SizeFlagsVertical = SizeFlags.ExpandFill;
            _commandButtons[i].ToggleMode = true;
            _commandButtons[i].ClipContents = true;
            _commandButtons[i].Text = string.Empty;
            _commandIconLabels[i] = CommandOverlayLabel("◆", TextRole.Icon);
            _commandIconLabels[i].SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            _commandIconLabels[i].HorizontalAlignment = HorizontalAlignment.Center;
            _commandIconLabels[i].VerticalAlignment = VerticalAlignment.Center;
            _commandButtons[i].AddChild(_commandIconLabels[i]);
            _commandHotkeyLabels[i] = CommandOverlayLabel(string.Empty, TextRole.Meta);
            _commandHotkeyLabels[i].AnchorLeft = 0.68f;
            _commandHotkeyLabels[i].AnchorRight = 1f;
            _commandHotkeyLabels[i].AnchorBottom = 0.42f;
            _commandHotkeyLabels[i].HorizontalAlignment = HorizontalAlignment.Center;
            _commandHotkeyLabels[i].VerticalAlignment = VerticalAlignment.Center;
            _commandButtons[i].AddChild(_commandHotkeyLabels[i]);
            _commandCostLabels[i] = CommandOverlayLabel(string.Empty, TextRole.Meta);
            _commandCostLabels[i].AnchorTop = 0.70f;
            _commandCostLabels[i].AnchorRight = 1f;
            _commandCostLabels[i].AnchorBottom = 1f;
            _commandCostLabels[i].HorizontalAlignment = HorizontalAlignment.Center;
            _commandCostLabels[i].VerticalAlignment = VerticalAlignment.Center;
            _commandCostLabels[i].TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            _commandCostLabels[i].ClipText = true;
            _commandButtons[i].AddChild(_commandCostLabels[i]);
            _commandButtons[i].Pressed += () =>
            {
                if (index >= _frame.Commands.Count) return;
                HudCommandFrame command = _frame.Commands[index];
                CommandRequested?.Invoke(command.Id, command.QueueFive && Input.IsKeyPressed(Key.Shift));
            };
            _commandGrid.AddChild(_commandButtons[i]);
        }
    }

    private void BuildTransientRegions()
    {
        _alertPanel = SurfacePanel("AlertAccess", false); _safeArea!.AddChild(_alertPanel);
        _alertButton = Button("", "ActionableAlert");
        _alertButton.Flat = true;
        _alertButton.Alignment = HorizontalAlignment.Center;
        _alertButton.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _alertButton.ClipText = true;
        _alertButton.Pressed += () =>
        {
            if (_frame.Alert.Actionable) AlertRequested?.Invoke();
        };
        _alertPanel.AddChild(_alertButton);

        _eventPanel = SurfacePanel("EventFeed", false); _safeArea.AddChild(_eventPanel);
        VBoxContainer eventBox = new(); _eventPanel.AddChild(eventBox);
        for (int i = 0; i < EventCapacity; i++)
        {
            _eventLabels[i] = Label(string.Empty, TextRole.Meta);
            _eventLabels[i].TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            _eventLabels[i].ClipText = true;
            eventBox.AddChild(_eventLabels[i]);
        }

        _objectivePanel = SurfacePanel("ObjectiveTracker", false); _safeArea.AddChild(_objectivePanel);
        _objectiveLabel = Label(string.Empty, TextRole.Body);
        _objectiveLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _objectiveLabel.CustomMinimumSize = new Vector2(320, 0);
        _objectiveLabel.SizeFlagsVertical = SizeFlags.ExpandFill;
        _objectiveLabel.VerticalAlignment = VerticalAlignment.Center;
        _objectiveLabel.MaxLinesVisible = 3;
        _objectiveLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _objectiveLabel.ClipText = true;
        _objectivePanel.AddChild(_objectiveLabel);

        _tooltipPanel = SurfacePanel("HudTooltip", true); _safeArea.AddChild(_tooltipPanel);
        VBoxContainer tooltipBox = new(); _tooltipPanel.AddChild(tooltipBox);
        _tooltipTitle = Label(string.Empty, TextRole.Title); tooltipBox.AddChild(_tooltipTitle);
        _tooltipBody = Label(string.Empty, TextRole.Body);
        _tooltipBody.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _tooltipBody.MaxLinesVisible = 7;
        _tooltipBody.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _tooltipBody.ClipText = true;
        _tooltipBody.SizeFlagsVertical = SizeFlags.ExpandFill;
        tooltipBox.AddChild(_tooltipBody);

        _energyPopover = SurfacePanel("EnergyDomainPopover", true); _safeArea.AddChild(_energyPopover);
        VBoxContainer energyBox = new(); _energyPopover.AddChild(energyBox);
        _energyPopoverTitle = Label("ENERGY DOMAINS", TextRole.Title); energyBox.AddChild(_energyPopoverTitle);
        _energyPopoverLabel = Label(string.Empty, TextRole.Body); _energyPopoverLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart; energyBox.AddChild(_energyPopoverLabel);
        _raisedSurfaceLabels.Add(_tooltipTitle);
        _raisedSurfaceLabels.Add(_tooltipBody);
        _raisedSurfaceLabels.Add(_energyPopoverTitle);
        _raisedSurfaceLabels.Add(_energyPopoverLabel);
    }

    private void LayoutPanels()
    {
        if (_safeArea is null || _topPanel is null || _bottomDeck is null || _bottomDeckContent is null || _minimapPanel is null || _selectionPanel is null || _portraitPanel is null || _commandPanel is null || _alertPanel is null ||
            _eventPanel is null || _objectivePanel is null || _tooltipPanel is null || _energyPopover is null) return;
        float safeInset = (100f - _profile.Layout.SafeAreaPercent) / 200f;
        _safeArea.AnchorLeft = safeInset; _safeArea.AnchorRight = 1f - safeInset;
        _safeArea.AnchorTop = safeInset; _safeArea.AnchorBottom = 1f - safeInset;
        _safeArea.OffsetLeft = _safeArea.OffsetRight = _safeArea.OffsetTop = _safeArea.OffsetBottom = 0f;
        Vector2 area = _safeArea.Size;
        if (area.X <= 1f || area.Y <= 1f) return;
        float scale = ResponsiveScale();
        float verticalScale = Math.Max(scale, ResponsiveTextScale());
        float gap = _profile.Layout.PanelGap * scale;
        float sectionGap = Mathf.Clamp(gap, 5f, 12f);
        float topHeight = _profile.Layout.TopStripHeight * verticalScale;
        float requestedBottomHeight = _profile.Layout.BottomRegionHeight * verticalScale;
        float availableBottomHeight = Math.Max(110f, area.Y - topHeight - gap * 2f);
        float bottomHeight = Math.Min(requestedBottomHeight, availableBottomHeight);
        float topWidth = Math.Min(area.X * 0.92f, 1280f * scale);
        SetRect(_topPanel, new Vector2((area.X - topWidth) * 0.5f, 0f), new Vector2(topWidth, topHeight));
        Rect2 topContentRect = _topSurfaceMask?.ContentRectFor(new Vector2(topWidth, topHeight)) ??
            new Rect2(Vector2.Zero, new Vector2(topWidth, topHeight));
        if (_resourceRow is not null) SetRect(_resourceRow, topContentRect.Position, topContentRect.Size);

        // Keep the command console coherent on wide screens instead of
        // stretching its information bays into large, unreadable voids.
        float deckWidth = Math.Min(area.X * 0.96f, 1680f * scale);
        float deckX = (area.X - deckWidth) * 0.5f;
        float deckY = area.Y - bottomHeight;
        SetRect(_bottomDeck, new Vector2(deckX, deckY), new Vector2(deckWidth, bottomHeight));
        Rect2 deckContentRect = _bottomSurfaceMask?.InteractiveRectFor(new Vector2(deckWidth, bottomHeight)) ??
            new Rect2(Vector2.Zero, new Vector2(deckWidth, bottomHeight));
        SetRect(_bottomDeckContent, deckContentRect.Position, deckContentRect.Size);
        float contentWidth = deckContentRect.Size.X;
        float contentHeight = deckContentRect.Size.Y;

        // Minimum sizes must be current before any container minimum is read.
        _portraitPanel.CustomMinimumSize = Vector2.Zero;
        if (_portraitView is not null) _portraitView.CustomMinimumSize = new Vector2(0f, 96f * scale);
        if (_groupScroll is not null) _groupScroll.CustomMinimumSize = Vector2.Zero;
        if (_groupList is not null) _groupList.CustomMinimumSize = Vector2.Zero;
        for (int i = 0; i < _groupButtons.Length; i++)
            _groupButtons[i].CustomMinimumSize = new Vector2(126f * scale, 54f * scale);
        for (int i = 0; i < _commandButtons.Length; i++)
            _commandButtons[i].CustomMinimumSize = new Vector2(62f * scale, 44f * scale);
        if (_queueScroll is not null) _queueScroll.CustomMinimumSize = new Vector2(0f, 36f * scale);
        for (int i = 0; i < _queueRows.Length; i++)
            _queueRows[i].CustomMinimumSize = new Vector2(98f * scale, 0f);

        float minimapMaximum = Math.Max(150f * scale,
            Math.Min(contentWidth * 0.24f, contentHeight * 1.12f));
        float minimapMinimum = Math.Min(minimapMaximum, Math.Max(150f * scale, contentHeight * 0.80f));
        float minimapWidth = Mathf.Clamp(_profile.Layout.MinimapSize * scale,
            minimapMinimum, minimapMaximum);
        float commandMaximum = Math.Max(248f * scale, Math.Min(contentWidth * 0.29f, 340f * scale));
        float commandWidth = Mathf.Clamp(_profile.Layout.CommandPanelWidth * scale,
            Math.Min(248f * scale, commandMaximum), commandMaximum);
        float portraitWidth = _portraitPanel.Visible
            ? Mathf.Clamp(148f * scale, 112f * scale, Math.Min(contentWidth * 0.14f, 164f * scale))
            : 0f;
        int gapCount = _portraitPanel.Visible ? 3 : 2;
        float availableSelectionWidth = Math.Max(250f * scale,
            contentWidth - minimapWidth - portraitWidth - commandWidth - sectionGap * gapCount);
        float selectionWidth = availableSelectionWidth;
        float functionalHeight = contentHeight;
        SetRect(_minimapPanel, Vector2.Zero, new Vector2(minimapWidth, functionalHeight));
        SetRect(_selectionPanel, new Vector2(minimapWidth + sectionGap, 0f),
            new Vector2(selectionWidth, functionalHeight));
        float portraitX = minimapWidth + sectionGap + selectionWidth + sectionGap;
        SetRect(_portraitPanel, new Vector2(portraitX, 0f),
            new Vector2(portraitWidth, functionalHeight));
        SetRect(_commandPanel, new Vector2(contentWidth - commandWidth, 0f),
            new Vector2(commandWidth, functionalHeight));
        _bottomDeckChrome?.SetJunctions(
            (deckContentRect.Position.X + minimapWidth + sectionGap * 0.5f) / deckWidth,
            (deckContentRect.End.X - commandWidth - sectionGap * 0.5f) / deckWidth);
        if (_portraitPanel.Visible)
            _bottomDeckStructuralChrome?.SetJunctions(
                (deckContentRect.Position.X + minimapWidth + sectionGap * 0.5f) / deckWidth,
                (deckContentRect.Position.X + portraitX - sectionGap * 0.5f) / deckWidth,
                (deckContentRect.End.X - commandWidth - sectionGap * 0.5f) / deckWidth);
        else
            _bottomDeckStructuralChrome?.SetJunctions(
                (deckContentRect.Position.X + minimapWidth + sectionGap * 0.5f) / deckWidth,
                (deckContentRect.End.X - commandWidth - sectionGap * 0.5f) / deckWidth);

        float alertHeight = 44f * scale;
        SetRect(_alertPanel, new Vector2(deckX, deckY - alertHeight - gap),
            new Vector2(Math.Min(deckWidth * 0.38f, Math.Max(minimapWidth, 390f * scale)), alertHeight));
        float eventHeight = Math.Max(76f * scale, _eventPanel.GetCombinedMinimumSize().Y);
        float tooltipHeight = Math.Max(196f * scale, _tooltipPanel.GetCombinedMinimumSize().Y);
        float commandX = deckX + deckContentRect.End.X - commandWidth;
        float topOverlayLimit = topHeight + gap;
        float overlayBottom = deckY - gap;
        float availableStackHeight = Math.Max(0f, overlayBottom - topOverlayLimit);
        tooltipHeight = Math.Min(tooltipHeight, availableStackHeight);
        float tooltipY = Math.Max(topOverlayLimit, overlayBottom - tooltipHeight);
        float leftOverlayBottom = _alertPanel.Visible ? deckY - alertHeight - gap * 2f : overlayBottom;
        float eventY = Math.Max(topOverlayLimit, leftOverlayBottom - eventHeight);
        SetRect(_eventPanel, new Vector2(deckX, eventY),
            new Vector2(Math.Min(area.X * 0.36f, 480f * scale), eventHeight));
        SetRect(_objectivePanel, new Vector2(deckX, topHeight + gap),
            new Vector2(Math.Min(area.X * 0.38f, 390f * scale), 82f * scale));
        SetRect(_tooltipPanel, new Vector2(commandX, tooltipY), new Vector2(commandWidth, tooltipHeight));
        float popoverWidth = Math.Min(390f * scale, area.X * 0.42f);
        SetRect(_energyPopover, new Vector2(deckX + deckWidth - popoverWidth, topHeight + gap),
            new Vector2(popoverWidth, 174f * scale));
    }

    private void ApplyTypography()
    {
        // Geometry may shrink aggressively for compact aspect ratios, but HUD
        // text keeps a 90% floor so 900p previews do not collapse into 9px type.
        float requestedScale = ResponsiveTextScale() * _profile.Typography.TextScale;
        int titleSize = RoundFont(18f * requestedScale);
        int sectionSize = Math.Max(11, RoundFont(12f * requestedScale));
        int bodySize = RoundFont(14f * requestedScale);
        int valueSize = Math.Max(bodySize, RoundFont(15f * requestedScale));
        int metaSize = Math.Max(10, RoundFont(11f * requestedScale));
        int buttonSize = Math.Max(11, RoundFont(13f * requestedScale));
        Font? headingFont = HudTypographyLibrary.Heading;
        Font? bodyFont = HudTypographyLibrary.Body;
        Font? mediumFont = HudTypographyLibrary.BodyMedium;
        ApplyType(_titleLabels, headingFont, titleSize);
        ApplyType(_sectionLabels, headingFont, sectionSize);
        ApplyType(_bodyLabels, bodyFont, bodySize);
        ApplyType(_valueLabels, mediumFont, valueSize);
        ApplyType(_metaLabels, bodyFont, metaSize);
        for (int i = 0; i < _iconLabels.Count; i++)
            _iconLabels[i].AddThemeFontSizeOverride("font_size", Math.Max(18, RoundFont(24f * requestedScale)));
        for (int i = 0; i < _allButtons.Count; i++)
        {
            _allButtons[i].AddThemeFontSizeOverride("font_size", buttonSize);
            if (mediumFont is not null) _allButtons[i].AddThemeFontOverride("font", mediumFont);
        }
        for (int i = 0; i < _commandIconLabels.Length; i++)
        {
            _commandIconLabels[i]?.AddThemeFontSizeOverride("font_size", Math.Max(20, RoundFont(24f * requestedScale)));
            _commandHotkeyLabels[i]?.AddThemeFontSizeOverride("font_size", Math.Max(10, RoundFont(10f * requestedScale)));
            _commandCostLabels[i]?.AddThemeFontSizeOverride("font_size", Math.Max(10, RoundFont(10f * requestedScale)));
        }
        if (_oreHeading is not null) _oreHeading.Visible = _profile.Content.ShowResourceLabels;
        if (_energyHeading is not null) _energyHeading.Visible = _profile.Content.ShowResourceLabels;
        if (_crystalHeading is not null) _crystalHeading.Visible = _profile.Content.ShowResourceLabels;
        if (_operationsHeading is not null) _operationsHeading.Visible = _profile.Content.ShowResourceLabels;
        if (_mechanicHeading is not null) _mechanicHeading.Visible = _profile.Content.ShowResourceLabels;
        Node? legend = FindChild("MinimapLegend", true, false);
        if (legend is CanvasItem canvas) canvas.Visible = _profile.Content.ShowMinimapLegend;
    }

    private static void ApplyType(IReadOnlyList<Label> labels, Font? font, int size)
    {
        for (int i = 0; i < labels.Count; i++)
        {
            labels[i].AddThemeFontSizeOverride("font_size", size);
            if (font is not null) labels[i].AddThemeFontOverride("font", font);
        }
    }

    private void ApplySurfaces()
    {
        Color background = Parse(_profile.Colors.Background, new Color("111820"));
        Color raised = Parse(_profile.Colors.Raised, new Color("1b2731"));
        Color recessed = Parse(_profile.Colors.Recessed, new Color("0b1016"));
        Color displayAccent = DisplayAccent();
        Color secondaryAccent = SecondaryAccent();
        Color factionSurface = HudFactionChrome.SurfaceForFaction(_profile.ArtSkin.Faction);
        Color deckSectionSurface = HudTextPalette.DeckPlateSurface(_profile);
        Color selectionSurface = HudTextPalette.SelectionPlateSurface(_profile);
        bool legacyFinish = _profile.ArtSkin.Enabled && _profile.ArtSkin.Finish == HudArtFinish.LegacyFrames;
        bool hybridFinish = _profile.ArtSkin.Enabled && _profile.ArtSkin.Finish == HudArtFinish.HybridConsole;
        bool rasterFrameFinish = legacyFinish || hybridFinish;
        bool structuralFinish = _profile.ArtSkin.Enabled &&
            _profile.ArtSkin.Finish is HudArtFinish.StructuralConsole or HudArtFinish.HybridConsole;
        float factionFill = legacyFinish ? 0.16f * _profile.ArtSkin.ChromeIntensity : 0f;
        HudTextColorSet textColors = HudTextPalette.Resolve(_profile);
        Color text = textColors.DeckPrimary;
        Color muted = textColors.DeckMuted;
        for (int i = 0; i < _surfacePanels.Count; i++)
        {
            PanelContainer panel = _surfacePanels[i];
            bool chromePanel = DirectChrome(panel) is not null;
            bool suppressGenericBorder = (rasterFrameFinish || structuralFinish) && chromePanel;
            if (panel == _bottomDeck || panel == _topPanel)
            {
                Color chassisSurface = panel == _bottomDeck
                    ? recessed.Lerp(factionSurface, factionFill * 1.25f)
                    : background.Lerp(factionSurface, factionFill);
                panel.AddThemeStyleboxOverride("panel", rasterFrameFinish
                    ? Style(Colors.Transparent, Colors.Transparent, 0, true)
                    : Style(chassisSurface, suppressGenericBorder ? Colors.Transparent : displayAccent, 0,
                        suppressGenericBorder));
            }
            else if (panel == _minimapPanel || panel == _selectionPanel || panel == _commandPanel)
            {
                Color sectionBackground = panel == _selectionPanel ? selectionSurface : deckSectionSurface;
                StyleBoxFlat section = hybridFinish
                    ? HybridSectionStyle(sectionBackground, panel == _minimapPanel, panel == _commandPanel)
                    : SectionStyle(sectionBackground, new Color(secondaryAccent, 0.46f),
                        panel == _minimapPanel, panel == _commandPanel);
                panel.AddThemeStyleboxOverride("panel", section);
            }
            else
            {
                Color panelBackground = chromePanel
                    ? background.Lerp(factionSurface, factionFill)
                    : background;
                panel.AddThemeStyleboxOverride("panel", Style(panelBackground,
                    suppressGenericBorder ? Colors.Transparent : displayAccent,
                    ChromeContentPadding(panel), suppressGenericBorder));
            }
        }
        for (int i = 0; i < _raisedPanels.Count; i++)
        {
            StyleBoxFlat raisedStyle = Style(raised, displayAccent);
            _raisedPanels[i].AddThemeStyleboxOverride("panel", raisedStyle);
        }
        if (_eventPanel is not null)
        {
            StyleBoxFlat toastStyle = Style(recessed, Colors.Transparent, 7, true);
            toastStyle.BgColor = new Color(recessed, 0.62f);
            _eventPanel.AddThemeStyleboxOverride("panel", toastStyle);
        }
        if (_objectivePanel is not null)
        {
            StyleBoxFlat objectiveStyle = Style(recessed, Colors.Transparent, 7, true);
            objectiveStyle.BgColor = new Color(recessed, 0.52f);
            _objectivePanel.AddThemeStyleboxOverride("panel", objectiveStyle);
        }
        if (_portraitPanel is not null)
            _portraitPanel.AddThemeStyleboxOverride("panel", hybridFinish
                ? HybridSectionStyle(deckSectionSurface, false, false)
                : Style(deckSectionSurface, secondaryAccent));
        _portraitView?.ApplyProfile(_profile);
        HudArtFinish surfaceFinish = _profile.ArtSkin.Enabled
            ? _profile.ArtSkin.Finish
            : HudArtFinish.Clean;
        for (int i = 0; i < _factionSurfaceMasks.Count; i++)
        {
            HudFactionSurfaceMask mask = _factionSurfaceMasks[i];
            Color surface = mask.Role == HudFactionChromeRole.TopStrip
                ? background.Lerp(factionSurface, factionFill)
                : recessed.Lerp(factionSurface, factionFill * 1.25f);
            mask.Configure(_profile.ArtSkin.Faction, mask.Role, surfaceFinish,
                _profile.ArtSkin.ChromeScale, surface, _profile.Surface.PanelOpacity,
                Mathf.RoundToInt(_profile.Surface.InnerPadding * ResponsiveScale()));
        }
        for (int i = 0; i < _factionChrome.Count; i++)
        {
            HudFactionChrome chrome = _factionChrome[i];
            chrome.Configure(_profile.ArtSkin.Faction, chrome.Role,
                rasterFrameFinish ? _profile.ArtSkin.ChromeIntensity : 0f,
                _profile.ArtSkin.ChromeScale, 0f, hybridFinish);
        }
        for (int i = 0; i < _structuralChrome.Count; i++)
        {
            HudStructuralChrome chrome = _structuralChrome[i];
            chrome.Configure(_profile.ArtSkin.Faction, chrome.Role,
                structuralFinish ? _profile.ArtSkin.ChromeIntensity : 0f,
                _profile.ArtSkin.ChromeScale, _profile.Surface.InnerPadding,
                _profile.ArtSkin.Finish, background, recessed, displayAccent, secondaryAccent);
        }
        for (int i = 0; i < _rasterSurfaceOverlays.Count; i++)
        {
            HudRasterSurfaceOverlay overlay = _rasterSurfaceOverlays[i];
            overlay.Configure(overlay.Role, _profile.ArtSkin.Finish,
                _profile.ArtSkin.Enabled ? _profile.ArtSkin.ChromeIntensity : 0f,
                recessed, displayAccent);
        }
        for (int i = 0; i < _titleLabels.Count; i++)
            _titleLabels[i].AddThemeColorOverride("font_color", textColors.DeckPrimary);
        for (int i = 0; i < _sectionLabels.Count; i++)
            _sectionLabels[i].AddThemeColorOverride("font_color", textColors.DeckAccent);
        for (int i = 0; i < _bodyLabels.Count; i++)
            _bodyLabels[i].AddThemeColorOverride("font_color", textColors.DeckPrimary);
        for (int i = 0; i < _valueLabels.Count; i++)
            _valueLabels[i].AddThemeColorOverride("font_color", textColors.DeckPrimary);
        for (int i = 0; i < _metaLabels.Count; i++)
            _metaLabels[i].AddThemeColorOverride("font_color", textColors.DeckMuted);
        for (int i = 0; i < _iconLabels.Count; i++)
            _iconLabels[i].AddThemeColorOverride("font_color", textColors.DeckPrimary);

        SetLabelColor(_selectionTitle, textColors.SelectionPrimary);
        SetLabelColor(_selectionStatus, textColors.SelectionPrimary);
        SetLabelColor(_selectionHealthText, textColors.SelectionMuted);
        SetLabelColor(_selectionSubtitle, textColors.SelectionMuted);
        SetLabelColor(_priorityHeading, textColors.SelectionAccent);

        for (int i = 0; i < _topStripLabels.Count; i++)
            _topStripLabels[i].AddThemeColorOverride("font_color", textColors.TopPrimary);
        for (int i = 0; i < _topStripIconLabels.Count; i++)
            _topStripIconLabels[i].AddThemeColorOverride("font_color", textColors.TopAccent);
        SetLabelColor(_oreHeading, textColors.TopMuted);
        SetLabelColor(_energyHeading, textColors.TopMuted);
        SetLabelColor(_crystalHeading, textColors.TopMuted);
        SetLabelColor(_operationsHeading, textColors.TopMuted);
        SetLabelColor(_mechanicHeading, textColors.TopMuted);
        SetLabelColor(_matchState, textColors.TopMuted);

        for (int i = 0; i < _raisedSurfaceLabels.Count; i++)
            _raisedSurfaceLabels[i].AddThemeColorOverride("font_color", textColors.RaisedPrimary);
        SetLabelColor(_tooltipTitle, textColors.RaisedPrimary);
        SetLabelColor(_energyPopoverTitle, textColors.RaisedPrimary);
        for (int i = 0; i < _commandHotkeyLabels.Length; i++)
        {
            _commandIconLabels[i]?.AddThemeColorOverride("font_color", textColors.CommandPrimary);
            _commandHotkeyLabels[i]?.AddThemeColorOverride("font_color", textColors.CommandAccent);
            _commandCostLabels[i]?.AddThemeColorOverride("font_color", HudTextPalette.EnsureReadable(
                Parse(_profile.Colors.Warning, Colors.Orange), HudTextPalette.CommandSurface(_profile),
                textColors.CommandPrimary));
        }
        for (int i = 0; i < _topDividers.Count; i++)
            _topDividers[i].Color = new Color(secondaryAccent, 0.32f);
        StyleBoxFlat normalButton = Style(raised, muted);
        StyleBoxFlat hoverButton = Style(raised.Lightened(0.10f), displayAccent);
        StyleBoxFlat pressedButton = Style(recessed, displayAccent);
        for (int i = 0; i < _allButtons.Count; i++)
        {
            Button button = _allButtons[i];
            bool commandButton = button.Name.ToString().StartsWith("Command", StringComparison.Ordinal);
            bool groupButton = button.Name.ToString().StartsWith("SelectionGroup", StringComparison.Ordinal);
            bool flatUtility = button.Name == "EnergyButton" || button.Name == "ActionableAlert";
            button.AddThemeColorOverride("font_color", textColors.RaisedPrimary);
            button.AddThemeColorOverride("font_hover_color", textColors.RaisedPrimary);
            button.AddThemeColorOverride("font_pressed_color", textColors.DeckAccent);
            button.AddThemeColorOverride("font_disabled_color", new Color(textColors.RaisedMuted, 0.52f));
            if (commandButton)
            {
                Color commandSurface = HudTextPalette.CommandSurface(_profile);
                Color commandNormal = _profile.Surface.SolidCommandButtons ? commandSurface : Colors.Transparent;
                Color commandHover = HudTextPalette.CommandHoverSurface(_profile);
                Color commandPressed = HudTextPalette.CommandPressedSurface(_profile);
                Color commandDisabled = _profile.Surface.SolidCommandButtons
                    ? new Color(commandSurface, 0.62f)
                    : Colors.Transparent;
                button.AddThemeStyleboxOverride("normal", ButtonStyle(commandNormal, new Color(secondaryAccent, 0.58f)));
                button.AddThemeStyleboxOverride("hover", ButtonStyle(commandHover, displayAccent));
                button.AddThemeStyleboxOverride("pressed", ButtonStyle(commandPressed, displayAccent));
                button.AddThemeStyleboxOverride("focus", ButtonStyle(commandPressed, displayAccent));
                button.AddThemeStyleboxOverride("disabled", ButtonStyle(commandDisabled,
                    new Color(textColors.CommandMuted, 0.28f)));
            }
            else if (groupButton)
            {
                button.AddThemeStyleboxOverride("normal", ButtonStyle(raised.Darkened(0.06f), new Color(secondaryAccent, 0.42f)));
                button.AddThemeStyleboxOverride("hover", ButtonStyle(raised.Lightened(0.08f), secondaryAccent));
                button.AddThemeStyleboxOverride("pressed", ButtonStyle(recessed, secondaryAccent));
                button.AddThemeStyleboxOverride("focus", ButtonStyle(recessed, secondaryAccent));
            }
            else if (flatUtility)
            {
                StyleBoxFlat transparent = new() { BgColor = Colors.Transparent };
                button.AddThemeStyleboxOverride("normal", transparent);
                button.AddThemeStyleboxOverride("hover", ButtonStyle(new Color(raised, 0.54f), new Color(displayAccent, 0.55f)));
                button.AddThemeStyleboxOverride("pressed", ButtonStyle(new Color(recessed, 0.72f), displayAccent));
                button.AddThemeStyleboxOverride("focus", transparent);
                button.AddThemeStyleboxOverride("disabled", transparent);
            }
            else
            {
                button.AddThemeStyleboxOverride("normal", normalButton);
                button.AddThemeStyleboxOverride("hover", hoverButton);
                button.AddThemeStyleboxOverride("pressed", pressedButton);
                button.AddThemeStyleboxOverride("focus", pressedButton);
            }
        }
        if (_selectionHealth is not null)
        {
            _selectionHealth.AddThemeStyleboxOverride("background", Style(recessed, recessed, 0, true));
            _selectionHealth.AddThemeStyleboxOverride("fill", Style(Parse(_profile.Colors.Good, Colors.Green),
                Parse(_profile.Colors.Good, Colors.Green), 0, true));
        }
        for (int i = 0; i < _groupHealth.Length; i++)
        {
            _groupHealth[i].AddThemeStyleboxOverride("background", Style(recessed, recessed, 0, true));
            _groupHealth[i].AddThemeStyleboxOverride("fill", Style(Parse(_profile.Colors.Good, Colors.Green),
                Parse(_profile.Colors.Good, Colors.Green), 0, true));
        }
        for (int i = 0; i < _queueProgress.Length; i++)
        {
            _queueProgress[i].AddThemeStyleboxOverride("background", Style(recessed, recessed, 0, true));
            _queueProgress[i].AddThemeStyleboxOverride("fill", Style(displayAccent, displayAccent, 0, true));
        }
        ApplyContainerSpacing();
    }

    private void ApplyContainerSpacing()
    {
        float scale = ResponsiveScale();
        int dense = Math.Max(3, Mathf.RoundToInt(4f * scale));
        int content = Math.Max(4, Mathf.RoundToInt(6f * scale));
        int section = Math.Max(6, Mathf.RoundToInt(9f * scale));
        SetBoxSpacing("ResourceRow", content);
        SetBoxSpacing("SelectionInformation", content);
        SetBoxSpacing("SelectionHeader", dense);
        SetBoxSpacing("ContextualEnergyPriority", dense);
        SetBoxSpacing("TacticalPortraitStack", content);
        SetBoxSpacing("CommandStack", content);
        SetBoxSpacing("ProductionQueue", dense);
        if (_groupList is not null)
        {
            _groupList.AddThemeConstantOverride("h_separation", section);
            _groupList.AddThemeConstantOverride("v_separation", content);
        }
        if (_commandGrid is not null)
        {
            _commandGrid.AddThemeConstantOverride("h_separation", content);
            _commandGrid.AddThemeConstantOverride("v_separation", content);
        }
    }

    private void SetBoxSpacing(string nodeName, int separation)
    {
        if (FindChild(nodeName, true, false) is BoxContainer box)
            box.AddThemeConstantOverride("separation", separation);
    }

    private void ApplySelectionGroups(IReadOnlyList<HudSelectionGroupFrame> groups)
    {
        for (int i = 0; i < GroupCapacity; i++)
        {
            bool visible = i < groups.Count;
            _groupButtons[i].GetParent<CanvasItem>().Visible = visible;
            if (!visible) continue;
            HudSelectionGroupFrame group = groups[i];
            _groupButtons[i].Text = $"{group.Name.ToUpperInvariant()}\n{group.Count} UNITS";
            _groupButtons[i].TooltipText = group.StateSummary;
            _groupHealth[i].Value = Math.Clamp(group.AverageHealthPercent, 0, 100);
            ApplyHealthFill(_groupHealth[i], group.AverageHealthPercent);
        }
        if (_groupList is not null) _groupList.Visible = groups.Count > 0;
    }

    private void ApplyCommands(IReadOnlyList<HudCommandFrame> commands)
    {
        for (int i = 0; i < CommandCapacity; i++)
        {
            Button button = _commandButtons[i];
            bool exists = i < commands.Count && commands[i].Visible;
            button.Visible = exists;
            if (!exists)
            {
                button.Disabled = true;
                button.ButtonPressed = false;
                button.TooltipText = string.Empty;
                button.Modulate = new Color(1f, 1f, 1f, 0.34f);
                _commandIconLabels[i].Text = string.Empty;
                _commandHotkeyLabels[i].Text = string.Empty;
                _commandCostLabels[i].Text = string.Empty;
                continue;
            }
            HudCommandFrame command = commands[i];
            button.Modulate = Colors.White;
            button.Text = string.Empty;
            _commandIconLabels[i].Text = CommandGlyph(command.Id);
            _commandHotkeyLabels[i].Text = _profile.Content.ShowHotkeys && command.Hotkey.Length > 0
                ? command.Hotkey.ToUpperInvariant()
                : string.Empty;
            _commandCostLabels[i].Text = _profile.Content.ShowCommandCosts ? command.Cost : string.Empty;
            button.TooltipText = command.Cost.Length > 0
                ? $"{command.Name} · {command.Cost}\n{command.Tooltip}"
                : $"{command.Name}\n{command.Tooltip}";
            button.Disabled = !command.Enabled;
            button.ButtonPressed = command.Active;
            float overlayAlpha = command.Enabled ? 1f : 0.52f;
            Color overlayModulate = new(1f, 1f, 1f, overlayAlpha);
            _commandIconLabels[i].Modulate = overlayModulate;
            _commandHotkeyLabels[i].Modulate = overlayModulate;
            _commandCostLabels[i].Modulate = overlayModulate;
        }
    }

    private static string CommandGlyph(string id) => id.Trim().ToLowerInvariant() switch
    {
        "move" => "➜", "attack" => "◎", "stop" => "■", "hold" => "⌂",
        "patrol" => "↻", "spread" => "↔", "repair" => "✚", "load" => "⇥",
        "unload" => "⇤", "special" => "◆", "state" => "⇄", "detail" => "≡",
        "crew" => "●", "scout" => "◇", "rider" => "▶", "dozer" => "▰",
        _ => "◆"
    };

    private void ApplyQueue(IReadOnlyList<HudQueueFrame> queue)
    {
        for (int i = 0; i < QueueCapacity; i++)
        {
            bool visible = i < queue.Count;
            _queueRows[i].Visible = visible;
            if (!visible) continue;
            HudQueueFrame item = queue[i];
            string count = item.Count > 1 ? $" ×{item.Count}" : string.Empty;
            _queueLabels[i].Text = $"{item.Name.ToUpperInvariant()}{count}{(item.Paused ? " · PAUSED" : string.Empty)}";
            _queueRows[i].TooltipText = $"Queue {i + 1}: {item.Name}{count}{(item.Paused ? " · paused" : string.Empty)}";
            _queueProgress[i].Value = item.ProgressPercent;
        }
        bool hasQueue = queue.Count > 0;
        if (_queueList is not null) _queueList.Visible = hasQueue;
        if (_queueHeading is not null) _queueHeading.Visible = false;
        if (_queueScroll is not null) _queueScroll.Visible = hasQueue;
    }

    private void ApplyEvents(IReadOnlyList<HudEventFrame> events)
    {
        for (int i = 0; i < EventCapacity; i++)
        {
            bool visible = i < events.Count;
            _eventLabels[i].Visible = visible;
            if (!visible) continue;
            HudEventFrame item = events[i];
            _eventLabels[i].Text = $"{item.Time}  {item.Text}";
            HudTextColorSet textColors = HudTextPalette.Resolve(_profile);
            Color recessed = Parse(_profile.Colors.Recessed, Colors.Black);
            _eventLabels[i].AddThemeColorOverride("font_color", HudTextPalette.EnsureReadable(
                PriorityColor(item.Priority), recessed, textColors.DeckPrimary));
        }
    }

    private void ApplyPriorityColor(Label? label, HudAlertPriority priority)
    {
        if (label is null) return;
        HudTextColorSet textColors = HudTextPalette.Resolve(_profile);
        Color background = Parse(_profile.Colors.Background, Colors.Black);
        Color color = priority == HudAlertPriority.None
            ? textColors.TopPrimary
            : HudTextPalette.EnsureReadable(PriorityColor(priority), background, textColors.TopPrimary);
        label.AddThemeColorOverride("font_color", color);
    }

    private void ApplyHealthFill(ProgressBar bar, int healthPercent)
    {
        Color good = Parse(_profile.Colors.Good, Colors.Green);
        Color fill = good;
        if (_profile.Surface.HealthStateColors)
        {
            Color warning = Parse(_profile.Colors.Warning, Colors.Orange);
            Color danger = Parse(_profile.Colors.Danger, Colors.Red);
            float normalized = Math.Clamp(healthPercent, 0, 100) / 100f;
            fill = normalized < 0.5f
                ? danger.Lerp(warning, normalized * 2f)
                : warning.Lerp(good, (normalized - 0.5f) * 2f);
        }
        bar.AddThemeStyleboxOverride("fill", Style(fill, fill, 0, true));
    }

    private void ApplyPriorityPanel(PanelContainer? panel, HudAlertPriority priority)
    {
        if (panel is null || priority == HudAlertPriority.None) return;
        Color color = PriorityColor(priority);
        panel.AddThemeStyleboxOverride("panel", Style(Parse(_profile.Colors.Background, Colors.Black), color));
    }

    private Color PriorityColor(HudAlertPriority priority) => priority switch
    {
        HudAlertPriority.Critical => Parse(_profile.Colors.Danger, Colors.Red),
        HudAlertPriority.High => Parse(_profile.Colors.Warning, Colors.Orange),
        HudAlertPriority.Normal => DisplayAccent(),
        HudAlertPriority.Informational => Parse(_profile.Colors.Selection, Colors.Cyan),
        _ => HudTextPalette.Resolve(_profile).DeckMuted
    };

    private Color DisplayAccent() => Parse(_profile.Colors.Accent,
        HudFactionChrome.AccentForFaction(_profile.ArtSkin.Faction));

    private Color SecondaryAccent() => _profile.ArtSkin.SurfacePalette == HudSurfacePalette.FactionBound
        ? HudFactionChrome.SecondaryAccentForFaction(_profile.ArtSkin.Faction)
        : Parse(_profile.Colors.Selection, new Color("166e7a"));

    private PanelContainer SurfacePanel(string name, bool raised)
    {
        PanelContainer panel = new() { Name = name, MouseFilter = MouseFilterEnum.Stop, ClipContents = true };
        (raised ? _raisedPanels : _surfacePanels).Add(panel);
        HudFactionChromeRole? chromeRole = name switch
        {
            "ResourceStrip" => HudFactionChromeRole.TopStrip,
            "BottomDeck" => HudFactionChromeRole.BottomDeck,
            _ => null
        };
        if (chromeRole.HasValue)
        {
            HudFactionSurfaceMask surfaceMask = new()
            {
                Name = $"{name}FactionSurfaceMask",
                MouseFilter = MouseFilterEnum.Ignore,
                CustomMinimumSize = Vector2.Zero,
                ZIndex = 0
            };
            surfaceMask.Configure(_profile.ArtSkin.Faction, chromeRole.Value, _profile.ArtSkin.Finish,
                _profile.ArtSkin.ChromeScale,
                chromeRole.Value == HudFactionChromeRole.TopStrip
                    ? Parse(_profile.Colors.Background, new Color("303534"))
                    : Parse(_profile.Colors.Recessed, new Color("171b1a")),
                _profile.Surface.PanelOpacity, _profile.Surface.InnerPadding);
            panel.AddChild(surfaceMask);
            _factionSurfaceMasks.Add(surfaceMask);

            if (name == "BottomDeck")
            {
                HudRasterSurfaceOverlay overlay = new()
                {
                    Name = $"{name}RasterSurface",
                    MouseFilter = MouseFilterEnum.Ignore,
                    CustomMinimumSize = Vector2.Zero
                };
                overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
                overlay.Configure(HudRasterSurfaceRole.BottomDeck, _profile.ArtSkin.Finish, 0f,
                    Parse(_profile.Colors.Recessed, new Color("8b8578")), DisplayAccent());
                surfaceMask.AddChild(overlay);
                _rasterSurfaceOverlays.Add(overlay);
            }

            HudStructuralChrome structural = new()
            {
                Name = $"{name}StructuralChrome",
                MouseFilter = MouseFilterEnum.Ignore,
                CustomMinimumSize = Vector2.Zero,
                ZIndex = 10
            };
            structural.Configure(_profile.ArtSkin.Faction, chromeRole.Value,
                0f, _profile.ArtSkin.ChromeScale, _profile.Surface.InnerPadding,
                _profile.ArtSkin.Finish,
                Parse(_profile.Colors.Background, new Color("cfc6ae")),
                Parse(_profile.Colors.Recessed, new Color("8b8578")),
                DisplayAccent(), SecondaryAccent());
            panel.AddChild(structural);
            _structuralChrome.Add(structural);
            HudFactionChrome chrome = new()
            {
                Name = $"{name}FactionChrome",
                MouseFilter = MouseFilterEnum.Ignore,
                CustomMinimumSize = Vector2.Zero,
                ZIndex = 20
            };
            chrome.Configure(_profile.ArtSkin.Faction, chromeRole.Value,
                _profile.ArtSkin.ChromeIntensity, _profile.ArtSkin.ChromeScale, _profile.Surface.InnerPadding);
            panel.AddChild(chrome);
            _factionChrome.Add(chrome);
        }
        return panel;
    }

    private void AddResourceChip(Container parent, string glyph, string title, out Label heading, out Label value)
    {
        HBoxContainer box = ResourceChip(glyph, title, out heading, out value);
        parent.AddChild(box);
    }

    private void AddTopDivider(Container parent)
    {
        ColorRect divider = new()
        {
            Name = $"ResourceDivider{_topDividers.Count}",
            CustomMinimumSize = new Vector2(1f, 0f),
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _topDividers.Add(divider);
        parent.AddChild(divider);
    }

    private HBoxContainer ResourceChip(string glyph, string title, out Label heading, out Label value)
    {
        HBoxContainer box = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 1f,
            Alignment = BoxContainer.AlignmentMode.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        Label icon = Label(glyph, TextRole.Icon);
        icon.HorizontalAlignment = HorizontalAlignment.Center;
        icon.VerticalAlignment = VerticalAlignment.Center;
        icon.MouseFilter = MouseFilterEnum.Ignore;
        box.AddChild(icon);
        heading = Label(title, TextRole.Section);
        heading.HorizontalAlignment = HorizontalAlignment.Left;
        heading.VerticalAlignment = VerticalAlignment.Center;
        heading.MouseFilter = MouseFilterEnum.Ignore;
        box.AddChild(heading);
        value = Label("—", TextRole.Value);
        value.HorizontalAlignment = HorizontalAlignment.Right;
        value.VerticalAlignment = VerticalAlignment.Center;
        value.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        value.MouseFilter = MouseFilterEnum.Ignore;
        box.AddChild(value);
        _topStripLabels.Add(icon);
        _topStripIconLabels.Add(icon);
        _topStripLabels.Add(heading);
        _topStripLabels.Add(value);
        return box;
    }

    private Label CommandOverlayLabel(string text, TextRole role)
    {
        Label label = Label(text, role);
        label.MouseFilter = MouseFilterEnum.Ignore;
        return label;
    }

    private Label Label(string text, TextRole role)
    {
        Label label = new() { Text = text };
        switch (role)
        {
            case TextRole.Title: _titleLabels.Add(label); break;
            case TextRole.Section: _sectionLabels.Add(label); break;
            case TextRole.Body: _bodyLabels.Add(label); break;
            case TextRole.Value: _valueLabels.Add(label); break;
            case TextRole.Icon: _iconLabels.Add(label); break;
            default: _metaLabels.Add(label); break;
        }
        return label;
    }

    private Button Button(string text, string name)
    {
        Button button = new() { Name = name, Text = text };
        _allButtons.Add(button);
        return button;
    }

    private int? ChromeContentPadding(PanelContainer panel)
    {
        if (!_profile.ArtSkin.Enabled ||
            _profile.ArtSkin.Finish is not (HudArtFinish.HybridConsole or HudArtFinish.LegacyFrames) ||
            DirectChrome(panel) is null) return null;
        if (panel.Name == "BottomDeck") return 0;
        float basePadding = 10f;
        return Math.Max(_profile.Surface.InnerPadding,
            Mathf.RoundToInt(basePadding * _profile.ArtSkin.ChromeScale));
    }

    private static HudFactionChrome? DirectChrome(PanelContainer panel) =>
        panel.FindChild($"{panel.Name}FactionChrome", false, false) as HudFactionChrome;

    private static HudFactionSurfaceMask? DirectSurfaceMask(PanelContainer panel) =>
        panel.FindChild($"{panel.Name}FactionSurfaceMask", false, false) as HudFactionSurfaceMask;

    private static HudStructuralChrome? DirectStructuralChrome(PanelContainer panel) =>
        panel.FindChild($"{panel.Name}StructuralChrome", false, false) as HudStructuralChrome;

    private StyleBoxFlat Style(Color background, Color border, int? paddingOverride = null, bool suppressBorder = false)
    {
        float scale = ResponsiveScale();
        int radius = Mathf.RoundToInt(_profile.Surface.CornerRadius * scale);
        int borderWidth = suppressBorder ? 0 : Math.Max(1, Mathf.RoundToInt(_profile.Surface.BorderWidth * scale));
        int padding = Mathf.RoundToInt((paddingOverride ?? _profile.Surface.InnerPadding) * scale);
        return new StyleBoxFlat
        {
            BgColor = new Color(background, _profile.Surface.PanelOpacity), BorderColor = border,
            BorderWidthLeft = borderWidth, BorderWidthTop = borderWidth, BorderWidthRight = borderWidth, BorderWidthBottom = borderWidth,
            CornerRadiusTopLeft = radius, CornerRadiusTopRight = radius, CornerRadiusBottomLeft = radius, CornerRadiusBottomRight = radius,
            ContentMarginLeft = padding, ContentMarginRight = padding, ContentMarginTop = padding, ContentMarginBottom = padding
        };
    }

    private StyleBoxFlat SectionStyle(Color background, Color divider, bool outerLeft, bool outerRight)
    {
        float scale = ResponsiveScale();
        int padding = Mathf.RoundToInt(_profile.Surface.InnerPadding * scale);
        int outerMargin = Mathf.RoundToInt(20f * scale);
        int innerMargin = Mathf.RoundToInt(10f * scale);
        int topMargin = Mathf.RoundToInt(6f * scale);
        int bottomMargin = Mathf.RoundToInt(6f * scale);
        return new StyleBoxFlat
        {
            BgColor = new Color(background, _profile.Surface.PanelOpacity),
            BorderColor = divider,
            BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2, CornerRadiusBottomLeft = 2, CornerRadiusBottomRight = 2,
            ContentMarginLeft = Math.Max(outerLeft ? outerMargin : innerMargin, padding),
            ContentMarginRight = Math.Max(outerRight ? outerMargin : innerMargin, padding),
            ContentMarginTop = Math.Max(topMargin, padding),
            ContentMarginBottom = Math.Max(bottomMargin, padding)
        };
    }

    private StyleBoxFlat HybridSectionStyle(Color background, bool outerLeft, bool outerRight)
    {
        float scale = ResponsiveScale();
        int padding = Mathf.RoundToInt(_profile.Surface.InnerPadding * scale);
        int outerMargin = Mathf.RoundToInt(20f * scale);
        int innerMargin = Mathf.RoundToInt(8f * scale);
        int verticalMargin = Mathf.RoundToInt(6f * scale);
        // The minimap is the lower deck's outer-left visual field. Let its
        // raster reach beneath the authored faction frame so the frame's own
        // alpha aperture shapes the corner. A rectangular inset here exposes
        // a second, square visual system inside the otherwise sculpted shell.
        int minimapEdgeMargin = outerLeft ? 0 : Math.Max(innerMargin, padding);
        int minimapVerticalMargin = outerLeft ? 0 : Math.Max(verticalMargin, padding);
        return new StyleBoxFlat
        {
            BgColor = new Color(background, 0.28f * _profile.Surface.PanelOpacity),
            BorderWidthLeft = 0, BorderWidthTop = 0, BorderWidthRight = 0, BorderWidthBottom = 0,
            ContentMarginLeft = minimapEdgeMargin,
            ContentMarginRight = Math.Max(outerRight ? outerMargin : innerMargin, padding),
            ContentMarginTop = minimapVerticalMargin,
            ContentMarginBottom = minimapVerticalMargin
        };
    }

    private static StyleBoxFlat ButtonStyle(Color background, Color border)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3, CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3,
            ContentMarginLeft = 6, ContentMarginRight = 6, ContentMarginTop = 3, ContentMarginBottom = 3
        };
    }

    private static IEnumerable<Node> GetChildrenRecursive(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            yield return child;
            foreach (Node descendant in GetChildrenRecursive(child)) yield return descendant;
        }
    }

    private static void SetRect(Control control, Vector2 position, Vector2 size)
    {
        control.Position = position;
        control.Size = size;
    }

    private static void SetText(Label? label, string text)
    {
        if (label is not null && label.Text != text) label.Text = text;
    }

    private static void SetLabelColor(Label? label, Color color) =>
        label?.AddThemeColorOverride("font_color", color);

    private static int RoundFont(float size) => Math.Max(8, Mathf.RoundToInt(size));

    private float ResponsiveScale()
    {
        float height = Size.Y > 1f ? Size.Y : 1080f;
        return _profile.Layout.UiScale * Mathf.Clamp(height / 1080f, 2f / 3f, 2f);
    }

    private float ResponsiveTextScale()
    {
        float height = Size.Y > 1f ? Size.Y : 1080f;
        return _profile.Layout.UiScale * Mathf.Clamp(height / 1080f, 0.90f, 2f);
    }

    private static Color Parse(string html, Color fallback) => Color.HtmlIsValid(html) ? new Color(html) : fallback;

    private enum TextRole { Title, Section, Body, Value, Meta, Icon }
}
