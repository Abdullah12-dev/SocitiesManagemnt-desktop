namespace SocietiesManagementSystem.Models;

public class SocietyTask
{
    public int TaskID { get; set; }
    public int SocietyID { get; set; }
    public string SocietyName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int AssignedToStudentID { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public int AssignedByStudentID { get; set; }
    public string AssignedByName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
