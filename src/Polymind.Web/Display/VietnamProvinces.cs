using System.Globalization;
using System.Text;

namespace Polymind.Web.Display;

/// <summary>
/// Danh sách 34 đơn vị hành chính cấp tỉnh của Việt Nam sau sáp nhập 2025
/// (6 thành phố trực thuộc trung ương + 28 tỉnh), hiệu lực từ 01/7/2025.
/// Dùng cho dropdown nhập tỉnh/thành ở form Lead/Ứng viên.
/// </summary>
public static class VietnamProvinces
{
    /// <summary>Sắp theo thứ tự Bắc → Nam (thành phố trực thuộc TW lên đầu).</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        // Thành phố trực thuộc trung ương
        "Hà Nội",
        "Hồ Chí Minh",
        "Hải Phòng",
        "Đà Nẵng",
        "Cần Thơ",
        "Huế",
        // Tỉnh
        "Cao Bằng",
        "Lạng Sơn",
        "Lai Châu",
        "Điện Biên",
        "Sơn La",
        "Lào Cai",
        "Tuyên Quang",
        "Thái Nguyên",
        "Phú Thọ",
        "Bắc Ninh",
        "Hưng Yên",
        "Ninh Bình",
        "Quảng Ninh",
        "Thanh Hóa",
        "Nghệ An",
        "Hà Tĩnh",
        "Quảng Trị",
        "Quảng Ngãi",
        "Gia Lai",
        "Khánh Hòa",
        "Đắk Lắk",
        "Lâm Đồng",
        "Đồng Nai",
        "Tây Ninh",
        "Vĩnh Long",
        "Đồng Tháp",
        "An Giang",
        "Cà Mau",
    };

    /// <summary>Tìm theo chuỗi nhập (không phân biệt hoa thường, không phân biệt dấu). Rỗng = trả toàn bộ.</summary>
    public static IEnumerable<string> Search(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return All;
        var needle = Normalize(value);
        return All.Where(p => Normalize(p).Contains(needle));
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
