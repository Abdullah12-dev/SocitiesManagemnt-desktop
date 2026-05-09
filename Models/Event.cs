namespace SocietiesManagementSystem.Models;

public class Event
{
    public int EventID { get; set; }
    public int SocietyID { get; set; }
    public string SocietyName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public int? MaxParticipants { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public int? ApprovedByAdminID { get; set; }
    public int RegistrationCount { get; set; }
}
