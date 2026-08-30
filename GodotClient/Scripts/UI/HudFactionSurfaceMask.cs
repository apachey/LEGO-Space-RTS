using Godot;

namespace LegoSpaceRTS.UI;

/// <summary>
/// Owns the faction-specific interior aperture below a top strip or command
/// deck. The mask is derived from the transparent center component of the
/// actual frame texture, then transformed with the exact same nine-slice
/// guides as the visible frame. This keeps the plate, raster and HUD content
/// inside the authored opening instead of clipping them to a generic rectangle.
/// </summary>
public partial class HudFactionSurfaceMask : Control
{
    private const byte TransparentThreshold = 48;
    private const string ApertureShaderPath = "res://Shaders/hud_faction_aperture.gdshader";
    private static readonly Dictionary<string, Texture2D> MaskCache = new(StringComparer.Ordinal);

    private readonly Dictionary<CanvasItem, Material?> _registeredSurfaceMaterials = new();
    private HudFactionSkinRecipe _recipe = HudFactionSkinLibrary.RecipeFor(0);
    private Texture2D? _frameTexture;
    private Texture2D? _maskTexture;
    private Texture2D? _gutterMaskTexture;
    private ShaderMaterial? _apertureMaterial;
    private HudArtFinish _finish = HudArtFinish.HybridConsole;
    private float _chromeScale = 1f;
    private float _panelOpacity = 1f;
    private Color _surfaceColor = new("171b1a");
    private int _fallbackPadding;

    public HudFactionChromeRole Role { get; private set; }
    public HudFaction Faction => _recipe.Faction;
    public bool IsConfigured => _frameTexture is not null && _maskTexture is not null &&
        _gutterMaskTexture is not null &&
        _apertureMaterial?.Shader is not null;
    public bool UsesFactionApertureMask =>
        _finish is HudArtFinish.HybridConsole or HudArtFinish.LegacyFrames && IsConfigured;
    public bool UsesSharedNineSliceGeometry => true;
    public bool UsesShaderApertureMask => UsesFactionApertureMask &&
        ClipChildren == ClipChildrenMode.Disabled;
    public bool ClipsRasterAndContent => UsesFactionApertureMask;
    public bool UsesShapedApertureCorners => _recipe.ApertureCornerRadiusFraction > 0f;
    public bool UsesFullBleedBottomDeck => Role == HudFactionChromeRole.BottomDeck;
    public Color SurfaceFillColor => new(_surfaceColor, _panelOpacity);
    public int RegisteredMaskedSurfaceCount => _registeredSurfaceMaterials.Count;

    public HudFactionSurfaceMask()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = true;
        ClipChildren = ClipChildrenMode.Disabled;
        TextureFilter = TextureFilterEnum.Linear;
        SetNotifyTransform(true);
    }

    public void Configure(int faction, HudFactionChromeRole role, HudArtFinish finish,
        float chromeScale, Color surfaceColor, float panelOpacity, int fallbackPadding)
    {
        _recipe = HudFactionSkinLibrary.RecipeFor(faction);
        Role = role;
        _finish = finish;
        _chromeScale = Mathf.Clamp(chromeScale, 0.75f, 1.35f);
        _surfaceColor = surfaceColor;
        _panelOpacity = Mathf.Clamp(panelOpacity, 0f, 1f);
        _fallbackPadding = Math.Max(0, fallbackPadding);
        string framePath = _finish == HudArtFinish.LegacyFrames
            ? _recipe.LegacyFramePath
            : _recipe.HybridFramePath;
        _frameTexture = ResourceLoader.Exists(framePath)
            ? GD.Load<Texture2D>(framePath)
            : null;
        _maskTexture = _frameTexture is null
            ? null
            : GetOrCreateMask(framePath, _frameTexture, _recipe,
                _recipe.ApertureCornerRadiusFraction);
        _gutterMaskTexture = _frameTexture is null
            ? null
            : GetOrCreateMask(framePath, _frameTexture, _recipe, 0f);
        EnsureApertureMaterial();
        ClipChildren = ClipChildrenMode.Disabled;
        Material = null;
        ApplyRegisteredSurfaceMaterials();
        UpdateApertureMaterialParameters();
        QueueRedraw();
    }

    /// <summary>
    /// Applies the aperture shader only to an edge surface that can meet the
    /// outer frame. Masking the whole UI subtree makes Godot reinterpret GUI
    /// primitive colors; inner controls already live inside readable margins.
    /// </summary>
    public void RegisterMaskedSurface(CanvasItem surface)
    {
        if (!_registeredSurfaceMaterials.TryAdd(surface, surface.Material)) return;
        surface.Material = UsesFactionApertureMask ? _apertureMaterial : surface.Material;
    }

    public void RegisterMaskedSurfaceTree(CanvasItem surface)
    {
        RegisterMaskedSurface(surface);
        foreach (Node child in surface.GetChildren())
        {
            if (child is CanvasItem canvasChild) RegisterMaskedSurfaceTree(canvasChild);
            else RegisterCanvasDescendants(child);
        }
    }

    public Rect2 ContentRectFor(Vector2 destinationSize)
    {
        // Lower-deck surfaces deliberately run under the authored frame. The
        // per-faction alpha aperture is the boundary; introducing a second
        // rectangular inset leaves a visible square minimap corner inside the
        // sculpted frame. Functional children retain their own safe margins.
        if (Role == HudFactionChromeRole.BottomDeck)
            return new Rect2(Vector2.Zero, destinationSize);

        if (_frameTexture is null || destinationSize.X <= 2f || destinationSize.Y <= 2f)
        {
            float fallback = Math.Min(_fallbackPadding,
                Math.Max(0f, Math.Min(destinationSize.X, destinationSize.Y) * 0.25f));
            return new Rect2(new Vector2(fallback, fallback),
                new Vector2(Math.Max(0f, destinationSize.X - fallback * 2f),
                    Math.Max(0f, destinationSize.Y - fallback * 2f)));
        }

        float corner = HudFactionChrome.DestinationCornerSize(_recipe, Role, destinationSize, _chromeScale);
        Vector4 ratios = _recipe.ApertureInsetRatios;
        // Top-strip labels clear the illustrated rail. The lower deck takes
        // the full-bleed branch above because its section controls provide
        // their own readable margins beneath the visible frame.
        const float horizontalFactor = 0.82f;
        const float topFactor = 0.60f;
        const float bottomFactor = 0.60f;
        float breathingRoom = 1f;
        float left = Mathf.Round(corner * ratios.X * horizontalFactor + breathingRoom);
        float top = Mathf.Round(corner * ratios.Y * topFactor + breathingRoom);
        float right = Mathf.Round(corner * ratios.Z * horizontalFactor + breathingRoom);
        float bottom = Mathf.Round(corner * ratios.W * bottomFactor + breathingRoom);
        float width = destinationSize.X - left - right;
        float height = destinationSize.Y - top - bottom;
        if (width < 8f || height < 8f)
        {
            float fallback = Math.Min(_fallbackPadding,
                Math.Max(0f, Math.Min(destinationSize.X, destinationSize.Y) * 0.2f));
            return new Rect2(new Vector2(fallback, fallback),
                new Vector2(Math.Max(0f, destinationSize.X - fallback * 2f),
                    Math.Max(0f, destinationSize.Y - fallback * 2f)));
        }
        return new Rect2(new Vector2(left, top), new Vector2(width, height));
    }

    public override void _Notification(int what)
    {
        if (what == (int)NotificationResized || what == (int)NotificationTransformChanged ||
            what == (int)NotificationEnterCanvas)
        {
            UpdateApertureMaterialParameters();
            if (what == (int)NotificationResized) QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (!UsesFactionApertureMask || _maskTexture is null || _gutterMaskTexture is null ||
            Size.X < 4f || Size.Y < 4f) return;
        int sourceCorner = HudFactionChrome.SourceCornerSize(_maskTexture, _recipe);
        float destinationCorner = HudFactionChrome.DestinationCornerSize(_recipe, Role, Size, _chromeScale);
        // The generated frame PNGs leave a square transparent opening behind
        // their sculpted corners. A darker aperture-derived gutter prevents
        // the game world from shining through the deliberately round plate.
        DrawNineSlice(_gutterMaskTexture, new Rect2(Vector2.Zero, Size), sourceCorner,
            destinationCorner, new Color(_surfaceColor.Darkened(0.58f), Math.Max(0.96f, _panelOpacity)));
        DrawNineSlice(_maskTexture, new Rect2(Vector2.Zero, Size), sourceCorner, destinationCorner,
            new Color(_surfaceColor, _panelOpacity));
    }

    private void EnsureApertureMaterial()
    {
        if (_apertureMaterial?.Shader is not null) return;
        Shader? shader = ResourceLoader.Exists(ApertureShaderPath)
            ? GD.Load<Shader>(ApertureShaderPath)
            : null;
        if (shader is not null) _apertureMaterial = new ShaderMaterial { Shader = shader };
    }

    private void UpdateApertureMaterialParameters()
    {
        if (_apertureMaterial is null || _maskTexture is null || !IsInsideTree() ||
            Size.X < 2f || Size.Y < 2f) return;
        Vector2 viewportSize = GetViewportRect().Size;
        if (viewportSize.X < 2f || viewportSize.Y < 2f) return;
        Rect2 global = GetGlobalRect();
        _apertureMaterial.SetShaderParameter("aperture_mask", _maskTexture);
        _apertureMaterial.SetShaderParameter("mask_source_size",
            new Vector2(_maskTexture.GetWidth(), _maskTexture.GetHeight()));
        _apertureMaterial.SetShaderParameter("mask_source_corner",
            (float)HudFactionChrome.SourceCornerSize(_maskTexture, _recipe));
        _apertureMaterial.SetShaderParameter("mask_screen_rect", new Vector4(
            global.Position.X / viewportSize.X, global.Position.Y / viewportSize.Y,
            global.Size.X / viewportSize.X, global.Size.Y / viewportSize.Y));
        _apertureMaterial.SetShaderParameter("mask_destination_size", Size);
        _apertureMaterial.SetShaderParameter("mask_destination_corner",
            HudFactionChrome.DestinationCornerSize(_recipe, Role, Size, _chromeScale));
    }

    private void ApplyRegisteredSurfaceMaterials()
    {
        foreach ((CanvasItem surface, Material? original) in _registeredSurfaceMaterials)
            surface.Material = UsesFactionApertureMask ? _apertureMaterial : original;
    }

    private void RegisterCanvasDescendants(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is CanvasItem canvasChild) RegisterMaskedSurfaceTree(canvasChild);
            else RegisterCanvasDescendants(child);
        }
    }

    private void DrawNineSlice(Texture2D texture, Rect2 destination, float sourceCorner,
        float destinationCorner, Color modulate)
    {
        float[] sourceX = { 0f, sourceCorner, texture.GetWidth() - sourceCorner, texture.GetWidth() };
        float[] sourceY = { 0f, sourceCorner, texture.GetHeight() - sourceCorner, texture.GetHeight() };
        float[] destinationX = { destination.Position.X, destination.Position.X + destinationCorner,
            destination.End.X - destinationCorner, destination.End.X };
        float[] destinationY = { destination.Position.Y, destination.Position.Y + destinationCorner,
            destination.End.Y - destinationCorner, destination.End.Y };

        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            Vector2 sourceSize = new(sourceX[x + 1] - sourceX[x], sourceY[y + 1] - sourceY[y]);
            Vector2 destinationPatchSize = new(destinationX[x + 1] - destinationX[x],
                destinationY[y + 1] - destinationY[y]);
            if (sourceSize.X <= 0f || sourceSize.Y <= 0f ||
                destinationPatchSize.X <= 0f || destinationPatchSize.Y <= 0f) continue;
            DrawTextureRectRegion(texture,
                new Rect2(new Vector2(destinationX[x], destinationY[y]), destinationPatchSize),
                new Rect2(new Vector2(sourceX[x], sourceY[y]), sourceSize), modulate);
        }
    }

    private static Texture2D? GetOrCreateMask(string path, Texture2D frameTexture,
        HudFactionSkinRecipe recipe, float cornerRadiusFraction)
    {
        string cacheKey = $"{path}|{cornerRadiusFraction:0.000}";
        if (MaskCache.TryGetValue(cacheKey, out Texture2D? cached)) return cached;
        Image sourceImage = frameTexture.GetImage();
        if (sourceImage.IsEmpty()) return null;
        if (sourceImage.IsCompressed() && sourceImage.Decompress() != Error.Ok) return null;
        sourceImage.Convert(Image.Format.Rgba8);
        int width = sourceImage.GetWidth();
        int height = sourceImage.GetHeight();
        byte[] source = sourceImage.GetData();
        int pixelCount = checked(width * height);
        if (source.Length < pixelCount * 4) return null;

        int center = (height / 2) * width + width / 2;
        if (source[center * 4 + 3] > TransparentThreshold) return null;
        byte[] aperture = new byte[pixelCount];
        int[] queue = new int[pixelCount];
        int read = 0;
        int write = 0;
        queue[write++] = center;
        aperture[center] = 1;
        int minX = width / 2;
        int maxX = minX;
        int minY = height / 2;
        int maxY = minY;
        bool touchesExterior = false;
        while (read < write)
        {
            int index = queue[read++];
            int x = index % width;
            int y = index / width;
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
            if (x == 0 || y == 0 || x == width - 1 || y == height - 1) touchesExterior = true;
            Visit(index - 1, x > 0);
            Visit(index + 1, x + 1 < width);
            Visit(index - width, y > 0);
            Visit(index + width, y + 1 < height);
        }
        if (touchesExterior || write < pixelCount / 20) return null;

        int sourceCorner = HudFactionChrome.SourceCornerSize(frameTexture, recipe);
        if (cornerRadiusFraction > 0.001f)
        {
            int apertureRadius = Math.Max(2,
                Mathf.RoundToInt(sourceCorner * cornerRadiusFraction));
            apertureRadius = Math.Min(apertureRadius,
                Math.Max(2, Math.Min(maxX - minX + 1, maxY - minY + 1) / 3));
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                int index = y * width + x;
                if (aperture[index] != 0 &&
                    !InsideRoundedRect(x, y, minX, minY, maxX, maxY, apertureRadius))
                    aperture[index] = 0;
            }
        }

        int overlap = Math.Max(1, Mathf.RoundToInt(sourceCorner * 0.07f));
        byte[] horizontal = new byte[pixelCount];
        for (int y = 0; y < height; y++)
        {
            int row = y * width;
            int count = 0;
            for (int x = 0; x <= Math.Min(overlap, width - 1); x++) count += aperture[row + x];
            for (int x = 0; x < width; x++)
            {
                horizontal[row + x] = count > 0 ? (byte)1 : (byte)0;
                int remove = x - overlap;
                int add = x + overlap + 1;
                if (remove >= 0) count -= aperture[row + remove];
                if (add < width) count += aperture[row + add];
            }
        }

        byte[] rgba = new byte[pixelCount * 4];
        for (int x = 0; x < width; x++)
        {
            int count = 0;
            for (int y = 0; y <= Math.Min(overlap, height - 1); y++) count += horizontal[y * width + x];
            for (int y = 0; y < height; y++)
            {
                int index = y * width + x;
                if (count > 0)
                {
                    int target = index * 4;
                    rgba[target] = rgba[target + 1] = rgba[target + 2] = rgba[target + 3] = 255;
                }
                int remove = y - overlap;
                int add = y + overlap + 1;
                if (remove >= 0) count -= horizontal[remove * width + x];
                if (add < height) count += horizontal[add * width + x];
            }
        }

        Image maskImage = Image.CreateFromData(width, height, false, Image.Format.Rgba8, rgba);
        maskImage.GenerateMipmaps();
        Texture2D mask = ImageTexture.CreateFromImage(maskImage);
        MaskCache[cacheKey] = mask;
        return mask;

        void Visit(int candidate, bool inBounds)
        {
            if (!inBounds || aperture[candidate] != 0 || source[candidate * 4 + 3] > TransparentThreshold) return;
            aperture[candidate] = 1;
            queue[write++] = candidate;
        }
    }

    private static bool InsideRoundedRect(int x, int y, int minX, int minY,
        int maxX, int maxY, int radius)
    {
        int nearestX = Math.Clamp(x, minX + radius, maxX - radius);
        int nearestY = Math.Clamp(y, minY + radius, maxY - radius);
        long dx = x - nearestX;
        long dy = y - nearestY;
        return dx * dx + dy * dy <= (long)radius * radius;
    }
}
