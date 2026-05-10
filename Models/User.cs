namespace SocietiesManagementSystem.Models;

public class User
{
    public int UserID { get; set; }
    public virtual string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public virtual bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
