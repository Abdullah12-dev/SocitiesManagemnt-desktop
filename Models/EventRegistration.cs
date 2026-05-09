namespace SocietiesManagementSystem.Models;

public class EventRegistration
{
    public int RegistrationID { get; set; }
    public int EventID { get; set; }
    public int StudentID { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string SocietyName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string AttendanceStatus { get; set; } = "Registered";
    public string StudentName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
}
