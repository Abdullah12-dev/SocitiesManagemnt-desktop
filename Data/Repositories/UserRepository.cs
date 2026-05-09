using Microsoft.Data.SqlClient;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Data.Repositories;

public class UserRepository : IRepository<User>
{
    private readonly DatabaseConnection _db = DatabaseConnection.Instance;

    public User? GetById(int id)
    {
        const string sql = "SELECT UserID, Email, PasswordHash, Role, IsActive, CreatedAt FROM Users WHERE UserID = @ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public User? GetByEmail(string email)
    {
        const string sql = "SELECT UserID, Email, PasswordHash, Role, IsActive, CreatedAt FROM Users WHERE Email = @Email";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", email);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public IEnumerable<User> GetAll()
    {
        const string sql = "SELECT UserID, Email, PasswordHash, Role, IsActive, CreatedAt FROM Users ORDER BY CreatedAt DESC";
        var users = new List<User>();
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            users.Add(MapUser(reader));
        return users;
    }

    public int Insert(User user)
    {
        const string sql = @"
            INSERT INTO Users (Email, PasswordHash, Role, IsActive)
            VALUES (@Email, @Hash, @Role, @IsActive);
            SELECT SCOPE_IDENTITY();";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@Hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@Role", user.Role);
        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool Update(User user)
    {
        const string sql = "UPDATE Users SET Email=@Email, IsActive=@IsActive WHERE UserID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
        cmd.Parameters.AddWithValue("@ID", user.UserID);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdateRole(int userId, string role)
    {
        const string sql = "UPDATE Users SET Role=@Role WHERE UserID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Role", role);
        cmd.Parameters.AddWithValue("@ID",   userId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdatePassword(int userId, string newHash)
    {
        const string sql = "UPDATE Users SET PasswordHash=@Hash WHERE UserID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Hash", newHash);
        cmd.Parameters.AddWithValue("@ID", userId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        const string sql = "UPDATE Users SET IsActive=0 WHERE UserID=@ID";
        using var conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ID", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    private static User MapUser(SqlDataReader r) => new()
    {
        UserID       = r.GetInt32(0),
        Email        = r.GetString(1),
        PasswordHash = r.GetString(2),
        Role         = r.GetString(3),
        IsActive     = r.GetBoolean(4),
        CreatedAt    = r.GetDateTime(5)
    };
}
