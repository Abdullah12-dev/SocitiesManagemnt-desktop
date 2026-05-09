namespace SocietiesManagementSystem.Models;

public class Society
{
    public int SocietyID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? HeadStudentID { get; set; }
    public string HeadName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public int? ApprovedByAdminID { get; set; }
    public int MemberCount { get; set; }
}
