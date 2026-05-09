using System.Drawing.Drawing2D;
using SocietiesManagementSystem.UI.Theme;

namespace SocietiesManagementSystem.UI.Controls;

public class RoundedPanel : Panel
{
    public int   CornerRadius { get; set; } = AppTheme.CornerRadius;
    public Color BorderColor  { get; set; } = AppTheme.Border;
    public bool  ShowBorder   { get; set; } = true;

    public RoundedPanel()
    {
        SetStyle(
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint  |
            ControlStyles.ResizeRedraw          |
            ControlStyles.UserPaint,
            true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = GetRoundedPath(rect, CornerRadius);
        using var brush = new SolidBrush(BackColor);
        e.Graphics.FillPath(brush, path);
        if (ShowBorder)
        {
            using var pen = new Pen(BorderColor, 1);
            e.Graphics.DrawPath(pen, path);
        }
    }

    private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X,          rect.Y,          d, d, 180, 90);
        path.AddArc(rect.Right - d,  rect.Y,          d, d, 270, 90);
        path.AddArc(rect.Right - d,  rect.Bottom - d, d, d,   0, 90);
        path.AddArc(rect.X,          rect.Bottom - d, d, d,  90, 90);
        path.CloseFigure();
        return path;
    }
}
