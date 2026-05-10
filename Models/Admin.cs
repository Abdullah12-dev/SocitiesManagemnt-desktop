namespace SocietiesManagementSystem.Models;

public class Admin : User
{
    public int AdminID { get; set; }
    // UserID, Email, IsActive, PasswordHash, Role, CreatedAt inherited from User

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
}
