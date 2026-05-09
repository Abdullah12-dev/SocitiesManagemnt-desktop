using SocietiesManagementSystem.UI.Theme;

namespace SocietiesManagementSystem.UI.Controls;

public class StatCard : RoundedPanel
{
    private readonly Label _lblValue;
    private readonly Label _lblTitle;
    private readonly Label _lblIcon;

    public string Title  { get => _lblTitle.Text;  set => _lblTitle.Text  = value; }
    public string Value  { get => _lblValue.Text;  set => _lblValue.Text  = value; }
    public string Icon   { get => _lblIcon.Text;   set => _lblIcon.Text   = value; }
    public Color  AccentColor { get => _lblIcon.ForeColor; set => _lblIcon.ForeColor = value; }

    public StatCard()
    {
        BackColor   = AppTheme.CardBackground;
        ShowBorder  = true;
        BorderColor = AppTheme.Border;
        Size        = new Size(200, 110);
        Padding     = new Padding(16);

        _lblIcon = new Label
        {
            Font      = new Font("Segoe UI", 22f),
            ForeColor = AppTheme.Accent,
            AutoSize  = false,
            Size      = new Size(50, 50),
            Location  = new Point(Width - 66, 16),
            TextAlign = ContentAlignment.MiddleCenter,
            Anchor    = AnchorStyles.Top | AnchorStyles.Right
        };

        _lblValue = new Label
        {
            Font      = new Font("Segoe UI", 28f, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = false,
            Size      = new Size(120, 40),
            Location  = new Point(16, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _lblTitle = new Label
        {
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = false,
            Size      = new Size(Width - 32, 20),
            Location  = new Point(16, 64),
            TextAlign = ContentAlignment.MiddleLeft,
            Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        Controls.AddRange(new Control[] { _lblIcon, _lblValue, _lblTitle });
        Resize += (_, _) =>
        {
            _lblIcon.Location  = new Point(Width - 66, 16);
            _lblTitle.Width    = Width - 32;
            _lblTitle.Location = new Point(16, Height - 34);
        };
    }
}
