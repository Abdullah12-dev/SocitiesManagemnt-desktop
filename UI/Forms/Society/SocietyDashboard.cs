using SocietiesManagementSystem.Models;
using SocietiesManagementSystem.Services;
using SocietiesManagementSystem.UI.Controls;
using SocietiesManagementSystem.UI.Forms.Shared;
using SocietiesManagementSystem.UI.Theme;
using SocietyModel = SocietiesManagementSystem.Models.Society;

namespace SocietiesManagementSystem.UI.Forms.Society;

public class SocietyDashboard : BaseShellForm
{
    private readonly User          _user;
    private readonly Models.Student _head;
    private readonly SocietyService      _societyService  = new();
    private readonly MembershipService   _memberService   = new();
    private readonly EventService        _eventService    = new();
    private readonly TaskService         _taskService     = new();
    private readonly AnnouncementService _announceSvc     = new();
    private readonly ReportService       _reportService   = new();
    private SocietyModel? _society;

    public SocietyDashboard(User user, Models.Student head)
    {
        _user    = user;
        _head    = head;
        _society = _societyService.GetSocietiesByHead(head.StudentID).FirstOrDefault();
        BuildShell("Society Dashboard", head.FullName, "Society Head");
        BuildNav();
        ActivateFirstNav();
    }

    private void BuildNav()
    {
        int y = 155;
        AddNavItem("   Dashboard",       y,      () => { LblPageTitle.Text = "Dashboard";          ShowPage(BuildHomePage()); });
        AddNavItem("   Society Profile", y+44,   () => { LblPageTitle.Text = "Society Profile";    ShowPage(BuildSocietyProfilePage()); });
        AddNavItem("   Membership Reqs", y+88,   () => { LblPageTitle.Text = "Membership Requests";ShowPage(BuildMemberRequestsPage()); });
        AddNavItem("   Members",         y+132,  () => { LblPageTitle.Text = "Society Members";    ShowPage(BuildMembersPage()); });
        AddNavItem("   Events",          y+176,  () => { LblPageTitle.Text = "Events";             ShowPage(BuildEventsPage()); });
        AddNavItem("   Tasks",           y+220,  () => { LblPageTitle.Text = "Task Management";    ShowPage(BuildTasksPage()); });
        AddNavItem("   Announcements",   y+264,  () => { LblPageTitle.Text = "Announcements";      ShowPage(BuildAnnouncementsPage()); });
        AddNavItem("   Reports",         y+308,  () => { LblPageTitle.Text = "Reports";            ShowPage(BuildReportsPage()); });
    }

    // Returns null and shows a message if the head has no assigned society yet
    private SocietyModel? RequireSociety()
    {
        if (_society != null) return _society;
        var lbl = new Label
        {
            Text      = "You have no society assigned yet.\nContact an administrator.",
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(0, 20)
        };
        var page = new Panel { BackColor = AppTheme.Background };
        page.Controls.Add(lbl);
        ShowPage(page);
        return null;
    }

    // ── Home ─────────────────────────────────────────────────────────
    private Panel BuildHomePage()
    {
        var page = new Panel { BackColor = AppTheme.Background };

        if (_society == null)
        {
            page.Controls.Add(new Label
            {
                Text      = "No society assigned. Contact an administrator.",
                Font      = AppTheme.FontH2,
                ForeColor = AppTheme.TextSecondary,
                AutoSize  = true,
                Location  = new Point(0, 20)
            });
            return page;
        }

        var stats = _reportService.GetSocietyDashboardStats(_society.SocietyID);
        var cards = new[]
        {
            ("Members",  stats.GetValueOrDefault("Members").ToString(),         "👥", AppTheme.Primary),
            ("Pending",  stats.GetValueOrDefault("PendingRequests").ToString(), "⏳", AppTheme.Warning),
            ("Events",   stats.GetValueOrDefault("ApprovedEvents").ToString(),  "📅", AppTheme.Accent),
            ("Tasks",    stats.GetValueOrDefault("PendingTasks").ToString(),    "✅", AppTheme.Success)
        };

        var lblSoc    = new Label { Text = _society.Name, Font = AppTheme.FontH1, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(0, 0) };
        var lblStatus = new Label { Text = _society.Status, Font = AppTheme.FontBadge, ForeColor = AppTheme.GetStatusColor(_society.Status), AutoSize = true, Location = new Point(0, 32) };
        page.Controls.AddRange(new Control[] { lblSoc, lblStatus });

        int cx = 0;
        foreach (var (title, value, icon, color) in cards)
        {
            page.Controls.Add(new StatCard { Title = title, Value = value, Icon = icon, AccentColor = color, Location = new Point(cx, 55), Size = new Size(195, 110) });
            cx += 210;
        }

        return page;
    }

    // ── Society Profile (view + edit) ─────────────────────────────────
    private Panel BuildSocietyProfilePage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        if (RequireSociety() == null) return page;

        var card = new RoundedPanel { BackColor = Color.White, Location = new Point(0, 0), Size = new Size(560, 440) };

        var lblHeading = new Label { Text = "Society Profile", Font = AppTheme.FontH2, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(20, 16) };
        var lblStatus  = new Label { Text = _society!.Status, Font = AppTheme.FontBadge, ForeColor = AppTheme.GetStatusColor(_society.Status), AutoSize = true, Location = new Point(20, 44) };

        var lblName = MakeLabel("Society Name", 20, 76);
        var txtName = new ModernTextBox { Location = new Point(20, 98), Size = new Size(500, 40), Text = _society.Name };

        var lblCat = MakeLabel("Category", 20, 152);
        var cboCat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(20, 174), Size = new Size(500, 32), Font = AppTheme.FontInput };
        cboCat.Items.AddRange(new object[] { "Technology", "Sports", "Arts & Culture", "Academic", "Social", "Media", "Other" });
        cboCat.Text = _society.Category;
        if (cboCat.SelectedIndex < 0) cboCat.SelectedIndex = 0;

        var lblDesc = MakeLabel("Description", 20, 218);
        var txtDesc = new ModernTextBox { Location = new Point(20, 240), Size = new Size(500, 90), Multiline = true, Text = _society.Description };

        var lblMsg   = new Label { AutoSize = false, Size = new Size(500, 26), Location = new Point(20, 342), Font = AppTheme.FontSmall, TextAlign = ContentAlignment.MiddleLeft };
        var btnSave  = new ModernButton { Text = "SAVE CHANGES", Location = new Point(20, 374), Size = new Size(240, 40) };

        btnSave.Click += (_, _) =>
        {
            _society.Name        = txtName.Text;
            _society.Category    = cboCat.Text;
            _society.Description = txtDesc.Text;
            var (ok, msg)        = _societyService.UpdateSociety(_society);
            lblMsg.ForeColor     = ok ? AppTheme.Success : AppTheme.Danger;
            lblMsg.Text          = msg;
            if (ok) lblStatus.Text = _society.Status;
        };

        card.Controls.AddRange(new Control[] { lblHeading, lblStatus, lblName, txtName, lblCat, cboCat, lblDesc, txtDesc, lblMsg, btnSave });
        page.Controls.Add(card);
        return page;
    }

    // ── Membership Requests ───────────────────────────────────────────
    private Panel BuildMemberRequestsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        if (RequireSociety() == null) return page;

        var grid = MakeGrid(8);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.Size   = new Size(page.Width - 20, page.Height - 60);
        page.Resize += (_, _) => grid.Size = new Size(page.Width - 20, page.Height - 60);

        var headers = new[] { "ID", "Student", "Reg #", "Status", "Applied", "Society", "", "" };
        for (int i = 0; i < 8; i++) grid.Columns[i].HeaderText = headers[i];
        grid.Columns[6].Width = 80; grid.Columns[7].Width = 80;

        void LoadRequests()
        {
            grid.Rows.Clear();
            foreach (var r in _memberService.GetRequestsForSociety(_society!.SocietyID))
                grid.Rows.Add(r.RequestID, r.StudentName, r.RegistrationNumber, r.Status,
                              r.RequestedAt.ToString("dd-MMM-yyyy"), r.SocietyName, "Approve", "Reject");
        }
        LoadRequests();

        grid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            var requestId = (int)grid.Rows[e.RowIndex].Cells[0].Value;
            var action = e.ColumnIndex == 6 ? "Approve" : e.ColumnIndex == 7 ? "Reject" : null;
            if (action == null) return;

            string? reason = null;
            if (action == "Reject")
                reason = Microsoft.VisualBasic.Interaction.InputBox("Rejection reason (optional):", "Reject");

            var (success, msg) = _memberService.ProcessRequest(requestId, action, _head.StudentID, reason);
            MessageBox.Show(msg, success ? "Done" : "Error", MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            if (success) LoadRequests();
        };

        var btnRefresh = new ModernButton { Text = "Refresh", Location = new Point(0, 0), Size = new Size(120, 34), ButtonStyle = ButtonStyle.Outline };
        btnRefresh.Click += (_, _) => LoadRequests();

        page.Controls.AddRange(new Control[] { btnRefresh, grid });
        grid.Location = new Point(0, 44);
        return page;
    }

    // ── Members ───────────────────────────────────────────────────────
    private Panel BuildMembersPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        if (RequireSociety() == null) return page;

        // 6 columns: MemberID, StudentID (hidden), Name, Reg#, Role, Joined
        var grid = new DataGridView
        {
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            ReadOnly              = true,
            ColumnCount           = 6,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.None,
            RowHeadersVisible     = false,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            Font                  = AppTheme.FontSmall,
            GridColor             = AppTheme.Border,
            Location              = new Point(0, 44),
            Anchor                = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight         = 36;
        grid.EnableHeadersVisualStyles   = false;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

        grid.Columns[0].HeaderText = "ID";
        grid.Columns[1].HeaderText = "StudentID";
        grid.Columns[1].Visible    = false;       // hidden, used to detect self
        grid.Columns[2].HeaderText = "Student";
        grid.Columns[3].HeaderText = "Reg #";
        grid.Columns[4].HeaderText = "Role";
        grid.Columns[5].HeaderText = "Joined";

        page.Resize += (_, _) => grid.Size = new Size(page.Width - 20, page.Height - 60);
        grid.Size    = new Size(page.Width - 20, page.Height - 60);

        void LoadMembers()
        {
            grid.Rows.Clear();
            foreach (var m in _societyService.GetMembers(_society!.SocietyID))
                grid.Rows.Add(m.MemberID, m.StudentID, m.StudentName, m.RegistrationNumber,
                              m.Role, m.JoinedAt.ToString("dd-MMM-yyyy"));
        }
        LoadMembers();

        var btnRemove = new ModernButton { Text = "Remove Member", Location = new Point(0, 0), Size = new Size(160, 34), ButtonStyle = ButtonStyle.Danger };
        btnRemove.Click += (_, _) =>
        {
            if (grid.SelectedRows.Count == 0) return;

            int selectedStudentId = (int)grid.SelectedRows[0].Cells[1].Value;
            if (selectedStudentId == _head.StudentID)
            {
                MessageBox.Show("You cannot remove yourself from the society.", "Not Allowed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int memberId = (int)grid.SelectedRows[0].Cells[0].Value;
            if (MessageBox.Show("Remove this member?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            if (_societyService.RemoveMember(memberId)) LoadMembers();
        };

        page.Controls.AddRange(new Control[] { btnRemove, grid });
        return page;
    }

    // ── Events ────────────────────────────────────────────────────────
    private Panel BuildEventsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        if (RequireSociety() == null) return page;

        var grid = MakeGrid(7);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.Size   = new Size(page.Width - 20, page.Height - 100);
        page.Resize += (_, _) => grid.Size = new Size(page.Width - 20, page.Height - 100);
        var headers = new[] { "ID", "Title", "Type", "Date", "Venue", "Status", "Registered" };
        for (int i = 0; i < 7; i++) grid.Columns[i].HeaderText = headers[i];

        void LoadEvents()
        {
            grid.Rows.Clear();
            foreach (var ev in _eventService.GetEventsBySociety(_society!.SocietyID))
                grid.Rows.Add(ev.EventID, ev.Title, ev.EventType, ev.EventDate.ToString("dd-MMM-yyyy"),
                              ev.Venue, ev.Status, ev.RegistrationCount);
        }
        LoadEvents();

        var btnNew = new ModernButton { Text = "+ New Event", Location = new Point(0, 0), Size = new Size(130, 34), ButtonStyle = ButtonStyle.Accent };
        btnNew.Click += (_, _) =>
        {
            var form = new EventFormDialog(_society!.SocietyID, _eventService);
            if (form.ShowDialog() == DialogResult.OK) LoadEvents();
        };

        var btnEdit = new ModernButton { Text = "Edit Event", Location = new Point(145, 0), Size = new Size(120, 34), ButtonStyle = ButtonStyle.Outline };
        btnEdit.Click += (_, _) =>
        {
            if (grid.SelectedRows.Count == 0) return;
            int id = (int)grid.SelectedRows[0].Cells[0].Value;
            var ev = _eventService.GetById(id);
            if (ev == null) return;
            var form = new EventFormDialog(_society!.SocietyID, _eventService, ev);
            if (form.ShowDialog() == DialogResult.OK) LoadEvents();
        };

        var btnCancel = new ModernButton { Text = "Cancel Event", Location = new Point(278, 0), Size = new Size(130, 34), ButtonStyle = ButtonStyle.Danger };
        btnCancel.Click += (_, _) =>
        {
            if (grid.SelectedRows.Count == 0) return;
            int id = (int)grid.SelectedRows[0].Cells[0].Value;
            if (MessageBox.Show("Cancel this event?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _eventService.CancelEvent(id);
            LoadEvents();
        };

        page.Controls.AddRange(new Control[] { btnNew, btnEdit, btnCancel, grid });
        grid.Location = new Point(0, 44);
        return page;
    }

    // ── Tasks ─────────────────────────────────────────────────────────
    private Panel BuildTasksPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        if (RequireSociety() == null) return page;

        var members = _societyService.GetMembers(_society!.SocietyID).ToList();
        var grid    = MakeGrid(7);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.Size   = new Size(page.Width - 20, page.Height - 100);
        page.Resize += (_, _) => grid.Size = new Size(page.Width - 20, page.Height - 100);
        var headers = new[] { "ID", "Title", "Assigned To", "Due Date", "Status", "Assigned By", "Created" };
        for (int i = 0; i < 7; i++) grid.Columns[i].HeaderText = headers[i];

        void LoadTasks()
        {
            grid.Rows.Clear();
            foreach (var t in _taskService.GetTasksBySociety(_society.SocietyID))
                grid.Rows.Add(t.TaskID, t.Title, t.AssignedToName,
                              t.DueDate?.ToString("dd-MMM-yyyy") ?? "—",
                              t.Status, t.AssignedByName, t.CreatedAt.ToString("dd-MMM-yyyy"));
        }
        LoadTasks();

        var btnNew = new ModernButton { Text = "+ Assign Task", Location = new Point(0, 0), Size = new Size(140, 34), ButtonStyle = ButtonStyle.Accent };
        btnNew.Click += (_, _) =>
        {
            var form = new TaskFormDialog(_society.SocietyID, _head.StudentID, members, _taskService);
            if (form.ShowDialog() == DialogResult.OK) LoadTasks();
        };

        page.Controls.AddRange(new Control[] { btnNew, grid });
        grid.Location = new Point(0, 44);
        return page;
    }

    // ── Announcements ─────────────────────────────────────────────────
    private Panel BuildAnnouncementsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        if (RequireSociety() == null) return page;

        var grid = MakeGrid(5);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.Size   = new Size(page.Width - 20, page.Height - 100);
        page.Resize += (_, _) => grid.Size = new Size(page.Width - 20, page.Height - 100);
        var headers = new[] { "ID", "Title", "Priority", "Posted By", "Date" };
        for (int i = 0; i < 5; i++) grid.Columns[i].HeaderText = headers[i];

        void Load()
        {
            grid.Rows.Clear();
            foreach (var a in _announceSvc.GetBySociety(_society!.SocietyID))
                grid.Rows.Add(a.AnnouncementID, a.Title, a.Priority, a.CreatedByName, a.CreatedAt.ToString("dd-MMM-yyyy"));
        }
        Load();

        var btnNew = new ModernButton { Text = "+ Post Announcement", Location = new Point(0, 0), Size = new Size(180, 34), ButtonStyle = ButtonStyle.Accent };
        btnNew.Click += (_, _) =>
        {
            var form = new AnnouncementFormDialog(_society!.SocietyID, _head.StudentID, _announceSvc);
            if (form.ShowDialog() == DialogResult.OK) Load();
        };

        var btnDel = new ModernButton { Text = "Delete", Location = new Point(192, 0), Size = new Size(100, 34), ButtonStyle = ButtonStyle.Danger };
        btnDel.Click += (_, _) =>
        {
            if (grid.SelectedRows.Count == 0) return;
            int id = (int)grid.SelectedRows[0].Cells[0].Value;
            if (MessageBox.Show("Delete this announcement?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _announceSvc.DeleteAnnouncement(id);
            Load();
        };

        page.Controls.AddRange(new Control[] { btnNew, btnDel, grid });
        grid.Location = new Point(0, 44);
        return page;
    }

    // ── Reports ───────────────────────────────────────────────────────
    private Panel BuildReportsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        if (RequireSociety() == null) return page;

        var lblTitle = new Label { Text = "Society Reports", Font = AppTheme.FontH2, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(0, 0) };
        page.Controls.Add(lblTitle);

        var btnMembers = new ModernButton { Text = "Member Report", Location = new Point(0, 35), Size = new Size(180, 36), ButtonStyle = ButtonStyle.Outline };
        btnMembers.Click += (_, _) =>
        {
            var dt = _reportService.GetSocietyMembersReport(_society!.SocietyID);
            ShowReport("Members — " + _society.Name, dt, page);
        };

        var btnEvents = new ModernButton { Text = "Events Report", Location = new Point(196, 35), Size = new Size(180, 36), ButtonStyle = ButtonStyle.Outline };
        btnEvents.Click += (_, _) =>
        {
            var dt = _reportService.GetSocietyEventsReport(_society!.SocietyID);
            ShowReport("Events — " + _society.Name, dt, page);
        };

        page.Controls.AddRange(new Control[] { btnMembers, btnEvents });
        return page;
    }

    private void ShowReport(string title, System.Data.DataTable dt, Panel parent)
    {
        var existing = parent.Controls.OfType<DataGridView>().FirstOrDefault();
        if (existing != null) parent.Controls.Remove(existing);

        var grid = new DataGridView
        {
            DataSource            = dt,
            ReadOnly              = true,
            AllowUserToAddRows    = false,
            Location              = new Point(0, 82),
            Size                  = new Size(parent.Width - 20, parent.Height - 100),
            Anchor                = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.None,
            RowHeadersVisible     = false,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            Font                  = AppTheme.FontSmall
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.EnableHeadersVisualStyles = false;
        parent.Controls.Add(grid);
        parent.Resize += (_, _) => grid.Size = new Size(parent.Width - 20, parent.Height - 100);
    }

    // ── Helpers ───────────────────────────────────────────────────────
    private static Label MakeLabel(string text, int x, int y) => new()
    {
        Text      = text,
        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
        ForeColor = AppTheme.TextSecondary,
        AutoSize  = true,
        Location  = new Point(x, y)
    };

    private static DataGridView MakeGrid(int cols)
    {
        var grid = new DataGridView
        {
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            ReadOnly              = true,
            ColumnCount           = cols,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.None,
            RowHeadersVisible     = false,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            Font                  = AppTheme.FontSmall,
            GridColor             = AppTheme.Border,
            Location              = new Point(0, 44)
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight         = 36;
        grid.EnableHeadersVisualStyles   = false;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        return grid;
    }
}
