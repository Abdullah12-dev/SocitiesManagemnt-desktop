using Microsoft.Data.SqlClient;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Data.Repositories;

public class AnnouncementRepository : IRepository<Announcement>
{
    private readonly DatabaseConnection _db = DatabaseConnection.Instance;

    private const string SelectBase = @"
        SELECT a.AnnouncementID, a.SocietyID, a.Title, a.Content, a.CreatedByStudentID,
               a.Priority, a.IsActive, a.CreatedAt,
               soc.Name AS SocietyName,
               s.FirstName + ' ' + s.LastName AS CreatedByName
        FROM Announcements a
        INNER JOIN Societies soc ON soc.SocietyID = a.SocietyID
        INNER JOIN Students s ON s.StudentID = a.CreatedByStudentID";

    public Announcement? GetById(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE a.AnnouncementID = @ID", conn);
        cmd.Parameters.AddWithValue("@ID", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapAnnouncement(r) : null;
    }

    public IEnumerable<Announcement> GetAll()
    {
        var list = new List<Announcement>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE a.IsActive=1 ORDER BY a.CreatedAt DESC", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapAnnouncement(r));
        return list;
    }

    public IEnumerable<Announcement> GetBySociety(int societyId)
    {
        var list = new List<Announcement>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE a.SocietyID=@SocID AND a.IsActive=1 ORDER BY a.CreatedAt DESC", conn);
        cmd.Parameters.AddWithValue("@SocID", societyId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapAnnouncement(r));
        return list;
    }

    public IEnumerable<Announcement> GetBySocieties(IEnumerable<int> societyIds)
    {
        var list = new List<Announcement>();
        if (!societyIds.Any()) return list;
        var ids = string.Join(",", societyIds);
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE a.SocietyID IN ({ids}) AND a.IsActive=1 ORDER BY a.CreatedAt DESC", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapAnnouncement(r));
        return list;
    }

    public int Insert(Announcement ann)
    {
        const string sql = @"
            INSERT INTO Announcements (SocietyID, Title, Content, CreatedByStudentID, Priority)
            VALUES (@SocID, @Title, @Content, @By, @Priority);
            SELECT SCOPE_IDENTITY();";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SocID",    ann.SocietyID);
        cmd.Parameters.AddWithValue("@Title",    ann.Title);
        cmd.Parameters.AddWithValue("@Content",  ann.Content);
        cmd.Parameters.AddWithValue("@By",       ann.CreatedByStudentID);
        cmd.Parameters.AddWithValue("@Priority", ann.Priority);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Update(Announcement ann)
    {
        const string sql = "UPDATE Announcements SET Title=@Title, Content=@Content, Priority=@Priority WHERE AnnouncementID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Title",    ann.Title);
        cmd.Parameters.AddWithValue("@Content",  ann.Content);
        cmd.Parameters.AddWithValue("@Priority", ann.Priority);
        cmd.Parameters.AddWithValue("@ID",       ann.AnnouncementID);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        const string sql = "UPDATE Announcements SET IsActive=0 WHERE AnnouncementID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    private static Announcement MapAnnouncement(SqlDataReader r) => new()
    {
        AnnouncementID     = r.GetInt32(0),
        SocietyID          = r.GetInt32(1),
        Title              = r.GetString(2),
        Content            = r.GetString(3),
        CreatedByStudentID = r.GetInt32(4),
        Priority           = r.GetString(5),
        IsActive           = r.GetBoolean(6),
        CreatedAt          = r.GetDateTime(7),
        SocietyName        = r.GetString(8),
        CreatedByName      = r.GetString(9)
    };
}
