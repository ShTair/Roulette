using System;
using System.Globalization;

namespace Roulette.Models;

public static class ColorUtil
{
    public static string OklchToHex(double l, double c, double h)
    {
        var hr = Math.PI * h / 180.0;
        var a = Math.Cos(hr) * c;
        var b = Math.Sin(hr) * c;

        var l_ = l + 0.3963377774 * a + 0.2158037573 * b;
        var m_ = l - 0.1055613458 * a - 0.0638541728 * b;
        var s_ = l - 0.0894841775 * a - 1.2914855480 * b;

        var l3 = l_ * l_ * l_;
        var m3 = m_ * m_ * m_;
        var s3 = s_ * s_ * s_;

        var r = +4.0767416621 * l3 - 3.3077115913 * m3 + 0.2309699292 * s3;
        var g = -1.2684380046 * l3 + 2.6097574011 * m3 - 0.3413193965 * s3;
        var b2 = -0.0041960863 * l3 - 0.7034186147 * m3 + 1.7076147010 * s3;

        double srgb(double x) => x <= 0.0031308 ? 12.92 * x : 1.055 * Math.Pow(x, 1.0 / 2.4) - 0.055;

        var rr = srgb(r);
        var gg = srgb(g);
        var bb = srgb(b2);

        int clamp(double v) => (int)Math.Round(Math.Clamp(v, 0, 1) * 255);

        return $"#{clamp(rr):X2}{clamp(gg):X2}{clamp(bb):X2}";
    }

    public static (double l, double c, double h) HexToOklch(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return (0.8, 0.1, 0);
        if (hex.StartsWith('#')) hex = hex[1..];
        if (hex.Length != 6) return (0.8, 0.1, 0);

        var r = Convert.ToInt32(hex.Substring(0, 2), 16) / 255.0;
        var g = Convert.ToInt32(hex.Substring(2, 2), 16) / 255.0;
        var b = Convert.ToInt32(hex.Substring(4, 2), 16) / 255.0;

        double lin(double x) => x <= 0.04045 ? x / 12.92 : Math.Pow((x + 0.055) / 1.055, 2.4);
        r = lin(r); g = lin(g); b = lin(b);

        var l = 0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b;
        var m = 0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b;
        var s = 0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b;

        var l_ = Math.Cbrt(l);
        var m_ = Math.Cbrt(m);
        var s_ = Math.Cbrt(s);

        var L = 0.2104542553 * l_ + 0.7936177850 * m_ - 0.0040720468 * s_;
        var a = 1.9779984951 * l_ - 2.4285922050 * m_ + 0.4505937099 * s_;
        var b2 = 0.0259040371 * l_ + 0.7827717662 * m_ - 0.8086757660 * s_;

        var C = Math.Sqrt(a * a + b2 * b2);
        var H = Math.Atan2(b2, a) * 180.0 / Math.PI;
        if (H < 0) H += 360.0;

        return (L, C, H);
    }

    public static string GetContrastColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return "#000000";
        hex = hex.TrimStart('#');
        if (hex.Length == 3)
        {
            hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
        }
        if (hex.Length != 6) return "#000000";

        var r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        var g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        var b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

        var brightness = (r * 299 + g * 587 + b * 114) / 1000;
        return brightness > 128 ? "#000000" : "#ffffff";
    }
}

