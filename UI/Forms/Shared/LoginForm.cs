using SocietiesManagementSystem.Models;
using SocietiesManagementSystem.Services;
using SocietiesManagementSystem.UI.Controls;
using SocietiesManagementSystem.UI.Theme;

namespace SocietiesManagementSystem.UI.Forms.Shared;

public class LoginForm : Form
{
    private readonly AuthService _authService = new();
    private ModernTextBox _txtEmail = null!;
    private ModernTextBox _txtPassword = null!;
    private ModernButton  _btnLogin = null!;
    private ModernButton  _btnRegister = null!;
    private Label         _lblError = null!;
    private CheckBox      _chkShow = null!;

    public LoginForm()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Text            = "FAST Societies Management System";
        Size            = new Size(1000, 620);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Color.White;
        Font            = AppTheme.FontBody;
        DoubleBuffered  = true;

        // Left panel (branding)
        var leftPanel = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = 420,
            BackColor = AppTheme.Primary
        };

        var lblBrand = new Label
        {
            Text      = "FAST\nSocieties",
            Font      = new Font("Segoe UI", 36f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = false,
            Size      = new Size(340, 120),
            Location  = new Point(40, 160),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var lblSub = new Label
        {
            Text      = "Management System",
            Font      = new Font("Segoe UI", 14f),
            ForeColor = Color.FromArgb(180, 220, 255),
            AutoSize  = false,
            Size      = new Size(340, 30),
            Location  = new Point(40, 280),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var lblDesc = new Label
        {
            Text      = "Connecting students, societies\nand administration — digitally.",
            Font      = AppTheme.FontBody,
            ForeColor = Color.FromArgb(160, 200, 240),
            AutoSize  = false,
            Size      = new Size(340, 60),
            Location  = new Point(40, 320)
        };

        var accent = new Panel
        {
            BackColor = AppTheme.Accent,
            Size      = new Size(60, 5),
            Location  = new Point(40, 155)
        };

        leftPanel.Controls.AddRange(new Control[] { accent, lblBrand, lblSub, lblDesc });

        // Right panel (login form)
        var rightPanel = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.White,
            Padding   = new Padding(50, 0, 50, 0)
        };

        var lblTitle = new Label
        {
            Text      = "Welcome Back",
            Font      = AppTheme.FontH1,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(50, 120)
        };

        var lblSub2 = new Label
        {
            Text      = "Sign in to your account",
            Font      = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(50, 150)
        };

        var lblEmail = MakeLabel("Email Address", 50, 200);
        _txtEmail = new ModernTextBox
        {
            Placeholder = "student@fast.edu.pk",
            Location    = new Point(50, 225),
            Size        = new Size(380, 40)
        };

        var lblPass = MakeLabel("Password", 50, 280);
        _txtPassword = new ModernTextBox
        {
            Placeholder          = "Enter your password",
            UseSystemPasswordChar = true,
            Location             = new Point(50, 305),
            Size                 = new Size(380, 40)
        };

        _chkShow = new CheckBox
        {
            Text      = "Show password",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            Location  = new Point(50, 353),
            AutoSize  = true
        };
        _chkShow.CheckedChanged += (_, _) =>
            _txtPassword.UseSystemPasswordChar = !_chkShow.Checked;

        _lblError = new Label
        {
            Text      = "",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.Danger,
            AutoSize  = false,
            Size      = new Size(380, 30),
            Location  = new Point(50, 375),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _btnLogin = new ModernButton
        {
            Text     = "SIGN IN",
            Location = new Point(50, 410),
            Size     = new Size(380, 42)
        };
        _btnLogin.Click += OnLoginClick;

        var divider = new Panel
        {
            BackColor = AppTheme.Border,
            Size      = new Size(380, 1),
            Location  = new Point(50, 465)
        };

        var lblNewAcc = new Label
        {
            Text      = "Don't have an account?",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(50, 480)
        };

        _btnRegister = new ModernButton
        {
            Text        = "Create Student Account",
            ButtonStyle = ButtonStyle.Outline,
            Location    = new Point(50, 505),
            Size        = new Size(380, 38)
        };
        _btnRegister.Click += (_, _) =>
        {
            var reg = new RegisterForm();
            reg.ShowDialog(this);
        };

        rightPanel.Controls.AddRange(new Control[]
        {
            lblTitle, lblSub2, lblEmail, _txtEmail, lblPass, _txtPassword,
            _chkShow, _lblError, _btnLogin, divider, lblNewAcc, _btnRegister
        });

        Controls.AddRange(new Control[] { rightPanel, leftPanel });

        // Enter key submits
        KeyPreview = true;
        KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) OnLoginClick(this, EventArgs.Empty); };
    }

    private static Label MakeLabel(string text, int x, int y) => new()
    {
        Text      = text,
        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
        ForeColor = AppTheme.TextSecondary,
        AutoSize  = true,
        Location  = new Point(x, y)
    };

    private void OnLoginClick(object? sender, EventArgs e)
    {
        _lblError.Text = "";
        _btnLogin.Enabled = false;
        _btnLogin.Text = "Signing in…";

        var (success, message, user) = _authService.Login(_txtEmail.Text, _txtPassword.Text);

        _btnLogin.Enabled = true;
        _btnLogin.Text    = "SIGN IN";

        if (!success) { _lblError.Text = message; return; }

        var student = _authService.GetStudentProfile(user!.UserID);
        OpenDashboard(user, student);
        Hide();
    }

    private void OpenDashboard(User user, Models.Student? student)
    {
        Form dashboard = user.Role switch
        {
            "Admin"       => new Admin.AdminDashboard(user),
            "SocietyHead" => new Society.SocietyDashboard(user, student!),
            _             => new Student.StudentDashboard(user, student!)
        };

        dashboard.FormClosed += (_, _) => Show();
        dashboard.Show();
    }
}
