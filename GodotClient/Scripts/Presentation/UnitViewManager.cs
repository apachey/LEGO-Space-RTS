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
    private readonly Dictionary<uint, uint> _seenFireSequence = new();
    private readonly Dictionary<uint, float> _fireFlashRemaining = new();

    private readonly StandardMaterial3D _friendly = MakeMaterial(new Color(0.10f, 0.82f, 0.66f));
    private readonly StandardMaterial3D _other = MakeMaterial(new Color(0.92f, 0.30f, 0.18f));
    private readonly StandardMaterial3D _hover = MakeMaterial(new Color(0.55f, 0.95f, 1f));
    private readonly StandardMaterial3D _resource = MakeMaterial(new Color(0.72f, 0.48f, 0.20f));
    private readonly StandardMaterial3D _resourceExhausted = MakeMaterial(new Color(0.28f, 0.25f, 0.22f));
    private readonly StandardMaterial3D _construction = MakeConstructionMaterial();
    private readonly StandardMaterial3D _brownout = MakeMaterial(new Color(0.20f, 0.22f, 0.25f));

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
            if (c.SelectableKind == SelectableKind.Building) UpdateBuildingView(view, c);
            if (c.SelectableKind == SelectableKind.ResourceNode)
            {
                view.MaterialOverride = c.ResourceState == ResourceVisualState.Exhausted ? _resourceExhausted : _resource;
                view.Scale = ResourceScale(c.Footprint, c.ResourceState);
            }
            else if (c.SelectableKind == SelectableKind.Building && c.IsConstructionSite) view.MaterialOverride = _construction;
            else if (c.SelectableKind == SelectableKind.Building && c.IsEnergyConsumer && !c.IsPowered) view.MaterialOverride = _brownout;
            else view.MaterialOverride = hovered && !selected ? _hover : (c.Owner == 0 ? _friendly : _other);
            Node3D? ring = view.GetNodeOrNull<Node3D>("SelectionRing");
            if (ring is not null) ring.Visible = selected || hovered;
            Node3D? targetRing = view.GetNodeOrNull<Node3D>("TargetRing");
            if (targetRing is not null) targetRing.Visible = IsCurrentTarget(c.EntityId);
            UpdateWeaponFlash(view, c, (float)delta);
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
        for (int i = 0; i < _remove.Count; i++) { uint id = _remove[i]; _views.Remove(id); _seenFireSequence.Remove(id); _fireFlashRemaining.Remove(id); }
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
    private bool IsCurrentTarget(EntityId id)
    {
        if (_selection is null || _bridge is null) return false;
        for (int i = 0; i < _selection.Selected.Count; i++)
            if (_bridge.World.Entities.Targeting.TryGet(_selection.Selected[i], out Targeting targeting) && targeting.CurrentTarget == id) return true;
        return false;
    }

    private void UpdateWeaponFlash(MeshInstance3D view, PresentationEntity entity, float delta)
    {
        if (!_seenFireSequence.TryGetValue(entity.EntityId.Value, out uint seen)) _seenFireSequence[entity.EntityId.Value] = entity.WeaponFireSequence;
        else if (seen != entity.WeaponFireSequence)
        {
            _seenFireSequence[entity.EntityId.Value] = entity.WeaponFireSequence;
            _fireFlashRemaining[entity.EntityId.Value] = 0.14f;
        }
        float remaining = _fireFlashRemaining.TryGetValue(entity.EntityId.Value, out float value) ? value : 0f;
        Node3D? flash = view.GetNodeOrNull<Node3D>("WeaponFlash");
        if (flash is not null) flash.Visible = remaining > 0f;
        if (remaining > 0f) _fireFlashRemaining[entity.EntityId.Value] = Mathf.Max(0f, remaining - delta);
    }

    private MeshInstance3D CreateView(PresentationEntity entity)
    {
        Vector3 visualScale;
        float ringRadiusWorld;
        float labelHeightWorld;
        PrimitiveMesh mesh;
        if (entity.SelectableKind == SelectableKind.Building && entity.BuildingWidth > 0 && entity.BuildingHeight > 0)
        {
            mesh = new BoxMesh { Size = new Vector3(1f, 1f, 1f) };
            visualScale = BuildingScale(entity);
            ringRadiusWorld = Mathf.Max(entity.BuildingWidth, entity.BuildingHeight) * GodotConversions.WorldUnitsPerBuildCell * 0.55f;
            labelHeightWorld = entity.IsConstructionSite ? 0.8f : 2.8f;
        }
        else if (entity.SelectableKind == SelectableKind.ResourceNode)
        {
            mesh = new SphereMesh { Radius = 0.65f, Height = 1.1f, RadialSegments = 12, Rings = 6 };
            visualScale = ResourceScale(entity.Footprint, entity.ResourceState);
            ringRadiusWorld = entity.Footprint switch { FootprintClass.Medium => 1.6f, FootprintClass.Large => 2.1f, FootprintClass.Huge => 2.8f, _ => 1.2f };
            labelHeightWorld = 1.5f;
        }
        else switch (entity.Footprint)
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

        TorusMesh targetRingMesh = new()
        {
            InnerRadius = ringRadiusWorld * 0.92f,
            OuterRadius = ringRadiusWorld * 1.12f,
            Rings = 20,
            RingSegments = 40
        };
        StandardMaterial3D targetRingMaterial = MakeMaterial(new Color(1f, 0.20f, 0.08f));
        targetRingMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        targetRingMaterial.NoDepthTest = true;
        targetRingMesh.Material = targetRingMaterial;
        MeshInstance3D targetRing = new()
        {
            Name = "TargetRing",
            Mesh = targetRingMesh,
            Position = new Vector3(0f, -0.40f / visualScale.Y, 0f),
            Visible = false,
            Scale = new Vector3(1f / visualScale.X, 1f / visualScale.Y, 1f / visualScale.Z)
        };
        view.AddChild(targetRing);

        SphereMesh flashMesh = new() { Radius = 0.22f, Height = 0.44f, RadialSegments = 10, Rings = 5 };
        StandardMaterial3D flashMaterial = MakeMaterial(new Color(1f, 0.72f, 0.16f));
        flashMaterial.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        flashMesh.Material = flashMaterial;
        view.AddChild(new MeshInstance3D
        {
            Name = "WeaponFlash", Mesh = flashMesh, Visible = false,
            Position = new Vector3(0f, 0.75f / visualScale.Y, 0f),
            Scale = new Vector3(1f / visualScale.X, 1f / visualScale.Y, 1f / visualScale.Z)
        });

        Label3D groupLabel = new()
        {
            Name = "ControlGroupLabel",
            Text = string.Empty,
            Visible = false,
            FontSize = 22,
            OutlineSize = 3,
            PixelSize = 0.03f,
            Modulate = new Color(1f, 0.94f, 0.25f),
            OutlineModulate = new Color(0.02f, 0.02f, 0.02f, 0.95f),
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            FixedSize = false,
            NoDepthTest = true,
            Position = new Vector3(0f, labelHeightWorld / visualScale.Y, 0f),
            Scale = new Vector3(1f / visualScale.X, 1f / visualScale.Y, 1f / visualScale.Z)
        };
        view.AddChild(groupLabel);
        view.AddChild(CreateConstructionProgressBar());
        Label3D brownoutLabel = new()
        {
            Name = "BrownoutLabel", Text = "⚡ BROWNOUT", Visible = false, FontSize = 22, OutlineSize = 3, PixelSize = 0.03f,
            Modulate = new Color(1f, 0.42f, 0.12f), OutlineModulate = new Color(0.02f, 0.02f, 0.02f, 0.95f),
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, FixedSize = false, NoDepthTest = true
        };
        view.AddChild(brownoutLabel);
        return view;
    }

    private static Vector3 BuildingScale(PresentationEntity entity)
    {
        float progress = Mathf.Clamp(entity.ConstructionProgressBasisPoints / 10000f, 0f, 1f);
        float height = entity.IsConstructionSite ? Mathf.Lerp(0.35f, 2.4f, progress) : 2.4f;
        return new Vector3(entity.BuildingWidth * GodotConversions.WorldUnitsPerBuildCell, height, entity.BuildingHeight * GodotConversions.WorldUnitsPerBuildCell);
    }

    private static void UpdateBuildingView(MeshInstance3D view, PresentationEntity entity)
    {
        Vector3 scale = BuildingScale(entity);
        view.Scale = scale;
        Node3D? ring = view.GetNodeOrNull<Node3D>("SelectionRing");
        if (ring is not null)
        {
            ring.Position = new Vector3(0f, -0.42f / scale.Y, 0f);
            ring.Scale = new Vector3(1f / scale.X, 1f / scale.Y, 1f / scale.Z);
        }
        Node3D? targetRing = view.GetNodeOrNull<Node3D>("TargetRing");
        if (targetRing is not null)
        {
            targetRing.Position = new Vector3(0f, -0.40f / scale.Y, 0f);
            targetRing.Scale = new Vector3(1f / scale.X, 1f / scale.Y, 1f / scale.Z);
        }
        Node3D? progressBar = view.GetNodeOrNull<Node3D>("ConstructionProgressBar");
        if (progressBar is not null)
        {
            progressBar.Visible = entity.IsConstructionSite;
            progressBar.Position = new Vector3(0f, (scale.Y + 0.55f) / scale.Y, 0f);
            progressBar.Scale = new Vector3(1f / scale.X, 1f / scale.Y, 1f / scale.Z);
            MeshInstance3D? fill = progressBar.GetNodeOrNull<MeshInstance3D>("Fill");
            if (fill is not null)
            {
                float progress = Mathf.Clamp(entity.ConstructionProgressBasisPoints / 10000f, 0f, 1f);
                float width = 2.32f * progress;
                fill.Scale = new Vector3(Mathf.Max(width, 0.01f), 0.08f, 0.24f);
                fill.Position = new Vector3(-1.16f + width * 0.5f, 0.06f, 0f);
            }
        }
        Label3D? brownoutLabel = view.GetNodeOrNull<Label3D>("BrownoutLabel");
        if (brownoutLabel is not null)
        {
            brownoutLabel.Visible = entity.IsEnergyConsumer && !entity.IsPowered;
            brownoutLabel.Position = new Vector3(0f, (scale.Y + 0.8f) / scale.Y, 0f);
            brownoutLabel.Scale = new Vector3(1f / scale.X, 1f / scale.Y, 1f / scale.Z);
        }
    }

    private static Vector3 ResourceScale(FootprintClass size, ResourceVisualState state)
    {
        Vector3 baseline = size switch
        {
            FootprintClass.Medium => new Vector3(2.6f, 1.7f, 2.2f),
            FootprintClass.Large => new Vector3(3.4f, 2.2f, 2.9f),
            FootprintClass.Huge => new Vector3(4.4f, 2.8f, 3.7f),
            _ => new Vector3(2.0f, 1.3f, 1.7f)
        };
        float multiplier = state switch
        {
            ResourceVisualState.Reduced => 0.86f,
            ResourceVisualState.Low => 0.70f,
            ResourceVisualState.Critical => 0.52f,
            ResourceVisualState.Exhausted => 0.30f,
            _ => 1.0f
        };
        return baseline * multiplier;
    }

    private static Node3D CreateConstructionProgressBar()
    {
        Node3D bar = new() { Name = "ConstructionProgressBar", Visible = false };
        BoxMesh backgroundMesh = new() { Size = Vector3.One, Material = MakeOverlayMaterial(new Color(0.04f, 0.06f, 0.07f, 0.88f)) };
        bar.AddChild(new MeshInstance3D { Name = "Background", Mesh = backgroundMesh, Scale = new Vector3(2.5f, 0.06f, 0.36f) });
        BoxMesh fillMesh = new() { Size = Vector3.One, Material = MakeOverlayMaterial(new Color(0.96f, 0.66f, 0.10f, 0.94f)) };
        bar.AddChild(new MeshInstance3D { Name = "Fill", Mesh = fillMesh, Scale = new Vector3(0.01f, 0.08f, 0.24f), Position = new Vector3(-1.155f, 0.06f, 0f) });
        return bar;
    }

    private static StandardMaterial3D MakeMaterial(Color color) => new() { AlbedoColor = color, Roughness = 0.45f };
    private static StandardMaterial3D MakeOverlayMaterial(Color color) => new()
    {
        AlbedoColor = color,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        NoDepthTest = true
    };
    private static StandardMaterial3D MakeConstructionMaterial() => new()
    {
        AlbedoColor = new Color(1.0f, 0.72f, 0.12f, 0.78f),
        Roughness = 0.55f,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha
    };
}
