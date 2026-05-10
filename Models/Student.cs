namespace SocietiesManagementSystem.Models;

public class Student : User
{
    public int StudentID { get; set; }
    // UserID, Email, IsActive, PasswordHash, Role, CreatedAt inherited from User

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Semester { get; set; }
    public string Phone { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
}
