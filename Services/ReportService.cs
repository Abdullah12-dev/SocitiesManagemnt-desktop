using System.Data;
using Microsoft.Data.SqlClient;
using SocietiesManagementSystem.Data;

namespace SocietiesManagementSystem.Services;

public class ReportService
{
    private readonly DatabaseConnection _db = DatabaseConnection.Instance;

    public Dictionary<string, int> GetAdminDashboardStats()
    {
        const string sql = @"
            SELECT
                (SELECT COUNT(*) FROM Students s INNER JOIN Users u ON u.UserID = s.UserID WHERE u.IsActive=1) AS TotalStudents,
                (SELECT COUNT(*) FROM Societies WHERE Status='Active') AS ActiveSocieties,
                (SELECT COUNT(*) FROM Societies WHERE Status='Pending') AS PendingSocieties,
                (SELECT COUNT(*) FROM Events WHERE Status='Pending') AS PendingEvents,
                (SELECT COUNT(*) FROM Events WHERE Status='Approved' AND EventDate >= GETDATE()) AS UpcomingEvents,
                (SELECT COUNT(*) FROM MembershipRequests WHERE Status='Pending') AS PendingMemberships";

        var stats = new Dictionary<string, int>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();
        if (r.Read())
        {
            stats["TotalStudents"]      = r.GetInt32(0);
            stats["ActiveSocieties"]    = r.GetInt32(1);
            stats["PendingSocieties"]   = r.GetInt32(2);
            stats["PendingEvents"]      = r.GetInt32(3);
            stats["UpcomingEvents"]     = r.GetInt32(4);
            stats["PendingMemberships"] = r.GetInt32(5);
        }
        return stats;
    }

    public Dictionary<string, int> GetStudentDashboardStats(int studentId)
    {
        const string sql = @"
            SELECT
                (SELECT COUNT(*) FROM SocietyMembers WHERE StudentID=@SID AND IsActive=1) AS MemberOf,
                (SELECT COUNT(*) FROM MembershipRequests WHERE StudentID=@SID AND Status='Pending') AS PendingRequests,
                (SELECT COUNT(*) FROM EventRegistrations er
                 INNER JOIN Events e ON e.EventID=er.EventID
                 WHERE er.StudentID=@SID AND e.EventDate >= GETDATE()) AS UpcomingEvents,
                (SELECT COUNT(*) FROM Tasks WHERE AssignedToStudentID=@SID AND Status IN('Pending','InProgress')) AS PendingTasks";

        var stats = new Dictionary<string, int>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SID", studentId);
        using var r = cmd.ExecuteReader();
        if (r.Read())
        {
            stats["MemberOf"]        = r.GetInt32(0);
            stats["PendingRequests"] = r.GetInt32(1);
            stats["UpcomingEvents"]  = r.GetInt32(2);
            stats["PendingTasks"]    = r.GetInt32(3);
        }
        return stats;
    }

    public Dictionary<string, int> GetSocietyDashboardStats(int societyId)
    {
        const string sql = @"
            SELECT
                (SELECT COUNT(*) FROM SocietyMembers WHERE SocietyID=@SocID AND IsActive=1) AS Members,
                (SELECT COUNT(*) FROM MembershipRequests WHERE SocietyID=@SocID AND Status='Pending') AS PendingRequests,
                (SELECT COUNT(*) FROM Events WHERE SocietyID=@SocID AND Status='Approved') AS ApprovedEvents,
                (SELECT COUNT(*) FROM Tasks WHERE SocietyID=@SocID AND Status='Pending') AS PendingTasks";

        var stats = new Dictionary<string, int>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SocID", societyId);
        using var r = cmd.ExecuteReader();
        if (r.Read())
        {
            stats["Members"]         = r.GetInt32(0);
            stats["PendingRequests"] = r.GetInt32(1);
            stats["ApprovedEvents"]  = r.GetInt32(2);
            stats["PendingTasks"]    = r.GetInt32(3);
        }
        return stats;
    }

    public DataTable GetSocietyMembersReport(int societyId)
    {
        const string sql = @"
            SELECT s.RegistrationNumber AS 'Reg Number',
                   s.FirstName + ' ' + s.LastName AS 'Name',
                   s.Department, s.Semester, sm.Role,
                   FORMAT(sm.JoinedAt, 'dd-MMM-yyyy') AS 'Joined'
            FROM SocietyMembers sm
            INNER JOIN Students s ON s.StudentID = sm.StudentID
            WHERE sm.SocietyID = @SocID AND sm.IsActive = 1
            ORDER BY sm.JoinedAt";

        return ExecuteDataTable(sql, ("@SocID", societyId));
    }

    public DataTable GetSocietyEventsReport(int societyId)
    {
        const string sql = @"
            SELECT e.Title AS 'Event', e.EventType AS 'Type',
                   FORMAT(e.EventDate,'dd-MMM-yyyy HH:mm') AS 'Date',
                   ISNULL(e.Venue,'TBA') AS 'Venue', e.Status,
                   (SELECT COUNT(*) FROM EventRegistrations er WHERE er.EventID=e.EventID) AS 'Registrations',
                   ISNULL(CAST(e.MaxParticipants AS NVARCHAR),'Open') AS 'Capacity'
            FROM Events e
            WHERE e.SocietyID=@SocID AND e.Status != 'Cancelled'
            ORDER BY e.EventDate DESC";
        return ExecuteDataTable(sql, ("@SocID", societyId));
    }

    public DataTable GetEventRegistrationsReport(int eventId)
    {
        const string sql = @"
            SELECT s.RegistrationNumber AS 'Reg Number',
                   s.FirstName + ' ' + s.LastName AS 'Student Name',
                   s.Department, er.TicketCode AS 'Ticket',
                   FORMAT(er.RegisteredAt, 'dd-MMM-yyyy HH:mm') AS 'Registered At',
                   er.AttendanceStatus AS 'Attendance'
            FROM EventRegistrations er
            INNER JOIN Students s ON s.StudentID = er.StudentID
            WHERE er.EventID = @EID ORDER BY er.RegisteredAt";

        return ExecuteDataTable(sql, ("@EID", eventId));
    }

    public DataTable GetUniversityReport()
    {
        const string sql = @"
            SELECT soc.Name AS 'Society', soc.Status, soc.Category,
                   (SELECT COUNT(*) FROM SocietyMembers sm WHERE sm.SocietyID=soc.SocietyID AND sm.IsActive=1) AS 'Members',
                   (SELECT COUNT(*) FROM Events e WHERE e.SocietyID=soc.SocietyID AND e.Status='Completed') AS 'Events Held',
                   FORMAT(soc.CreatedAt,'dd-MMM-yyyy') AS 'Created'
            FROM Societies soc WHERE soc.Status != 'Deleted'
            ORDER BY soc.Name";

        return ExecuteDataTable(sql);
    }

    private System.Data.DataTable ExecuteDataTable(string sql, params (string name, object value)[] parameters)
    {
        var dt = new System.Data.DataTable();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value);
        using var adapter = new SqlDataAdapter(cmd);
        adapter.Fill(dt);
        return dt;
    }
}
