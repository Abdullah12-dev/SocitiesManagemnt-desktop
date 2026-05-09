using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Data;

public class DatabaseConnection
{
    private static DatabaseConnection? _instance;
    private static readonly object _lock = new();
    private readonly string _connectionString;

    private DatabaseConnection()
    {
        _connectionString = Properties.Settings.Default.ConnectionString;
    }

    public static DatabaseConnection Instance
    {
        get
        {
            lock (_lock)
            {
                _instance ??= new DatabaseConnection();
                return _instance;
            }
        }
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public bool TestConnection()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
