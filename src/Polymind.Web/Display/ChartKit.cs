using System.Globalization;

namespace Polymind.Web.Display;

/// <summary>Dữ liệu + tiện ích cho bộ biểu đồ SVG tự dựng (không phụ thuộc thư viện ngoài).</summary>
public static class ChartKit
{
    /// <summary>Bảng màu thương hiệu + phụ trợ, dùng vòng lặp theo chỉ số.</summary>
    public static readonly string[] Palette =
    {
        "#1A5FA8", "#0167D8", "#4CAF50", "#FF9800", "#9C27B0",
        "#00ACC1", "#E91E63", "#8BC34A", "#795548", "#607D8B",
    };

    public static string Color(int i) => Palette[((i % Palette.Length) + Palette.Length) % Palette.Length];

    /// <summary>Định dạng số bất biến văn hoá để toạ độ SVG luôn dùng dấu chấm thập phân.</summary>
    public static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>Rút gọn số tiền/lớn: 1.2 tỷ, 850 tr, 12 ng.</summary>
    public static string Compact(double v)
    {
        var abs = Math.Abs(v);
        if (abs >= 1_000_000_000) return (v / 1_000_000_000).ToString("0.#", CultureInfo.InvariantCulture) + " tỷ";
        if (abs >= 1_000_000) return (v / 1_000_000).ToString("0.#", CultureInfo.InvariantCulture) + " tr";
        if (abs >= 1_000) return (v / 1_000).ToString("0.#", CultureInfo.InvariantCulture) + " ng";
        return v.ToString("0", CultureInfo.InvariantCulture);
    }

    // ---- Trình dựng phần tử SVG (tránh xung đột thẻ <text> của Razor) ----
    private static string Enc(string s) => System.Net.WebUtility.HtmlEncode(s);

    public static string Text(double x, double y, string content, double size, string anchor = "start",
        string fill = "currentColor", string weight = "400") =>
        $"<text x=\"{F(x)}\" y=\"{F(y)}\" text-anchor=\"{anchor}\" font-size=\"{F(size)}\" font-weight=\"{weight}\" fill=\"{fill}\">{Enc(content)}</text>";

    public static string Line(double x1, double y1, double x2, double y2, string stroke, double w = 1, double opacity = 1) =>
        $"<line x1=\"{F(x1)}\" y1=\"{F(y1)}\" x2=\"{F(x2)}\" y2=\"{F(y2)}\" stroke=\"{stroke}\" stroke-width=\"{F(w)}\" opacity=\"{F(opacity)}\" />";

    public static string Rect(double x, double y, double w, double h, string fill, double rx = 3, double opacity = 1) =>
        $"<rect x=\"{F(x)}\" y=\"{F(y)}\" width=\"{F(Math.Max(0, w))}\" height=\"{F(Math.Max(0, h))}\" rx=\"{F(rx)}\" fill=\"{fill}\" opacity=\"{F(opacity)}\" />";

    public static string Circle(double cx, double cy, double r, string fill, string? title = null, double opacity = 1) =>
        title is null
            ? $"<circle cx=\"{F(cx)}\" cy=\"{F(cy)}\" r=\"{F(r)}\" fill=\"{fill}\" opacity=\"{F(opacity)}\" />"
            : $"<circle cx=\"{F(cx)}\" cy=\"{F(cy)}\" r=\"{F(r)}\" fill=\"{fill}\" opacity=\"{F(opacity)}\"><title>{Enc(title)}</title></circle>";

    public static string Path(string d, string fill, string stroke = "none", double strokeWidth = 0, double fillOpacity = 1) =>
        $"<path d=\"{d}\" fill=\"{fill}\" fill-opacity=\"{F(fillOpacity)}\" stroke=\"{stroke}\" stroke-width=\"{F(strokeWidth)}\" />";

    public static string Polyline(string points, string stroke, double w) =>
        $"<polyline points=\"{points}\" fill=\"none\" stroke=\"{stroke}\" stroke-width=\"{F(w)}\" stroke-linejoin=\"round\" stroke-linecap=\"round\" />";

    private static (double X, double Y) Polar(double cx, double cy, double r, double angleDeg)
    {
        var a = (angleDeg - 90) * Math.PI / 180.0;
        return (cx + r * Math.Cos(a), cy + r * Math.Sin(a));
    }

    /// <summary>Path cung tròn (pie/donut). innerR=0 → pie đặc.</summary>
    public static string Arc(double cx, double cy, double r, double innerR, double startDeg, double endDeg)
    {
        var (sx, sy) = Polar(cx, cy, r, endDeg);
        var (ex, ey) = Polar(cx, cy, r, startDeg);
        var large = (endDeg - startDeg) <= 180 ? 0 : 1;
        if (innerR <= 0)
        {
            return $"M {F(cx)} {F(cy)} L {F(sx)} {F(sy)} A {F(r)} {F(r)} 0 {large} 0 {F(ex)} {F(ey)} Z";
        }
        var (isx, isy) = Polar(cx, cy, innerR, endDeg);
        var (iex, iey) = Polar(cx, cy, innerR, startDeg);
        return $"M {F(sx)} {F(sy)} A {F(r)} {F(r)} 0 {large} 0 {F(ex)} {F(ey)} " +
               $"L {F(iex)} {F(iey)} A {F(innerR)} {F(innerR)} 0 {large} 1 {F(isx)} {F(isy)} Z";
    }
}

/// <summary>Một cột/lát: nhãn + giá trị (+ màu tuỳ chọn).</summary>
public record ChartDatum(string Label, double Value, string? Color = null, string? Caption = null);

/// <summary>Một chuỗi dữ liệu cho biểu đồ đường/kết hợp.</summary>
public record ChartLineSeries(string Name, string Color, double[] Values);

/// <summary>Điểm cho biểu đồ phân tán.</summary>
public record ChartScatterPoint(double X, double Y, string Label, string? Color = null);
