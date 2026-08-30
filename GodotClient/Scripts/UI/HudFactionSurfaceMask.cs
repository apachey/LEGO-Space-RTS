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
    private static readonly Dictionary<string, Texture2D> MaskCache = new(StringComparer.Ordinal);

    private readonly ColorRect _surfaceFill;
    private HudFactionSkinRecipe _recipe = HudFactionSkinLibrary.RecipeFor(0);
    private Texture2D? _frameTexture;
    private Texture2D? _maskTexture;
    private HudArtFinish _finish = HudArtFinish.HybridConsole;
    private float _chromeScale = 1f;
    private float _panelOpacity = 1f;
    private Color _surfaceColor = new("171b1a");
    private int _fallbackPadding;

    public HudFactionChromeRole Role { get; private set; }
    public HudFaction Faction => _recipe.Faction;
    public bool IsConfigured => _frameTexture is not null && _maskTexture is not null;
    public bool UsesFactionApertureMask =>
        _finish is HudArtFinish.HybridConsole or HudArtFinish.LegacyFrames && IsConfigured;
    public bool UsesSharedNineSliceGeometry => true;
    public bool ClipsRasterAndContent => UsesFactionApertureMask;

    public HudFactionSurfaceMask()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = true;
        ClipChildren = ClipChildrenMode.Disabled;
        TextureFilter = TextureFilterEnum.Linear;
        _surfaceFill = new ColorRect
        {
            Name = "FactionSurfaceFill",
            MouseFilter = MouseFilterEnum.Ignore,
            ZIndex = -100
        };
        AddChild(_surfaceFill);
        _surfaceFill.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
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
            : GetOrCreateMask(framePath, _frameTexture, _recipe);
        ClipChildren = UsesFactionApertureMask
            ? ClipChildrenMode.Only
            : ClipChildrenMode.Disabled;
        _surfaceFill.Visible = UsesFactionApertureMask;
        _surfaceFill.Color = new Color(_surfaceColor, _panelOpacity);
        QueueRedraw();
    }

    public Rect2 ContentRectFor(Vector2 destinationSize)
    {
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
        bool topStrip = Role == HudFactionChromeRole.TopStrip;
        // The alpha aperture owns the visual plate boundary. Content needs a
        // smaller ergonomic inset so mixed-selection cards and the canonical
        // minimap target are not shrunk by the full illustrated frame width.
        float horizontalFactor = topStrip ? 0.50f : 0.28f;
        // Bottom content may extend beneath the illustrated rail because the
        // exact alpha aperture clips it safely. Keeping this inset shallow
        // preserves the canonical command-card height at 21:9.
        float verticalFactor = topStrip ? 0.50f : 0.08f;
        float breathingRoom = topStrip ? 0.5f : 1f;
        float left = Mathf.Round(corner * ratios.X * horizontalFactor + breathingRoom);
        float top = Mathf.Round(corner * ratios.Y * verticalFactor + breathingRoom);
        float right = Mathf.Round(corner * ratios.Z * horizontalFactor + breathingRoom);
        float bottom = Mathf.Round(corner * ratios.W * verticalFactor + breathingRoom);
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
        if (what == NotificationResized) QueueRedraw();
    }

    public override void _Draw()
    {
        if (!UsesFactionApertureMask || _maskTexture is null || Size.X < 4f || Size.Y < 4f) return;
        int sourceCorner = HudFactionChrome.SourceCornerSize(_maskTexture, _recipe);
        float destinationCorner = HudFactionChrome.DestinationCornerSize(_recipe, Role, Size, _chromeScale);
        DrawNineSlice(_maskTexture, new Rect2(Vector2.Zero, Size), sourceCorner, destinationCorner,
            Colors.White);
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
        HudFactionSkinRecipe recipe)
    {
        if (MaskCache.TryGetValue(path, out Texture2D? cached)) return cached;
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
        bool touchesExterior = false;
        while (read < write)
        {
            int index = queue[read++];
            int x = index % width;
            int y = index / width;
            if (x == 0 || y == 0 || x == width - 1 || y == height - 1) touchesExterior = true;
            Visit(index - 1, x > 0);
            Visit(index + 1, x + 1 < width);
            Visit(index - width, y > 0);
            Visit(index + width, y + 1 < height);
        }
        if (touchesExterior || write < pixelCount / 20) return null;

        int sourceCorner = HudFactionChrome.SourceCornerSize(frameTexture, recipe);
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
        MaskCache[path] = mask;
        return mask;

        void Visit(int candidate, bool inBounds)
        {
            if (!inBounds || aperture[candidate] != 0 || source[candidate * 4 + 3] > TransparentThreshold) return;
            aperture[candidate] = 1;
            queue[write++] = candidate;
        }
    }
}
