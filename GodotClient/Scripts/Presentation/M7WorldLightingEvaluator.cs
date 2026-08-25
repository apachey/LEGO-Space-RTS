using Godot;

namespace LegoSpaceRTS.Presentation;

/// <summary>
/// Presentation-only world-light identities used by the M7 art-direction lab.
/// Values deliberately match <see cref="WorldCycleLook.Environment"/> so the
/// evaluator can consume existing schema-6 profiles without mutating them.
/// </summary>
public enum M7WorldEnvironment
{
    Earth = 0,
    Mars = 1,
    Moon = 2,
    PlanetU = 3,
    Underground = 4
}

/// <summary>
/// Artist-facing phase classification derived from signed solar altitude.
/// Dawn and dusk share a phase; <see cref="M7WorldLightingFrame.IsMorning"/>
/// and <see cref="M7WorldLightingFrame.PhaseName"/> disambiguate them.
/// </summary>
public enum M7WorldLightPhase
{
    DeepNight,
    BlueHour,
    SunriseSunset,
    GoldenHour,
    NeutralDay,
    Underground
}

/// <summary>
/// Immutable output of <see cref="M7WorldLightingEvaluator"/>. It contains no
/// gameplay state and is safe to evaluate repeatedly at render frequency.
/// </summary>
public readonly record struct M7WorldLightingFrame(
    M7WorldEnvironment Environment,
    M7WorldLightPhase Phase,
    string PhaseName,
    bool IsMorning,
    float SolarAltitude,
    float KeyAzimuth,
    float KeyElevation,
    Color KeyColor,
    float KeyEnergy,
    float FillAzimuth,
    float FillElevation,
    Color FillColor,
    float FillEnergy,
    Color AmbientColor,
    float AmbientEnergy,
    bool RimEnabled,
    Color RimColor,
    float RimEnergy,
    Color BackgroundColor,
    float KeyAngularSize,
    float ShadowBlur,
    float ShadowOpacity,
    float DaylightFactor,
    float TwilightFactor,
    float NightFactor,
    float LocalLightMultiplier,
    float FunctionalLightFactor)
{
    public bool DirectSunVisible => KeyEnergy > 0f;
}

/// <summary>
/// Pure, presentation-only solar and color-ramp evaluator for M7. It does not
/// advance time, touch Godot nodes or write back to <see cref="WorldCycleLook"/>.
/// </summary>
public static class M7WorldLightingEvaluator
{
    // Wide transition bands are intentional: the short physical instant at
    // the horizon is aesthetically valuable in an RTS and needs enough review
    // time to read as a real blue/golden hour rather than a color flash.
    private const float BlueHourAltitude = -18f;
    private const float SunriseAltitude = -4f;
    private const float GoldenHourStartAltitude = 3f;
    private const float NeutralDayAltitude = 24f;

    private static readonly WorldPalette Earth = new(
        MaxSolarAltitude: 68f,
        MaxNightDepth: 42f,
        FillElevation: 38f,
        FillAzimuthOffset: 155f,
        Stops:
        [
            Stop(-42f, "7184ad", 0f,    "48658f", 0.17f, "243757", 0.30f, "030814", "668bc4", 0.18f, 2.8f, 2.8f, 0.54f),
            Stop(-12f, "7795c8", 0f,    "587bac", 0.18f, "304f78", 0.34f, "10233f", "7aa7e0", 0.20f, 2.4f, 2.2f, 0.64f),
            Stop(-4f,  "f28a72", 0f,    "6b79a1", 0.16f, "5a5d79", 0.34f, "59384f", "91add3", 0.18f, 2.0f, 1.9f, 0.73f),
            Stop(2f,   "ffad6f", 0.32f, "7f88a5", 0.13f, "8a766f", 0.35f, "a85e49", "a9bfd5", 0.15f, 1.7f, 1.7f, 0.82f),
            Stop(8f,   "ffd099", 0.82f, "99adc0", 0.10f, "a6a6a1", 0.36f, "806d65", "b7cce0", 0.11f, 1.4f, 1.5f, 0.89f),
            Stop(15f,  "fff0d3", 1.16f, "abc8df", 0.08f, "aebdca", 0.36f, "587d96", "abc8e2", 0.08f, 1.2f, 1.3f, 0.94f),
            Stop(90f,  "fffaf0", 1.34f, "b4cce0", 0.07f, "b6c5d0", 0.36f, "6b90a8", "abc8e2", 0.06f, 1.0f, 1.1f, 0.96f)
        ]);

    private static readonly WorldPalette Mars = new(
        MaxSolarAltitude: 57f,
        MaxNightDepth: 38f,
        FillElevation: 34f,
        FillAzimuthOffset: 150f,
        Stops:
        [
            Stop(-38f, "6079ad", 0f,    "3d5180", 0.18f, "27365c", 0.31f, "090d1c", "7899d6", 0.20f, 1.8f, 1.8f, 0.50f),
            Stop(-12f, "728ac0", 0f,    "526590", 0.18f, "413f66", 0.33f, "1c1e38", "88a7dd", 0.20f, 1.5f, 1.5f, 0.62f),
            Stop(-4f,  "d77973", 0f,    "756178", 0.16f, "76515a", 0.34f, "55303b", "a3add6", 0.18f, 1.2f, 1.25f, 0.72f),
            Stop(2f,   "ff8550", 0.30f, "986f76", 0.13f, "9a6552", 0.35f, "a84830", "b8b7d4", 0.15f, 1.0f, 1.1f, 0.81f),
            Stop(8f,   "ffb06f", 0.86f, "b38c84", 0.10f, "b57a60", 0.36f, "a75d40", "c6c4dc", 0.11f, 0.85f, 0.95f, 0.88f),
            Stop(15f,  "ffdbac", 1.22f, "c6a59a", 0.08f, "bd8b72", 0.36f, "85584b", "b9c8e5", 0.08f, 0.75f, 0.86f, 0.93f),
            Stop(90f,  "fff0d2", 1.42f, "d0b5a7", 0.07f, "c58b70", 0.34f, "784f42", "aabfe5", 0.06f, 0.70f, 0.78f, 0.95f)
        ]);

    private static readonly WorldPalette Moon = new(
        MaxSolarAltitude: 52f,
        MaxNightDepth: 48f,
        FillElevation: 42f,
        FillAzimuthOffset: 165f,
        Stops:
        [
            // The Moon intentionally has no colorful atmospheric sky. Cool
            // ambient is a gameplay readability concession, not simulated air.
            Stop(-48f, "7185ad", 0f,    "3a4968", 0.14f, "263149", 0.27f, "000102", "7f9bc9", 0.17f, 0.12f, 0.18f, 0.48f),
            Stop(-12f, "8396bb", 0f,    "485979", 0.14f, "34405a", 0.29f, "010204", "91a9d2", 0.17f, 0.11f, 0.17f, 0.60f),
            Stop(-4f,  "a9b3c5", 0f,    "59667e", 0.13f, "465064", 0.29f, "020305", "9fb2d2", 0.15f, 0.10f, 0.16f, 0.72f),
            Stop(2f,   "eee6d8", 0.38f, "69758a", 0.11f, "59616d", 0.27f, "030405", "aabbd5", 0.12f, 0.09f, 0.15f, 0.84f),
            Stop(8f,   "fff4df", 1.08f, "7a8494", 0.09f, "6f747b", 0.23f, "030405", "b2c1d8", 0.09f, 0.08f, 0.14f, 0.93f),
            Stop(15f,  "fffbed", 1.52f, "89919d", 0.07f, "85888b", 0.20f, "020305", "b6c4d9", 0.06f, 0.08f, 0.13f, 0.97f),
            Stop(90f,  "fffdf4", 1.72f, "9299a3", 0.06f, "8d96a3", 0.18f, "020305", "b6c4d9", 0.05f, 0.08f, 0.12f, 0.98f)
        ]);

    private static readonly WorldPalette PlanetU = new(
        MaxSolarAltitude: 61f,
        MaxNightDepth: 36f,
        FillElevation: 40f,
        FillAzimuthOffset: 145f,
        Stops:
        [
            Stop(-36f, "6972ed", 0f,    "4855a2", 0.19f, "302d68", 0.34f, "050719", "55d4c8", 0.24f, 3.3f, 3.4f, 0.48f),
            Stop(-12f, "787ff8", 0f,    "5666bc", 0.20f, "403a82", 0.36f, "0d1030", "61e2d1", 0.25f, 3.1f, 3.1f, 0.58f),
            Stop(-4f,  "d77de1", 0f,    "657fc2", 0.19f, "67528f", 0.37f, "3c2257", "6cead2", 0.23f, 2.9f, 2.9f, 0.69f),
            Stop(2f,   "f28cda", 0.28f, "6e9bb8", 0.16f, "8b6890", 0.39f, "71365f", "72efd3", 0.20f, 2.7f, 2.7f, 0.78f),
            Stop(8f,   "f4bad9", 0.72f, "78b4b7", 0.13f, "8b8da0", 0.39f, "5e566d", "6ef0d3", 0.16f, 2.5f, 2.5f, 0.86f),
            Stop(15f,  "dff5f7", 1.04f, "89c7c3", 0.10f, "79aba9", 0.39f, "35616a", "62f0d0", 0.12f, 2.3f, 2.3f, 0.91f),
            Stop(90f,  "e7f6ff", 1.28f, "93d2ca", 0.08f, "70b5b2", 0.38f, "28535b", "62f0d0", 0.08f, 2.1f, 2.2f, 0.94f)
        ]);

    /// <summary>Evaluates the profile at its current local time.</summary>
    public static M7WorldLightingFrame Evaluate(WorldCycleLook cycle)
    {
        ArgumentNullException.ThrowIfNull(cycle);
        return Evaluate(cycle, cycle.LocalTimeHours);
    }

    /// <summary>
    /// Evaluates the profile at an explicit review time without changing the
    /// supplied profile. This overload is intended for fixed screenshot grids.
    /// </summary>
    public static M7WorldLightingFrame Evaluate(WorldCycleLook cycle, float localTimeHours)
    {
        ArgumentNullException.ThrowIfNull(cycle);
        M7WorldEnvironment environment = (M7WorldEnvironment)Math.Clamp(cycle.Environment, 0, 4);
        float readability = ClampFinite(cycle.NightReadability, 0.1f, 1f, 0.58f);
        float localLightBoost = ClampFinite(cycle.LocalLightBoost, 0.5f, 4f, 1.35f);

        if (environment == M7WorldEnvironment.Underground)
        {
            // Readability is a floor for the cave, never a multiplier that
            // makes an already-dark palette darker. Local sources still own
            // the drama, but terrain and silhouettes remain playable.
            float ambientEnergy = 0.30f + 0.26f * readability;
            float fillEnergy = 0.12f + 0.12f * readability;
            return new M7WorldLightingFrame(
                environment, M7WorldLightPhase.Underground, "Underground · fixed", false,
                -90f, 145f, -90f, Hex("536a88"), 0f,
                320f, 58f, Hex("7e9ab5"), fillEnergy,
                Hex("63768b"), ambientEnergy, true, Hex("7ac4d8"), 0.10f,
                Hex("080d12"), 0f, 4.5f, 0.42f,
                0f, 0f, 1f, localLightBoost, 1f);
        }

        WorldPalette palette = Palette(environment);
        float time = PositiveModulo(FiniteOr(localTimeHours, 12f), 24f);
        float daylightPeriod = PositiveOr(cycle.DaylightHours, 12f);
        float darknessPeriod = PositiveOr(cycle.DarknessHours, 12f);
        float daylightSpan = 24f * daylightPeriod / (daylightPeriod + darknessPeriod);
        float darknessSpan = 24f - daylightSpan;
        float sunrise = 12f - daylightSpan * 0.5f;
        float sunset = 12f + daylightSpan * 0.5f;
        bool inDaylight = time >= sunrise && time <= sunset;
        bool isMorning = time <= 12f;

        float solarAltitude;
        if (inDaylight)
        {
            float progress = (time - sunrise) / Math.Max(0.001f, daylightSpan);
            solarAltitude = MathF.Sin(progress * MathF.PI) * palette.MaxSolarAltitude;
        }
        else
        {
            float progress = time > sunset
                ? (time - sunset) / Math.Max(0.001f, darknessSpan)
                : (time + 24f - sunset) / Math.Max(0.001f, darknessSpan);
            solarAltitude = -MathF.Sin(progress * MathF.PI) * palette.MaxNightDepth;
        }

        LightingStop sample = Sample(palette.Stops, solarAltitude);
        float daylightFactor = Smooth01(InverseLerp(0f, NeutralDayAltitude, solarAltitude));
        float nightFactor = 1f - Smooth01(InverseLerp(BlueHourAltitude, NeutralDayAltitude, solarAltitude));
        float twilightFactor = TwilightFactor(solarAltitude);
        // NightReadability is a floor/boost, not a dimmer. The old evaluator
        // multiplied already-dark palette stops by 0.58, which made blue hour
        // and dawn numerically different but visually almost black.
        float readabilityBoost = Lerp(1f, 1f + readability * 1.35f, nightFactor);
        Color readabilityTarget = sample.FillColor.Lerp(Colors.White, 0.22f);
        Color readableAmbient = sample.AmbientColor.Lerp(readabilityTarget,
            nightFactor * readability * 0.80f);
        Color readableBackground = sample.BackgroundColor.Lerp(sample.AmbientColor,
            nightFactor * readability * 0.12f);
        float readableAmbientEnergy = Math.Max(sample.AmbientEnergy * readabilityBoost,
            nightFactor * (0.42f + readability * 0.24f));
        float readableFillEnergy = Math.Max(sample.FillEnergy * readabilityBoost,
            nightFactor * (0.24f + readability * 0.16f));
        float keyEnergy = solarAltitude <= 0f ? 0f : sample.KeyEnergy;
        float azimuth = PositiveModulo(time / 24f * 360f - 90f, 360f);
        M7WorldLightPhase phase = ResolvePhase(solarAltitude);
        bool airless = environment == M7WorldEnvironment.Moon;
        // Atmospheric low sun must read as a broad soft shadow, not the
        // stippled black bands produced when an almost-horizontal shadow map
        // is asked to retain midday opacity. The airless Moon keeps its hard
        // high-contrast identity.
        float shadowMaximum = airless ? 0.92f : 0.74f;
        float shadowMinimum = airless ? 0.70f : 0.42f;
        float shadowOpacity = Lerp(shadowMinimum, Math.Min(sample.ShadowOpacity, shadowMaximum), daylightFactor);
        float shadowBlur = airless
            ? sample.ShadowBlur
            : Math.Max(sample.ShadowBlur, Lerp(2.80f, 2.00f, daylightFactor));
        // Near-horizontal sunlight projects scene geometry across hundreds of
        // metres, beyond the useful shadow-map texel budget of this RTS view.
        // Fade only that technically unresolved tail; light colour and solar
        // motion remain continuous through sunrise and sunset.
        float resolvedShadowFactor = Smooth01(InverseLerp(1f, 8f, solarAltitude));
        shadowOpacity *= resolvedShadowFactor;
        // Work lamps come on gradually around civil twilight. Signals and
        // inherently luminous resources are handled independently by role.
        float functionalLightFactor = 1f - Smooth01(InverseLerp(-4f, 3f, solarAltitude));

        return new M7WorldLightingFrame(
            environment, phase, ResolvePhaseName(phase, isMorning), isMorning,
            solarAltitude, azimuth, solarAltitude, sample.KeyColor, keyEnergy,
            PositiveModulo(azimuth + palette.FillAzimuthOffset, 360f), palette.FillElevation,
            sample.FillColor, readableFillEnergy,
            readableAmbient, readableAmbientEnergy,
            true, sample.RimColor, sample.RimEnergy,
            readableBackground, sample.KeyAngularSize, shadowBlur, shadowOpacity,
            daylightFactor, twilightFactor, nightFactor,
            Lerp(1f, localLightBoost, nightFactor), functionalLightFactor);
    }

    /// <summary>
    /// Lightweight deterministic contract check that can run in a Godot smoke
    /// without constructing a scene. It validates every environment, phase
    /// finiteness, direct-sun cutoff and distinct Earth transition samples.
    /// </summary>
    public static bool ValidateDeterministic(out string error)
    {
        float[] times = [0f, 5f, 6f, 6.5f, 8f, 12f, 18f, 19f, 23f];
        for (int environmentIndex = 0; environmentIndex <= 4; environmentIndex++)
        {
            WorldCycleLook cycle = new()
            {
                Environment = environmentIndex,
                DaylightHours = environmentIndex == 3 ? 18f : 12f,
                DarknessHours = 12f,
                NightReadability = 0.58f,
                LocalLightBoost = 1.6f
            };

            foreach (float time in times)
            {
                M7WorldLightingFrame first = Evaluate(cycle, time);
                M7WorldLightingFrame second = Evaluate(cycle, time);
                if (first != second)
                    return Fail($"Non-repeatable frame for {(M7WorldEnvironment)environmentIndex} at {time:0.##}h.", out error);
                if (!IsFinite(first) || string.IsNullOrWhiteSpace(first.PhaseName))
                    return Fail($"Invalid frame for {(M7WorldEnvironment)environmentIndex} at {time:0.##}h.", out error);
                if (first.SolarAltitude <= 0f && first.KeyEnergy != 0f)
                    return Fail($"Direct sun remained active below the horizon for {(M7WorldEnvironment)environmentIndex}.", out error);
            }

            M7WorldLightingFrame midnight = Evaluate(cycle, 0f);
            M7WorldLightingFrame noon = Evaluate(cycle, 12f);
            if (environmentIndex == (int)M7WorldEnvironment.Underground)
            {
                if (midnight.Phase != M7WorldLightPhase.Underground || noon.KeyEnergy != 0f)
                    return Fail("Underground did not remain on its fixed no-sun profile.", out error);
            }
            else if (midnight.KeyEnergy != 0f || noon.KeyEnergy <= 0f || noon.SolarAltitude <= 0f)
            {
                return Fail($"Solar cutoff contract failed for {(M7WorldEnvironment)environmentIndex}.", out error);
            }
        }

        WorldCycleLook earth = new()
        {
            Environment = (int)M7WorldEnvironment.Earth,
            DaylightHours = 12f,
            DarknessHours = 12f,
            NightReadability = 0.58f,
            LocalLightBoost = 1.6f
        };
        M7WorldLightingFrame deepNight = Evaluate(earth, 0f);
        M7WorldLightingFrame blueHour = Evaluate(earth, 5f);
        M7WorldLightingFrame laterBlueHour = Evaluate(earth, 5.5f);
        M7WorldLightingFrame sunrise = Evaluate(earth, 6f);
        M7WorldLightingFrame golden = Evaluate(earth, 6.5f);
        M7WorldLightingFrame laterGolden = Evaluate(earth, 7.2f);
        M7WorldLightingFrame day = Evaluate(earth, 12f);
        if (deepNight.Phase != M7WorldLightPhase.DeepNight ||
            blueHour.Phase != M7WorldLightPhase.BlueHour ||
            laterBlueHour.Phase != M7WorldLightPhase.BlueHour ||
            sunrise.Phase != M7WorldLightPhase.SunriseSunset ||
            golden.Phase != M7WorldLightPhase.GoldenHour ||
            laterGolden.Phase != M7WorldLightPhase.GoldenHour ||
            day.Phase != M7WorldLightPhase.NeutralDay)
            return Fail("Earth review times do not traverse the expected five lighting phases.", out error);
        if (ColorDistance(deepNight.AmbientColor, blueHour.AmbientColor) < 0.08f ||
            ColorDistance(sunrise.BackgroundColor, golden.BackgroundColor) < 0.08f ||
            ColorDistance(golden.KeyColor, day.KeyColor) < 0.08f)
            return Fail("Earth multi-stop color ramps collapsed into visually indistinct samples.", out error);
        if (deepNight.LocalLightMultiplier <= day.LocalLightMultiplier)
            return Fail("Night local-light boost is not stronger than the day value.", out error);
        if (deepNight.FunctionalLightFactor != 1f || day.FunctionalLightFactor != 0f ||
            sunrise.FunctionalLightFactor <= 0f || sunrise.FunctionalLightFactor >= 1f)
            return Fail("Automatic functional-light switching did not span night, twilight and day.", out error);
        M7WorldLightingFrame underground = Evaluate(new WorldCycleLook
        {
            Environment = (int)M7WorldEnvironment.Underground,
            NightReadability = 0.58f,
            LocalLightBoost = 1.6f
        }, 12f);
        if (underground.FunctionalLightFactor != 1f)
            return Fail("Underground functional lighting must remain enabled.", out error);

        error = string.Empty;
        return true;
    }

    private static WorldPalette Palette(M7WorldEnvironment environment) => environment switch
    {
        M7WorldEnvironment.Mars => Mars,
        M7WorldEnvironment.Moon => Moon,
        M7WorldEnvironment.PlanetU => PlanetU,
        _ => Earth
    };

    private static M7WorldLightPhase ResolvePhase(float altitude) => altitude switch
    {
        < BlueHourAltitude => M7WorldLightPhase.DeepNight,
        < SunriseAltitude => M7WorldLightPhase.BlueHour,
        < GoldenHourStartAltitude => M7WorldLightPhase.SunriseSunset,
        < NeutralDayAltitude => M7WorldLightPhase.GoldenHour,
        _ => M7WorldLightPhase.NeutralDay
    };

    private static string ResolvePhaseName(M7WorldLightPhase phase, bool isMorning) => phase switch
    {
        M7WorldLightPhase.DeepNight => "Deep night",
        M7WorldLightPhase.BlueHour => isMorning ? "Blue hour · dawn" : "Blue hour · dusk",
        M7WorldLightPhase.SunriseSunset => isMorning ? "Sunrise" : "Sunset",
        M7WorldLightPhase.GoldenHour => isMorning ? "Golden hour · morning" : "Golden hour · evening",
        M7WorldLightPhase.NeutralDay => "Neutral day",
        _ => "Underground · fixed"
    };

    private static LightingStop Sample(LightingStop[] stops, float altitude)
    {
        if (altitude <= stops[0].Altitude) return stops[0];
        for (int i = 1; i < stops.Length; i++)
        {
            if (altitude > stops[i].Altitude) continue;
            LightingStop from = stops[i - 1];
            LightingStop to = stops[i];
            float weight = Smooth01(InverseLerp(from.Altitude, to.Altitude, altitude));
            return LightingStop.Lerp(from, to, weight);
        }
        return stops[^1];
    }

    private static LightingStop Stop(
        float altitude, string key, float keyEnergy, string fill, float fillEnergy,
        string ambient, float ambientEnergy, string background, string rim, float rimEnergy,
        float angularSize, float shadowBlur, float shadowOpacity) =>
        new(altitude, Hex(key), keyEnergy, Hex(fill), fillEnergy, Hex(ambient), ambientEnergy,
            Hex(background), Hex(rim), rimEnergy, angularSize, shadowBlur, shadowOpacity);

    private static Color Hex(string value) => new(value);

    private static float TwilightFactor(float altitude)
    {
        if (altitude <= BlueHourAltitude || altitude >= NeutralDayAltitude) return 0f;
        float rise = Smooth01(InverseLerp(BlueHourAltitude, 1f, altitude));
        float fall = 1f - Smooth01(InverseLerp(1f, NeutralDayAltitude, altitude));
        return Math.Min(rise, fall);
    }

    private static float PositiveModulo(float value, float modulus)
    {
        float result = value % modulus;
        return result < 0f ? result + modulus : result;
    }

    private static float PositiveOr(float value, float fallback) =>
        float.IsFinite(value) && value > 0f ? value : fallback;

    private static float FiniteOr(float value, float fallback) => float.IsFinite(value) ? value : fallback;

    private static float ClampFinite(float value, float minimum, float maximum, float fallback) =>
        Math.Clamp(FiniteOr(value, fallback), minimum, maximum);

    private static float InverseLerp(float from, float to, float value) =>
        Math.Clamp((value - from) / Math.Max(0.0001f, to - from), 0f, 1f);

    private static float Smooth01(float value) => value * value * (3f - 2f * value);

    private static float Lerp(float from, float to, float weight) => from + (to - from) * weight;

    private static float ColorDistance(Color a, Color b)
    {
        float red = a.R - b.R;
        float green = a.G - b.G;
        float blue = a.B - b.B;
        return MathF.Sqrt(red * red + green * green + blue * blue);
    }

    private static bool IsFinite(M7WorldLightingFrame frame) =>
        float.IsFinite(frame.SolarAltitude) && float.IsFinite(frame.KeyAzimuth) &&
        float.IsFinite(frame.KeyElevation) && float.IsFinite(frame.KeyEnergy) &&
        float.IsFinite(frame.FillAzimuth) && float.IsFinite(frame.FillElevation) &&
        float.IsFinite(frame.FillEnergy) && float.IsFinite(frame.AmbientEnergy) &&
        float.IsFinite(frame.RimEnergy) && float.IsFinite(frame.KeyAngularSize) &&
        float.IsFinite(frame.ShadowBlur) && float.IsFinite(frame.ShadowOpacity) &&
        float.IsFinite(frame.DaylightFactor) && float.IsFinite(frame.TwilightFactor) &&
        float.IsFinite(frame.NightFactor) && float.IsFinite(frame.LocalLightMultiplier) &&
        float.IsFinite(frame.FunctionalLightFactor) && frame.FunctionalLightFactor is >= 0f and <= 1f &&
        IsFinite(frame.KeyColor) && IsFinite(frame.FillColor) && IsFinite(frame.AmbientColor) &&
        IsFinite(frame.RimColor) && IsFinite(frame.BackgroundColor);

    private static bool IsFinite(Color color) =>
        float.IsFinite(color.R) && float.IsFinite(color.G) && float.IsFinite(color.B) && float.IsFinite(color.A);

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }

    private readonly record struct WorldPalette(
        float MaxSolarAltitude,
        float MaxNightDepth,
        float FillElevation,
        float FillAzimuthOffset,
        LightingStop[] Stops);

    private readonly record struct LightingStop(
        float Altitude,
        Color KeyColor,
        float KeyEnergy,
        Color FillColor,
        float FillEnergy,
        Color AmbientColor,
        float AmbientEnergy,
        Color BackgroundColor,
        Color RimColor,
        float RimEnergy,
        float KeyAngularSize,
        float ShadowBlur,
        float ShadowOpacity)
    {
        public static LightingStop Lerp(LightingStop from, LightingStop to, float weight) => new(
            LerpScalar(from.Altitude, to.Altitude, weight),
            from.KeyColor.Lerp(to.KeyColor, weight),
            LerpScalar(from.KeyEnergy, to.KeyEnergy, weight),
            from.FillColor.Lerp(to.FillColor, weight),
            LerpScalar(from.FillEnergy, to.FillEnergy, weight),
            from.AmbientColor.Lerp(to.AmbientColor, weight),
            LerpScalar(from.AmbientEnergy, to.AmbientEnergy, weight),
            from.BackgroundColor.Lerp(to.BackgroundColor, weight),
            from.RimColor.Lerp(to.RimColor, weight),
            LerpScalar(from.RimEnergy, to.RimEnergy, weight),
            LerpScalar(from.KeyAngularSize, to.KeyAngularSize, weight),
            LerpScalar(from.ShadowBlur, to.ShadowBlur, weight),
            LerpScalar(from.ShadowOpacity, to.ShadowOpacity, weight));

        private static float LerpScalar(float from, float to, float weight) => from + (to - from) * weight;
    }
}
