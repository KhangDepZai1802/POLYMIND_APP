using MudBlazor;

namespace Polymind.Web.Theme;

/// <summary>
/// Theme dùng chung: giữ thương hiệu navy + cyan, bo góc mềm, có bảng màu sáng và tối.
/// </summary>
public static class PolymindTheme
{
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1e3a8a",
            Secondary = "#0ea5e9",
            Tertiary = "#6366f1",
            Info = "#0284c7",
            Success = "#16a34a",
            Warning = "#f59e0b",
            Error = "#dc2626",
            Dark = "#1f2937",
            AppbarBackground = "#1e3a8a",
            AppbarText = "#ffffff",
            Background = "#f4f6fb",
            BackgroundGray = "#eef1f7",
            Surface = "#ffffff",
            DrawerBackground = "#ffffff",
            DrawerText = "#1f2937",
            DrawerIcon = "#475569",
            TextPrimary = "#1f2937",
            TextSecondary = "#6b7280",
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
            DefaultBorderRadius = "12px",
        },
    };
}
