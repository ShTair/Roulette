using System.Text.Json.Serialization;

namespace Roulette.Models;

public class RouletteItem
{
    public string Text { get; set; } = "";
    private OklchColor? _backgroundOklch;
    private OklchColor? _foregroundOklch;
    private bool _autoForegroundColor = true;

    // Serialized as { "l": ..., "c": ..., "h": ... } in JSON
    public OklchColor? BackgroundOklch
    {
        get => _backgroundOklch;
        set
        {
            _backgroundOklch = value;
            if (_autoForegroundColor && value.HasValue)
                _foregroundOklch = ColorUtil.GetContrastOklch(value.Value);
        }
    }

    public OklchColor? ForegroundOklch
    {
        get => _foregroundOklch;
        set => _foregroundOklch = value;
    }

    // Backward compatibility: reads old hex/oklch-string BackgroundColor from JSON, never written
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BackgroundColor
    {
        get => null;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var (l, c, h) = ColorUtil.ParseOklchCss(value);
            BackgroundOklch = new OklchColor(l, c, h);
        }
    }

    // Even older legacy compatibility (very old JSON had "color" instead of "backgroundColor")
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color
    {
        get => null;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || _backgroundOklch.HasValue) return;
            var (l, c, h) = ColorUtil.ParseOklchCss(value);
            BackgroundOklch = new OklchColor(l, c, h);
        }
    }

    // Backward compatibility: reads old ForegroundColor string from JSON, never written
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ForegroundColor
    {
        get => null;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var (l, c, h) = ColorUtil.ParseOklchCss(value);
            _foregroundOklch = new OklchColor(l, c, h);
        }
    }

    // UI: CSS oklch string for inline styles (not serialized)
    [JsonIgnore]
    public string BackgroundColorCss =>
        _backgroundOklch.HasValue ? _backgroundOklch.Value.ToCss() : ColorUtil.OklchToCss(0.95, 0.05, 0);

    [JsonIgnore]
    public string ForegroundColorCss =>
        (_foregroundOklch ?? OklchColor.Black).ToCss();

    // UI: hex string for <input type="color"> (not serialized)
    [JsonIgnore]
    public string BackgroundColorHex
    {
        get => _backgroundOklch.HasValue ? _backgroundOklch.Value.ToHex() : "#F2EFE5";
        set
        {
            var (l, c, h) = ColorUtil.HexToOklch(value);
            BackgroundOklch = new OklchColor(l, c, h);
        }
    }

    [JsonIgnore]
    public string ForegroundColorHex
    {
        get => (_foregroundOklch ?? OklchColor.Black).ToHex();
        set
        {
            var (l, c, h) = ColorUtil.HexToOklch(value);
            _foregroundOklch = new OklchColor(l, c, h);
        }
    }

    public bool AutoForegroundColor
    {
        get => _autoForegroundColor;
        set
        {
            _autoForegroundColor = value;
            if (_autoForegroundColor && _backgroundOklch.HasValue)
                _foregroundOklch = ColorUtil.GetContrastOklch(_backgroundOklch.Value);
        }
    }

    public int Count { get; set; }

    public double Size { get; set; } = 1;

    public RouletteItemState State { get; set; } = RouletteItemState.Locked;

    [JsonIgnore]
    public double Weight { get; set; } = 1;

    private static readonly Random s_rand = new();

    public static RouletteItem Create(string text = "", OklchColor? baseColor = null)
    {
        return new RouletteItem
        {
            Text = text,
            BackgroundOklch = RandomOklchColor(baseColor),
            Size = 1,
            State = RouletteItemState.Locked
        };
    }

    public static OklchColor RandomOklchColor(OklchColor? baseColor = null)
    {
        double l = 0.95;
        double c = 0.05;
        if (baseColor.HasValue)
        {
            l = baseColor.Value.L;
            c = baseColor.Value.C;
        }
        return new OklchColor(l, c, s_rand.NextDouble() * 360);
    }

    // Kept for backward compatibility with string-based callers
    public static string RandomColor(string? baseColor = null)
    {
        OklchColor? base2 = null;
        if (!string.IsNullOrWhiteSpace(baseColor))
        {
            var (l, c, _) = ColorUtil.ParseOklchCss(baseColor);
            base2 = new OklchColor(l, c, 0);
        }
        return RandomOklchColor(base2).ToCss();
    }
}

public enum RouletteItemState
{
    Enabled,
    Locked,
    Disabled
}

