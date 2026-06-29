using System.Globalization;
using System.Text;
using MudBlazor;

namespace Polymind.Web.Display;

/// <summary>
/// Hiển thị trực quan quốc gia của đơn hàng: cờ (emoji) + tên chuẩn + màu badge.
/// Country lưu dạng chuỗi tự do nên khớp theo từ khóa (bỏ dấu, không phân biệt hoa thường).
/// </summary>
public static class CountryDisplay
{
    public readonly record struct CountryStyle(string Flag, string Name, Color Color);

    private static readonly (string[] Keys, CountryStyle Style)[] Map =
    {
        (new[] { "nhat", "japan", "jp" },            new CountryStyle("🇯🇵", "Nhật Bản", Color.Error)),
        (new[] { "han quoc", "han", "korea", "kr" }, new CountryStyle("🇰🇷", "Hàn Quốc", Color.Info)),
        (new[] { "dai loan", "dai", "taiwan", "tw" },new CountryStyle("🇹🇼", "Đài Loan", Color.Success)),
        (new[] { "singapore", "sing", "sg" },        new CountryStyle("🇸🇬", "Singapore", Color.Warning)),
        (new[] { "duc", "german", "de" },            new CountryStyle("🇩🇪", "Đức", Color.Warning)),
        (new[] { "uc", "australia", "au" },          new CountryStyle("🇦🇺", "Úc", Color.Secondary)),
        (new[] { "rumani", "romania", "ro" },        new CountryStyle("🇷🇴", "Rumani", Color.Primary)),
        (new[] { "ba lan", "poland", "pl" },         new CountryStyle("🇵🇱", "Ba Lan", Color.Tertiary)),
        (new[] { "hungary", "hung" },                new CountryStyle("🇭🇺", "Hungary", Color.Tertiary)),
        (new[] { "trung quoc", "china", "cn" },      new CountryStyle("🇨🇳", "Trung Quốc", Color.Error)),
        (new[] { "canada", "ca" },                   new CountryStyle("🇨🇦", "Canada", Color.Error)),
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
