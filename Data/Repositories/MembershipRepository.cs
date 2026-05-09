using Microsoft.Data.SqlClient;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Data.Repositories;

public class MembershipRepository : IRepository<MembershipRequest>
{
    private readonly DatabaseConnection _db = DatabaseConnection.Instance;

    public MembershipRequest? GetById(int id)
    {
        const string sql = @"
            SELECT mr.RequestID, mr.StudentID, mr.SocietyID, mr.Status, mr.RequestedAt,
                   mr.ProcessedAt, mr.RejectionReason,
                   s.FirstName + ' ' + s.LastName AS StudentName, s.RegistrationNumber,
                   soc.Name AS SocietyName
            FROM MembershipRequests mr
            INNER JOIN Students s ON s.StudentID = mr.StudentID
            INNER JOIN Societies soc ON soc.SocietyID = mr.SocietyID
            WHERE mr.RequestID = @ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapRequest(r) : null;
    }

    public IEnumerable<MembershipRequest> GetAll()
    {
        return GetByStatus(null);
    }

    public IEnumerable<MembershipRequest> GetByStatus(string? status)
    {
        var list = new List<MembershipRequest>();
        var where = status != null ? "AND mr.Status = @Status" : "";
        var sql = $@"
            SELECT mr.RequestID, mr.StudentID, mr.SocietyID, mr.Status, mr.RequestedAt,
                   mr.ProcessedAt, mr.RejectionReason,
                   s.FirstName + ' ' + s.LastName AS StudentName, s.RegistrationNumber,
                   soc.Name AS SocietyName
            FROM MembershipRequests mr
            INNER JOIN Students s ON s.StudentID = mr.StudentID
            INNER JOIN Societies soc ON soc.SocietyID = mr.SocietyID
            WHERE 1=1 {where}
            ORDER BY mr.RequestedAt DESC";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        if (status != null) cmd.Parameters.AddWithValue("@Status", status);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapRequest(r));
        return list;
    }

    public IEnumerable<MembershipRequest> GetByStudent(int studentId)
    {
        var list = new List<MembershipRequest>();
        const string sql = @"
            SELECT mr.RequestID, mr.StudentID, mr.SocietyID, mr.Status, mr.RequestedAt,
                   mr.ProcessedAt, mr.RejectionReason,
                   s.FirstName + ' ' + s.LastName AS StudentName, s.RegistrationNumber,
                   soc.Name AS SocietyName
            FROM MembershipRequests mr
            INNER JOIN Students s ON s.StudentID = mr.StudentID
            INNER JOIN Societies soc ON soc.SocietyID = mr.SocietyID
            WHERE mr.StudentID = @SID ORDER BY mr.RequestedAt DESC";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SID", studentId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapRequest(r));
        return list;
    }

    public IEnumerable<MembershipRequest> GetBySociety(int societyId, string? status = null)
    {
        var list = new List<MembershipRequest>();
        var filter = status != null ? "AND mr.Status = @Status" : "";
        var sql = $@"
            SELECT mr.RequestID, mr.StudentID, mr.SocietyID, mr.Status, mr.RequestedAt,
                   mr.ProcessedAt, mr.RejectionReason,
                   s.FirstName + ' ' + s.LastName AS StudentName, s.RegistrationNumber,
                   soc.Name AS SocietyName
            FROM MembershipRequests mr
            INNER JOIN Students s ON s.StudentID = mr.StudentID
            INNER JOIN Societies soc ON soc.SocietyID = mr.SocietyID
            WHERE mr.SocietyID = @SocID {filter} ORDER BY mr.RequestedAt DESC";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SocID", societyId);
        if (status != null) cmd.Parameters.AddWithValue("@Status", status);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapRequest(r));
        return list;
    }

    public int Insert(MembershipRequest req)
    {
        const string sql = @"
            INSERT INTO MembershipRequests (StudentID, SocietyID, Status)
            VALUES (@SID, @SocID, 'Pending');
            SELECT SCOPE_IDENTITY();";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SID",   req.StudentID);
        cmd.Parameters.AddWithValue("@SocID", req.SocietyID);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool UpdateStatus(int requestId, string status, int processedBy, string? reason = null)
    {
        const string sql = @"
            UPDATE MembershipRequests
            SET Status=@Status, ProcessedAt=GETDATE(), ProcessedByHeadID=@PBy, RejectionReason=@Reason
            WHERE RequestID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@PBy",    processedBy);
        cmd.Parameters.AddWithValue("@Reason", (object?)reason ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ID",     requestId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool HasPendingRequest(int studentId, int societyId)
    {
        const string sql = "SELECT COUNT(1) FROM MembershipRequests WHERE StudentID=@SID AND SocietyID=@SocID AND Status='Pending'";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SID",   studentId);
        cmd.Parameters.AddWithValue("@SocID", societyId);
        return (int)cmd.ExecuteScalar() > 0;
    }

    public bool IsMember(int studentId, int societyId)
    {
        const string sql = "SELECT COUNT(1) FROM SocietyMembers WHERE StudentID=@SID AND SocietyID=@SocID AND IsActive=1";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SID",   studentId);
        cmd.Parameters.AddWithValue("@SocID", societyId);
        return (int)cmd.ExecuteScalar() > 0;
    }

    public bool AddMember(int studentId, int societyId, string role = "Member")
    {
        const string sql = @"
            IF NOT EXISTS (SELECT 1 FROM SocietyMembers WHERE StudentID=@SID AND SocietyID=@SocID)
                INSERT INTO SocietyMembers (StudentID, SocietyID, Role) VALUES (@SID, @SocID, @Role)
            ELSE
                UPDATE SocietyMembers SET IsActive=1, Role=@Role WHERE StudentID=@SID AND SocietyID=@SocID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SID",   studentId);
        cmd.Parameters.AddWithValue("@SocID", societyId);
        cmd.Parameters.AddWithValue("@Role",  role);
        return cmd.ExecuteNonQuery() > 0;
    }

    public IEnumerable<SocietyMember> GetMembers(int societyId)
    {
        var list = new List<SocietyMember>();
        const string sql = @"
            SELECT sm.MemberID, sm.StudentID, sm.SocietyID, sm.Role, sm.JoinedAt, sm.IsActive,
                   s.FirstName + ' ' + s.LastName AS StudentName, s.RegistrationNumber
            FROM SocietyMembers sm
            INNER JOIN Students s ON s.StudentID = sm.StudentID
            WHERE sm.SocietyID = @SocID AND sm.IsActive = 1
            ORDER BY sm.JoinedAt";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SocID", societyId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new SocietyMember
            {
                MemberID           = r.GetInt32(0),
                StudentID          = r.GetInt32(1),
                SocietyID          = r.GetInt32(2),
                Role               = r.GetString(3),
                JoinedAt           = r.GetDateTime(4),
                IsActive           = r.GetBoolean(5),
                StudentName        = r.GetString(6),
                RegistrationNumber = r.GetString(7)
            });
        }
        return list;
    }

    public bool RemoveMember(int memberId)
    {
        const string sql = "UPDATE SocietyMembers SET IsActive=0 WHERE MemberID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ID", memberId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Update(MembershipRequest entity) => false;
    public bool Delete(int id) => false;

    private static MembershipRequest MapRequest(SqlDataReader r) => new()
    {
        RequestID          = r.GetInt32(0),
        StudentID          = r.GetInt32(1),
        SocietyID          = r.GetInt32(2),
        Status             = r.GetString(3),
        RequestedAt        = r.GetDateTime(4),
        ProcessedAt        = r.IsDBNull(5) ? null : r.GetDateTime(5),
        RejectionReason    = r.IsDBNull(6) ? "" : r.GetString(6),
        StudentName        = r.GetString(7),
        RegistrationNumber = r.GetString(8),
        SocietyName        = r.GetString(9)
    };
}
