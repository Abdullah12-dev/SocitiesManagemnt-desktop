using SocietiesManagementSystem.Data.Repositories;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Services;

public class AnnouncementService
{
    private readonly AnnouncementRepository _annRepo = new();

    public IEnumerable<Announcement> GetBySociety(int societyId) =>
        _annRepo.GetBySociety(societyId);

    public IEnumerable<Announcement> GetForStudent(IEnumerable<int> societyIds) =>
        _annRepo.GetBySocieties(societyIds);

    public IEnumerable<Announcement> GetAll() => _annRepo.GetAll();

    public Announcement? GetById(int id) => _annRepo.GetById(id);

    public (bool success, string message, int id) PostAnnouncement(
        int societyId, string title, string content, int createdByStudentId, string priority)
    {
        if (string.IsNullOrWhiteSpace(title))
            return (false, "Title is required.", 0);

        if (string.IsNullOrWhiteSpace(content))
            return (false, "Content is required.", 0);

        var ann = new Announcement
        {
            SocietyID          = societyId,
            Title              = title.Trim(),
            Content            = content.Trim(),
            CreatedByStudentID = createdByStudentId,
            Priority           = priority,
            IsActive           = true
        };

        var id = _annRepo.Insert(ann);
        return id > 0
            ? (true, "Announcement posted successfully.", id)
            : (false, "Failed to post announcement.", 0);
    }

    public (bool success, string message) UpdateAnnouncement(Announcement ann)
    {
        if (string.IsNullOrWhiteSpace(ann.Title) || string.IsNullOrWhiteSpace(ann.Content))
            return (false, "Title and content are required.");

        return _annRepo.Update(ann)
            ? (true, "Announcement updated successfully.")
            : (false, "Update failed.");
    }

    public bool DeleteAnnouncement(int announcementId) =>
        _annRepo.Delete(announcementId);
}
