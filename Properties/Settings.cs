namespace SocietiesManagementSystem.Properties;

internal sealed class Settings
{
    private static readonly Settings _default = new();
    public static Settings Default => _default;

    public string ConnectionString { get; set; } =
        "Server=.\\SQLEXPRESS;Database=SocietiesManagementSystem;Trusted_Connection=True;TrustServerCertificate=True;";
}
