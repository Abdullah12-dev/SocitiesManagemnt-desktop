using Microsoft.Data.SqlClient;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Data.Repositories;

public class TaskRepository : IRepository<SocietyTask>
{
    private readonly DatabaseConnection _db = DatabaseConnection.Instance;

    private const string SelectBase = @"
        SELECT t.TaskID, t.SocietyID, t.Title, t.Description,
               t.AssignedToStudentID, t.AssignedByStudentID,
               t.DueDate, t.Status, t.CreatedAt, t.CompletedAt,
               soc.Name AS SocietyName,
               sa.FirstName + ' ' + sa.LastName AS AssignedToName,
               sb.FirstName + ' ' + sb.LastName AS AssignedByName
        FROM Tasks t
        INNER JOIN Societies soc ON soc.SocietyID = t.SocietyID
        INNER JOIN Students sa ON sa.StudentID = t.AssignedToStudentID
        INNER JOIN Students sb ON sb.StudentID = t.AssignedByStudentID";

    public SocietyTask? GetById(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE t.TaskID = @ID", conn);
        cmd.Parameters.AddWithValue("@ID", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapTask(r) : null;
    }

    public IEnumerable<SocietyTask> GetAll()
    {
        var list = new List<SocietyTask>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} ORDER BY t.CreatedAt DESC", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapTask(r));
        return list;
    }

    public IEnumerable<SocietyTask> GetBySociety(int societyId)
    {
        var list = new List<SocietyTask>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE t.SocietyID = @SocID ORDER BY t.DueDate", conn);
        cmd.Parameters.AddWithValue("@SocID", societyId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapTask(r));
        return list;
    }

    public IEnumerable<SocietyTask> GetByStudent(int studentId)
    {
        var list = new List<SocietyTask>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE t.AssignedToStudentID = @SID ORDER BY t.DueDate", conn);
        cmd.Parameters.AddWithValue("@SID", studentId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapTask(r));
        return list;
    }

    public int Insert(SocietyTask task)
    {
        const string sql = @"
            INSERT INTO Tasks (SocietyID, Title, Description, AssignedToStudentID, AssignedByStudentID, DueDate)
            VALUES (@SocID, @Title, @Desc, @AssTo, @AssBy, @Due);
            SELECT SCOPE_IDENTITY();";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SocID", task.SocietyID);
        cmd.Parameters.AddWithValue("@Title", task.Title);
        cmd.Parameters.AddWithValue("@Desc",  (object?)task.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AssTo", task.AssignedToStudentID);
        cmd.Parameters.AddWithValue("@AssBy", task.AssignedByStudentID);
        cmd.Parameters.AddWithValue("@Due",   (object?)task.DueDate ?? DBNull.Value);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Update(SocietyTask task)
    {
        const string sql = @"
            UPDATE Tasks SET Title=@Title, Description=@Desc, AssignedToStudentID=@AssTo,
                DueDate=@Due, Status=@Status,
                CompletedAt = CASE WHEN @Status='Completed' THEN GETDATE() ELSE CompletedAt END
            WHERE TaskID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Title",  task.Title);
        cmd.Parameters.AddWithValue("@Desc",   (object?)task.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AssTo",  task.AssignedToStudentID);
        cmd.Parameters.AddWithValue("@Due",    (object?)task.DueDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Status", task.Status);
        cmd.Parameters.AddWithValue("@ID",     task.TaskID);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdateStatus(int taskId, string status)
    {
        const string sql = @"
            UPDATE Tasks SET Status=@Status,
                CompletedAt = CASE WHEN @Status='Completed' THEN GETDATE() ELSE CompletedAt END
            WHERE TaskID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@ID",     taskId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        return UpdateStatus(id, "Cancelled");
    }

    private static SocietyTask MapTask(SqlDataReader r) => new()
    {
        TaskID              = r.GetInt32(0),
        SocietyID           = r.GetInt32(1),
        Title               = r.GetString(2),
        Description         = r.IsDBNull(3)  ? "" : r.GetString(3),
        AssignedToStudentID = r.GetInt32(4),
        AssignedByStudentID = r.GetInt32(5),
        DueDate             = r.IsDBNull(6)  ? null : r.GetDateTime(6),
        Status              = r.GetString(7),
        CreatedAt           = r.GetDateTime(8),
        CompletedAt         = r.IsDBNull(9)  ? null : r.GetDateTime(9),
        SocietyName         = r.GetString(10),
        AssignedToName      = r.GetString(11),
        AssignedByName      = r.GetString(12)
    };
}
