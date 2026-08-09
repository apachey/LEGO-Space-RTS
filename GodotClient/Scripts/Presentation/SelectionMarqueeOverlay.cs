using Godot;

namespace LegoSpaceRTS.Presentation;

/// <summary>
/// Presentation-only RTS drag-selection rectangle. It deliberately owns no selection logic.
/// </summary>
public partial class SelectionMarqueeOverlay : Control
{
    private bool _active;
    private Vector2 _start;
    private Vector2 _end;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ZIndex = 100;
    }

    public void SetDrag(Vector2 start, Vector2 end)
    {
        _start = start;
        _end = end;
        _active = true;
        QueueRedraw();
    }

    public void HideDrag()
    {
        if (!_active) return;
        _active = false;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!_active) return;
        Rect2 rect = MakeRect(_start, _end);
        DrawRect(rect, new Color(0.12f, 0.90f, 0.72f, 0.10f), true);
        DrawRect(rect, new Color(0.25f, 1.00f, 0.82f, 0.95f), false, 2.0f);
    }

    private static Rect2 MakeRect(Vector2 a, Vector2 b) => new(
        new Vector2(Mathf.Min(a.X, b.X), Mathf.Min(a.Y, b.Y)),
        new Vector2(Mathf.Abs(b.X - a.X), Mathf.Abs(b.Y - a.Y)));
}
