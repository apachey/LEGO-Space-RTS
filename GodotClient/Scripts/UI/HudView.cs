using Godot;

namespace LegoSpaceRTS.UI;

public partial class HudView : Control
{
    private const int CommandCapacity = 12;
    private const int GroupCapacity = 8;
    private const int QueueCapacity = 8;
    private const int EventCapacity = 4;

    private readonly List<Label> _headingLabels = new();
    private readonly List<Label> _bodyLabels = new();
    private readonly List<Label> _microLabels = new();
    private readonly List<PanelContainer> _surfacePanels = new();
    private readonly List<PanelContainer> _raisedPanels = new();
    private readonly List<HudFactionChrome> _factionChrome = new();
    private readonly List<Button> _allButtons = new();
    private readonly List<ColorRect> _topDividers = new();
    private readonly Button[] _commandButtons = new Button[CommandCapacity];
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
    private PanelContainer? _bottomDeck;
    private Control? _bottomDeckContent;
    private HudFactionChrome? _bottomDeckChrome;
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
    private VBoxContainer? _groupList;
    private ScrollContainer? _groupScroll;
    private HBoxContainer? _priorityRow;
    private Label? _commandTitle;
    private GridContainer? _commandGrid;
    private Label? _queueHeading;
    private ScrollContainer? _queueScroll;
    private VBoxContainer? _queueList;
    private Button? _alertButton;
    private Label? _objectiveLabel;
    private Label? _tooltipTitle;
    private Label? _tooltipBody;
    private Label? _energyPopoverLabel;
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

        if (_selectionHealth is not null)
        {
            _selectionHealth.Visible = frame.Selection.HealthPercent >= 0;
            _selectionHealth.Value = Math.Clamp(frame.Selection.HealthPercent, 0, 100);
            ApplyHealthFill(_selectionHealth, frame.Selection.HealthPercent);
        }
        if (_selectionHealthText is not null) _selectionHealthText.Visible = frame.Selection.HealthPercent >= 0;
        if (_priorityRow is not null) _priorityRow.Visible = frame.Selection.ShowPowerPriority;
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
        BuildCommandRegion();
        BuildTransientRegions();
    }

    private void BuildBottomDeck()
    {
        _bottomDeck = SurfacePanel("BottomDeck", false);
        _safeArea!.AddChild(_bottomDeck);
        _bottomDeckContent = new Control { Name = "BottomDeckContent", MouseFilter = MouseFilterEnum.Ignore };
        _bottomDeck.AddChild(_bottomDeckContent);
        _bottomDeckChrome = DirectChrome(_bottomDeck);
        _bottomDeckChrome?.MoveToFront();
    }

    private void BuildTopStrip()
    {
        _topPanel = SurfacePanel("ResourceStrip", false);
        _safeArea!.AddChild(_topPanel);
        HBoxContainer row = new() { Name = "ResourceRow", Alignment = BoxContainer.AlignmentMode.Center };
        _topPanel.AddChild(row);
        AddResourceBlock(row, "◆  ORE", out _oreHeading, out _oreValue);
        AddTopDivider(row);
        Button energyButton = Button("", "EnergyButton");
        energyButton.Flat = true;
        energyButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        energyButton.SizeFlagsStretchRatio = 1.35f;
        VBoxContainer energy = ResourceBlock("ϟ  ENERGY", out _energyHeading, out _energyValue);
        energyButton.AddChild(energy);
        energyButton.Pressed += () => EnergyDetailsRequested?.Invoke();
        row.AddChild(energyButton);
        AddTopDivider(row);
        AddResourceBlock(row, "◇  CRYSTALS", out _crystalHeading, out _crystalValue);
        AddTopDivider(row);
        AddResourceBlock(row, "▦  OPERATIONS", out _operationsHeading, out _operationsValue);
        AddTopDivider(row);
        VBoxContainer mechanic = ResourceBlock("SYSTEM", out _mechanicHeading, out _mechanicValue);
        mechanic.SizeFlagsStretchRatio = 1.55f;
        row.AddChild(mechanic);
        AddTopDivider(row);
        _matchState = Label("MATCH", TextRole.Micro);
        _matchState.HorizontalAlignment = HorizontalAlignment.Center;
        _matchState.VerticalAlignment = VerticalAlignment.Center;
        _matchState.CustomMinimumSize = new Vector2(150, 0);
        row.AddChild(_matchState);
    }

    private void BuildMinimapRegion()
    {
        _minimapPanel = SurfacePanel("MinimapRegion", false);
        _bottomDeckContent!.AddChild(_minimapPanel);
        VBoxContainer box = new();
        _minimapPanel.AddChild(box);
        HBoxContainer header = new(); box.AddChild(header);
        Label title = Label("TACTICAL MAP", TextRole.Heading); title.SizeFlagsHorizontal = SizeFlags.ExpandFill; header.AddChild(title);
        Label north = Label("N ↑", TextRole.Micro); header.AddChild(north);
        _minimap = new HudMinimapView { Name = "MinimapSlot", SizeFlagsVertical = SizeFlags.ExpandFill };
        _minimap.Configure(_profile);
        _minimap.CameraRequested += cell => MinimapCameraRequested?.Invoke(cell);
        _minimap.GroundCommandRequested += (cell, queued) => MinimapGroundCommandRequested?.Invoke(cell, queued);
        _minimap.TooltipText = "North-up tactical map. Left-click or drag: camera. Right-click: move/rally. Shift + right-click: queue.";
        box.AddChild(_minimap);
        Label legend = Label("● MOBILE   ■ STRUCTURE   ▲ AIR", TextRole.Micro);
        legend.Name = "MinimapLegend"; legend.HorizontalAlignment = HorizontalAlignment.Center; box.AddChild(legend);
    }

    private void BuildSelectionRegion()
    {
        _selectionPanel = SurfacePanel("SelectionPanel", false);
        _bottomDeckContent!.AddChild(_selectionPanel);
        HBoxContainer row = new(); _selectionPanel.AddChild(row);
        _portraitPanel = SurfacePanel("PortraitSlot", true);
        _portraitPanel.CustomMinimumSize = new Vector2(144, 0);
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
        _portraitLabel = Label("NO SELECTION", TextRole.Micro);
        _portraitLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _portraitLabel.VerticalAlignment = VerticalAlignment.Center;
        _portraitLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _portraitLabel.ClipText = true;
        _portraitLabel.CustomMinimumSize = new Vector2(0, 18);
        portraitStack.AddChild(_portraitLabel);
        row.AddChild(_portraitPanel);

        VBoxContainer information = new() { Name = "SelectionInformation", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        row.AddChild(information);
        _selectionTitle = Label("NO SELECTION", TextRole.Heading); _selectionTitle.Name = "SelectionTitle"; information.AddChild(_selectionTitle);
        _selectionSubtitle = Label("Select an entity or issue a command.", TextRole.Micro);
        _selectionSubtitle.AutowrapMode = TextServer.AutowrapMode.WordSmart; information.AddChild(_selectionSubtitle);
        _selectionHealth = new ProgressBar { Name = "SelectionHealth", MinValue = 0, MaxValue = 100, ShowPercentage = false, CustomMinimumSize = new Vector2(0, 10) };
        information.AddChild(_selectionHealth);
        _selectionHealthText = Label(string.Empty, TextRole.Micro); information.AddChild(_selectionHealthText);
        _selectionStatus = Label(string.Empty, TextRole.Body);
        _selectionStatus.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _selectionStatus.SizeFlagsVertical = SizeFlags.ExpandFill;
        information.AddChild(_selectionStatus);
        _priorityRow = new HBoxContainer { Name = "ContextualEnergyPriority" };
        _priorityRow.AddChild(Label("POWER", TextRole.Micro));
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

        _groupScroll = new ScrollContainer { Name = "SelectionGroupScroll", CustomMinimumSize = new Vector2(216, 0), SizeFlagsVertical = SizeFlags.ExpandFill, HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        row.AddChild(_groupScroll);
        _groupList = new VBoxContainer { Name = "SelectionTypeGroups", CustomMinimumSize = new Vector2(200, 0), SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _groupScroll.AddChild(_groupList);
        _groupList.AddChild(Label("COMPOSITION", TextRole.Micro));
        for (int i = 0; i < GroupCapacity; i++)
        {
            int index = i;
            VBoxContainer group = new();
            _groupButtons[i] = Button("GROUP", $"SelectionGroup{i}");
            _groupButtons[i].Alignment = HorizontalAlignment.Left;
            _groupButtons[i].Pressed += () =>
            {
                if (index < _frame.Selection.Groups.Count) SelectionGroupRequested?.Invoke(_frame.Selection.Groups[index].Id);
            };
            group.AddChild(_groupButtons[i]);
            _groupHealth[i] = new ProgressBar { MinValue = 0, MaxValue = 100, ShowPercentage = false, CustomMinimumSize = new Vector2(0, 4) };
            group.AddChild(_groupHealth[i]);
            _groupList.AddChild(group);
        }
    }

    private void BuildCommandRegion()
    {
        _commandPanel = SurfacePanel("CommandPanel", false);
        _bottomDeckContent!.AddChild(_commandPanel);
        VBoxContainer box = new(); _commandPanel.AddChild(box);
        HBoxContainer header = new(); box.AddChild(header);
        _commandTitle = Label("COMMANDS", TextRole.Heading); _commandTitle.Name = "CommandTitle"; _commandTitle.SizeFlagsHorizontal = SizeFlags.ExpandFill; header.AddChild(_commandTitle);
        Label gridLabel = Label("ORDERS", TextRole.Micro); header.AddChild(gridLabel);
        _commandGrid = new GridContainer { Name = "CommandGrid", Columns = 3 };
        box.AddChild(_commandGrid);
        for (int i = 0; i < CommandCapacity; i++)
        {
            int index = i;
            _commandButtons[i] = Button("—", $"Command{i}");
            _commandButtons[i].CustomMinimumSize = new Vector2(0, 30);
            _commandButtons[i].SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _commandButtons[i].Alignment = HorizontalAlignment.Left;
            _commandButtons[i].Pressed += () =>
            {
                if (index >= _frame.Commands.Count) return;
                HudCommandFrame command = _frame.Commands[index];
                CommandRequested?.Invoke(command.Id, command.QueueFive && Input.IsKeyPressed(Key.Shift));
            };
            _commandGrid.AddChild(_commandButtons[i]);
        }
        _queueHeading = Label("QUEUE", TextRole.Micro); box.AddChild(_queueHeading);
        _queueScroll = new ScrollContainer { Name = "ProductionQueueScroll", CustomMinimumSize = new Vector2(0, 44), SizeFlagsVertical = SizeFlags.ExpandFill, HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        box.AddChild(_queueScroll);
        _queueList = new VBoxContainer { Name = "ProductionQueue", SizeFlagsHorizontal = SizeFlags.ExpandFill }; _queueScroll.AddChild(_queueList);
        for (int i = 0; i < QueueCapacity; i++)
        {
            VBoxContainer queueRow = new();
            _queueRows[i] = queueRow;
            HBoxContainer line = new(); queueRow.AddChild(line);
            _queueLabels[i] = Label(string.Empty, TextRole.Micro); _queueLabels[i].SizeFlagsHorizontal = SizeFlags.ExpandFill; line.AddChild(_queueLabels[i]);
            Button cancel = Button("×", $"CancelQueue{i}");
            cancel.CustomMinimumSize = new Vector2(26, 22);
            cancel.Disabled = true;
            cancel.TooltipText = "Cancel this queued item.";
            line.AddChild(cancel);
            _queueProgress[i] = new ProgressBar { MinValue = 0, MaxValue = 100, ShowPercentage = false, CustomMinimumSize = new Vector2(0, 4) };
            queueRow.AddChild(_queueProgress[i]);
            _queueList.AddChild(queueRow);
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
        VBoxContainer eventBox = new(); _eventPanel.AddChild(eventBox); eventBox.AddChild(Label("EVENT FEED", TextRole.Micro));
        for (int i = 0; i < EventCapacity; i++)
        {
            _eventLabels[i] = Label(string.Empty, TextRole.Micro);
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
        _tooltipTitle = Label(string.Empty, TextRole.Heading); tooltipBox.AddChild(_tooltipTitle);
        _tooltipBody = Label(string.Empty, TextRole.Body);
        _tooltipBody.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _tooltipBody.MaxLinesVisible = 7;
        _tooltipBody.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _tooltipBody.ClipText = true;
        _tooltipBody.SizeFlagsVertical = SizeFlags.ExpandFill;
        tooltipBox.AddChild(_tooltipBody);

        _energyPopover = SurfacePanel("EnergyDomainPopover", true); _safeArea.AddChild(_energyPopover);
        VBoxContainer energyBox = new(); _energyPopover.AddChild(energyBox); energyBox.AddChild(Label("ENERGY DOMAINS", TextRole.Heading));
        _energyPopoverLabel = Label(string.Empty, TextRole.Body); _energyPopoverLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart; energyBox.AddChild(_energyPopoverLabel);
    }

    private void LayoutPanels()
    {
        if (_safeArea is null || _topPanel is null || _bottomDeck is null || _bottomDeckContent is null || _minimapPanel is null || _selectionPanel is null || _commandPanel is null || _alertPanel is null ||
            _eventPanel is null || _objectivePanel is null || _tooltipPanel is null || _energyPopover is null) return;
        float safeInset = (100f - _profile.Layout.SafeAreaPercent) / 200f;
        _safeArea.AnchorLeft = safeInset; _safeArea.AnchorRight = 1f - safeInset;
        _safeArea.AnchorTop = safeInset; _safeArea.AnchorBottom = 1f - safeInset;
        _safeArea.OffsetLeft = _safeArea.OffsetRight = _safeArea.OffsetTop = _safeArea.OffsetBottom = 0f;
        Vector2 area = _safeArea.Size;
        if (area.X <= 1f || area.Y <= 1f) return;
        float scale = _profile.Layout.UiScale;
        float gap = _profile.Layout.PanelGap * scale;
        float sectionGap = Mathf.Clamp(gap * 0.35f, 2f, 6f);
        float topHeight = _profile.Layout.TopStripHeight * scale;
        float requestedBottomHeight = _profile.Layout.BottomRegionHeight * scale;
        float availableBottomHeight = Math.Max(120f, area.Y - topHeight - gap * 2f);
        float bottomHeight = Math.Min(requestedBottomHeight, availableBottomHeight);
        float sectionMinimumHeight = Math.Max(_minimapPanel.GetCombinedMinimumSize().Y,
            Math.Max(_selectionPanel.GetCombinedMinimumSize().Y, _commandPanel.GetCombinedMinimumSize().Y));
        bottomHeight = Math.Min(availableBottomHeight, Math.Max(bottomHeight, sectionMinimumHeight));
        float minimapWidth = Math.Min(
            Math.Max(_profile.Layout.MinimapSize * scale, _minimapPanel.GetCombinedMinimumSize().X),
            area.X * 0.24f);
        float commandWidth = Math.Min(
            Math.Max(_profile.Layout.CommandPanelWidth * scale, _commandPanel.GetCombinedMinimumSize().X),
            area.X * 0.35f);
        float topWidth = Math.Min(area.X, 1560f * scale);
        SetRect(_topPanel, new Vector2((area.X - topWidth) * 0.5f, 0f), new Vector2(topWidth, topHeight));
        float desiredDeckWidth = minimapWidth + commandWidth + _profile.Layout.SelectionMaxWidth * scale + sectionGap * 2f;
        float deckWidth = Math.Min(area.X, Math.Max(minimapWidth + commandWidth + 300f * scale + sectionGap * 2f, desiredDeckWidth));
        float deckX = (area.X - deckWidth) * 0.5f;
        float deckY = area.Y - bottomHeight;
        float selectionWidth = Math.Max(280f, deckWidth - minimapWidth - commandWidth - sectionGap * 2f);
        SetRect(_bottomDeck, new Vector2(deckX, deckY), new Vector2(deckWidth, bottomHeight));
        SetRect(_bottomDeckContent, Vector2.Zero, new Vector2(deckWidth, bottomHeight));
        SetRect(_minimapPanel, Vector2.Zero, new Vector2(minimapWidth, bottomHeight));
        SetRect(_selectionPanel, new Vector2(minimapWidth + sectionGap, 0f), new Vector2(selectionWidth, bottomHeight));
        SetRect(_commandPanel, new Vector2(deckWidth - commandWidth, 0f), new Vector2(commandWidth, bottomHeight));
        _bottomDeckChrome?.SetJunctions((minimapWidth + sectionGap * 0.5f) / deckWidth,
            (deckWidth - commandWidth - sectionGap * 0.5f) / deckWidth);

        SetRect(_alertPanel, new Vector2(deckX, deckY - 52f * scale - gap), new Vector2(Math.Min(deckWidth * 0.42f, Math.Max(minimapWidth, 390f * scale)), 52f * scale));
        float eventHeight = Math.Max(96f * scale, _eventPanel.GetCombinedMinimumSize().Y);
        float tooltipHeight = Math.Max(196f * scale, _tooltipPanel.GetCombinedMinimumSize().Y);
        float commandX = deckX + deckWidth - commandWidth;
        float topOverlayLimit = topHeight + gap;
        float overlayBottom = deckY - gap;
        float availableStackHeight = Math.Max(0f, overlayBottom - topOverlayLimit);
        if (_tooltipPanel.Visible && _eventPanel.Visible)
        {
            float desiredStackHeight = eventHeight + sectionGap + tooltipHeight;
            if (desiredStackHeight > availableStackHeight)
            {
                float overflow = desiredStackHeight - availableStackHeight;
                float reducibleTooltip = Math.Max(0f, tooltipHeight - 120f * scale);
                float tooltipReduction = Math.Min(overflow, reducibleTooltip);
                tooltipHeight -= tooltipReduction;
                overflow -= tooltipReduction;
                eventHeight = Math.Max(72f * scale, eventHeight - overflow);
            }
        }
        else if (_tooltipPanel.Visible) tooltipHeight = Math.Min(tooltipHeight, availableStackHeight);
        else if (_eventPanel.Visible) eventHeight = Math.Min(eventHeight, availableStackHeight);
        float tooltipY = Math.Max(topOverlayLimit, overlayBottom - tooltipHeight);
        float eventY = _tooltipPanel.Visible
            ? Math.Max(topOverlayLimit, tooltipY - eventHeight - sectionGap)
            : Math.Max(topOverlayLimit, overlayBottom - eventHeight);
        SetRect(_eventPanel, new Vector2(commandX, eventY), new Vector2(commandWidth, eventHeight));
        SetRect(_objectivePanel, new Vector2(area.X - 360f * scale, topHeight + gap), new Vector2(360f * scale, 88f * scale));
        SetRect(_tooltipPanel, new Vector2(commandX, tooltipY), new Vector2(commandWidth, tooltipHeight));
        SetRect(_energyPopover, new Vector2((area.X - 390f * scale) * 0.5f, topHeight + gap), new Vector2(390f * scale, 184f * scale));
        if (_portraitPanel is not null) _portraitPanel.CustomMinimumSize = new Vector2(144f * scale, 0f);
        if (_portraitView is not null) _portraitView.CustomMinimumSize = new Vector2(128f * scale, 106f * scale);
        if (_groupScroll is not null) _groupScroll.CustomMinimumSize = new Vector2(216f * scale, 0f);
        if (_groupList is not null) _groupList.CustomMinimumSize = new Vector2(200f * scale, 0f);
    }

    private void ApplyTypography()
    {
        float requestedScale = _profile.Layout.UiScale * _profile.Typography.TextScale;
        int headingSize = RoundFont(_profile.Typography.HeadingSize * requestedScale);
        int bodySize = RoundFont(_profile.Typography.BodySize * requestedScale);
        int microSize = RoundFont(_profile.Typography.MicroSize * requestedScale);
        for (int i = 0; i < _headingLabels.Count; i++) _headingLabels[i].AddThemeFontSizeOverride("font_size", headingSize);
        for (int i = 0; i < _bodyLabels.Count; i++) _bodyLabels[i].AddThemeFontSizeOverride("font_size", bodySize);
        for (int i = 0; i < _microLabels.Count; i++) _microLabels[i].AddThemeFontSizeOverride("font_size", microSize);
        for (int i = 0; i < _allButtons.Count; i++) _allButtons[i].AddThemeFontSizeOverride("font_size", microSize);
        if (_oreHeading is not null) _oreHeading.Visible = _profile.Content.ShowResourceLabels;
        if (_energyHeading is not null) _energyHeading.Visible = _profile.Content.ShowResourceLabels;
        if (_crystalHeading is not null) _crystalHeading.Visible = _profile.Content.ShowResourceLabels;
        if (_operationsHeading is not null) _operationsHeading.Visible = _profile.Content.ShowResourceLabels;
        if (_mechanicHeading is not null) _mechanicHeading.Visible = _profile.Content.ShowResourceLabels;
        Node? legend = FindChild("MinimapLegend", true, false);
        if (legend is CanvasItem canvas) canvas.Visible = _profile.Content.ShowMinimapLegend;
    }

    private void ApplySurfaces()
    {
        Color background = Parse(_profile.Colors.Background, new Color("111820"));
        Color raised = Parse(_profile.Colors.Raised, new Color("1b2731"));
        Color recessed = Parse(_profile.Colors.Recessed, new Color("0b1016"));
        Color displayAccent = DisplayAccent();
        Color secondaryAccent = SecondaryAccent();
        Color factionSurface = HudFactionChrome.SurfaceForFaction(_profile.ArtSkin.Faction);
        float factionFill = _profile.ArtSkin.Enabled ? 0.16f * _profile.ArtSkin.ChromeIntensity : 0f;
        Color text = Parse(_profile.Colors.TextPrimary, Colors.White);
        Color muted = Parse(_profile.Colors.TextMuted, new Color("aab4b8"));
        for (int i = 0; i < _surfacePanels.Count; i++)
        {
            PanelContainer panel = _surfacePanels[i];
            bool chromePanel = DirectChrome(panel) is not null;
            bool suppressGenericBorder = _profile.ArtSkin.Enabled && chromePanel;
            if (panel == _bottomDeck)
                panel.AddThemeStyleboxOverride("panel", Style(recessed.Lerp(factionSurface, factionFill * 1.25f), Colors.Transparent, 0, true));
            else if (panel == _minimapPanel || panel == _selectionPanel || panel == _commandPanel)
            {
                Color sectionBackground = panel == _selectionPanel ? background.Lightened(0.025f) : recessed.Lightened(0.025f);
                sectionBackground = sectionBackground.Lerp(factionSurface,
                    factionFill * (panel == _selectionPanel ? 1.15f : 0.85f));
                panel.AddThemeStyleboxOverride("panel", SectionStyle(sectionBackground,
                    new Color(secondaryAccent, 0.46f), panel == _minimapPanel, panel == _commandPanel));
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
            _raisedPanels[i].AddThemeStyleboxOverride("panel", Style(raised, displayAccent));
        if (_portraitPanel is not null)
            _portraitPanel.AddThemeStyleboxOverride("panel", Style(
                recessed.Lightened(0.03f).Lerp(factionSurface, factionFill * 1.35f), secondaryAccent));
        _portraitView?.ApplyProfile(_profile);
        for (int i = 0; i < _factionChrome.Count; i++)
        {
            HudFactionChrome chrome = _factionChrome[i];
            PanelContainer panel = chrome.GetParent<PanelContainer>();
            int expansion = ChromeContentPadding(panel) ?? _profile.Surface.InnerPadding;
            chrome.Configure(_profile.ArtSkin.Faction, chrome.Role,
                _profile.ArtSkin.Enabled ? _profile.ArtSkin.ChromeIntensity : 0f,
                _profile.ArtSkin.ChromeScale, expansion);
        }
        for (int i = 0; i < _headingLabels.Count; i++) _headingLabels[i].AddThemeColorOverride("font_color", displayAccent);
        for (int i = 0; i < _bodyLabels.Count; i++) _bodyLabels[i].AddThemeColorOverride("font_color", text);
        for (int i = 0; i < _microLabels.Count; i++) _microLabels[i].AddThemeColorOverride("font_color", muted);
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
            button.AddThemeColorOverride("font_color", text);
            button.AddThemeColorOverride("font_hover_color", text);
            button.AddThemeColorOverride("font_pressed_color", displayAccent);
            button.AddThemeColorOverride("font_disabled_color", new Color(muted, 0.48f));
            if (commandButton)
            {
                Color commandNormal = _profile.Surface.SolidCommandButtons
                    ? raised.Darkened(0.10f)
                    : Colors.Transparent;
                Color commandDisabled = _profile.Surface.SolidCommandButtons
                    ? new Color(recessed, 0.72f)
                    : Colors.Transparent;
                button.AddThemeStyleboxOverride("normal", ButtonStyle(commandNormal, new Color(secondaryAccent, 0.58f)));
                button.AddThemeStyleboxOverride("hover", ButtonStyle(raised.Lightened(0.08f), displayAccent));
                button.AddThemeStyleboxOverride("pressed", ButtonStyle(recessed, displayAccent));
                button.AddThemeStyleboxOverride("focus", ButtonStyle(recessed, displayAccent));
                button.AddThemeStyleboxOverride("disabled", ButtonStyle(commandDisabled, new Color(muted, 0.20f)));
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
            _selectionHealth.AddThemeStyleboxOverride("background", Style(recessed, recessed));
            _selectionHealth.AddThemeStyleboxOverride("fill", Style(Parse(_profile.Colors.Good, Colors.Green), Parse(_profile.Colors.Good, Colors.Green)));
        }
        for (int i = 0; i < _groupHealth.Length; i++)
        {
            _groupHealth[i].AddThemeStyleboxOverride("background", Style(recessed, recessed));
            _groupHealth[i].AddThemeStyleboxOverride("fill", Style(Parse(_profile.Colors.Good, Colors.Green), Parse(_profile.Colors.Good, Colors.Green)));
        }
        for (int i = 0; i < _queueProgress.Length; i++)
        {
            _queueProgress[i].AddThemeStyleboxOverride("background", Style(recessed, recessed));
            _queueProgress[i].AddThemeStyleboxOverride("fill", Style(displayAccent, displayAccent));
        }
        ApplyContainerSpacing();
    }

    private void ApplyContainerSpacing()
    {
        int separation = _profile.Surface.Separation;
        foreach (Node node in GetChildrenRecursive(this))
        {
            if (node is BoxContainer box) box.AddThemeConstantOverride("separation", separation);
            else if (node is GridContainer grid)
            {
                grid.AddThemeConstantOverride("h_separation", separation);
                grid.AddThemeConstantOverride("v_separation", separation);
            }
        }
    }

    private void ApplySelectionGroups(IReadOnlyList<HudSelectionGroupFrame> groups)
    {
        for (int i = 0; i < GroupCapacity; i++)
        {
            bool visible = i < groups.Count;
            _groupButtons[i].GetParent<CanvasItem>().Visible = visible;
            if (!visible) continue;
            HudSelectionGroupFrame group = groups[i];
            _groupButtons[i].Text = $"▰  {group.Name} ×{group.Count}\n{group.StateSummary}";
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
            if (!exists) continue;
            HudCommandFrame command = commands[i];
            string hotkey = _profile.Content.ShowHotkeys && command.Hotkey.Length > 0 ? $"  [{command.Hotkey}]" : string.Empty;
            string cost = _profile.Content.ShowCommandCosts && command.Cost.Length > 0 ? $"\n{command.Cost}" : string.Empty;
            button.Text = $"{CommandGlyph(command.Id)}  {command.Name}{hotkey}{cost}";
            button.TooltipText = command.Tooltip;
            button.Disabled = !command.Enabled;
            button.ButtonPressed = command.Active;
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
            _queueLabels[i].Text = $"{i + 1}. {item.Name}{(item.Count > 1 ? $" ×{item.Count}" : string.Empty)}{(item.Paused ? "  PAUSED" : string.Empty)}";
            _queueProgress[i].Value = item.ProgressPercent;
        }
        bool hasQueue = queue.Count > 0;
        if (_queueList is not null) _queueList.Visible = hasQueue;
        if (_queueHeading is not null) _queueHeading.Visible = hasQueue;
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
            _eventLabels[i].AddThemeColorOverride("font_color", PriorityColor(item.Priority));
        }
    }

    private void ApplyPriorityColor(Label? label, HudAlertPriority priority)
    {
        if (label is null) return;
        label.AddThemeColorOverride("font_color", priority == HudAlertPriority.None ? Parse(_profile.Colors.TextPrimary, Colors.White) : PriorityColor(priority));
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
        bar.AddThemeStyleboxOverride("fill", Style(fill, fill));
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
        _ => Parse(_profile.Colors.TextMuted, Colors.Gray)
    };

    private Color DisplayAccent() => _profile.ArtSkin.Enabled
        ? HudFactionChrome.AccentForFaction(_profile.ArtSkin.Faction)
        : Parse(_profile.Colors.Accent, new Color("e6ad28"));

    private Color SecondaryAccent() => _profile.ArtSkin.Enabled
        ? HudFactionChrome.SecondaryAccentForFaction(_profile.ArtSkin.Faction)
        : Parse(_profile.Colors.Selection, new Color("5fc4d8"));

    private PanelContainer SurfacePanel(string name, bool raised)
    {
        PanelContainer panel = new() { Name = name, MouseFilter = MouseFilterEnum.Stop };
        (raised ? _raisedPanels : _surfacePanels).Add(panel);
        HudFactionChromeRole? chromeRole = name switch
        {
            "ResourceStrip" => HudFactionChromeRole.TopStrip,
            "BottomDeck" => HudFactionChromeRole.BottomDeck,
            _ => null
        };
        if (chromeRole.HasValue)
        {
            HudFactionChrome chrome = new()
            {
                Name = $"{name}FactionChrome",
                MouseFilter = MouseFilterEnum.Ignore,
                CustomMinimumSize = Vector2.Zero
            };
            chrome.Configure(_profile.ArtSkin.Faction, chromeRole.Value,
                _profile.ArtSkin.ChromeIntensity, _profile.ArtSkin.ChromeScale, _profile.Surface.InnerPadding);
            panel.AddChild(chrome);
            _factionChrome.Add(chrome);
        }
        return panel;
    }

    private void AddResourceBlock(Container parent, string title, out Label heading, out Label value)
    {
        VBoxContainer box = ResourceBlock(title, out heading, out value);
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

    private VBoxContainer ResourceBlock(string title, out Label heading, out Label value)
    {
        VBoxContainer box = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 1f,
            MouseFilter = MouseFilterEnum.Ignore
        };
        heading = Label(title, TextRole.Micro);
        heading.HorizontalAlignment = HorizontalAlignment.Center;
        heading.MouseFilter = MouseFilterEnum.Ignore;
        box.AddChild(heading);
        value = Label("—", TextRole.Body);
        value.HorizontalAlignment = HorizontalAlignment.Center;
        value.MouseFilter = MouseFilterEnum.Ignore;
        box.AddChild(value);
        return box;
    }

    private Label Label(string text, TextRole role)
    {
        Label label = new() { Text = text };
        switch (role)
        {
            case TextRole.Heading: _headingLabels.Add(label); break;
            case TextRole.Body: _bodyLabels.Add(label); break;
            default: _microLabels.Add(label); break;
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
        if (!_profile.ArtSkin.Enabled || DirectChrome(panel) is null) return null;
        if (panel.Name == "BottomDeck") return 0;
        float basePadding = 10f;
        return Math.Max(_profile.Surface.InnerPadding,
            Mathf.RoundToInt(basePadding * _profile.ArtSkin.ChromeScale));
    }

    private static HudFactionChrome? DirectChrome(PanelContainer panel) =>
        panel.FindChild($"{panel.Name}FactionChrome", false, false) as HudFactionChrome;

    private StyleBoxFlat Style(Color background, Color border, int? paddingOverride = null, bool suppressBorder = false)
    {
        int radius = _profile.Surface.CornerRadius;
        int borderWidth = suppressBorder ? 0 : _profile.Surface.BorderWidth;
        int padding = paddingOverride ?? _profile.Surface.InnerPadding;
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
        int padding = _profile.Surface.InnerPadding;
        const int outerMargin = 30;
        const int innerMargin = 14;
        const int topMargin = 29;
        const int bottomMargin = 27;
        return new StyleBoxFlat
        {
            BgColor = new Color(background, _profile.Surface.PanelOpacity),
            BorderColor = divider,
            BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2, CornerRadiusBottomLeft = 2, CornerRadiusBottomRight = 2,
            ContentMarginLeft = SectionMargin(outerLeft ? outerMargin : innerMargin, padding),
            ContentMarginRight = SectionMargin(outerRight ? outerMargin : innerMargin, padding),
            ContentMarginTop = SectionMargin(topMargin, padding),
            ContentMarginBottom = SectionMargin(bottomMargin, padding)
        };
    }

    private static int SectionMargin(int authoredMargin, int padding) =>
        Math.Max(0, authoredMargin + padding - 10);

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

    private static int RoundFont(float size) => Math.Max(8, Mathf.RoundToInt(size));

    private static Color Parse(string html, Color fallback) => Color.HtmlIsValid(html) ? new Color(html) : fallback;

    private enum TextRole { Heading, Body, Micro }
}
