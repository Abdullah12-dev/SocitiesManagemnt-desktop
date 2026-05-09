using SocietiesManagementSystem.Models;
using SocietiesManagementSystem.Services;
using SocietiesManagementSystem.UI.Controls;
using SocietiesManagementSystem.UI.Theme;

namespace SocietiesManagementSystem.UI.Forms.Society;

public class TaskFormDialog : Form
{
    private readonly int                   _societyId;
    private readonly int                   _assignedBy;
    private readonly List<SocietyMember>   _members;
    private readonly TaskService           _taskService;

    private ModernTextBox _txtTitle = null!;
    private ModernTextBox _txtDesc  = null!;
    private ComboBox      _cboMember = null!;
    private DateTimePicker _dtpDue  = null!;
    private CheckBox      _chkDue   = null!;
    private Label         _lblMsg   = null!;

    public TaskFormDialog(int societyId, int assignedBy, List<SocietyMember> members, TaskService taskService)
    {
        _societyId   = societyId;
        _assignedBy  = assignedBy;
        _members     = members;
        _taskService = taskService;
        Build();
    }

    private void Build()
    {
        Text            = "Assign Task";
        Size            = new Size(500, 420);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = Color.White;

        var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = AppTheme.Primary };
        header.Controls.Add(new Label { Text = "Assign Task to Member", Font = AppTheme.FontH2, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 14) });

        var body = new Panel { Location = new Point(0, 56), Size = new Size(500, 340), Padding = new Padding(24) };

        int y = 10;
        body.Controls.Add(L("Task Title", 0, y));
        _txtTitle = new ModernTextBox { Location = new Point(0, y + 22), Size = new Size(440, 40) }; body.Controls.Add(_txtTitle); y += 70;

        body.Controls.Add(L("Description", 0, y));
        _txtDesc = new ModernTextBox { Location = new Point(0, y + 22), Size = new Size(440, 60), Multiline = true }; body.Controls.Add(_txtDesc); y += 90;

        body.Controls.Add(L("Assign To", 0, y));
        _cboMember = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(0, y + 22), Size = new Size(440, 30), Font = AppTheme.FontInput };
        _members.ForEach(m => _cboMember.Items.Add($"{m.StudentName} ({m.RegistrationNumber})"));
        if (_members.Count > 0) _cboMember.SelectedIndex = 0;
        body.Controls.Add(_cboMember); y += 60;

        _chkDue = new CheckBox { Text = "Set Due Date", Location = new Point(0, y), Font = AppTheme.FontSmall, AutoSize = true };
        _dtpDue = new DateTimePicker { Location = new Point(130, y - 4), Size = new Size(200, 28), Enabled = false, Format = DateTimePickerFormat.Custom, CustomFormat = "dd-MMM-yyyy" };
        _chkDue.CheckedChanged += (_, _) => _dtpDue.Enabled = _chkDue.Checked;
        body.Controls.AddRange(new Control[] { _chkDue, _dtpDue }); y += 44;

        _lblMsg = new Label { AutoSize = false, Size = new Size(440, 22), Location = new Point(0, y), Font = AppTheme.FontSmall, TextAlign = ContentAlignment.MiddleLeft }; body.Controls.Add(_lblMsg); y += 28;

        var btnAssign = new ModernButton { Text = "ASSIGN", Location = new Point(0, y), Size = new Size(210, 38) };
        btnAssign.Click += OnAssign;
        var btnCancel = new ModernButton { Text = "CANCEL", ButtonStyle = ButtonStyle.Ghost, Location = new Point(220, y), Size = new Size(210, 38) };
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        body.Controls.AddRange(new Control[] { btnAssign, btnCancel });
        Controls.AddRange(new Control[] { header, body });
    }

    private void OnAssign(object? s, EventArgs e)
    {
        if (_cboMember.SelectedIndex < 0) { _lblMsg.Text = "Select a member."; _lblMsg.ForeColor = AppTheme.Danger; return; }
        int toStudentId = _members[_cboMember.SelectedIndex].StudentID;
        DateTime? due   = _chkDue.Checked ? _dtpDue.Value : null;

        var (success, msg, _) = _taskService.AssignTask(_societyId, _txtTitle.Text, _txtDesc.Text, toStudentId, _assignedBy, due);
        _lblMsg.ForeColor = success ? AppTheme.Success : AppTheme.Danger;
        _lblMsg.Text      = msg;
        if (success) { DialogResult = DialogResult.OK; Close(); }
    }

    private static Label L(string t, int x, int y) =>
        new() { Text = t, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(x, y) };
}
