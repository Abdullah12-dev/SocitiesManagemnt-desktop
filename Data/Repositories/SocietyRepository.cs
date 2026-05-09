using Microsoft.Data.SqlClient;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Data.Repositories;

public class SocietyRepository : IRepository<Society>
{
    private readonly DatabaseConnection _db = DatabaseConnection.Instance;

    private const string SelectBase = @"
        SELECT soc.SocietyID, soc.Name, soc.Description, soc.Category,
               soc.HeadStudentID, soc.Status, soc.CreatedAt, soc.ApprovedByAdminID,
               ISNULL(s.FirstName + ' ' + s.LastName, 'N/A') AS HeadName,
               (SELECT COUNT(*) FROM SocietyMembers sm WHERE sm.SocietyID = soc.SocietyID AND sm.IsActive = 1) AS MemberCount
        FROM Societies soc
        LEFT JOIN Students s ON s.StudentID = soc.HeadStudentID";

    public Society? GetById(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE soc.SocietyID = @ID", conn);
        cmd.Parameters.AddWithValue("@ID", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapSociety(r) : null;
    }

    public IEnumerable<Society> GetAll()
    {
        var list = new List<Society>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE soc.Status != 'Deleted' ORDER BY soc.Name", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapSociety(r));
        return list;
    }

    public IEnumerable<Society> GetActive()
    {
        var list = new List<Society>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE soc.Status = 'Active' ORDER BY soc.Name", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapSociety(r));
        return list;
    }

    public IEnumerable<Society> GetByHeadStudent(int studentId)
    {
        var list = new List<Society>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE soc.HeadStudentID = @SID AND soc.Status != 'Deleted' ORDER BY soc.Name", conn);
        cmd.Parameters.AddWithValue("@SID", studentId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapSociety(r));
        return list;
    }

    public int Insert(Society s)
    {
        const string sql = @"
            INSERT INTO Societies (Name, Description, Category, HeadStudentID, Status)
            VALUES (@Name, @Desc, @Cat, @Head, 'Pending');
            SELECT SCOPE_IDENTITY();";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", s.Name);
        cmd.Parameters.AddWithValue("@Desc", (object?)s.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Cat",  (object?)s.Category ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Head", (object?)s.HeadStudentID ?? DBNull.Value);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Update(Society s)
    {
        const string sql = @"
            UPDATE Societies SET Name=@Name, Description=@Desc, Category=@Cat,
                HeadStudentID=@Head WHERE SocietyID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", s.Name);
        cmd.Parameters.AddWithValue("@Desc", (object?)s.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Cat",  (object?)s.Category ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Head", (object?)s.HeadStudentID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ID",   s.SocietyID);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdateStatus(int societyId, string status, int? adminId = null)
    {
        const string sql = @"
            UPDATE Societies SET Status=@Status,
                ApprovedByAdminID = CASE WHEN @AdminID IS NULL THEN ApprovedByAdminID ELSE @AdminID END
            WHERE SocietyID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Status",  status);
        cmd.Parameters.AddWithValue("@AdminID", (object?)adminId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ID",      societyId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool AssignHead(int societyId, int studentId)
    {
        const string sql = "UPDATE Societies SET HeadStudentID=@Head WHERE SocietyID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Head", studentId);
        cmd.Parameters.AddWithValue("@ID",   societyId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        return UpdateStatus(id, "Deleted");
    }

    private static Society MapSociety(SqlDataReader r) => new()
    {
        SocietyID         = r.GetInt32(0),
        Name              = r.GetString(1),
        Description       = r.IsDBNull(2) ? "" : r.GetString(2),
        Category          = r.IsDBNull(3) ? "" : r.GetString(3),
        HeadStudentID     = r.IsDBNull(4) ? null : r.GetInt32(4),
        Status            = r.GetString(5),
        CreatedAt         = r.GetDateTime(6),
        ApprovedByAdminID = r.IsDBNull(7) ? null : r.GetInt32(7),
        HeadName          = r.GetString(8),
        MemberCount       = r.GetInt32(9)
    };
}
