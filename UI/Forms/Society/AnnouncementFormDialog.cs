using SocietiesManagementSystem.Services;
using SocietiesManagementSystem.UI.Controls;
using SocietiesManagementSystem.UI.Theme;

namespace SocietiesManagementSystem.UI.Forms.Society;

public class AnnouncementFormDialog : Form
{
    private readonly int                 _societyId;
    private readonly int                 _createdBy;
    private readonly AnnouncementService _announceSvc;

    private ModernTextBox _txtTitle   = null!;
    private ModernTextBox _txtContent = null!;
    private ComboBox      _cboPriority = null!;
    private Label         _lblMsg     = null!;

    public AnnouncementFormDialog(int societyId, int createdBy, AnnouncementService announceSvc)
    {
        _societyId   = societyId;
        _createdBy   = createdBy;
        _announceSvc = announceSvc;
        Build();
    }

    private void Build()
    {
        Text            = "Post Announcement";
        Size            = new Size(520, 400);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = Color.White;

        var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = AppTheme.Primary };
        header.Controls.Add(new Label { Text = "New Announcement", Font = AppTheme.FontH2, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 14) });

        var body = new Panel { Location = new Point(0, 56), Size = new Size(520, 330), Padding = new Padding(24) };

        int y = 10;
        body.Controls.Add(L("Title", 0, y));
        _txtTitle = new ModernTextBox { Location = new Point(0, y + 22), Size = new Size(460, 40) }; body.Controls.Add(_txtTitle); y += 70;

        body.Controls.Add(L("Priority", 0, y));
        _cboPriority = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(0, y + 22), Size = new Size(150, 30), Font = AppTheme.FontInput };
        _cboPriority.Items.AddRange(new object[] { "Low", "Normal", "High" });
        _cboPriority.SelectedIndex = 1;
        body.Controls.Add(_cboPriority); y += 60;

        body.Controls.Add(L("Content", 0, y));
        _txtContent = new ModernTextBox { Location = new Point(0, y + 22), Size = new Size(460, 90), Multiline = true }; body.Controls.Add(_txtContent); y += 122;

        _lblMsg = new Label { AutoSize = false, Size = new Size(460, 22), Location = new Point(0, y), Font = AppTheme.FontSmall, TextAlign = ContentAlignment.MiddleLeft }; body.Controls.Add(_lblMsg); y += 28;

        var btnPost = new ModernButton { Text = "POST", Location = new Point(0, y), Size = new Size(220, 38) };
        btnPost.Click += (_, _) =>
        {
            var (success, msg, _) = _announceSvc.PostAnnouncement(_societyId, _txtTitle.Text, _txtContent.Text, _createdBy, _cboPriority.Text);
            _lblMsg.ForeColor = success ? AppTheme.Success : AppTheme.Danger;
            _lblMsg.Text      = msg;
            if (success) { DialogResult = DialogResult.OK; Close(); }
        };
        var btnCancel = new ModernButton { Text = "CANCEL", ButtonStyle = ButtonStyle.Ghost, Location = new Point(230, y), Size = new Size(220, 38) };
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        body.Controls.AddRange(new Control[] { btnPost, btnCancel });
        Controls.AddRange(new Control[] { header, body });
    }

    private static Label L(string t, int x, int y) =>
        new() { Text = t, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(x, y) };
}
