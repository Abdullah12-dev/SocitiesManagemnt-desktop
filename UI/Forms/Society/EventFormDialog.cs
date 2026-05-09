using SocietiesManagementSystem.Models;
using SocietiesManagementSystem.Services;
using SocietiesManagementSystem.UI.Controls;
using SocietiesManagementSystem.UI.Theme;

namespace SocietiesManagementSystem.UI.Forms.Society;

public class EventFormDialog : Form
{
    private readonly int          _societyId;
    private readonly EventService _eventService;
    private readonly Event?       _existing;

    private ModernTextBox  _txtTitle = null!;
    private ModernTextBox  _txtDesc  = null!;
    private ModernTextBox  _txtVenue = null!;
    private DateTimePicker _dtpDate  = null!;
    private DateTimePicker _dtpEnd   = null!;
    private NumericUpDown  _nudMax   = null!;
    private ComboBox       _cboType  = null!;
    private Label          _lblMsg   = null!;

    public EventFormDialog(int societyId, EventService eventService, Event? existing = null)
    {
        _societyId    = societyId;
        _eventService = eventService;
        _existing     = existing;
        Build();
    }

    private void Build()
    {
        bool isEdit     = _existing != null;
        Text            = isEdit ? "Edit Event" : "Create New Event";
        Size            = new Size(540, 540);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = Color.White;

        var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = AppTheme.Primary };
        var lblH   = new Label { Text = isEdit ? "Edit Event" : "New Event", Font = AppTheme.FontH1, ForeColor = Color.White, AutoSize = true, Location = new Point(20, 16) };
        header.Controls.Add(lblH);

        var body = new Panel { Location = new Point(0, 60), Size = new Size(540, 420), Padding = new Padding(24, 12, 24, 12) };
        int y = 16;

        body.Controls.Add(L("Event Title", 0, y));
        _txtTitle = T(0, y + 22, 472); body.Controls.Add(_txtTitle);
        y += 72;

        body.Controls.Add(L("Description", 0, y));
        _txtDesc = T(0, y + 22, 472); _txtDesc.Multiline = true; _txtDesc.Height = 70; body.Controls.Add(_txtDesc);
        y += 110;

        body.Controls.Add(L("Event Type", 0, y));
        _cboType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(0, y + 22), Size = new Size(220, 30), Font = AppTheme.FontInput };
        _cboType.Items.AddRange(new object[] { "Workshop", "Competition", "Seminar", "Recruitment", "Awareness", "Social", "Other" });
        _cboType.SelectedIndex = 0;
        body.Controls.Add(_cboType);

        body.Controls.Add(L("Venue", 236, y));
        _txtVenue = new ModernTextBox { Location = new Point(236, y + 22), Size = new Size(236, 40) };
        body.Controls.Add(_txtVenue);
        y += 72;

        body.Controls.Add(L("Start Date & Time", 0, y));
        _dtpDate = new DateTimePicker { Location = new Point(0, y + 22), Size = new Size(220, 30), Format = DateTimePickerFormat.Custom, CustomFormat = "dd-MMM-yyyy  HH:mm", ShowUpDown = false, MinDate = DateTime.Now };
        body.Controls.Add(_dtpDate);

        body.Controls.Add(L("End Date & Time (optional)", 236, y));
        _dtpEnd = new DateTimePicker { Location = new Point(236, y + 22), Size = new Size(236, 30), Format = DateTimePickerFormat.Custom, CustomFormat = "dd-MMM-yyyy  HH:mm", ShowUpDown = false };
        body.Controls.Add(_dtpEnd);
        y += 72;

        body.Controls.Add(L("Max Participants (0 = unlimited)", 0, y));
        _nudMax = new NumericUpDown { Location = new Point(0, y + 22), Size = new Size(120, 30), Minimum = 0, Maximum = 5000, Value = 0 };
        body.Controls.Add(_nudMax);
        y += 70;

        _lblMsg = new Label { AutoSize = false, Size = new Size(472, 24), Location = new Point(0, y), Font = AppTheme.FontSmall, TextAlign = ContentAlignment.MiddleLeft };
        body.Controls.Add(_lblMsg);
        y += 30;

        var btnSave   = new ModernButton { Text = isEdit ? "SAVE CHANGES" : "CREATE EVENT", Location = new Point(0, y), Size = new Size(232, 38) };
        var btnCancel = new ModernButton { Text = "CANCEL", ButtonStyle = ButtonStyle.Ghost, Location = new Point(240, y), Size = new Size(232, 38) };
        btnSave.Click   += OnSaveClick;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        body.Controls.AddRange(new Control[] { btnSave, btnCancel });
        Controls.AddRange(new Control[] { header, body });

        if (isEdit) PreFill();
    }

    private void PreFill()
    {
        _txtTitle.Text  = _existing!.Title;
        _txtDesc.Text   = _existing.Description;
        _txtVenue.Text  = _existing.Venue;
        _dtpDate.Value  = _existing.EventDate > DateTime.Now ? _existing.EventDate : DateTime.Now.AddHours(1);
        _dtpEnd.Value   = _existing.EndDate ?? _existing.EventDate.AddHours(2);
        _nudMax.Value   = _existing.MaxParticipants ?? 0;
        var idx = _cboType.Items.IndexOf(_existing.EventType);
        if (idx >= 0) _cboType.SelectedIndex = idx;
    }

    private void OnSaveClick(object? s, EventArgs e)
    {
        int? max = _nudMax.Value > 0 ? (int)_nudMax.Value : null;
        bool success; string msg;

        if (_existing == null)
        {
            (success, msg, _) = _eventService.CreateEvent(
                _societyId, _txtTitle.Text, _txtDesc.Text,
                _dtpDate.Value, _dtpEnd.Value, _txtVenue.Text, max, _cboType.Text);
        }
        else
        {
            _existing.Title           = _txtTitle.Text;
            _existing.Description     = _txtDesc.Text;
            _existing.Venue           = _txtVenue.Text;
            _existing.EventDate       = _dtpDate.Value;
            _existing.EndDate         = _dtpEnd.Value;
            _existing.MaxParticipants = max;
            _existing.EventType       = _cboType.Text;
            (success, msg) = _eventService.UpdateEvent(_existing);
        }

        _lblMsg.ForeColor = success ? AppTheme.Success : AppTheme.Danger;
        _lblMsg.Text      = msg;
        if (success) { DialogResult = DialogResult.OK; Close(); }
    }

    private static Label L(string text, int x, int y) =>
        new() { Text = text, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(x, y) };

    private static ModernTextBox T(int x, int y, int w) =>
        new() { Location = new Point(x, y), Size = new Size(w, 40) };
}
