namespace SocietiesManagementSystem.Models;

public class MembershipRequest
{
    public int RequestID { get; set; }
    public int StudentID { get; set; }
    public int SocietyID { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string SocietyName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}
