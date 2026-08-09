using Godot;

namespace LegoSpaceRTS.Client;

public static class InputBindings
{
    public static void ConfigureDefaults()
    {
        BindKey("camera_pan_left", Key.Left);
        BindKey("camera_pan_right", Key.Right);
        BindKey("camera_pan_up", Key.Up);
        BindKey("camera_pan_down", Key.Down);
        BindKey("camera_rotate_left", Key.Comma);
        BindKey("camera_rotate_right", Key.Period);
        BindKey("camera_reset", Key.Home);
        BindKey("command_move", Key.M);
        BindKey("command_stop", Key.S);
        BindKey("command_hold", Key.H);
        BindKey("debug_open_excavatable", Key.F9);
        Key[] digits = { Key.Key0, Key.Key1, Key.Key2, Key.Key3, Key.Key4, Key.Key5, Key.Key6, Key.Key7, Key.Key8, Key.Key9 };
        for (int i = 0; i < digits.Length; i++) BindKey($"group_{i}", digits[i]);
    }

    private static void BindKey(StringName action, Key key)
    {
        if (!InputMap.HasAction(action)) InputMap.AddAction(action, 0.5f);
        if (InputMap.ActionGetEvents(action).Count > 0) return;
        InputEventKey ev = new() { PhysicalKeycode = key };
        InputMap.ActionAddEvent(action, ev);
    }
}
