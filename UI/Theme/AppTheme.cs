namespace SocietiesManagementSystem.UI.Theme;

public static class AppTheme
{
    // Colors
    public static readonly Color Primary        = Color.FromArgb(30, 58, 95);
    public static readonly Color PrimaryLight   = Color.FromArgb(41, 82, 136);
    public static readonly Color Accent         = Color.FromArgb(0, 180, 216);
    public static readonly Color AccentHover    = Color.FromArgb(0, 150, 186);
    public static readonly Color Background     = Color.FromArgb(245, 247, 250);
    public static readonly Color CardBackground = Color.White;
    public static readonly Color SidebarBg      = Color.FromArgb(22, 43, 72);
    public static readonly Color SidebarHover   = Color.FromArgb(41, 65, 105);
    public static readonly Color TextPrimary    = Color.FromArgb(33, 37, 41);
    public static readonly Color TextSecondary  = Color.FromArgb(108, 117, 125);
    public static readonly Color TextLight      = Color.White;
    public static readonly Color Success        = Color.FromArgb(40, 167, 69);
    public static readonly Color Warning        = Color.FromArgb(255, 193, 7);
    public static readonly Color Danger         = Color.FromArgb(220, 53, 69);
    public static readonly Color Border         = Color.FromArgb(222, 226, 230);
    public static readonly Color InputBg        = Color.FromArgb(248, 249, 250);

    // Fonts
    public static readonly Font FontTitle    = new("Segoe UI", 22f, FontStyle.Bold);
    public static readonly Font FontH1       = new("Segoe UI", 16f, FontStyle.Bold);
    public static readonly Font FontH2       = new("Segoe UI", 13f, FontStyle.Bold);
    public static readonly Font FontBody     = new("Segoe UI", 10f);
    public static readonly Font FontSmall    = new("Segoe UI", 9f);
    public static readonly Font FontButton   = new("Segoe UI", 10f, FontStyle.Bold);
    public static readonly Font FontSidebar  = new("Segoe UI", 10f);
    public static readonly Font FontBadge    = new("Segoe UI", 9f, FontStyle.Bold);
    public static readonly Font FontInput    = new("Segoe UI", 10f);

    // Sizes
    public const int SidebarWidth    = 220;
    public const int TopBarHeight    = 60;
    public const int CardPadding     = 20;
    public const int CornerRadius    = 8;
    public const int ButtonHeight    = 38;

    // Status badge colors
    public static Color GetStatusColor(string status) => status switch
    {
        "Active"    or "Approved" or "Completed" or "Attended" => Success,
        "Pending"   or "InProgress" or "Registered"            => Warning,
        "Suspended" or "Rejected"  or "Cancelled" or "Absent"  => Danger,
        _                                                       => TextSecondary
    };

    public static Color GetPriorityColor(string priority) => priority switch
    {
        "High"   => Danger,
        "Normal" => Accent,
        "Low"    => TextSecondary,
        _        => TextSecondary
    };
}
