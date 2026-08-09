using Godot;

namespace LegoSpaceRTS.Presentation;

public partial class RtsCameraController : Camera3D
{
    private Vector3 _focus = new(160f, 0f, 160f);
    private float _zoomCells = 44f;
    private float _pitch = 58f;
    private float _yaw = 45f;
    private Vector2? _middleDragStart;

    public override void _Ready()
    {
        Fov = 36f;
        Current = true;
        ProcessPriority = -50;
        ApplyTransform();
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        float yawRad = Mathf.DegToRad(_yaw);
        // Screen-up on the ground points from the camera toward the focus.
        // Godot's camera looks down local -Z, so this is the inverse of the
        // camera offset's horizontal direction.
        Vector3 screenForward = new(-Mathf.Sin(yawRad), 0f, -Mathf.Cos(yawRad));
        Vector3 right = new(Mathf.Cos(yawRad), 0f, -Mathf.Sin(yawRad));
        Vector3 pan = Vector3.Zero;
        if (Input.IsActionPressed("camera_pan_left")) pan -= right;
        if (Input.IsActionPressed("camera_pan_right")) pan += right;
        if (Input.IsActionPressed("camera_pan_up")) pan += screenForward;
        if (Input.IsActionPressed("camera_pan_down")) pan -= screenForward;

        Vector2 mouse = GetViewport().GetMousePosition();
        Vector2 size = GetViewport().GetVisibleRect().Size;
        const float edge = 8f;
        if (mouse.X <= edge) pan -= right; else if (mouse.X >= size.X - edge) pan += right;
        if (mouse.Y <= edge) pan += screenForward; else if (mouse.Y >= size.Y - edge) pan -= screenForward;

        float speed = Mathf.Lerp(18f, 46f, Mathf.InverseLerp(24f, 72f, _zoomCells)) * GodotConversions.WorldUnitsPerBuildCell;
        if (pan.LengthSquared() > 0.0001f) _focus += pan.Normalized() * speed * dt;

        if (Input.IsActionJustPressed("camera_rotate_left")) _yaw = Mathf.PosMod(_yaw - 90f, 360f);
        if (Input.IsActionJustPressed("camera_rotate_right")) _yaw = Mathf.PosMod(_yaw + 90f, 360f);
        if (Input.IsActionJustPressed("camera_reset")) { _yaw = 45f; _pitch = 58f; }

        if (_middleDragStart.HasValue && Input.IsMouseButtonPressed(MouseButton.Middle))
        {
            Vector2 now = mouse;
            Vector2 drag = now - _middleDragStart.Value;
            _middleDragStart = now;
            _focus += (-right * drag.X + screenForward * drag.Y) * (0.03f * _zoomCells / 44f);
        }
        else if (!Input.IsMouseButtonPressed(MouseButton.Middle)) _middleDragStart = null;

        _focus.X = Mathf.Clamp(_focus.X, -16f, 336f);
        _focus.Z = Mathf.Clamp(_focus.Z, -16f, 336f);
        ApplyTransform();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse)
        {
            if (mouse.ButtonIndex == MouseButton.Middle)
            {
                if (mouse.Pressed) _middleDragStart = mouse.Position;
                else _middleDragStart = null;
            }
            if (mouse.Pressed && mouse.ButtonIndex == MouseButton.WheelUp) AdjustWheel(+1f);
            if (mouse.Pressed && mouse.ButtonIndex == MouseButton.WheelDown) AdjustWheel(-1f);
        }
    }

    private void AdjustWheel(float direction)
    {
        if (Input.IsKeyPressed(Key.Alt)) _pitch = Mathf.Clamp(_pitch + direction * 2f, 52f, 64f);
        else _zoomCells = Mathf.Clamp(_zoomCells * (direction > 0f ? 0.92f : 1.08f), 24f, 72f);
        ApplyTransform();
    }

    public void CenterOn(Vector3 world)
    {
        _focus = new Vector3(world.X, 0f, world.Z);
        ApplyTransform();
    }

    public bool TryProjectToGround(Vector2 screen, out Vector3 point)
    {
        Vector3 origin = ProjectRayOrigin(screen);
        Vector3 direction = ProjectRayNormal(screen);
        if (Mathf.Abs(direction.Y) < 0.0001f) { point = default; return false; }
        float t = -origin.Y / direction.Y;
        if (t < 0f) { point = default; return false; }
        point = origin + direction * t;
        return true;
    }

    private void ApplyTransform()
    {
        Vector2 size = GetViewport()?.GetVisibleRect().Size ?? new Vector2(1920f, 1080f);
        float aspect = size.Y <= 1f ? 16f / 9f : size.X / size.Y;
        float halfWidthWorld = _zoomCells * GodotConversions.WorldUnitsPerBuildCell * 0.5f;
        float halfVerticalFov = Mathf.DegToRad(Fov * 0.5f);
        float distance = halfWidthWorld / Mathf.Max(0.1f, Mathf.Tan(halfVerticalFov) * aspect);
        float yawRad = Mathf.DegToRad(_yaw);
        float pitchRad = Mathf.DegToRad(_pitch);
        Vector3 offsetDirection = new(Mathf.Sin(yawRad) * Mathf.Cos(pitchRad), Mathf.Sin(pitchRad), Mathf.Cos(yawRad) * Mathf.Cos(pitchRad));
        GlobalPosition = _focus + offsetDirection * distance;
        LookAt(_focus, Vector3.Up);
    }
}
