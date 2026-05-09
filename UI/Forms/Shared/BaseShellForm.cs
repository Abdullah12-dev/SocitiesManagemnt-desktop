using SocietiesManagementSystem.UI.Theme;

namespace SocietiesManagementSystem.UI.Forms.Shared;

/// <summary>
/// Shell form providing a sidebar + content area used by all three dashboards.
/// </summary>
public class BaseShellForm : Form
{
    protected Panel SidebarPanel  { get; private set; } = null!;
    protected Panel ContentPanel  { get; private set; } = null!;
    protected Panel TopBar        { get; private set; } = null!;
    protected Label LblPageTitle  { get; private set; } = null!;

    private readonly List<(Panel item, string page)> _navItems = new();
    private Panel? _activeItem;

    protected void BuildShell(string appTitle, string userName, string role)
    {
        DoubleBuffered  = true;
        Size            = new Size(1280, 760);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize     = new Size(1100, 640);
        BackColor       = AppTheme.Background;
        Text            = $"SMS — {role}";

        // ── Sidebar ──────────────────────────────────────────────
        SidebarPanel = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = AppTheme.SidebarWidth,
            BackColor = AppTheme.SidebarBg
        };

        var logo = new Panel { Location = new Point(0, 0), Size = new Size(AppTheme.SidebarWidth, 70), BackColor = AppTheme.Primary };
        var lblLogo = new Label
        {
            Text      = "SMS",
            Font      = new Font("Segoe UI", 20f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = true,
            Location  = new Point(20, 18)
        };
        var lblLogoSub = new Label
        {
            Text      = "Societies Platform",
            Font      = AppTheme.FontSmall,
            ForeColor = Color.FromArgb(160, 200, 240),
            AutoSize  = true,
            Location  = new Point(20, 48)
        };
        logo.Controls.AddRange(new Control[] { lblLogo, lblLogoSub });

        var userCard = new Panel
        {
            Location  = new Point(0, 70),
            Size      = new Size(AppTheme.SidebarWidth, 70),
            BackColor = Color.FromArgb(15, 35, 60)
        };
        var avatar = new Panel
        {
            Size      = new Size(36, 36),
            Location  = new Point(14, 17),
            BackColor = AppTheme.Accent
        };
        var lblAv = new Label
        {
            Text      = userName.Length > 0 ? userName[0].ToString().ToUpper() : "?",
            Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = false,
            Size      = new Size(36, 36),
            TextAlign = ContentAlignment.MiddleCenter
        };
        avatar.Controls.Add(lblAv);
        var lblUserName = new Label
        {
            Text      = userName,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = false,
            Size      = new Size(140, 20),
            Location  = new Point(58, 18),
            AutoEllipsis = true
        };
        var lblRole = new Label
        {
            Text      = role,
            Font      = AppTheme.FontSmall,
            ForeColor = Color.FromArgb(160, 200, 240),
            AutoSize  = true,
            Location  = new Point(58, 38)
        };
        userCard.Controls.AddRange(new Control[] { avatar, lblUserName, lblRole });

        SidebarPanel.Controls.AddRange(new Control[] { logo, userCard });

        // ── Top bar ──────────────────────────────────────────────
        TopBar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = AppTheme.TopBarHeight,
            BackColor = Color.White
        };

        var topBorder = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 1,
            BackColor = AppTheme.Border
        };

        LblPageTitle = new Label
        {
            Text      = appTitle,
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(20, 18)
        };

        var btnLogout = new Button
        {
            Text      = "Logout",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.Danger,
            FlatStyle = FlatStyle.Flat,
            AutoSize  = true,
            Cursor    = Cursors.Hand,
            Anchor    = AnchorStyles.Top | AnchorStyles.Right
        };
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.Click += (_, _) =>
        {
            if (MessageBox.Show("Log out?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Close();
            }
        };

        TopBar.Controls.AddRange(new Control[] { LblPageTitle, topBorder, btnLogout });
        TopBar.Resize += (_, _) => btnLogout.Location = new Point(TopBar.Width - btnLogout.Width - 16, 20);

        // ── Content panel ─────────────────────────────────────────
        ContentPanel = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppTheme.Background,
            Padding   = new Padding(20)
        };

        Controls.AddRange(new Control[] { ContentPanel, TopBar, SidebarPanel });
    }

    private readonly List<(Panel item, Action activate, Action onClick)> _navEntries = new();

    protected Panel AddNavItem(string text, int yPos, Action onClick)
    {
        var item = new Panel
        {
            Location  = new Point(0, yPos),
            Size      = new Size(AppTheme.SidebarWidth, 44),
            BackColor = Color.Transparent,
            Cursor    = Cursors.Hand
        };

        var dot = new Panel
        {
            Size      = new Size(3, 24),
            Location  = new Point(0, 10),
            BackColor = Color.Transparent
        };

        var lbl = new Label
        {
            Text      = text,
            Font      = AppTheme.FontSidebar,
            ForeColor = Color.FromArgb(180, 210, 240),
            AutoSize  = false,
            Size      = new Size(AppTheme.SidebarWidth - 20, 44),
            Location  = new Point(18, 0),
            TextAlign = ContentAlignment.MiddleLeft
        };

        item.Controls.AddRange(new Control[] { dot, lbl });

        void Hover(bool on)
        {
            if (item == _activeItem) return;
            item.BackColor = on ? AppTheme.SidebarHover : Color.Transparent;
        }

        void Activate()
        {
            if (_activeItem != null)
            {
                _activeItem.BackColor = Color.Transparent;
                ((Panel)_activeItem.Controls[0]).BackColor = Color.Transparent;
                ((Label)_activeItem.Controls[1]).ForeColor = Color.FromArgb(180, 210, 240);
                ((Label)_activeItem.Controls[1]).Font      = AppTheme.FontSidebar;
            }
            _activeItem    = item;
            item.BackColor = AppTheme.SidebarHover;
            dot.BackColor  = AppTheme.Accent;
            lbl.ForeColor  = Color.White;
            lbl.Font       = new Font("Segoe UI", 10f, FontStyle.Bold);
        }

        item.MouseEnter += (_, _) => Hover(true);
        item.MouseLeave += (_, _) => Hover(false);
        lbl.MouseEnter  += (_, _) => Hover(true);
        lbl.MouseLeave  += (_, _) => Hover(false);
        item.Click += (_, _) => { Activate(); onClick(); };
        lbl.Click  += (_, _) => { Activate(); onClick(); };
        dot.Click  += (_, _) => { Activate(); onClick(); };

        SidebarPanel.Controls.Add(item);
        _navItems.Add((item, text));
        _navEntries.Add((item, Activate, onClick));
        return item;
    }

    protected void ActivateFirstNav()
    {
        if (_navEntries.Count > 0)
        {
            var (_, activate, action) = _navEntries[0];
            activate();
            action();
        }
    }

    protected void ShowPage(Control page)
    {
        ContentPanel.Controls.Clear();
        page.Dock = DockStyle.Fill;
        ContentPanel.Controls.Add(page);
        page.BringToFront();
    }
}
