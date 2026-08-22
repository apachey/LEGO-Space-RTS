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
    private readonly List<Button> _allButtons = new();
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
        LayoutPanels();
        _minimap?.ApplyProfile(profile);
        ApplyFrame(_frame, true);
    }

    public void ApplyFrame(HudFrame frame, bool force = false)
    {
        if (!_treeBuilt) return;
        string signature = frame.ContentSignature();
        if (!force && signature == _lastSignature) return;
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
        SetText(_portraitLabel, frame.Selection.PortraitCaption);
        SetText(_selectionHealthText, frame.Selection.HealthText);
        SetText(_selectionStatus, frame.Selection.StatusText);
        if (_alertButton is not null)
        {
            _alertButton.Text = frame.Alert.Text;
            _alertButton.TooltipText = frame.Alert.Text;
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
    }

    private void BuildTree()
    {
        _treeBuilt = true;
        _safeArea = new Control { Name = "HudSafeArea", MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_safeArea);

        BuildTopStrip();
        BuildMinimapRegion();
        BuildSelectionRegion();
        BuildCommandRegion();
        BuildTransientRegions();
    }

    private void BuildTopStrip()
    {
        _topPanel = SurfacePanel("ResourceStrip", false);
        _safeArea!.AddChild(_topPanel);
        HBoxContainer row = new() { Name = "ResourceRow", Alignment = BoxContainer.AlignmentMode.Center };
        _topPanel.AddChild(row);
        AddResourceBlock(row, "ORE", out _oreHeading, out _oreValue);
        Button energyButton = Button("", "EnergyButton");
        energyButton.Flat = true;
        energyButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        VBoxContainer energy = ResourceBlock("ENERGY", out _energyHeading, out _energyValue);
        energyButton.AddChild(energy);
        energyButton.Pressed += () => EnergyDetailsRequested?.Invoke();
        row.AddChild(energyButton);
        AddResourceBlock(row, "CRYSTALS", out _crystalHeading, out _crystalValue);
        AddResourceBlock(row, "OPERATIONS", out _operationsHeading, out _operationsValue);
        AddResourceBlock(row, "FACTION", out _mechanicHeading, out _mechanicValue);
        _matchState = Label("MATCH", TextRole.Micro);
        _matchState.HorizontalAlignment = HorizontalAlignment.Center;
        _matchState.VerticalAlignment = VerticalAlignment.Center;
        _matchState.CustomMinimumSize = new Vector2(150, 0);
        row.AddChild(_matchState);
    }

    private void BuildMinimapRegion()
    {
        _minimapPanel = SurfacePanel("MinimapRegion", false);
        _safeArea!.AddChild(_minimapPanel);
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
        _safeArea!.AddChild(_selectionPanel);
        HBoxContainer row = new(); _selectionPanel.AddChild(row);
        _portraitPanel = SurfacePanel("PortraitSlot", true);
        _portraitPanel.CustomMinimumSize = new Vector2(132, 0);
        _portraitLabel = Label("NO SELECTION", TextRole.Body);
        _portraitLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _portraitLabel.VerticalAlignment = VerticalAlignment.Center;
        _portraitLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _portraitPanel.AddChild(_portraitLabel);
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
        _groupList.AddChild(Label("TYPE GROUPS", TextRole.Micro));
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
        _safeArea!.AddChild(_commandPanel);
        VBoxContainer box = new(); _commandPanel.AddChild(box);
        HBoxContainer header = new(); box.AddChild(header);
        _commandTitle = Label("COMMANDS", TextRole.Heading); _commandTitle.Name = "CommandTitle"; _commandTitle.SizeFlagsHorizontal = SizeFlags.ExpandFill; header.AddChild(_commandTitle);
        Label gridLabel = Label("3 × 4", TextRole.Micro); header.AddChild(gridLabel);
        _commandGrid = new GridContainer { Name = "CommandGrid", Columns = 3 };
        box.AddChild(_commandGrid);
        for (int i = 0; i < CommandCapacity; i++)
        {
            int index = i;
            _commandButtons[i] = Button("—", $"Command{i}");
            _commandButtons[i].CustomMinimumSize = new Vector2(0, 25);
            _commandButtons[i].SizeFlagsHorizontal = SizeFlags.ExpandFill;
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
            cancel.TooltipText = "Production cancellation and refund preview are completed in T073.";
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
        _alertButton.Pressed += () => AlertRequested?.Invoke();
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
        _tooltipBody = Label(string.Empty, TextRole.Body); _tooltipBody.AutowrapMode = TextServer.AutowrapMode.WordSmart; tooltipBox.AddChild(_tooltipBody);

        _energyPopover = SurfacePanel("EnergyDomainPopover", true); _safeArea.AddChild(_energyPopover);
        VBoxContainer energyBox = new(); _energyPopover.AddChild(energyBox); energyBox.AddChild(Label("ENERGY DOMAINS", TextRole.Heading));
        _energyPopoverLabel = Label(string.Empty, TextRole.Body); _energyPopoverLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart; energyBox.AddChild(_energyPopoverLabel);
    }

    private void LayoutPanels()
    {
        if (_safeArea is null || _topPanel is null || _minimapPanel is null || _selectionPanel is null || _commandPanel is null || _alertPanel is null ||
            _eventPanel is null || _objectivePanel is null || _tooltipPanel is null || _energyPopover is null) return;
        float safeInset = (100f - _profile.Layout.SafeAreaPercent) / 200f;
        _safeArea.AnchorLeft = safeInset; _safeArea.AnchorRight = 1f - safeInset;
        _safeArea.AnchorTop = safeInset; _safeArea.AnchorBottom = 1f - safeInset;
        _safeArea.OffsetLeft = _safeArea.OffsetRight = _safeArea.OffsetTop = _safeArea.OffsetBottom = 0f;
        Vector2 area = _safeArea.Size;
        if (area.X <= 1f || area.Y <= 1f) return;
        float scale = _profile.Layout.UiScale;
        float gap = _profile.Layout.PanelGap * scale;
        float topHeight = _profile.Layout.TopStripHeight * scale;
        float bottomHeight = Math.Min(_profile.Layout.BottomRegionHeight * scale, area.Y * 0.25f);
        float minimapWidth = Math.Min(_profile.Layout.MinimapSize * scale, area.X * 0.22f);
        float commandWidth = Math.Min(_profile.Layout.CommandPanelWidth * scale, area.X * 0.31f);
        float topWidth = Math.Min(area.X, 1560f * scale);
        SetRect(_topPanel, new Vector2((area.X - topWidth) * 0.5f, 0f), new Vector2(topWidth, topHeight));
        SetRect(_minimapPanel, new Vector2(0f, area.Y - bottomHeight), new Vector2(minimapWidth, bottomHeight));
        SetRect(_commandPanel, new Vector2(area.X - commandWidth, area.Y - bottomHeight), new Vector2(commandWidth, bottomHeight));
        float centerStart = minimapWidth + gap;
        float centerEnd = area.X - commandWidth - gap;
        float availableCenter = Math.Max(280f, centerEnd - centerStart);
        float centerWidth = Math.Min(_profile.Layout.SelectionMaxWidth * scale, availableCenter);
        SetRect(_selectionPanel, new Vector2(centerStart + (availableCenter - centerWidth) * 0.5f, area.Y - bottomHeight), new Vector2(centerWidth, bottomHeight));
        SetRect(_alertPanel, new Vector2(0f, area.Y - bottomHeight - 52f * scale - gap), new Vector2(Math.Max(minimapWidth, 390f * scale), 52f * scale));
        SetRect(_eventPanel, new Vector2(area.X - commandWidth, area.Y - bottomHeight - 118f * scale - gap), new Vector2(commandWidth, 118f * scale));
        SetRect(_objectivePanel, new Vector2(area.X - 360f * scale, topHeight + gap), new Vector2(360f * scale, 88f * scale));
        SetRect(_tooltipPanel, new Vector2(Math.Max(0f, area.X - commandWidth - 340f * scale - gap), area.Y - bottomHeight - 154f * scale - gap), new Vector2(340f * scale, 154f * scale));
        SetRect(_energyPopover, new Vector2((area.X - 390f * scale) * 0.5f, topHeight + gap), new Vector2(390f * scale, 184f * scale));
        if (_portraitPanel is not null) _portraitPanel.CustomMinimumSize = new Vector2(132f * scale, 0f);
        if (_groupScroll is not null) _groupScroll.CustomMinimumSize = new Vector2(216f * scale, 0f);
        if (_groupList is not null) _groupList.CustomMinimumSize = new Vector2(200f * scale, 0f);
    }

    private void ApplyTypography()
    {
        float textScale = _profile.Layout.UiScale * _profile.Typography.TextScale;
        for (int i = 0; i < _headingLabels.Count; i++) _headingLabels[i].AddThemeFontSizeOverride("font_size", RoundFont(_profile.Typography.HeadingSize * textScale));
        for (int i = 0; i < _bodyLabels.Count; i++) _bodyLabels[i].AddThemeFontSizeOverride("font_size", RoundFont(_profile.Typography.BodySize * textScale));
        for (int i = 0; i < _microLabels.Count; i++) _microLabels[i].AddThemeFontSizeOverride("font_size", RoundFont(_profile.Typography.MicroSize * textScale));
        for (int i = 0; i < _allButtons.Count; i++) _allButtons[i].AddThemeFontSizeOverride("font_size", RoundFont(_profile.Typography.MicroSize * textScale));
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
        Color accent = Parse(_profile.Colors.Accent, new Color("e6ad28"));
        Color text = Parse(_profile.Colors.TextPrimary, Colors.White);
        Color muted = Parse(_profile.Colors.TextMuted, new Color("aab4b8"));
        for (int i = 0; i < _surfacePanels.Count; i++) _surfacePanels[i].AddThemeStyleboxOverride("panel", Style(background, accent));
        for (int i = 0; i < _raisedPanels.Count; i++) _raisedPanels[i].AddThemeStyleboxOverride("panel", Style(raised, accent));
        for (int i = 0; i < _headingLabels.Count; i++) _headingLabels[i].AddThemeColorOverride("font_color", accent);
        for (int i = 0; i < _bodyLabels.Count; i++) _bodyLabels[i].AddThemeColorOverride("font_color", text);
        for (int i = 0; i < _microLabels.Count; i++) _microLabels[i].AddThemeColorOverride("font_color", muted);
        StyleBoxFlat normalButton = Style(_profile.Surface.SolidCommandButtons ? raised : background, muted);
        StyleBoxFlat hoverButton = Style(raised.Lightened(0.10f), accent);
        StyleBoxFlat pressedButton = Style(recessed, accent);
        for (int i = 0; i < _allButtons.Count; i++)
        {
            Button button = _allButtons[i];
            button.AddThemeColorOverride("font_color", text);
            button.AddThemeColorOverride("font_hover_color", text);
            button.AddThemeColorOverride("font_pressed_color", accent);
            button.AddThemeStyleboxOverride("normal", normalButton);
            button.AddThemeStyleboxOverride("hover", hoverButton);
            button.AddThemeStyleboxOverride("pressed", pressedButton);
            button.AddThemeStyleboxOverride("focus", pressedButton);
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
            _queueProgress[i].AddThemeStyleboxOverride("fill", Style(accent, accent));
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
            _groupButtons[i].Text = $"{group.Name} ×{group.Count}\n{group.StateSummary}";
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
            button.Text = $"{command.Name}{hotkey}{cost}";
            button.TooltipText = command.Tooltip;
            button.Disabled = !command.Enabled;
            button.ButtonPressed = command.Active;
        }
    }

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
        HudAlertPriority.Normal => Parse(_profile.Colors.Accent, Colors.Yellow),
        HudAlertPriority.Informational => Parse(_profile.Colors.Selection, Colors.Cyan),
        _ => Parse(_profile.Colors.TextMuted, Colors.Gray)
    };

    private PanelContainer SurfacePanel(string name, bool raised)
    {
        PanelContainer panel = new() { Name = name, MouseFilter = MouseFilterEnum.Stop };
        (raised ? _raisedPanels : _surfacePanels).Add(panel);
        return panel;
    }

    private void AddResourceBlock(Container parent, string title, out Label heading, out Label value)
    {
        VBoxContainer box = ResourceBlock(title, out heading, out value);
        parent.AddChild(box);
    }

    private VBoxContainer ResourceBlock(string title, out Label heading, out Label value)
    {
        VBoxContainer box = new() { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        heading = Label(title, TextRole.Micro); heading.HorizontalAlignment = HorizontalAlignment.Center; box.AddChild(heading);
        value = Label("—", TextRole.Body); value.HorizontalAlignment = HorizontalAlignment.Center; box.AddChild(value);
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

    private StyleBoxFlat Style(Color background, Color border)
    {
        int radius = _profile.Surface.CornerRadius;
        int borderWidth = _profile.Surface.BorderWidth;
        int padding = _profile.Surface.InnerPadding;
        return new StyleBoxFlat
        {
            BgColor = new Color(background, _profile.Surface.PanelOpacity), BorderColor = border,
            BorderWidthLeft = borderWidth, BorderWidthTop = borderWidth, BorderWidthRight = borderWidth, BorderWidthBottom = borderWidth,
            CornerRadiusTopLeft = radius, CornerRadiusTopRight = radius, CornerRadiusBottomLeft = radius, CornerRadiusBottomRight = radius,
            ContentMarginLeft = padding, ContentMarginRight = padding, ContentMarginTop = padding, ContentMarginBottom = padding
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
