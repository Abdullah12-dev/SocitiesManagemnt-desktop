using SocietiesManagementSystem.Models;
using SocietiesManagementSystem.Services;
using SocietiesManagementSystem.UI.Controls;
using SocietiesManagementSystem.UI.Forms.Shared;
using SocietiesManagementSystem.UI.Theme;
using AdminModel = SocietiesManagementSystem.Models.Admin;

namespace SocietiesManagementSystem.UI.Forms.Admin;

public class AdminDashboard : BaseShellForm
{
    private readonly User           _user;
    private readonly SocietyService      _societyService  = new();
    private readonly EventService        _eventService    = new();
    private readonly ReportService       _reportService   = new();
    private readonly MembershipService   _memberService   = new();
    private readonly AnnouncementService _announceService = new();
    private AdminModel?  _admin;

    public AdminDashboard(User user)
    {
        _user = user;
        LoadAdmin();
        BuildShell("Admin Dashboard", _admin != null ? $"{_admin.FirstName} {_admin.LastName}" : "Admin", "Administrator");
        BuildNav();
        ActivateFirstNav();
    }

    private void LoadAdmin()
    {
        using var conn = Data.DatabaseConnection.Instance.GetConnection();
        conn.Open();
        using var cmd = new Microsoft.Data.SqlClient.SqlCommand(
            "SELECT AdminID, UserID, FirstName, LastName, Phone FROM Admins WHERE UserID=@UID", conn);
        cmd.Parameters.AddWithValue("@UID", _user.UserID);
        using var r = cmd.ExecuteReader();
        if (r.Read())
            _admin = new AdminModel { AdminID = r.GetInt32(0), UserID = r.GetInt32(1), FirstName = r.GetString(2), LastName = r.GetString(3), Phone = r.IsDBNull(4) ? "" : r.GetString(4) };
    }

    private void BuildNav()
    {
        int y = 155;
        AddNavItem("   Dashboard",     y,      () => { LblPageTitle.Text = "Dashboard";          ShowPage(BuildHomePage()); });
        AddNavItem("   Manage Students", y+44, () => { LblPageTitle.Text = "Student Accounts";   ShowPage(BuildStudentsPage()); });
        AddNavItem("   Societies",     y+88,   () => { LblPageTitle.Text = "Society Management"; ShowPage(BuildSocietiesPage()); });
        AddNavItem("   Event Approvals", y+132,() => { LblPageTitle.Text = "Event Approvals";    ShowPage(BuildEventsPage()); });
        AddNavItem("   Reports",       y+176,  () => { LblPageTitle.Text = "University Reports"; ShowPage(BuildReportsPage()); });
    }

    // ── Home ──────────────────────────────────────────────────────────
    private Panel BuildHomePage()
    {
        var page  = new Panel { BackColor = AppTheme.Background };
        var stats = _reportService.GetAdminDashboardStats();

        var greeting = new Label { Text = "Administrator Overview", Font = AppTheme.FontH1, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(0, 0) };
        page.Controls.Add(greeting);

        var items = new[]
        {
            ("Students",    stats.GetValueOrDefault("TotalStudents").ToString(),      "🎓", AppTheme.Primary),
            ("Active Soc.", stats.GetValueOrDefault("ActiveSocieties").ToString(),    "⭐", AppTheme.Accent),
            ("Pending Soc.",stats.GetValueOrDefault("PendingSocieties").ToString(),   "⏳", AppTheme.Warning),
            ("Pending Ev.", stats.GetValueOrDefault("PendingEvents").ToString(),      "📅", AppTheme.Danger),
            ("Upcoming Ev.",stats.GetValueOrDefault("UpcomingEvents").ToString(),     "🗓️", AppTheme.Success),
            ("Pending Mem.",stats.GetValueOrDefault("PendingMemberships").ToString(), "👥", Color.FromArgb(111, 66, 193))
        };

        int cx = 0;
        foreach (var (title, value, icon, color) in items)
        {
            var card = new StatCard { Title = title, Value = value, Icon = icon, AccentColor = color, Location = new Point(cx, 40), Size = new Size(195, 110) };
            page.Controls.Add(card);
            cx += 210;
            if (cx > 1000) { cx = 0; }
        }

        // Quick notice
        var notice = new RoundedPanel { BackColor = Color.FromArgb(230, 243, 255), Location = new Point(0, 170), Size = new Size(800, 60), CornerRadius = 6, ShowBorder = false };
        var lblNotice = new Label { Text = $"ℹ  Welcome {_admin?.FirstName}. Use the left menu to manage students, approve societies, events, and generate reports.", Font = AppTheme.FontSmall, ForeColor = AppTheme.Primary, AutoSize = false, Size = new Size(780, 60), Location = new Point(10, 0), TextAlign = ContentAlignment.MiddleLeft };
        notice.Controls.Add(lblNotice);
        page.Controls.Add(notice);

        return page;
    }

    // ── Students ──────────────────────────────────────────────────────
    private Panel BuildStudentsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };

        var grid = MakeGrid(7);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.Size   = new Size(page.Width - 20, page.Height - 100);
        page.Resize += (_, _) => grid.Size = new Size(page.Width - 20, page.Height - 100);
        var headers = new[] { "ID", "Name", "Reg #", "Email", "Department", "Semester", "Active" };
        for (int i = 0; i < 7; i++) grid.Columns[i].HeaderText = headers[i];

        var txtSearch = new ModernTextBox { Location = new Point(0, 0), Size = new Size(280, 38), Placeholder = "Search by name, email, or reg #..." };
        var btnSearch = new ModernButton { Text = "Search", Location = new Point(294, 0), Size = new Size(100, 38), ButtonStyle = ButtonStyle.Outline };
        var btnToggle = new ModernButton { Text = "Toggle Active", Location = new Point(408, 0), Size = new Size(140, 38), ButtonStyle = ButtonStyle.Danger };

        void LoadStudents(string? term = null)
        {
            var studentRepo = new Data.Repositories.StudentRepository();
            var students    = string.IsNullOrWhiteSpace(term) ? studentRepo.GetAllForAdmin() : studentRepo.Search(term);
            grid.Rows.Clear();
            foreach (var s in students)
                grid.Rows.Add(s.StudentID, s.FullName, s.RegistrationNumber, s.Email, s.Department, s.Semester, s.IsActive ? "Yes" : "No");
        }

        btnSearch.Click += (_, _) => LoadStudents(txtSearch.Text);
        txtSearch.TextChanged += (_, _) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) LoadStudents(); };

        btnToggle.Click += (_, _) =>
        {
            if (grid.SelectedRows.Count == 0) return;
            int sid = (int)grid.SelectedRows[0].Cells[0].Value;
            var userRepo = new Data.Repositories.UserRepository();
            var studentRepo = new Data.Repositories.StudentRepository();
            var s = studentRepo.GetById(sid);
            if (s == null) return;
            var u = userRepo.GetById(s.UserID);
            if (u == null) return;
            u.IsActive = !u.IsActive;
            userRepo.Update(u);
            LoadStudents();
        };

        LoadStudents();
        page.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnToggle, grid });
        grid.Location = new Point(0, 48);
        return page;
    }

    // ── Societies ─────────────────────────────────────────────────────
    private Panel BuildSocietiesPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };

        var tabControl = new TabControl { Location = new Point(0, 0), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, Font = AppTheme.FontBody };
        tabControl.Resize += (_, _) => tabControl.Size = new Size(page.Width, page.Height);
        page.Resize += (_, _) => tabControl.Size = new Size(page.Width, page.Height);

        var tabPending = new TabPage("Pending Approval");
        var tabAll     = new TabPage("All Societies");
        var tabCreate  = new TabPage("Create Society");

        BuildSocietyTab(tabPending, "Pending");
        BuildSocietyTab(tabAll,     null);
        BuildCreateSocietyAdminTab(tabCreate);

        tabControl.TabPages.AddRange(new[] { tabPending, tabAll, tabCreate });
        page.Controls.Add(tabControl);
        return page;
    }

    private void BuildSocietyTab(TabPage tab, string? statusFilter)
    {
        var grid = MakeGrid(7);
        grid.Dock = DockStyle.Fill;
        var headers = new[] { "ID", "Name", "Category", "Head", "Status", "Members", "Created" };
        for (int i = 0; i < 7; i++) grid.Columns[i].HeaderText = headers[i];

        void Load()
        {
            grid.Rows.Clear();
            var societies = statusFilter != null
                ? _societyService.GetAllSocieties().Where(s => s.Status == statusFilter)
                : _societyService.GetAllSocieties();
            foreach (var soc in societies)
                grid.Rows.Add(soc.SocietyID, soc.Name, soc.Category, soc.HeadName, soc.Status,
                              soc.MemberCount, soc.CreatedAt.ToString("dd-MMM-yyyy"));
        }
        Load();

        var btnApprove    = new ModernButton { Text = "Approve",      Location = new Point(0,   0), Size = new Size(110, 34), ButtonStyle = ButtonStyle.Success };
        var btnSuspend    = new ModernButton { Text = "Suspend",      Location = new Point(118, 0), Size = new Size(110, 34), ButtonStyle = ButtonStyle.Danger };
        var btnDelete     = new ModernButton { Text = "Delete",       Location = new Point(236, 0), Size = new Size(110, 34), ButtonStyle = ButtonStyle.Danger };
        var btnAssignHead = new ModernButton { Text = "Assign Head",  Location = new Point(360, 0), Size = new Size(130, 34), ButtonStyle = ButtonStyle.Outline };

        var btnActivity   = new ModernButton { Text = "View Activity", Location = new Point(504, 0), Size = new Size(130, 34), ButtonStyle = ButtonStyle.Ghost };

        btnApprove.Click    += (_, _) => ActOnSociety(grid, "Approve", Load);
        btnSuspend.Click    += (_, _) => ActOnSociety(grid, "Suspend", Load);
        btnDelete.Click     += (_, _) => ActOnSociety(grid, "Delete",  Load);
        btnAssignHead.Click += (_, _) =>
        {
            if (grid.SelectedRows.Count == 0) { MessageBox.Show("Select a society first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int societyId = (int)grid.SelectedRows[0].Cells[0].Value;
            ShowAssignHeadDialog(societyId, Load);
        };
        btnActivity.Click   += (_, _) =>
        {
            if (grid.SelectedRows.Count == 0) { MessageBox.Show("Select a society first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int societyId   = (int)grid.SelectedRows[0].Cells[0].Value;
            string socName  = grid.SelectedRows[0].Cells[1].Value?.ToString() ?? "Society";
            ShowSocietyActivityDialog(societyId, socName);
        };

        var bar = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(4) };
        bar.Controls.AddRange(new Control[] { btnApprove, btnSuspend, btnDelete, btnAssignHead, btnActivity });

        tab.Controls.Add(grid);
        tab.Controls.Add(bar);
    }

    private void ActOnSociety(DataGridView grid, string action, Action reload)
    {
        if (grid.SelectedRows.Count == 0) return;
        int id = (int)grid.SelectedRows[0].Cells[0].Value;
        if (MessageBox.Show($"{action} this society?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        bool ok = action switch
        {
            "Approve" => _societyService.ApproveSociety(id, _admin!.AdminID),
            "Suspend" => _societyService.SuspendSociety(id),
            "Delete"  => _societyService.DeleteSociety(id),
            _         => false
        };

        if (ok) reload();
        else MessageBox.Show("Action failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    // ── Event Approvals ───────────────────────────────────────────────
    private Panel BuildEventsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };

        var grid = MakeGrid(8);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.Size   = new Size(page.Width - 20, page.Height - 55);
        page.Resize += (_, _) => grid.Size = new Size(page.Width - 20, page.Height - 55);
        var headers = new[] { "ID", "Title", "Society", "Type", "Date", "Venue", "Status", "Registered" };
        for (int i = 0; i < 8; i++) grid.Columns[i].HeaderText = headers[i];

        void LoadEvents()
        {
            grid.Rows.Clear();
            foreach (var ev in _eventService.GetAllEvents())
                grid.Rows.Add(ev.EventID, ev.Title, ev.SocietyName, ev.EventType,
                              ev.EventDate.ToString("dd-MMM-yyyy"), ev.Venue, ev.Status, ev.RegistrationCount);
        }
        LoadEvents();

        var btnApprove = new ModernButton { Text = "Approve", Location = new Point(0, 0), Size = new Size(110, 34), ButtonStyle = ButtonStyle.Success };
        var btnReject  = new ModernButton { Text = "Cancel",  Location = new Point(118, 0), Size = new Size(110, 34), ButtonStyle = ButtonStyle.Danger };

        btnApprove.Click += (_, _) =>
        {
            if (grid.SelectedRows.Count == 0) return;
            int id = (int)grid.SelectedRows[0].Cells[0].Value;
            _eventService.ApproveEvent(id, _admin!.AdminID);
            LoadEvents();
        };

        btnReject.Click += (_, _) =>
        {
            if (grid.SelectedRows.Count == 0) return;
            int id = (int)grid.SelectedRows[0].Cells[0].Value;
            if (MessageBox.Show("Cancel this event?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _eventService.CancelEvent(id);
            LoadEvents();
        };

        page.Controls.AddRange(new Control[] { btnApprove, btnReject, grid });
        grid.Location = new Point(0, 44);
        return page;
    }

    // ── Reports ───────────────────────────────────────────────────────
    private Panel BuildReportsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };

        var grid = new DataGridView
        {
            Location              = new Point(0, 54),
            Anchor                = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly              = true,
            AllowUserToAddRows    = false,
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.None,
            RowHeadersVisible     = false,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            Font                  = AppTheme.FontSmall
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.EnableHeadersVisualStyles = false;
        page.Resize += (_, _) => grid.Size = new Size(page.Width - 20, page.Height - 60);

        var btnUni = new ModernButton { Text = "University Overview", Location = new Point(0, 0), Size = new Size(200, 38), ButtonStyle = ButtonStyle.Outline };
        btnUni.Click += (_, _) => grid.DataSource = _reportService.GetUniversityReport();

        page.Controls.AddRange(new Control[] { btnUni, grid });
        btnUni.PerformClick();
        return page;
    }

    private void BuildCreateSocietyAdminTab(TabPage tab)
    {
        var card = new RoundedPanel { BackColor = Color.White, Location = new Point(16, 16), Size = new Size(500, 380) };

        var lblName = new Label { Text = "Society Name", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(20, 20) };
        var txtName = new ModernTextBox { Location = new Point(20, 42), Size = new Size(440, 40) };

        var lblCat = new Label { Text = "Category", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(20, 96) };
        var cboCat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(20, 118), Size = new Size(440, 32), Font = AppTheme.FontInput };
        cboCat.Items.AddRange(new object[] { "Technology", "Sports", "Arts & Culture", "Academic", "Social", "Media", "Other" });
        cboCat.SelectedIndex = 0;

        var lblDesc = new Label { Text = "Description", Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(20, 162) };
        var txtDesc = new ModernTextBox { Location = new Point(20, 184), Size = new Size(440, 80), Multiline = true };

        var lblMsg  = new Label { AutoSize = false, Size = new Size(440, 26), Location = new Point(20, 272), Font = AppTheme.FontSmall, TextAlign = ContentAlignment.MiddleLeft };
        var btnCreate = new ModernButton { Text = "CREATE & ACTIVATE SOCIETY", Location = new Point(20, 304), Size = new Size(440, 40) };

        btnCreate.Click += (_, _) =>
        {
            var (ok, msg, _) = _societyService.AdminCreateSociety(txtName.Text, txtDesc.Text, cboCat.Text, _admin!.AdminID);
            lblMsg.ForeColor = ok ? AppTheme.Success : AppTheme.Danger;
            lblMsg.Text      = msg;
            if (ok) { txtName.Text = ""; txtDesc.Text = ""; }
        };

        card.Controls.AddRange(new Control[] { lblName, txtName, lblCat, cboCat, lblDesc, txtDesc, lblMsg, btnCreate });
        tab.Controls.Add(card);
    }

    private void ShowSocietyActivityDialog(int societyId, string societyName)
    {
        var dlg = new Form
        {
            Text            = $"Activity — {societyName}",
            Size            = new Size(760, 520),
            StartPosition   = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.Sizable,
            BackColor       = AppTheme.Background,
            Font            = AppTheme.FontBody
        };

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = AppTheme.FontBody };

        // Events tab
        var tabEvents = new TabPage("Events");
        var evGrid    = MakeGrid(6);
        evGrid.Dock   = DockStyle.Fill;
        evGrid.Columns[0].HeaderText = "Title";
        evGrid.Columns[1].HeaderText = "Type";
        evGrid.Columns[2].HeaderText = "Date";
        evGrid.Columns[3].HeaderText = "Venue";
        evGrid.Columns[4].HeaderText = "Status";
        evGrid.Columns[5].HeaderText = "Registered";
        foreach (var ev in _eventService.GetEventsBySociety(societyId))
            evGrid.Rows.Add(ev.Title, ev.EventType, ev.EventDate.ToString("dd-MMM-yyyy"), ev.Venue, ev.Status, ev.RegistrationCount);
        tabEvents.Controls.Add(evGrid);

        // Announcements tab
        var tabAnn  = new TabPage("Announcements");
        var annGrid = MakeGrid(4);
        annGrid.Dock = DockStyle.Fill;
        annGrid.Columns[0].HeaderText = "Title";
        annGrid.Columns[1].HeaderText = "Priority";
        annGrid.Columns[2].HeaderText = "Posted By";
        annGrid.Columns[3].HeaderText = "Date";
        foreach (var a in _announceService.GetBySociety(societyId))
            annGrid.Rows.Add(a.Title, a.Priority, a.CreatedByName, a.CreatedAt.ToString("dd-MMM-yyyy"));
        tabAnn.Controls.Add(annGrid);

        tabs.TabPages.AddRange(new[] { tabEvents, tabAnn });
        dlg.Controls.Add(tabs);
        dlg.ShowDialog(this);
    }

    private void ShowAssignHeadDialog(int societyId, Action reload)
    {
        var dlg = new Form
        {
            Text            = "Assign Society Head",
            Size            = new Size(620, 440),
            StartPosition   = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            MinimizeBox     = false,
            BackColor       = AppTheme.Background,
            Font            = AppTheme.FontBody
        };

        var lbl = new Label
        {
            Text      = "Select a student to become the society head.\nTheir role will be changed to Society Head.",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = false,
            Size      = new Size(560, 36),
            Location  = new Point(16, 12)
        };

        var grid = MakeGrid(5);
        grid.Location = new Point(16, 52);
        grid.Size     = new Size(572, 290);
        grid.Columns[0].HeaderText = "ID";
        grid.Columns[1].HeaderText = "Name";
        grid.Columns[2].HeaderText = "Reg #";
        grid.Columns[3].HeaderText = "Department";
        grid.Columns[4].HeaderText = "Email";

        var studentRepo = new Data.Repositories.StudentRepository();
        foreach (var s in studentRepo.GetAll())
            grid.Rows.Add(s.StudentID, s.FullName, s.RegistrationNumber, s.Department, s.Email);

        var btnConfirm = new ModernButton
        {
            Text        = "Assign as Head",
            Location    = new Point(16, 356),
            Size        = new Size(160, 38),
            ButtonStyle = ButtonStyle.Success
        };
        var btnCancel = new ModernButton
        {
            Text        = "Cancel",
            Location    = new Point(184, 356),
            Size        = new Size(100, 38),
            ButtonStyle = ButtonStyle.Ghost
        };

        btnCancel.Click  += (_, _) => dlg.Close();
        btnConfirm.Click += (_, _) =>
        {
            if (grid.SelectedRows.Count == 0) return;
            int studentId = (int)grid.SelectedRows[0].Cells[0].Value;
            var (ok, msg) = _societyService.AssignHead(societyId, studentId);
            MessageBox.Show(msg, ok ? "Success" : "Error", MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            if (ok) { reload(); dlg.Close(); }
        };

        dlg.Controls.AddRange(new Control[] { lbl, grid, btnConfirm, btnCancel });
        dlg.ShowDialog(this);
    }

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
