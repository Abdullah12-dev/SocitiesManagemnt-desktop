using Microsoft.Data.SqlClient;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Data.Repositories;

public class EventRegistrationRepository : IRepository<EventRegistration>
{
    private readonly DatabaseConnection _db = DatabaseConnection.Instance;

    private const string SelectBase = @"
        SELECT er.RegistrationID, er.EventID, er.StudentID, er.RegisteredAt,
               er.TicketCode, er.AttendanceStatus,
               e.Title AS EventTitle, e.EventDate, e.Venue,
               soc.Name AS SocietyName,
               s.FirstName + ' ' + s.LastName AS StudentName, s.RegistrationNumber
        FROM EventRegistrations er
        INNER JOIN Events e ON e.EventID = er.EventID
        INNER JOIN Societies soc ON soc.SocietyID = e.SocietyID
        INNER JOIN Students s ON s.StudentID = er.StudentID";

    public EventRegistration? GetById(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE er.RegistrationID = @ID", conn);
        cmd.Parameters.AddWithValue("@ID", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapRegistration(r) : null;
    }

    public IEnumerable<EventRegistration> GetAll()
    {
        var list = new List<EventRegistration>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} ORDER BY er.RegisteredAt DESC", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapRegistration(r));
        return list;
    }

    public IEnumerable<EventRegistration> GetByStudent(int studentId)
    {
        var list = new List<EventRegistration>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE er.StudentID = @SID ORDER BY e.EventDate DESC", conn);
        cmd.Parameters.AddWithValue("@SID", studentId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapRegistration(r));
        return list;
    }

    public IEnumerable<EventRegistration> GetByEvent(int eventId)
    {
        var list = new List<EventRegistration>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE er.EventID = @EID ORDER BY er.RegisteredAt", conn);
        cmd.Parameters.AddWithValue("@EID", eventId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapRegistration(r));
        return list;
    }

    public bool IsRegistered(int studentId, int eventId)
    {
        const string sql = "SELECT COUNT(1) FROM EventRegistrations WHERE StudentID=@SID AND EventID=@EID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SID", studentId);
        cmd.Parameters.AddWithValue("@EID", eventId);
        return (int)cmd.ExecuteScalar() > 0;
    }

    public int Insert(EventRegistration reg)
    {
        const string sql = @"
            INSERT INTO EventRegistrations (EventID, StudentID, TicketCode, AttendanceStatus)
            VALUES (@EID, @SID, @Ticket, 'Registered');
            SELECT SCOPE_IDENTITY();";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@EID",    reg.EventID);
        cmd.Parameters.AddWithValue("@SID",    reg.StudentID);
        cmd.Parameters.AddWithValue("@Ticket", reg.TicketCode);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool UpdateAttendance(int registrationId, string status)
    {
        const string sql = "UPDATE EventRegistrations SET AttendanceStatus=@Status WHERE RegistrationID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@ID",     registrationId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Update(EventRegistration entity) => false;
    public bool Delete(int id) => false;

    private static EventRegistration MapRegistration(SqlDataReader r) => new()
    {
        RegistrationID    = r.GetInt32(0),
        EventID           = r.GetInt32(1),
        StudentID         = r.GetInt32(2),
        RegisteredAt      = r.GetDateTime(3),
        TicketCode        = r.GetString(4),
        AttendanceStatus  = r.GetString(5),
        EventTitle        = r.GetString(6),
        EventDate         = r.GetDateTime(7),
        Venue             = r.IsDBNull(8) ? "" : r.GetString(8),
        SocietyName       = r.GetString(9),
        StudentName       = r.GetString(10),
        RegistrationNumber= r.GetString(11)
    };
}
