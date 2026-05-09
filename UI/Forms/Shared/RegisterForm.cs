using SocietiesManagementSystem.Services;
using SocietiesManagementSystem.UI.Controls;
using SocietiesManagementSystem.UI.Theme;

namespace SocietiesManagementSystem.UI.Forms.Shared;

public class RegisterForm : Form
{
    private readonly AuthService _authService = new();

    private ModernTextBox _txtFirstName = null!;
    private ModernTextBox _txtLastName  = null!;
    private ModernTextBox _txtEmail     = null!;
    private ModernTextBox _txtRegNum    = null!;
    private ModernTextBox _txtPhone     = null!;
    private ModernTextBox _txtPassword  = null!;
    private ModernTextBox _txtConfirm   = null!;
    private ComboBox      _cboDept      = null!;
    private ComboBox      _cboSemester  = null!;
    private Label         _lblMessage   = null!;

    public RegisterForm()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Text            = "Create Student Account";
        Size            = new Size(680, 680);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = Color.White;

        var header = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 70,
            BackColor = AppTheme.Primary
        };

        var lblTitle = new Label
        {
            Text      = "Student Registration",
            Font      = AppTheme.FontH1,
            ForeColor = Color.White,
            AutoSize  = true,
            Location  = new Point(24, 22)
        };
        header.Controls.Add(lblTitle);

        var body = new Panel
        {
            Location  = new Point(0, 70),
            Size      = new Size(680, 540),
            Padding   = new Padding(30, 20, 30, 20),
            AutoScroll = true
        };

        int y = 20;

        // Row 1: First + Last name
        body.Controls.Add(MakeLabel("First Name", 0, y));
        body.Controls.Add(MakeLabel("Last Name", 290, y));
        y += 22;

        _txtFirstName = MakeTxt(0, y, 260);
        _txtLastName  = MakeTxt(290, y, 260);
        body.Controls.AddRange(new Control[] { _txtFirstName, _txtLastName });
        y += 55;

        body.Controls.Add(MakeLabel("Email Address", 0, y));
        y += 22;
        _txtEmail = MakeTxt(0, y, 560);
        body.Controls.Add(_txtEmail);
        y += 55;

        body.Controls.Add(MakeLabel("Registration Number", 0, y));
        body.Controls.Add(MakeLabel("Phone", 290, y));
        y += 22;
        _txtRegNum = MakeTxt(0, y, 260);
        _txtRegNum.Placeholder = "e.g. 21L-3001";
        _txtPhone  = MakeTxt(290, y, 260);
        _txtPhone.Placeholder  = "03xx-xxxxxxx";
        body.Controls.AddRange(new Control[] { _txtRegNum, _txtPhone });
        y += 55;

        body.Controls.Add(MakeLabel("Department", 0, y));
        body.Controls.Add(MakeLabel("Semester", 290, y));
        y += 22;

        _cboDept = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = AppTheme.FontInput,
            Location      = new Point(0, y),
            Size          = new Size(260, 30),
            FlatStyle     = FlatStyle.Flat
        };
        _cboDept.Items.AddRange(new object[]
        {
            "Computer Science", "Software Engineering", "Electrical Engineering",
            "Mechanical Engineering", "Civil Engineering", "Business Administration",
            "Mathematics", "Physics", "Other"
        });
        _cboDept.SelectedIndex = 0;

        _cboSemester = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = AppTheme.FontInput,
            Location      = new Point(290, y),
            Size          = new Size(260, 30),
            FlatStyle     = FlatStyle.Flat
        };
        for (int i = 1; i <= 8; i++) _cboSemester.Items.Add($"Semester {i}");
        _cboSemester.SelectedIndex = 0;
        body.Controls.AddRange(new Control[] { _cboDept, _cboSemester });
        y += 50;

        body.Controls.Add(MakeLabel("Password (min 8 characters)", 0, y));
        body.Controls.Add(MakeLabel("Confirm Password", 290, y));
        y += 22;
        _txtPassword = MakeTxt(0, y, 260);
        _txtPassword.UseSystemPasswordChar = true;
        _txtConfirm  = MakeTxt(290, y, 260);
        _txtConfirm.UseSystemPasswordChar  = true;
        body.Controls.AddRange(new Control[] { _txtPassword, _txtConfirm });
        y += 55;

        _lblMessage = new Label
        {
            AutoSize  = false,
            Size      = new Size(560, 28),
            Location  = new Point(0, y),
            Font      = AppTheme.FontSmall,
            TextAlign = ContentAlignment.MiddleLeft
        };
        body.Controls.Add(_lblMessage);

        var btnRegister = new ModernButton
        {
            Text     = "CREATE ACCOUNT",
            Location = new Point(0, y + 32),
            Size     = new Size(270, 42)
        };
        btnRegister.Click += OnRegisterClick;

        var btnCancel = new ModernButton
        {
            Text        = "CANCEL",
            ButtonStyle = ButtonStyle.Ghost,
            Location    = new Point(290, y + 32),
            Size        = new Size(270, 42)
        };
        btnCancel.Click += (_, _) => Close();

        body.Controls.AddRange(new Control[] { btnRegister, btnCancel });

        Controls.AddRange(new Control[] { header, body });
    }

    private ModernTextBox MakeTxt(int x, int y, int w) => new()
    {
        Location = new Point(x, y),
        Size     = new Size(w, 40)
    };

    private static Label MakeLabel(string text, int x, int y) => new()
    {
        Text      = text,
        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
        ForeColor = AppTheme.TextSecondary,
        AutoSize  = true,
        Location  = new Point(x, y)
    };

    private void OnRegisterClick(object? sender, EventArgs e)
    {
        _lblMessage.Text      = "";
        _lblMessage.ForeColor = AppTheme.Danger;

        if (_txtPassword.Text != _txtConfirm.Text)
        {
            _lblMessage.Text = "Passwords do not match.";
            return;
        }

        var (success, message) = _authService.RegisterStudent(
            email:      _txtEmail.Text,
            password:   _txtPassword.Text,
            firstName:  _txtFirstName.Text,
            lastName:   _txtLastName.Text,
            regNumber:  _txtRegNum.Text,
            department: _cboDept.Text,
            semester:   _cboSemester.SelectedIndex + 1,
            phone:      _txtPhone.Text
        );

        if (success)
        {
            _lblMessage.ForeColor = AppTheme.Success;
            _lblMessage.Text      = message;
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        else
        {
            _lblMessage.Text = message;
        }
    }
}
