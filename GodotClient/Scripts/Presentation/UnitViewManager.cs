using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public partial class UnitViewManager : Node3D
{
    internal const float ConstructionProgressHeightWorld = 1.75f;
    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private ControlGroups? _groups;
    private readonly Dictionary<uint, MeshInstance3D> _views = new();
    private readonly HashSet<uint> _live = new();
    private readonly List<uint> _remove = new();
    private readonly Dictionary<uint, uint> _seenFireSequence = new();
    private readonly Dictionary<uint, float> _fireFlashRemaining = new();
    private readonly Dictionary<uint, MeshInstance3D> _projectileViews = new();
    private readonly HashSet<uint> _liveProjectiles = new();
    private readonly List<uint> _removeProjectiles = new();
    private readonly Dictionary<uint, DebrisSeed> _pendingDebris = new();
    private readonly Dictionary<uint, DebrisVisual> _debrisViews = new();
    private readonly List<uint> _debrisScratch = new();
    private readonly List<uint> _removeDebris = new();

    private readonly StandardMaterial3D _friendly = MakeMaterial(new Color(0.10f, 0.82f, 0.66f));
    private readonly StandardMaterial3D _other = MakeMaterial(new Color(0.92f, 0.30f, 0.18f));
    private readonly StandardMaterial3D _hover = MakeMaterial(new Color(0.55f, 0.95f, 1f));
    private readonly StandardMaterial3D _resource = MakeMaterial(new Color(0.72f, 0.48f, 0.20f));
    private readonly StandardMaterial3D _resourceExhausted = MakeMaterial(new Color(0.28f, 0.25f, 0.22f));
    private readonly StandardMaterial3D _construction = MakeConstructionMaterial();
    private readonly StandardMaterial3D _brownout = MakeMaterial(new Color(0.20f, 0.22f, 0.25f));
    private readonly StandardMaterial3D _projectile = MakeProjectileMaterial();
    private readonly StandardMaterial3D _wreck = MakeMaterial(new Color(0.18f, 0.17f, 0.15f));
    private readonly StandardMaterial3D _healthGood = MakeHealthBarMaterial(new Color(0.24f, 0.82f, 0.38f, 0.96f));
    private readonly StandardMaterial3D _healthDamaged = MakeHealthBarMaterial(new Color(0.98f, 0.68f, 0.12f, 0.96f));
    private readonly StandardMaterial3D _healthCritical = MakeHealthBarMaterial(new Color(1f, 0.24f, 0.12f, 0.96f));

    public void Configure(GodotSimBridge bridge, SelectionController selection, ControlGroups groups)
    {
        _bridge = bridge; _selection = selection; _groups = groups; ProcessPriority = 100;
    }

    internal bool TryGetEntityView(EntityId id, out MeshInstance3D view)
    {
        if (_views.TryGetValue(id.Value, out MeshInstance3D? stored)) { view = stored; return true; }
        view = null!;
        return false;
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
            if (c.IsDestroyed) view.MaterialOverride = _wreck;
            Node3D? ring = view.GetNodeOrNull<Node3D>("SelectionRing");
            if (ring is not null) ring.Visible = !c.IsDestroyed && (selected || hovered);
            Node3D? targetRing = view.GetNodeOrNull<Node3D>("TargetRing");
            if (targetRing is not null) targetRing.Visible = !c.IsDestroyed && IsCurrentTarget(c.EntityId);
            UpdateHealthBar(view, c, !c.IsDestroyed && !c.IsConstructionSite &&
                (selected || IsCurrentTarget(c.EntityId) || (c.HasHealth && c.CurrentHitPointsRaw < c.MaximumHitPointsRaw)));
            if (c.IsDestroyed)
            {
                HideWeaponFeedback(view);
                UpdateDestructionView(view, c);
                _pendingDebris[c.EntityId.Value] = new DebrisSeed(c.NonBlockingDebrisTicks / (float)SimClock.TicksPerSecond, DebrisScale(c), c.PersistentDebris);
            }
            else
            {
                _pendingDebris.Remove(c.EntityId.Value);
                UpdateWeaponFlash(view, c, (float)delta);
            }
            Label3D? groupLabel = view.GetNodeOrNull<Label3D>("ControlGroupLabel");
            if (groupLabel is not null)
            {
                string membership = !c.IsDestroyed && c.Owner == 0 ? _groups.GetMembershipText(c.EntityId) : string.Empty;
                groupLabel.Text = membership;
                groupLabel.Visible = membership.Length > 0;
            }
        }
        _remove.Clear();
        foreach ((uint id, MeshInstance3D view) in _views) if (!_live.Contains(id))
        {
            if (_pendingDebris.TryGetValue(id, out DebrisSeed debris) && (debris.Seconds > 0f || debris.Persistent))
            {
                PrepareDebrisView(id, view, debris.Scale);
                _debrisViews[id] = new DebrisVisual(view, debris.Seconds, debris.Persistent);
            }
            else view.QueueFree();
            _remove.Add(id);
        }
        for (int i = 0; i < _remove.Count; i++) { uint id = _remove[i]; _views.Remove(id); _seenFireSequence.Remove(id); _fireFlashRemaining.Remove(id); }
        for (int i = 0; i < _remove.Count; i++) _pendingDebris.Remove(_remove[i]);
        UpdateProjectileViews(previous, current, alpha);
        UpdateDebrisViews((float)delta);
    }

    private void UpdateProjectileViews(PresentationSnapshot previous, PresentationSnapshot current, float alpha)
    {
        _liveProjectiles.Clear();
        for (int i = 0; i < current.Projectiles.Count; i++)
        {
            PresentationProjectile projectile = current.Projectiles[i];
            uint id = projectile.ProjectileId.Value;
            _liveProjectiles.Add(id);
            if (!_projectileViews.TryGetValue(id, out MeshInstance3D? view))
            {
                view = new MeshInstance3D
                {
                    Name = $"SimProjectile_{id}",
                    Mesh = new SphereMesh { Radius = 0.16f, Height = 0.32f, RadialSegments = 10, Rings = 5 },
                    MaterialOverride = _projectile
                };
                _projectileViews.Add(id, view);
                AddChild(view);
            }
            FixVec2 previousPosition = projectile.Position;
            for (int p = 0; p < previous.Projectiles.Count; p++)
                if (previous.Projectiles[p].ProjectileId == projectile.ProjectileId) { previousPosition = previous.Projectiles[p].Position; break; }
            view.GlobalPosition = previousPosition.ToWorld(1.05f).Lerp(projectile.Position.ToWorld(1.05f), alpha);
        }

        _removeProjectiles.Clear();
        foreach ((uint id, MeshInstance3D view) in _projectileViews)
            if (!_liveProjectiles.Contains(id)) { view.QueueFree(); _removeProjectiles.Add(id); }
        for (int i = 0; i < _removeProjectiles.Count; i++) _projectileViews.Remove(_removeProjectiles[i]);
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
        Node3D? contact = view.GetNodeOrNull<Node3D>("ContactImpact");
        bool contactDelivery = entity.WeaponDelivery == WeaponDeliveryKind.Contact;
        if (flash is not null) flash.Visible = remaining > 0f && !contactDelivery;
        if (contact is not null) contact.Visible = remaining > 0f && contactDelivery;
        if (remaining > 0f) _fireFlashRemaining[entity.EntityId.Value] = Mathf.Max(0f, remaining - delta);
    }

    private static void HideWeaponFeedback(MeshInstance3D view)
    {
        Node3D? flash = view.GetNodeOrNull<Node3D>("WeaponFlash"); if (flash is not null) flash.Visible = false;
        Node3D? contact = view.GetNodeOrNull<Node3D>("ContactImpact"); if (contact is not null) contact.Visible = false;
    }

    private static void UpdateDestructionView(MeshInstance3D view, PresentationEntity entity)
    {
        // Placeholder primitives do not pretend to be final LEGO breakup animation.
        // Gameplay timing is shown by the dark wreck state; actual flattening happens
        // only when collision clears and the view transitions to cosmetic debris.
        view.Scale = entity.DestructionProgressBasisPoints >= 10_000 ? DebrisScale(entity) : BaseVisualScale(entity);
    }

    internal static Vector3 DebrisScale(PresentationEntity entity)
    {
        Vector3 baseline = BaseVisualScale(entity);
        float horizontal = entity.DestructionKind == DestructionKind.Structure ? 0.82f : 0.72f;
        float vertical = entity.DestructionKind == DestructionKind.Structure ? 0.10f : 0.16f;
        return new Vector3(baseline.X * horizontal, baseline.Y * vertical, baseline.Z * horizontal);
    }

    internal static Vector3 BaseVisualScale(PresentationEntity entity)
    {
        if (entity.SelectableKind == SelectableKind.Building)
            return new Vector3(entity.BuildingWidth * GodotConversions.WorldUnitsPerBuildCell, 2.4f, entity.BuildingHeight * GodotConversions.WorldUnitsPerBuildCell);
        return entity.Footprint switch
        {
            FootprintClass.Tiny => new Vector3(2.0f, 1.45f, 2.0f),
            FootprintClass.Small => new Vector3(2.2f, 1.55f, 2.2f),
            FootprintClass.Medium => new Vector3(2.8f, 1.9f, 2.8f),
            FootprintClass.Large => new Vector3(4.6f, 2.6f, 4.6f),
            _ => new Vector3(4.25f, 3.0f, 4.25f)
        };
    }

    private static void PrepareDebrisView(uint id, MeshInstance3D view, Vector3 scale)
    {
        view.Name = $"Debris_{id}";
        view.Scale = scale;
        string[] hidden = { "SelectionRing", "TargetRing", "HealthBar", "ConstructionProgressBar", "ControlGroupLabel", "BrownoutLabel", "WeaponFlash", "ContactImpact" };
        for (int i = 0; i < hidden.Length; i++)
        {
            Node3D? node = view.GetNodeOrNull<Node3D>(hidden[i]);
            if (node is not null) node.Visible = false;
        }
    }

    private void UpdateDebrisViews(float delta)
    {
        _debrisScratch.Clear();
        foreach (uint id in _debrisViews.Keys) _debrisScratch.Add(id);
        _removeDebris.Clear();
        for (int i = 0; i < _debrisScratch.Count; i++)
        {
            uint id = _debrisScratch[i];
            DebrisVisual debris = _debrisViews[id];
            if (debris.Persistent) continue;
            debris.Remaining -= delta;
            if (debris.Remaining <= 0f)
            {
                debris.View.QueueFree();
                _removeDebris.Add(id);
            }
            else _debrisViews[id] = debris;
        }
        for (int i = 0; i < _removeDebris.Count; i++) _debrisViews.Remove(_removeDebris[i]);
    }

    private readonly struct DebrisSeed
    {
        public readonly float Seconds;
        public readonly Vector3 Scale;
        public readonly bool Persistent;
        public DebrisSeed(float seconds, Vector3 scale, bool persistent) { Seconds = seconds; Scale = scale; Persistent = persistent; }
    }

    private struct DebrisVisual
    {
        public readonly MeshInstance3D View;
        public readonly bool Persistent;
        public float Remaining;
        public DebrisVisual(MeshInstance3D view, float remaining, bool persistent) { View = view; Remaining = remaining; Persistent = persistent; }
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

        BoxMesh contactMesh = new() { Size = new Vector3(0.62f, 0.18f, 0.82f) };
        StandardMaterial3D contactMaterial = MakeProjectileMaterial();
        contactMaterial.AlbedoColor = new Color(1f, 0.86f, 0.28f);
        contactMaterial.Emission = new Color(1f, 0.48f, 0.06f);
        contactMesh.Material = contactMaterial;
        view.AddChild(new MeshInstance3D
        {
            Name = "ContactImpact", Mesh = contactMesh, Visible = false,
            Position = new Vector3(ringRadiusWorld * 0.72f / visualScale.X, 0.12f / visualScale.Y, 0f),
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
        view.AddChild(CreateHealthBar());
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
            progressBar.Position = new Vector3(0f, ConstructionProgressLocalY(scale.Y), 0f);
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

    internal static float ConstructionProgressLocalY(float buildingHeight) => ConstructionProgressHeightWorld / buildingHeight;

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

    private static Node3D CreateHealthBar()
    {
        Node3D bar = new()
        {
            Name = "HealthBar", Visible = false, TopLevel = true
        };
        StandardMaterial3D backgroundMaterial = MakeHealthBarMaterial(new Color(0.035f, 0.045f, 0.05f, 0.96f));
        backgroundMaterial.RenderPriority = 0;
        QuadMesh backgroundMesh = new() { Size = new Vector2(2.5f, 0.38f), Material = backgroundMaterial };
        bar.AddChild(new MeshInstance3D { Name = "Background", Mesh = backgroundMesh });
        bar.AddChild(new MeshInstance3D { Name = "Fill", Mesh = new QuadMesh { Size = new Vector2(2.32f, 0.26f) } });
        return bar;
    }

    private void UpdateHealthBar(MeshInstance3D view, PresentationEntity entity, bool visible)
    {
        Node3D? bar = view.GetNodeOrNull<Node3D>("HealthBar");
        if (bar is null) return;
        bar.GlobalPosition = view.GlobalPosition + Vector3.Up * HealthBarHeightWorld(entity);
        bar.GlobalRotation = Vector3.Zero;
        bar.Scale = Vector3.One;
        bar.Visible = entity.HasHealth && visible;
        if (!bar.Visible) return;
        MeshInstance3D? fill = bar.GetNodeOrNull<MeshInstance3D>("Fill");
        if (fill?.Mesh is not QuadMesh) return;
        float ratio = entity.MaximumHitPointsRaw <= 0 ? 0f : Mathf.Clamp((float)entity.CurrentHitPointsRaw / entity.MaximumHitPointsRaw, 0f, 1f);
        UpdateHealthBarFillGeometry(fill, ratio);
        fill.MaterialOverride = ratio >= 0.70f ? _healthGood : ratio >= 0.35f ? _healthDamaged : _healthCritical;
    }

    internal static void UpdateHealthBarFillGeometry(MeshInstance3D fill, float ratio)
    {
        if (fill.Mesh is not QuadMesh fillMesh) return;
        ratio = Mathf.Clamp(ratio, 0f, 1f);
        float width = Mathf.Max(2.32f * ratio, 0.01f);
        fillMesh.Size = new Vector2(width, 0.26f);
        fillMesh.CenterOffset = new Vector3(-1.16f + width * 0.5f, 0f, 0f);
        fill.Position = Vector3.Zero;
        fill.Scale = Vector3.One;
    }

    private static float HealthBarHeightWorld(PresentationEntity entity)
    {
        if (entity.SelectableKind == SelectableKind.Building) return BuildingScale(entity).Y + 1.12f;
        float labelHeight = entity.Footprint switch
        {
            FootprintClass.Tiny => 1.35f,
            FootprintClass.Small => 1.45f,
            FootprintClass.Medium => 1.65f,
            FootprintClass.Large => 2.05f,
            _ => 2.35f
        };
        return labelHeight + 0.38f;
    }

    private static StandardMaterial3D MakeMaterial(Color color) => new() { AlbedoColor = color, Roughness = 0.45f };
    private static StandardMaterial3D MakeProjectileMaterial() => new()
    {
        AlbedoColor = new Color(1f, 0.68f, 0.10f),
        EmissionEnabled = true,
        Emission = new Color(1f, 0.36f, 0.04f),
        EmissionEnergyMultiplier = 2.4f,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
    };
    private static StandardMaterial3D MakeOverlayMaterial(Color color) => new()
    {
        AlbedoColor = color,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        NoDepthTest = true
    };
    private static StandardMaterial3D MakeHealthBarMaterial(Color color) => new()
    {
        AlbedoColor = color,
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        NoDepthTest = true,
        BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
        RenderPriority = 1
    };
    private static StandardMaterial3D MakeConstructionMaterial() => new()
    {
        AlbedoColor = new Color(1.0f, 0.72f, 0.12f, 0.78f),
        Roughness = 0.55f,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha
    };
}
