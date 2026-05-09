using SocietiesManagementSystem.Models;
using SocietiesManagementSystem.Services;
using SocietiesManagementSystem.UI.Controls;
using SocietiesManagementSystem.UI.Forms.Shared;
using SocietiesManagementSystem.UI.Theme;
using SocietyModel = SocietiesManagementSystem.Models.Society;

namespace SocietiesManagementSystem.UI.Forms.Student;

public class StudentDashboard : BaseShellForm
{
    private readonly User          _user;
    private readonly Models.Student _student;
    private readonly SocietyService     _societyService     = new();
    private readonly MembershipService  _membershipService  = new();
    private readonly EventService       _eventService       = new();
    private readonly AnnouncementService _announcementService = new();
    private readonly ReportService      _reportService      = new();

    public StudentDashboard(User user, Models.Student student)
    {
        _user    = user;
        _student = student;
        BuildShell("Student Dashboard", student.FullName, "Student");
        BuildNav();
        ActivateFirstNav();
    }

    private void BuildNav()
    {
        int y = 155;
        AddNavItem("   Dashboard",    y,       () => { LblPageTitle.Text = "Dashboard";         ShowPage(BuildDashboardPage()); });
        AddNavItem("   Browse Societies", y+44, () => { LblPageTitle.Text = "Browse Societies"; ShowPage(BuildBrowseSocietiesPage()); });
        AddNavItem("   My Memberships",  y+88, () => { LblPageTitle.Text = "My Memberships";   ShowPage(BuildMyMembershipsPage()); });
        AddNavItem("   Events",      y+132,    () => { LblPageTitle.Text = "Upcoming Events";   ShowPage(BuildEventsPage()); });
        AddNavItem("   My Tickets",  y+176,    () => { LblPageTitle.Text = "My Event Tickets";  ShowPage(BuildTicketsPage()); });
        AddNavItem("   Announcements", y+220,  () => { LblPageTitle.Text = "Announcements";     ShowPage(BuildAnnouncementsPage()); });
    }

    // ── Dashboard home ────────────────────────────────────────────────
    private Panel BuildDashboardPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        var stats = _reportService.GetStudentDashboardStats(_student.StudentID);

        var greeting = new Label
        {
            Text      = $"Welcome back, {_student.FirstName}!",
            Font      = AppTheme.FontH1,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(0, 0)
        };
        page.Controls.Add(greeting);

        var cards = new[]
        {
            ("Societies", stats.GetValueOrDefault("MemberOf").ToString(),        "⭐", AppTheme.Primary),
            ("Pending",   stats.GetValueOrDefault("PendingRequests").ToString(), "⏳", AppTheme.Warning),
            ("Events",    stats.GetValueOrDefault("UpcomingEvents").ToString(),  "📅", AppTheme.Accent),
            ("Tasks",     stats.GetValueOrDefault("PendingTasks").ToString(),    "✅", AppTheme.Success)
        };

        int cx = 0;
        foreach (var (title, value, icon, color) in cards)
        {
            var card = new StatCard
            {
                Title       = title,
                Value       = value,
                Icon        = icon,
                AccentColor = color,
                Location    = new Point(cx, 40),
                Size        = new Size(195, 110)
            };
            page.Controls.Add(card);
            cx += 210;
        }

        // Upcoming events preview
        var lblEvents = new Label
        {
            Text      = "Upcoming Events",
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(0, 170)
        };
        page.Controls.Add(lblEvents);

        var grid = MakeGrid();
        grid.Location = new Point(0, 200);
        grid.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        page.Controls.Add(grid);
        page.Resize += (_, _) => grid.Width = page.Width;

        var events = _eventService.GetUpcomingEvents().Take(10).ToList();
        foreach (DataGridViewRow? _ in grid.Rows) { }
        grid.Rows.Clear();
        foreach (var ev in events)
        {
            grid.Rows.Add(ev.Title, ev.SocietyName, ev.EventDate.ToString("dd-MMM-yyyy HH:mm"),
                          ev.Venue, ev.MaxParticipants?.ToString() ?? "Open", ev.RegistrationCount);
        }
        grid.Columns[0].HeaderText = "Event";
        grid.Columns[1].HeaderText = "Society";
        grid.Columns[2].HeaderText = "Date & Time";
        grid.Columns[3].HeaderText = "Venue";
        grid.Columns[4].HeaderText = "Capacity";
        grid.Columns[5].HeaderText = "Registered";

        return page;
    }

    // ── Browse Societies ──────────────────────────────────────────────
    private Panel BuildBrowseSocietiesPage()
    {
        var page  = new Panel { BackColor = AppTheme.Background };
        var societies = _societyService.GetActiveSocieties().ToList();

        var flow = new FlowLayoutPanel
        {
            Dock            = DockStyle.Fill,
            AutoScroll      = true,
            FlowDirection   = FlowDirection.LeftToRight,
            WrapContents    = true,
            Padding         = new Padding(0, 5, 0, 5)
        };

        foreach (var soc in societies)
        {
            var card = BuildSocietyCard(soc);
            flow.Controls.Add(card);
        }

        if (societies.Count == 0)
        {
            flow.Controls.Add(new Label
            {
                Text      = "No active societies found.",
                Font      = AppTheme.FontH2,
                ForeColor = AppTheme.TextSecondary,
                AutoSize  = true,
                Margin    = new Padding(20)
            });
        }

        page.Controls.Add(flow);
        return page;
    }

    private RoundedPanel BuildSocietyCard(SocietyModel soc)
    {
        var card = new RoundedPanel
        {
            Size      = new Size(290, 180),
            BackColor = Color.White,
            Margin    = new Padding(0, 0, 12, 12)
        };

        var accent = new Panel
        {
            BackColor = AppTheme.Primary,
            Size      = new Size(290, 6),
            Location  = new Point(0, 0)
        };

        var lblName = new Label
        {
            Text      = soc.Name,
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = false,
            Size      = new Size(260, 40),
            Location  = new Point(16, 16),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var lblCat = new Label
        {
            Text      = soc.Category,
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.Accent,
            AutoSize  = true,
            Location  = new Point(16, 56)
        };

        var lblDesc = new Label
        {
            Text      = soc.Description.Length > 80 ? soc.Description[..80] + "…" : soc.Description,
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = false,
            Size      = new Size(258, 40),
            Location  = new Point(16, 76)
        };

        var lblMembers = new Label
        {
            Text      = $"👥 {soc.MemberCount} members   Head: {soc.HeadName}",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(16, 122)
        };

        var isMember = _membershipService.IsMember(_student.StudentID, soc.SocietyID);
        var hasPending = _membershipService.HasPending(_student.StudentID, soc.SocietyID);

        var btnApply = new ModernButton
        {
            Text        = isMember ? "✓ Member" : hasPending ? "⏳ Pending" : "Apply",
            ButtonStyle = isMember ? ButtonStyle.Success : hasPending ? ButtonStyle.Ghost : ButtonStyle.Accent,
            Enabled     = !isMember && !hasPending,
            Location    = new Point(16, 144),
            Size        = new Size(258, 30),
            Font        = new Font("Segoe UI", 9f, FontStyle.Bold)
        };

        if (!isMember && !hasPending)
        {
            btnApply.Click += (_, _) =>
            {
                var (success, message) = _membershipService.ApplyForMembership(_student.StudentID, soc.SocietyID);
                MessageBox.Show(message, success ? "Success" : "Error",
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                if (success) { btnApply.Text = "⏳ Pending"; btnApply.ButtonStyle = ButtonStyle.Ghost; btnApply.Enabled = false; }
            };
        }

        card.Controls.AddRange(new Control[] { accent, lblName, lblCat, lblDesc, lblMembers, btnApply });
        return card;
    }

    // ── My Memberships ────────────────────────────────────────────────
    private Panel BuildMyMembershipsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };

        var lblTitle = new Label { Text = "My Society Memberships", Font = AppTheme.FontH2, ForeColor = AppTheme.TextPrimary, AutoSize = true, Location = new Point(0, 0) };
        page.Controls.Add(lblTitle);

        var tabControl = new TabControl { Location = new Point(0, 30), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, Font = AppTheme.FontBody };
        tabControl.Resize += (_, _) => tabControl.Size = new Size(page.Width, page.Height - 30);
        page.Resize += (_, _) => tabControl.Size = new Size(page.Width, page.Height - 30);

        var tabMembers  = new TabPage("Active Memberships");
        var tabRequests = new TabPage("My Requests");

        var gridMembers = MakeGrid();
        gridMembers.Dock = DockStyle.Fill;
        var members = _societyService.GetMemberSocieties(_student.StudentID).ToList();
        foreach (var s in members)
            gridMembers.Rows.Add(s.Name, s.Category, s.HeadName, s.MemberCount, s.Status);
        gridMembers.Columns[0].HeaderText = "Society";
        gridMembers.Columns[1].HeaderText = "Category";
        gridMembers.Columns[2].HeaderText = "Head";
        gridMembers.Columns[3].HeaderText = "Members";
        gridMembers.Columns[4].HeaderText = "Status";
        tabMembers.Controls.Add(gridMembers);

        var gridReqs = MakeGrid();
        gridReqs.Dock = DockStyle.Fill;
        var reqs = _membershipService.GetStudentRequests(_student.StudentID).ToList();
        foreach (var r in reqs)
            gridReqs.Rows.Add(r.SocietyName, r.Status, r.RequestedAt.ToString("dd-MMM-yyyy"),
                              r.ProcessedAt?.ToString("dd-MMM-yyyy") ?? "—", r.RejectionReason);
        gridReqs.Columns[0].HeaderText = "Society";
        gridReqs.Columns[1].HeaderText = "Status";
        gridReqs.Columns[2].HeaderText = "Applied";
        gridReqs.Columns[3].HeaderText = "Processed";
        gridReqs.Columns[4].HeaderText = "Reason";
        tabRequests.Controls.Add(gridReqs);

        tabControl.TabPages.AddRange(new[] { tabMembers, tabRequests });
        page.Controls.Add(tabControl);
        return page;
    }

    // ── Events ────────────────────────────────────────────────────────
    private Panel BuildEventsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        var events = _eventService.GetUpcomingEvents().ToList();

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight, WrapContents = true
        };

        foreach (var ev in events)
            flow.Controls.Add(BuildEventCard(ev));

        if (events.Count == 0)
            flow.Controls.Add(new Label { Text = "No upcoming events.", Font = AppTheme.FontH2, ForeColor = AppTheme.TextSecondary, AutoSize = true, Margin = new Padding(20) });

        page.Controls.Add(flow);
        return page;
    }

    private RoundedPanel BuildEventCard(Event ev)
    {
        var card = new RoundedPanel { Size = new Size(360, 210), BackColor = Color.White, Margin = new Padding(0, 0, 12, 12) };

        var top = new Panel { BackColor = AppTheme.Primary, Size = new Size(360, 6), Location = new Point(0, 0) };
        var lblTitle   = new Label { Text = ev.Title, Font = AppTheme.FontH2, ForeColor = AppTheme.TextPrimary, AutoSize = false, Size = new Size(328, 36), Location = new Point(16, 14), TextAlign = ContentAlignment.MiddleLeft };
        var lblSociety = new Label { Text = ev.SocietyName, Font = AppTheme.FontSmall, ForeColor = AppTheme.Accent, AutoSize = true, Location = new Point(16, 50) };
        var lblDate    = new Label { Text = $"📅 {ev.EventDate:dd-MMM-yyyy  HH:mm}", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(16, 72) };
        var lblVenue   = new Label { Text = $"📍 {(string.IsNullOrEmpty(ev.Venue) ? "TBA" : ev.Venue)}", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(16, 92) };
        var lblType    = new Label { Text = $"🏷️ {ev.EventType}   👥 {ev.RegistrationCount}{(ev.MaxParticipants.HasValue ? "/" + ev.MaxParticipants : "")} registered", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(16, 112) };

        var isReg = _eventService.IsRegistered(_student.StudentID, ev.EventID);
        var full  = ev.MaxParticipants.HasValue && ev.RegistrationCount >= ev.MaxParticipants.Value;

        var btnReg = new ModernButton
        {
            Text        = isReg ? "✓ Registered" : full ? "Fully Booked" : "Register",
            ButtonStyle = isReg ? ButtonStyle.Success : full ? ButtonStyle.Ghost : ButtonStyle.Accent,
            Enabled     = !isReg && !full,
            Location    = new Point(16, 165),
            Size        = new Size(328, 34),
            Font        = new Font("Segoe UI", 9f, FontStyle.Bold)
        };

        if (!isReg && !full)
        {
            btnReg.Click += (_, _) =>
            {
                var (success, msg, ticket) = _eventService.RegisterForEvent(_student.StudentID, ev.EventID);
                MessageBox.Show(success ? $"{msg}\n\nTicket: {ticket}" : msg,
                    success ? "Registered!" : "Error",
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                if (success) { btnReg.Text = "✓ Registered"; btnReg.ButtonStyle = ButtonStyle.Success; btnReg.Enabled = false; }
            };
        }

        card.Controls.AddRange(new Control[] { top, lblTitle, lblSociety, lblDate, lblVenue, lblType, btnReg });
        return card;
    }

    // ── My Tickets ────────────────────────────────────────────────────
    private Panel BuildTicketsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        var registrations = _eventService.GetStudentRegistrations(_student.StudentID).ToList();

        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };

        foreach (var reg in registrations)
            flow.Controls.Add(BuildTicketCard(reg));

        if (registrations.Count == 0)
            flow.Controls.Add(new Label { Text = "No event registrations.", Font = AppTheme.FontH2, ForeColor = AppTheme.TextSecondary, AutoSize = true, Margin = new Padding(20) });

        page.Controls.Add(flow);
        return page;
    }

    private RoundedPanel BuildTicketCard(EventRegistration reg)
    {
        var card = new RoundedPanel { Size = new Size(370, 160), BackColor = Color.White, Margin = new Padding(0, 0, 12, 12) };

        var statusColor = AppTheme.GetStatusColor(reg.AttendanceStatus);
        var strip = new Panel { BackColor = statusColor, Size = new Size(6, 160), Location = new Point(0, 0) };

        var lblTitle    = new Label { Text = reg.EventTitle, Font = AppTheme.FontH2, ForeColor = AppTheme.TextPrimary, AutoSize = false, Size = new Size(340, 32), Location = new Point(18, 10) };
        var lblSociety  = new Label { Text = reg.SocietyName, Font = AppTheme.FontSmall, ForeColor = AppTheme.Accent, AutoSize = true, Location = new Point(18, 44) };
        var lblDate     = new Label { Text = $"📅 {reg.EventDate:dd-MMM-yyyy  HH:mm}", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(18, 64) };
        var lblVenue    = new Label { Text = $"📍 {(string.IsNullOrEmpty(reg.Venue) ? "TBA" : reg.Venue)}", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(18, 84) };

        var ticketBox = new RoundedPanel { BackColor = Color.FromArgb(245, 250, 255), Size = new Size(340, 36), Location = new Point(18, 110), CornerRadius = 4 };
        var lblTicket = new Label { Text = $"🎫 {reg.TicketCode}", Font = new Font("Courier New", 9f, FontStyle.Bold), ForeColor = AppTheme.Primary, AutoSize = false, Size = new Size(200, 36), Location = new Point(8, 0), TextAlign = ContentAlignment.MiddleLeft };
        var lblStatus = new Label { Text = reg.AttendanceStatus, Font = AppTheme.FontBadge, ForeColor = statusColor, AutoSize = false, Size = new Size(110, 36), Location = new Point(218, 0), TextAlign = ContentAlignment.MiddleRight };
        ticketBox.Controls.AddRange(new Control[] { lblTicket, lblStatus });

        card.Controls.AddRange(new Control[] { strip, lblTitle, lblSociety, lblDate, lblVenue, ticketBox });
        return card;
    }

    // ── Announcements ─────────────────────────────────────────────────
    private Panel BuildAnnouncementsPage()
    {
        var page = new Panel { BackColor = AppTheme.Background };
        var members = _societyService.GetMemberSocieties(_student.StudentID).Select(s => s.SocietyID).ToList();
        var announcements = _announcementService.GetForStudent(members).ToList();

        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };

        foreach (var ann in announcements)
            flow.Controls.Add(BuildAnnouncementCard(ann, flow));

        if (announcements.Count == 0)
            flow.Controls.Add(new Label { Text = "No announcements from your societies.", Font = AppTheme.FontH2, ForeColor = AppTheme.TextSecondary, AutoSize = true, Margin = new Padding(20) });

        page.Controls.Add(flow);
        return page;
    }

    private RoundedPanel BuildAnnouncementCard(Announcement ann, FlowLayoutPanel parent)
    {
        var card = new RoundedPanel { BackColor = Color.White, Margin = new Padding(0, 0, 0, 12) };
        parent.Resize += (_, _) => card.Width = parent.Width - 20;
        card.Width  = 800;
        card.Height = 110;

        var priorityColor = AppTheme.GetPriorityColor(ann.Priority);
        var strip = new Panel { BackColor = priorityColor, Size = new Size(5, 110), Location = new Point(0, 0) };

        var lblTitle   = new Label { Text = ann.Title, Font = AppTheme.FontH2, ForeColor = AppTheme.TextPrimary, AutoSize = false, Size = new Size(card.Width - 30, 28), Location = new Point(18, 10) };
        var lblMeta    = new Label { Text = $"{ann.SocietyName}  ·  {ann.CreatedByName}  ·  {ann.CreatedAt:dd-MMM-yyyy}", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary, AutoSize = true, Location = new Point(18, 38) };
        var lblContent = new Label { Text = ann.Content, Font = AppTheme.FontBody, ForeColor = AppTheme.TextPrimary, AutoSize = false, Size = new Size(card.Width - 30, 46), Location = new Point(18, 58) };
        var lblPriority = new Label { Text = ann.Priority, Font = AppTheme.FontBadge, ForeColor = priorityColor, AutoSize = true, Location = new Point(card.Width - 80, 14), Anchor = AnchorStyles.Top | AnchorStyles.Right };

        card.Controls.AddRange(new Control[] { strip, lblTitle, lblMeta, lblContent, lblPriority });
        return card;
    }

    // ── Helper ────────────────────────────────────────────────────────
    private static DataGridView MakeGrid()
    {
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
            Height                = 380
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor  = AppTheme.Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor  = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9f, FontStyle.Bold);
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 36;
        grid.EnableHeadersVisualStyles = false;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        return grid;
    }
}
