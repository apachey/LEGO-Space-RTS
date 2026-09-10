using System.Runtime.InteropServices;
using Godot;

namespace LegoSpaceRTS.Client;

/// <summary>
/// Ends disposable macOS smoke-test processes without entering Godot's native
/// teardown path. Normal game sessions always use SceneTree.Quit.
/// </summary>
public static class AutomatedSmokeExit
{
    public const string ImmediateExitArgument = "--automated-smoke-immediate-exit";

    [DllImport("libSystem.B.dylib", EntryPoint = "_exit")]
    private static extern void NativeImmediateExit(int exitCode);

    public static void Finish(Node context, int exitCode)
    {
        string[] userArguments = OS.GetCmdlineUserArgs();
        string[] engineArguments = OS.GetCmdlineArgs();
        bool disposableAutomation = userArguments.Contains(ImmediateExitArgument) ||
                                    userArguments.Contains("--capture-path") ||
                                    engineArguments.Contains("--headless");
        if (OS.GetName() == "macOS" && disposableAutomation)
        {
            GD.Print($"AUTOMATED SMOKE EXIT: immediate code={exitCode}");
            Console.Out.Flush();
            Console.Error.Flush();
            NativeImmediateExit(exitCode);
            return;
        }

        context.GetTree().Quit(exitCode);
    }
}
