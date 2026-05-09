using Microsoft.Data.SqlClient;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Data.Repositories;

public class StudentRepository : IRepository<Student>
{
    private readonly DatabaseConnection _db = DatabaseConnection.Instance;

    private const string SelectBase = @"
        SELECT s.StudentID, s.UserID, s.FirstName, s.LastName, s.RegistrationNumber,
               s.Department, s.Semester, s.Phone, u.Email, u.IsActive
        FROM Students s
        INNER JOIN Users u ON u.UserID = s.UserID";

    public Student? GetById(int id)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE s.StudentID = @ID", conn);
        cmd.Parameters.AddWithValue("@ID", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapStudent(r) : null;
    }

    public Student? GetByUserId(int userId)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE s.UserID = @UID", conn);
        cmd.Parameters.AddWithValue("@UID", userId);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapStudent(r) : null;
    }

    public IEnumerable<Student> GetAll()
    {
        var list = new List<Student>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} WHERE u.IsActive = 1 ORDER BY s.FirstName", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapStudent(r));
        return list;
    }

    public IEnumerable<Student> GetAllForAdmin()
    {
        var list = new List<Student>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand($"{SelectBase} ORDER BY s.FirstName", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapStudent(r));
        return list;
    }

    public IEnumerable<Student> Search(string term)
    {
        var list = new List<Student>();
        const string sql = @"
            SELECT s.StudentID, s.UserID, s.FirstName, s.LastName, s.RegistrationNumber,
                   s.Department, s.Semester, s.Phone, u.Email, u.IsActive
            FROM Students s INNER JOIN Users u ON u.UserID = s.UserID
            WHERE u.IsActive = 1
              AND (s.FirstName LIKE @T OR s.LastName LIKE @T
                   OR s.RegistrationNumber LIKE @T OR u.Email LIKE @T)
            ORDER BY s.FirstName";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@T", $"%{term}%");
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapStudent(r));
        return list;
    }

    public int Insert(Student s)
    {
        const string sql = @"
            INSERT INTO Students (UserID, FirstName, LastName, RegistrationNumber, Department, Semester, Phone)
            VALUES (@UID, @FN, @LN, @Reg, @Dept, @Sem, @Phone);
            SELECT SCOPE_IDENTITY();";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UID",   s.UserID);
        cmd.Parameters.AddWithValue("@FN",    s.FirstName);
        cmd.Parameters.AddWithValue("@LN",    s.LastName);
        cmd.Parameters.AddWithValue("@Reg",   s.RegistrationNumber);
        cmd.Parameters.AddWithValue("@Dept",  s.Department);
        cmd.Parameters.AddWithValue("@Sem",   s.Semester);
        cmd.Parameters.AddWithValue("@Phone", (object?)s.Phone ?? DBNull.Value);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Update(Student s)
    {
        const string sql = @"
            UPDATE Students SET FirstName=@FN, LastName=@LN, Department=@Dept,
                Semester=@Sem, Phone=@Phone WHERE StudentID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@FN",    s.FirstName);
        cmd.Parameters.AddWithValue("@LN",    s.LastName);
        cmd.Parameters.AddWithValue("@Dept",  s.Department);
        cmd.Parameters.AddWithValue("@Sem",   s.Semester);
        cmd.Parameters.AddWithValue("@Phone", (object?)s.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ID",    s.StudentID);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        const string sql = "UPDATE Users SET IsActive=0 WHERE UserID=(SELECT UserID FROM Students WHERE StudentID=@ID)";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    private static Student MapStudent(SqlDataReader r) => new()
    {
        StudentID          = r.GetInt32(0),
        UserID             = r.GetInt32(1),
        FirstName          = r.GetString(2),
        LastName           = r.GetString(3),
        RegistrationNumber = r.GetString(4),
        Department         = r.IsDBNull(5) ? "" : r.GetString(5),
        Semester           = r.IsDBNull(6) ? 0  : r.GetInt32(6),
        Phone              = r.IsDBNull(7) ? "" : r.GetString(7),
        Email              = r.GetString(8),
        IsActive           = r.GetBoolean(9)
    };
}
