using Microsoft.Data.SqlClient;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Data.Repositories;

public class EventRepository : IRepository<Event>
{
    private readonly DatabaseConnection _db = DatabaseConnection.Instance;

    private const string SelectBase = @"
        SELECT e.EventID, e.SocietyID, e.Title, e.Description, e.EventDate, e.EndDate,
               e.Venue, e.MaxParticipants, e.EventType, e.Status, e.CreatedAt, e.ApprovedByAdminID,
               soc.Name AS SocietyName,
               (SELECT COUNT(*) FROM EventRegistrations er WHERE er.EventID = e.EventID) AS RegCount
        FROM Events e
        INNER JOIN Societies soc ON soc.SocietyID = e.SocietyID";

    public Event? GetById(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE e.EventID = @ID", conn);
        cmd.Parameters.AddWithValue("@ID", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapEvent(r) : null;
    }

    public IEnumerable<Event> GetAll()
    {
        var list = new List<Event>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} ORDER BY e.EventDate DESC", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapEvent(r));
        return list;
    }

    public IEnumerable<Event> GetUpcoming()
    {
        var list = new List<Event>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE e.Status='Approved' AND e.EventDate >= GETDATE() ORDER BY e.EventDate", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapEvent(r));
        return list;
    }

    public IEnumerable<Event> GetBySociety(int societyId)
    {
        var list = new List<Event>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE e.SocietyID = @SocID ORDER BY e.EventDate DESC", conn);
        cmd.Parameters.AddWithValue("@SocID", societyId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapEvent(r));
        return list;
    }

    public IEnumerable<Event> GetPending()
    {
        var list = new List<Event>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE e.Status='Pending' ORDER BY e.CreatedAt", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapEvent(r));
        return list;
    }

    public int Insert(Event ev)
    {
        const string sql = @"
            INSERT INTO Events (SocietyID, Title, Description, EventDate, EndDate, Venue, MaxParticipants, EventType)
            VALUES (@SocID, @Title, @Desc, @Date, @EndDate, @Venue, @Max, @Type);
            SELECT SCOPE_IDENTITY();";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SocID",   ev.SocietyID);
        cmd.Parameters.AddWithValue("@Title",   ev.Title);
        cmd.Parameters.AddWithValue("@Desc",    (object?)ev.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Date",    ev.EventDate);
        cmd.Parameters.AddWithValue("@EndDate", (object?)ev.EndDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Venue",   (object?)ev.Venue ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Max",     (object?)ev.MaxParticipants ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Type",    (object?)ev.EventType ?? DBNull.Value);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Update(Event ev)
    {
        const string sql = @"
            UPDATE Events SET Title=@Title, Description=@Desc, EventDate=@Date,
                EndDate=@EndDate, Venue=@Venue, MaxParticipants=@Max, EventType=@Type
            WHERE EventID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Title",   ev.Title);
        cmd.Parameters.AddWithValue("@Desc",    (object?)ev.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Date",    ev.EventDate);
        cmd.Parameters.AddWithValue("@EndDate", (object?)ev.EndDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Venue",   (object?)ev.Venue ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Max",     (object?)ev.MaxParticipants ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Type",    (object?)ev.EventType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ID",      ev.EventID);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdateStatus(int eventId, string status, int? adminId = null)
    {
        const string sql = @"
            UPDATE Events SET Status=@Status,
                ApprovedByAdminID = CASE WHEN @AdminID IS NULL THEN ApprovedByAdminID ELSE @AdminID END
            WHERE EventID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Status",  status);
        cmd.Parameters.AddWithValue("@AdminID", (object?)adminId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ID",      eventId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        return UpdateStatus(id, "Cancelled");
    }

    private static Event MapEvent(SqlDataReader r) => new()
    {
        EventID           = r.GetInt32(0),
        SocietyID         = r.GetInt32(1),
        Title             = r.GetString(2),
        Description       = r.IsDBNull(3)  ? "" : r.GetString(3),
        EventDate         = r.GetDateTime(4),
        EndDate           = r.IsDBNull(5)  ? null : r.GetDateTime(5),
        Venue             = r.IsDBNull(6)  ? "" : r.GetString(6),
        MaxParticipants   = r.IsDBNull(7)  ? null : r.GetInt32(7),
        EventType         = r.IsDBNull(8)  ? "" : r.GetString(8),
        Status            = r.GetString(9),
        CreatedAt         = r.GetDateTime(10),
        ApprovedByAdminID = r.IsDBNull(11) ? null : r.GetInt32(11),
        SocietyName       = r.GetString(12),
        RegistrationCount = r.GetInt32(13)
    };
}
