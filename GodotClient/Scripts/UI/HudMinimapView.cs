using Godot;

namespace LegoSpaceRTS.UI;

public partial class HudMinimapView : Control
{
    private sealed class RenderMarker
    {
        public required HudMinimapMarkerFrame Frame;
        public Vector2 FromBuild;
        public Vector2 ToBuild;
    }

    private sealed class MarkerChannel
    {
        public required HudMinimapMarkerKind Kind;
        public required MultiMeshInstance2D Instance;
        public required MultiMesh Multimesh;
        public int Count;

        public void Begin() => Count = 0;

        public void Add(Vector2 position, float size, Color color)
        {
            if (Count >= HudMinimapFrame.MarkerCapacity) return;
            Multimesh.SetInstanceTransform2D(Count, new Transform2D(0f, new Vector2(size, size), 0f, position));
            Multimesh.SetInstanceColor(Count, color);
            Count++;
        }

        public void Finish() => Multimesh.VisibleInstanceCount = Count;
    }

    private M7HudProfile _profile = M7HudProfile.CreateDefault();
    private HudMinimapFrame _frame = new();
    private readonly List<RenderMarker> _renderMarkers = new();
    private readonly List<Vector2> _cameraPolygon = new();
    private readonly MarkerChannel[] _channels = new MarkerChannel[4];
    private ImageTexture? _terrainTexture;
    private ImageTexture? _fogTexture;
    private MinimapOverlay? _overlay;
    private ulong _transitionStartedMs;
    private int _appliedTerrainRevision = int.MinValue;
    private byte[]? _appliedTerrain;
    private bool _cameraDragging;
    private bool _built;

    public event Action<Vector2I>? CameraRequested;
    public event Action<Vector2I, bool>? GroundCommandRequested;

    public bool IsNorthUp => true;
    public bool IsConfigured => _frame.Terrain.Length == HudMinimapFrame.CellCount && _frame.Knowledge.Length == HudMinimapFrame.CellCount;
    public int MarkerCount => _frame.Markers.Count;
    public int RememberedMarkerCount => _frame.Markers.Count(marker => marker.Remembered);
    public int NetworkLineCount => _frame.Lines.Count;
    public int PingCount => _frame.Pings.Count;
    public int VisibleFogCells { get; private set; }
    public int ExploredFogCells { get; private set; }
    public HudMinimapFrame Frame => _frame;

    public void Configure(M7HudProfile profile)
    {
        Name = "MinimapSlot";
        MouseFilter = MouseFilterEnum.Stop;
        MouseDefaultCursorShape = CursorShape.PointingHand;
        ClipContents = true;
        TextureFilter = TextureFilterEnum.Nearest;
        EnsureBuilt();
        ApplyProfile(profile);
    }

    public void ApplyProfile(M7HudProfile profile)
    {
        _profile = profile;
        EnsureBuilt();
        RebuildTerrainTexture(force: true);
        RebuildFogTexture();
        QueueRedraw();
        _overlay?.QueueRedraw();
    }

    public void SetFrame(HudMinimapFrame frame)
    {
        EnsureBuilt();
        float transition = InterpolationFactor();
        Dictionary<uint, Vector2> previous = new(_renderMarkers.Count);
        for (int i = 0; i < _renderMarkers.Count; i++)
        {
            RenderMarker marker = _renderMarkers[i];
            previous[marker.Frame.StableId] = marker.FromBuild.Lerp(marker.ToBuild, transition);
        }

        _frame = frame;
        _renderMarkers.Clear();
        for (int i = 0; i < frame.Markers.Count && i < HudMinimapFrame.MarkerCapacity; i++)
        {
            HudMinimapMarkerFrame marker = frame.Markers[i];
            Vector2 target = new(marker.BuildX, marker.BuildY);
            _renderMarkers.Add(new RenderMarker
            {
                Frame = marker,
                FromBuild = previous.TryGetValue(marker.StableId, out Vector2 prior) ? prior : target,
                ToBuild = target
            });
        }
        _transitionStartedMs = Time.GetTicksMsec();
        RebuildTerrainTexture(force: false);
        RebuildFogTexture();
        UpdateMarkerInstances();
        QueueRedraw();
        _overlay?.QueueRedraw();
    }

    public void SetCameraPolygon(IReadOnlyList<Vector2> buildPoints)
    {
        _cameraPolygon.Clear();
        for (int i = 0; i < buildPoints.Count; i++)
            _cameraPolygon.Add(new Vector2(
                Mathf.Clamp(buildPoints[i].X, 0f, HudMinimapFrame.Width),
                Mathf.Clamp(buildPoints[i].Y, 0f, HudMinimapFrame.Height)));
        _overlay?.QueueRedraw();
    }

    public override void _Process(double delta)
    {
        _ = delta;
        if (!_built) return;
        UpdateMarkerInstances();
        if (_frame.Pings.Count > 0) _overlay?.QueueRedraw();
    }

    public override void _Notification(int what)
    {
        if (what != NotificationResized || !_built) return;
        if (_overlay is not null) { _overlay.Position = Vector2.Zero; _overlay.Size = Size; }
        UpdateMarkerInstances();
        QueueRedraw();
        _overlay?.QueueRedraw();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse)
        {
            if (mouse.ButtonIndex == MouseButton.Left)
            {
                _cameraDragging = mouse.Pressed;
                if (mouse.Pressed) CameraRequested?.Invoke(PixelToBuildCell(mouse.Position, Size));
                AcceptEvent();
            }
            else if (mouse.ButtonIndex == MouseButton.Right && mouse.Pressed)
            {
                GroundCommandRequested?.Invoke(PixelToBuildCell(mouse.Position, Size), mouse.ShiftPressed);
                AcceptEvent();
            }
        }
        else if (@event is InputEventMouseMotion motion && _cameraDragging &&
            (motion.ButtonMask & MouseButtonMask.Left) != 0)
        {
            CameraRequested?.Invoke(PixelToBuildCell(motion.Position, Size));
            AcceptEvent();
        }
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), Parse(_profile.Colors.Recessed, new Color("0b1016")));
        if (_terrainTexture is not null) DrawTextureRect(_terrainTexture, new Rect2(Vector2.Zero, Size), false);
        if (_fogTexture is not null) DrawTextureRect(_fogTexture, new Rect2(Vector2.Zero, Size), false);

        float gridOpacity = _profile.Minimap.GridOpacity;
        if (gridOpacity > 0.001f)
        {
            Color grid = new(Parse(_profile.Colors.TextMuted, Colors.Gray), gridOpacity);
            for (int i = 1; i < 8; i++)
            {
                float x = Size.X * i / 8f, y = Size.Y * i / 8f;
                DrawLine(new Vector2(x, 0f), new Vector2(x, Size.Y), grid, 1f);
                DrawLine(new Vector2(0f, y), new Vector2(Size.X, y), grid, 1f);
            }
        }

        if (_profile.Minimap.ShowNetworkLines)
        {
            for (int i = 0; i < _frame.Lines.Count && i < HudMinimapFrame.LineCapacity; i++) DrawNetworkLine(_frame.Lines[i]);
        }
    }

    public static Vector2I PixelToBuildCell(Vector2 localPosition, Vector2 controlSize)
    {
        float width = Math.Max(1f, controlSize.X), height = Math.Max(1f, controlSize.Y);
        int x = Math.Clamp(Mathf.FloorToInt(localPosition.X / width * HudMinimapFrame.Width), 0, HudMinimapFrame.Width - 1);
        int y = Math.Clamp(Mathf.FloorToInt(localPosition.Y / height * HudMinimapFrame.Height), 0, HudMinimapFrame.Height - 1);
        return new Vector2I(x, y);
    }

    public static Vector2 BuildToLocal(Vector2 buildPosition, Vector2 controlSize) => new(
        buildPosition.X / HudMinimapFrame.Width * controlSize.X,
        buildPosition.Y / HudMinimapFrame.Height * controlSize.Y);

    private void EnsureBuilt()
    {
        if (_built) return;
        _built = true;
        _channels[(int)HudMinimapMarkerKind.GroundMobile] = BuildChannel(HudMinimapMarkerKind.GroundMobile, "GroundMarkers");
        _channels[(int)HudMinimapMarkerKind.TrueAir] = BuildChannel(HudMinimapMarkerKind.TrueAir, "AirMarkers");
        _channels[(int)HudMinimapMarkerKind.Structure] = BuildChannel(HudMinimapMarkerKind.Structure, "StructureMarkers");
        _channels[(int)HudMinimapMarkerKind.Resource] = BuildChannel(HudMinimapMarkerKind.Resource, "ResourceMarkers");
        _overlay = new MinimapOverlay { Name = "MinimapOverlay", MouseFilter = MouseFilterEnum.Ignore };
        _overlay.Configure(this);
        AddChild(_overlay);
    }

    private MarkerChannel BuildChannel(HudMinimapMarkerKind kind, string name)
    {
        QuadMesh mesh = new() { Size = Vector2.One };
        MultiMesh multimesh = new()
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
            UseColors = true,
            Mesh = mesh,
            InstanceCount = HudMinimapFrame.MarkerCapacity,
            VisibleInstanceCount = 0
        };
        MultiMeshInstance2D instance = new()
        {
            Name = name,
            Multimesh = multimesh,
            Texture = CreateMarkerTexture(kind),
            ZIndex = 2
        };
        AddChild(instance);
        return new MarkerChannel { Kind = kind, Instance = instance, Multimesh = multimesh };
    }

    private static ImageTexture CreateMarkerTexture(HudMinimapMarkerKind kind)
    {
        const int size = 16;
        Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float nx = (x + 0.5f) / size * 2f - 1f;
            float ny = (y + 0.5f) / size * 2f - 1f;
            bool inside = kind switch
            {
                HudMinimapMarkerKind.GroundMobile => nx * nx + ny * ny <= 0.78f,
                HudMinimapMarkerKind.TrueAir => ny >= -0.78f && ny <= 0.82f && Math.Abs(nx) <= (0.82f - ny) * 0.62f,
                HudMinimapMarkerKind.Structure => Math.Abs(nx) <= 0.76f && Math.Abs(ny) <= 0.76f,
                _ => Math.Abs(nx) + Math.Abs(ny) <= 0.94f
            };
            image.SetPixel(x, y, inside ? Colors.White : Colors.Transparent);
        }
        return ImageTexture.CreateFromImage(image);
    }

    private void RebuildTerrainTexture(bool force)
    {
        if (_frame.Terrain.Length != HudMinimapFrame.CellCount) return;
        if (!force && _appliedTerrainRevision == _frame.TerrainRevision && ReferenceEquals(_appliedTerrain, _frame.Terrain)) return;
        Image image = Image.CreateEmpty(HudMinimapFrame.Width, HudMinimapFrame.Height, false, Image.Format.Rgba8);
        Color ground = Parse(_profile.Minimap.GroundColor, new Color("35403d"));
        Color rough = Parse(_profile.Minimap.RoughColor, new Color("594735"));
        Color blocked = Parse(_profile.Minimap.BlockedColor, new Color("1b2225"));
        Color excavatable = Parse(_profile.Minimap.ExcavatableColor, new Color("71512f"));
        for (int y = 0; y < HudMinimapFrame.Height; y++)
        for (int x = 0; x < HudMinimapFrame.Width; x++)
        {
            Color color = (HudMinimapTerrain)_frame.Terrain[y * HudMinimapFrame.Width + x] switch
            {
                HudMinimapTerrain.Rough => rough,
                HudMinimapTerrain.Blocked => blocked,
                HudMinimapTerrain.Excavatable => excavatable,
                _ => ground
            };
            image.SetPixel(x, y, color);
        }
        if (_terrainTexture is null) _terrainTexture = ImageTexture.CreateFromImage(image);
        else _terrainTexture.Update(image);
        _appliedTerrainRevision = _frame.TerrainRevision;
        _appliedTerrain = _frame.Terrain;
    }

    private void RebuildFogTexture()
    {
        if (_frame.Knowledge.Length != HudMinimapFrame.CellCount) return;
        Image image = Image.CreateEmpty(HudMinimapFrame.Width, HudMinimapFrame.Height, false, Image.Format.Rgba8);
        Color unseen = new(0f, 0f, 0f, _profile.Minimap.UnseenFogOpacity);
        Color explored = new(0f, 0f, 0f, _profile.Minimap.ExploredFogOpacity);
        VisibleFogCells = 0;
        ExploredFogCells = 0;
        for (int y = 0; y < HudMinimapFrame.Height; y++)
        for (int x = 0; x < HudMinimapFrame.Width; x++)
        {
            byte state = _frame.Knowledge[y * HudMinimapFrame.Width + x];
            if (state == 2) { VisibleFogCells++; image.SetPixel(x, y, Colors.Transparent); }
            else if (state == 1) { ExploredFogCells++; image.SetPixel(x, y, explored); }
            else image.SetPixel(x, y, unseen);
        }
        if (_fogTexture is null) _fogTexture = ImageTexture.CreateFromImage(image);
        else _fogTexture.Update(image);
    }

    private void UpdateMarkerInstances()
    {
        if (Size.X <= 1f || Size.Y <= 1f) return;
        for (int i = 0; i < _channels.Length; i++) _channels[i].Begin();
        float transition = InterpolationFactor();
        float sizeFactor = Mathf.Clamp(Math.Min(Size.X, Size.Y) / 206f, 0.6f, 1.5f) * _profile.Minimap.MarkerScale;
        for (int i = 0; i < _renderMarkers.Count; i++)
        {
            RenderMarker render = _renderMarkers[i];
            HudMinimapMarkerFrame marker = render.Frame;
            Vector2 build = render.FromBuild.Lerp(render.ToBuild, transition);
            Vector2 local = BuildToLocal(build, Size);
            float markerSize = marker.Kind switch
            {
                HudMinimapMarkerKind.GroundMobile => 4.2f,
                HudMinimapMarkerKind.TrueAir => 6.4f,
                HudMinimapMarkerKind.Structure => 6.8f,
                _ => 5.8f
            };
            if (marker.Selected) markerSize *= 1.35f;
            Color color = MarkerColor(marker);
            _channels[(int)marker.Kind].Add(local, markerSize * sizeFactor, color);
        }
        for (int i = 0; i < _channels.Length; i++) _channels[i].Finish();
    }

    private float InterpolationFactor()
    {
        float seconds = _profile.Minimap.InterpolationSeconds;
        if (seconds <= 0.0001f || _transitionStartedMs == 0) return 1f;
        return Mathf.Clamp((Time.GetTicksMsec() - _transitionStartedMs) / (seconds * 1000f), 0f, 1f);
    }

    private Color MarkerColor(HudMinimapMarkerFrame marker)
    {
        Color color = marker.Kind == HudMinimapMarkerKind.Resource
            ? Parse(_profile.Minimap.ResourceColor, new Color("d9f24b"))
            : marker.Relation switch
            {
                HudMinimapRelation.Owned => Parse(_profile.Minimap.OwnedColor, new Color("e6ad28")),
                HudMinimapRelation.Allied => Parse(_profile.Minimap.AlliedColor, new Color("65c987")),
                HudMinimapRelation.Enemy => Parse(_profile.Minimap.EnemyColor, new Color("ff6b45")),
                _ => Parse(_profile.Minimap.NeutralColor, new Color("b4bec1"))
            };
        return marker.Remembered ? new Color(color, _profile.Minimap.RememberedOpacity) : color;
    }

    private void DrawNetworkLine(HudMinimapLineFrame line)
    {
        Vector2 from = BuildToLocal(new Vector2(line.FromBuildX, line.FromBuildY), Size);
        Vector2 to = BuildToLocal(new Vector2(line.ToBuildX, line.ToBuildY), Size);
        Color color = line.Kind == HudMinimapLineKind.KnownEnemyNetwork
            ? Parse(_profile.Minimap.EnemyColor, new Color("ff6b45"))
            : Parse(_profile.Minimap.OwnedColor, new Color("e6ad28"));
        color = new Color(color, line.Operational ? 0.72f : 0.48f);
        if (line.Operational) DrawLine(from, to, color, 1.5f);
        else DrawDashedLine(from, to, color, 1.5f);
    }

    private void DrawDashedLine(Vector2 from, Vector2 to, Color color, float width)
    {
        float length = from.DistanceTo(to);
        if (length <= 0.1f) return;
        Vector2 direction = (to - from) / length;
        const float dash = 5f, gap = 3f;
        for (float distance = 0f; distance < length; distance += dash + gap)
            DrawLine(from + direction * distance, from + direction * Math.Min(length, distance + dash), color, width);
    }

    private static Color Parse(string html, Color fallback) => Color.HtmlIsValid(html) ? new Color(html) : fallback;

    private sealed partial class MinimapOverlay : Control
    {
        private HudMinimapView? _owner;
        public void Configure(HudMinimapView owner) { _owner = owner; ZIndex = 5; }

        public override void _Draw()
        {
            if (_owner is null) return;
            M7HudProfile profile = _owner._profile;
            if (profile.Minimap.ShowViewport && _owner._cameraPolygon.Count >= 3)
            {
                Vector2[] polygon = new Vector2[_owner._cameraPolygon.Count + 1];
                for (int i = 0; i < _owner._cameraPolygon.Count; i++)
                    polygon[i] = BuildToLocal(_owner._cameraPolygon[i], Size);
                polygon[^1] = polygon[0];
                DrawPolyline(polygon, Parse(profile.Minimap.ViewportColor, Colors.White), profile.Minimap.ViewportLineWidth, true);
            }
            if (!profile.Minimap.ShowAlerts) return;
            double seconds = Time.GetTicksMsec() / 1000.0;
            for (int i = 0; i < _owner._frame.Pings.Count && i < HudMinimapFrame.PingCapacity; i++)
            {
                HudMinimapPingFrame ping = _owner._frame.Pings[i];
                float phase = Mathf.PosMod(ping.Phase + (float)seconds * 0.72f, 1f);
                float radius = Mathf.Lerp(4f, 18f, phase) * profile.Minimap.AlertPulseScale;
                float alpha = 1f - phase;
                Color baseColor = ping.Priority == HudAlertPriority.Critical
                    ? Parse(profile.Minimap.EnemyColor, Colors.Red)
                    : Parse(profile.Minimap.AlertColor, Colors.Orange);
                DrawArc(BuildToLocal(new Vector2(ping.BuildX, ping.BuildY), Size), radius, 0f, Mathf.Tau, 32,
                    new Color(baseColor, alpha), Math.Max(1f, profile.Minimap.ViewportLineWidth));
            }
        }
    }
}
