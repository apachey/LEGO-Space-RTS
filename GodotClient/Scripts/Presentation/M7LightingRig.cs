using Godot;

namespace LegoSpaceRTS.Presentation;

public static class M7LightingRig
{
    public static void AddNeutralGameplayRig(Node3D root)
    {
        DirectionalLight3D sun = new()
        {
            Name = "Sun",
            RotationDegrees = new Vector3(-58f, -35f, 0f),
            LightColor = new Color("fff3df"),
            LightEnergy = 1.2f,
            ShadowEnabled = true
        };
        root.AddChild(sun);

        Godot.Environment environment = new()
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = new Color("171b21"),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = new Color("a3a8b5"),
            AmbientLightEnergy = 0.72f,
            GlowEnabled = true,
            GlowIntensity = 0.18f,
            GlowBloom = 0.05f
        };
        root.AddChild(new WorldEnvironment { Name = "WorldEnvironment", Environment = environment });
    }
}
