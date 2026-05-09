using System.Drawing.Drawing2D;
using SocietiesManagementSystem.UI.Theme;

namespace SocietiesManagementSystem.UI.Controls;

public enum ButtonStyle { Primary, Accent, Success, Danger, Outline, Ghost }

public class ModernButton : Button
{
    private ButtonStyle _style = ButtonStyle.Primary;
    private bool _isHovered;
    private bool _isPressed;

    public ButtonStyle ButtonStyle
    {
        get => _style;
        set { _style = value; Invalidate(); }
    }

    public ModernButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Font   = AppTheme.FontButton;
        Height = AppTheme.ButtonHeight;
        Cursor = Cursors.Hand;

        SetStyle(
            ControlStyles.UserPaint            |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);
    }

    protected override void OnMouseEnter(EventArgs e) { _isHovered = true;  Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _isPressed = true;  Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e)   { _isPressed = false; Invalidate(); base.OnMouseUp(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        var (bg, fg, border) = GetColors();

        // For non-solid styles resolve the real background instead of transparent,
        // which avoids the "parent paint bleeding through" artifact.
        if (bg == Color.Transparent)
            bg = Parent?.BackColor ?? SystemColors.Control;

        if (_isPressed)       bg = ControlPaint.Dark(bg, 0.08f);
        else if (_isHovered)  bg = ControlPaint.Light(bg, 0.06f);

        using var path  = RoundedRect(rect, AppTheme.CornerRadius);
        using var brush = new SolidBrush(bg);
        e.Graphics.FillPath(brush, path);

        if (_style == ButtonStyle.Outline || _style == ButtonStyle.Ghost)
        {
            using var pen = new Pen(border, 1.5f);
            e.Graphics.DrawPath(pen, path);
        }

        var sf = new StringFormat
        {
            Alignment     = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags   = StringFormatFlags.NoWrap
        };

        using var textBrush = new SolidBrush(fg);
        e.Graphics.DrawString(Text, Font, textBrush, new RectangleF(0, 0, Width, Height), sf);
    }

    private (Color bg, Color fg, Color border) GetColors() => _style switch
    {
        ButtonStyle.Accent  => (AppTheme.Accent,   Color.White,            AppTheme.Accent),
        ButtonStyle.Success => (AppTheme.Success,  Color.White,            AppTheme.Success),
        ButtonStyle.Danger  => (AppTheme.Danger,   Color.White,            AppTheme.Danger),
        ButtonStyle.Outline => (Color.Transparent, AppTheme.Primary,       AppTheme.Primary),
        ButtonStyle.Ghost   => (Color.Transparent, AppTheme.TextSecondary, AppTheme.Border),
        _                   => (AppTheme.Primary,  Color.White,            AppTheme.Primary)
    };

    private static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(r.X,         r.Y,          d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
        path.AddArc(r.X,         r.Bottom - d, d, d,  90, 90);
        path.CloseFigure();
        return path;
    }
}
