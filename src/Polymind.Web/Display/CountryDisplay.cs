using System.Globalization;
using System.Text;
using MudBlazor;

namespace Polymind.Web.Display;

/// <summary>
/// Hiển thị trực quan quốc gia của đơn hàng: cờ + tên chuẩn + màu badge.
/// Country lưu dạng chuỗi tự do nên khớp theo từ khóa (bỏ dấu, không phân biệt hoa thường).
/// Cờ dùng ảnh SVG local (wwwroot/img/flags) vì emoji cờ trên Windows chỉ hiện chữ viết tắt (JP, TW...).
/// </summary>
public static class CountryDisplay
{
    public readonly record struct CountryStyle(string Flag, string Name, Color Color, string? Iso = null)
    {
        /// <summary>Đường dẫn ảnh cờ SVG local; null = không có cờ (render icon quả cầu thay thế).</summary>
        public string? FlagUrl => Iso is null ? null : $"img/flags/{Iso}.svg";
    }

    private static readonly (string[] Keys, CountryStyle Style)[] Map =
    {
        (new[] { "nhat", "japan", "jp" },            new CountryStyle("🇯🇵", "Nhật Bản", Color.Error, "jp")),
        (new[] { "han quoc", "han", "korea", "kr" }, new CountryStyle("🇰🇷", "Hàn Quốc", Color.Info, "kr")),
        (new[] { "dai loan", "dai", "taiwan", "tw" },new CountryStyle("🇹🇼", "Đài Loan", Color.Success, "tw")),
        (new[] { "singapore", "sing", "sg" },        new CountryStyle("🇸🇬", "Singapore", Color.Warning, "sg")),
        (new[] { "viet nam", "vietnam", "vn" },      new CountryStyle("🇻🇳", "Việt Nam", Color.Error, "vn")),
        (new[] { "duc", "german", "de" },            new CountryStyle("🇩🇪", "Đức", Color.Warning, "de")),
        (new[] { "uc", "australia", "au" },          new CountryStyle("🇦🇺", "Úc", Color.Secondary, "au")),
        (new[] { "rumani", "romania", "ro" },        new CountryStyle("🇷🇴", "Rumani", Color.Primary, "ro")),
        (new[] { "ba lan", "poland", "pl" },         new CountryStyle("🇵🇱", "Ba Lan", Color.Tertiary, "pl")),
        (new[] { "hungary", "hung" },                new CountryStyle("🇭🇺", "Hungary", Color.Tertiary, "hu")),
        (new[] { "trung quoc", "china", "cn" },      new CountryStyle("🇨🇳", "Trung Quốc", Color.Error, "cn")),
        (new[] { "canada", "ca" },                   new CountryStyle("🇨🇦", "Canada", Color.Error, "ca")),
    };

    private static readonly CountryStyle Fallback = new("🌐", "", Color.Primary);

    public static CountryStyle Of(string? country)
    {
        if (string.IsNullOrWhiteSpace(country)) return Fallback with { Name = "Chưa rõ" };
        var needle = Normalize(country);
        foreach (var (keys, style) in Map)
        {
            if (keys.Any(k => needle.Contains(k)))
                return style;
        }
        return Fallback with { Name = country };
    }

    private static string Normalize(string s)
    {
        var lower = s.Trim().ToLowerInvariant();
        var decomposed = lower.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).Replace("đ", "d");
    }
}
