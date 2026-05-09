namespace SocietiesManagementSystem.Models;

public class SocietyMember
{
    public int MemberID { get; set; }
    public int StudentID { get; set; }
    public int SocietyID { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}
