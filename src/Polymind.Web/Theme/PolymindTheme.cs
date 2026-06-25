using MudBlazor;

namespace Polymind.Web.Theme;

/// <summary>
/// Theme dùng chung: giữ thương hiệu navy + cyan, bo góc mềm, có bảng màu sáng và tối.
/// </summary>
public static class PolymindTheme
{
    public static readonly MudTheme Default = new()
    {
        // Bộ nhận diện thương hiệu POLYMIND × VIETGROUP: xanh dương + nền trắng, tươi sáng.
        PaletteLight = new PaletteLight
        {
            Primary = "#1A5FA8",        // xanh thương hiệu chính
            Secondary = "#0167D8",      // xanh sáng nhấn
            Tertiary = "#0D3F78",       // navy đậm
            Info = "#0167D8",
            Success = "#16a34a",
            Warning = "#f59e0b",
            Error = "#dc2626",
            Dark = "#0D3F78",
            AppbarBackground = "#0D3F78", // top bar navy đậm, chữ trắng
            AppbarText = "#ffffff",
            Background = "#ffffff",       // nền trắng (yêu cầu user)
            BackgroundGray = "#EFF5FC",   // tint xanh rất nhạt thay xám
            Surface = "#ffffff",
            DrawerBackground = "#ffffff",
            DrawerText = "#0D3F78",
            DrawerIcon = "#1A5FA8",
            ActionDefault = "#1A5FA8",
            PrimaryContrastText = "#ffffff",
            TextPrimary = "#10243f",      // navy gần đen, đọc rõ trên trắng
            TextSecondary = "#5b6b80",
            LinesDefault = "#D1E4FA",     // viền nhạt tông thương hiệu
            TableLines = "#E3EEFB",
            HoverOpacity = 0.06,
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#3b82f6",
            Secondary = "#22d3ee",
            Tertiary = "#818cf8",
            Info = "#38bdf8",
            Success = "#22c55e",
            Warning = "#fbbf24",
            Error = "#f87171",
            Dark = "#0b1220",
            AppbarBackground = "#0f172a",
            AppbarText = "#e5e7eb",
            Background = "#0f172a",
            BackgroundGray = "#111827",
            Surface = "#1e293b",
            DrawerBackground = "#111827",
            DrawerText = "#cbd5e1",
            DrawerIcon = "#94a3b8",
            TextPrimary = "#e5e7eb",
            TextSecondary = "#94a3b8",
            Divider = "#334155",
            ActionDefault = "#94a3b8",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "16px",
        },
    };
}
