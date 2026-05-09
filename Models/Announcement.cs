namespace SocietiesManagementSystem.Models;

public class Announcement
{
    public int AnnouncementID { get; set; }
    public int SocietyID { get; set; }
    public string SocietyName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CreatedByStudentID { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
