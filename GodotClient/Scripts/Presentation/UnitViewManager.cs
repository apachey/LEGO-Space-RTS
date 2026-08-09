using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public partial class UnitViewManager : Node3D
{
    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private ControlGroups? _groups;
    private readonly Dictionary<uint, MeshInstance3D> _views = new();
    private readonly HashSet<uint> _live = new();
    private readonly List<uint> _remove = new();

    private readonly StandardMaterial3D _friendly = MakeMaterial(new Color(0.10f, 0.82f, 0.66f));
    private readonly StandardMaterial3D _other = MakeMaterial(new Color(0.92f, 0.30f, 0.18f));
    private readonly StandardMaterial3D _hover = MakeMaterial(new Color(0.55f, 0.95f, 1f));

    public void Configure(GodotSimBridge bridge, SelectionController selection, ControlGroups groups)
    {
        _bridge = bridge; _selection = selection; _groups = groups; ProcessPriority = 100;
    }

    public override void _Process(double delta)
    {
        if (_bridge?.Current is null || _bridge.Previous is null || _selection is null || _groups is null) return;
        PresentationSnapshot current = _bridge.Current, previous = _bridge.Previous;
        float alpha = _bridge.InterpolationAlpha;
        _live.Clear();
        for (int i = 0; i < current.Entities.Count; i++)
        {
            PresentationEntity c = current.Entities[i];
            _live.Add(c.EntityId.Value);
            if (!_views.TryGetValue(c.EntityId.Value, out MeshInstance3D? view)) { view = CreateView(c); _views.Add(c.EntityId.Value, view); AddChild(view); }
            PresentationEntity p = FindPrevious(previous, c);
            Vector3 a = p.Position.ToWorld(0.5f), b = c.Position.ToWorld(0.5f);
            view.GlobalPosition = c.Snap ? b : a.Lerp(b, alpha);
            float yawA = p.Orientation.Raw * (360f / 65536f), yawB = c.Orientation.Raw * (360f / 65536f);
            float renderedYaw = c.Snap ? yawB : Mathf.RadToDeg(Mathf.LerpAngle(Mathf.DegToRad(yawA), Mathf.DegToRad(yawB), alpha));
            view.RotationDegrees = new Vector3(0f, renderedYaw, 0f);
            bool selected = ContainsSelection(c.EntityId), hovered = _selection.Hovered == c.EntityId;
            view.MaterialOverride = hovered && !selected ? _hover : (c.Owner == 0 ? _friendly : _other);
            Node3D? ring = view.GetNodeOrNull<Node3D>("SelectionRing");
            if (ring is not null) ring.Visible = selected || hovered;
            Label3D? groupLabel = view.GetNodeOrNull<Label3D>("ControlGroupLabel");
            if (groupLabel is not null)
            {
                string membership = c.Owner == 0 ? _groups.GetMembershipText(c.EntityId) : string.Empty;
                groupLabel.Text = membership;
                groupLabel.Visible = membership.Length > 0;
            }
        }
        _remove.Clear();
        foreach ((uint id, MeshInstance3D view) in _views) if (!_live.Contains(id)) { view.QueueFree(); _remove.Add(id); }
        for (int i = 0; i < _remove.Count; i++) _views.Remove(_remove[i]);
    }

    private static PresentationEntity FindPrevious(PresentationSnapshot previous, PresentationEntity current)
    {
        for (int i = 0; i < previous.Entities.Count; i++) if (previous.Entities[i].EntityId == current.EntityId) return previous.Entities[i];
        return current;
    }
    private bool ContainsSelection(EntityId id)
    {
        if (_selection is null) return false;
        for (int i = 0; i < _selection.Selected.Count; i++) if (_selection.Selected[i] == id) return true;
        return false;
    }

    private MeshInstance3D CreateView(PresentationEntity entity)
    {
        Vector3 visualScale;
        float ringRadiusWorld;
        float labelHeightWorld;
        PrimitiveMesh mesh;
        switch (entity.Footprint)
        {
            case FootprintClass.Tiny:
                mesh = new CapsuleMesh { Radius = 0.35f, Height = 1.0f };
                visualScale = new Vector3(2.0f, 1.45f, 2.0f);
                ringRadiusWorld = 0.75f;
                labelHeightWorld = 1.35f;
                break;
            case FootprintClass.Small:
                mesh = new BoxMesh { Size = new Vector3(1f, 0.65f, 1.2f) };
                visualScale = new Vector3(2.2f, 1.55f, 2.2f);
                ringRadiusWorld = 1.15f;
                labelHeightWorld = 1.45f;
                break;
            case FootprintClass.Medium:
                mesh = new CylinderMesh { TopRadius = 0.5f, BottomRadius = 0.6f, Height = 0.7f };
                visualScale = new Vector3(2.8f, 1.9f, 2.8f);
                ringRadiusWorld = 1.75f;
                labelHeightWorld = 1.65f;
                break;
            case FootprintClass.Large:
                mesh = new BoxMesh { Size = new Vector3(1f, 0.55f, 1.2f) };
                visualScale = new Vector3(4.6f, 2.6f, 4.6f);
                ringRadiusWorld = 2.35f;
                labelHeightWorld = 2.05f;
                break;
            default:
                mesh = new CylinderMesh { TopRadius = 0.65f, BottomRadius = 0.75f, Height = 0.75f };
                visualScale = new Vector3(4.25f, 3.0f, 4.25f);
                ringRadiusWorld = 3.25f;
                labelHeightWorld = 2.35f;
                break;
        }

        MeshInstance3D view = new()
        {
            Name = $"SimEntity_{entity.EntityId.Value}_{entity.Footprint}",
            Mesh = mesh,
            Scale = visualScale
        };

        TorusMesh ringMesh = new()
        {
            InnerRadius = ringRadiusWorld * 0.82f,
            OuterRadius = ringRadiusWorld,
            Rings = 20,
            RingSegments = 40
        };
        StandardMaterial3D ringMaterial = MakeMaterial(new Color(1f, 0.92f, 0.15f));
        ringMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        ringMaterial.NoDepthTest = true;
        ringMesh.Material = ringMaterial;
        MeshInstance3D ring = new()
        {
            Name = "SelectionRing",
            Mesh = ringMesh,
            Position = new Vector3(0f, -0.42f / visualScale.Y, 0f),
            Visible = false,
            Scale = new Vector3(1f / visualScale.X, 1f / visualScale.Y, 1f / visualScale.Z)
        };
        // TorusMesh is already horizontal around the Y axis in Godot; rotating it made v0.3's ring edge-on.
        view.AddChild(ring);

        Label3D groupLabel = new()
        {
            Name = "ControlGroupLabel",
            Text = string.Empty,
            Visible = false,
            FontSize = 34,
            OutlineSize = 12,
            Modulate = new Color(1f, 0.94f, 0.25f),
            OutlineModulate = new Color(0.02f, 0.02f, 0.02f, 0.95f),
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            FixedSize = true,
            NoDepthTest = true,
            Position = new Vector3(0f, labelHeightWorld / visualScale.Y, 0f),
            Scale = new Vector3(1f / visualScale.X, 1f / visualScale.Y, 1f / visualScale.Z)
        };
        view.AddChild(groupLabel);
        return view;
    }

    private static StandardMaterial3D MakeMaterial(Color color) => new() { AlbedoColor = color, Roughness = 0.45f };
}
