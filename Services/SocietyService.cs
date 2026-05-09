using SocietiesManagementSystem.Data.Repositories;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Services;

public class SocietyService
{
    private readonly SocietyRepository _societyRepo = new();
    private readonly MembershipRepository _memberRepo = new();

    public IEnumerable<Society> GetAllSocieties() => _societyRepo.GetAll();

    public IEnumerable<Society> GetActiveSocieties() => _societyRepo.GetActive();

    public IEnumerable<Society> GetSocietiesByHead(int studentId) =>
        _societyRepo.GetByHeadStudent(studentId);

    public Society? GetById(int societyId) => _societyRepo.GetById(societyId);

    public (bool success, string message, int societyId) CreateSociety(
        string name, string description, string category, int headStudentId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Society name is required.", 0);

        var society = new Society
        {
            Name          = name.Trim(),
            Description   = description.Trim(),
            Category      = category.Trim(),
            HeadStudentID = headStudentId,
            Status        = "Pending"
        };

        var id = _societyRepo.Insert(society);
        if (id <= 0) return (false, "Failed to create society.", 0);

        _memberRepo.AddMember(headStudentId, id, "Head");
        return (true, "Society created and pending admin approval.", id);
    }

    public (bool success, string message) UpdateSociety(Society society)
    {
        if (string.IsNullOrWhiteSpace(society.Name))
            return (false, "Society name is required.");

        return _societyRepo.Update(society)
            ? (true, "Society updated successfully.")
            : (false, "Update failed.");
    }

    public (bool success, string message, int societyId) AdminCreateSociety(
        string name, string description, string category, int adminId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Society name is required.", 0);

        var society = new Society
        {
            Name        = name.Trim(),
            Description = description.Trim(),
            Category    = category.Trim()
        };

        var id = _societyRepo.Insert(society);
        if (id <= 0) return (false, "Failed to create society.", 0);

        _societyRepo.UpdateStatus(id, "Active", adminId);
        return (true, "Society created and activated.", id);
    }

    public (bool success, string message) AssignHead(int societyId, int newStudentId)
    {
        var society = _societyRepo.GetById(societyId);
        if (society == null) return (false, "Society not found.");

        var studentRepo = new StudentRepository();
        var userRepo    = new UserRepository();

        // Revert old head's role if they don't lead any other society
        if (society.HeadStudentID.HasValue && society.HeadStudentID.Value != newStudentId)
        {
            var oldHead = studentRepo.GetById(society.HeadStudentID.Value);
            if (oldHead != null)
            {
                bool leadsOther = _societyRepo.GetByHeadStudent(oldHead.StudentID)
                    .Any(s => s.SocietyID != societyId && s.Status != "Deleted");
                if (!leadsOther)
                    userRepo.UpdateRole(oldHead.UserID, "Student");
            }
        }

        var newHead = studentRepo.GetById(newStudentId);
        if (newHead == null) return (false, "Student not found.");

        userRepo.UpdateRole(newHead.UserID, "SocietyHead");
        _societyRepo.AssignHead(societyId, newStudentId);

        if (!_memberRepo.IsMember(newStudentId, societyId))
            _memberRepo.AddMember(newStudentId, societyId, "Head");

        return (true, "Society head assigned successfully.");
    }

    public bool ApproveSociety(int societyId, int adminId) =>
        _societyRepo.UpdateStatus(societyId, "Active", adminId);

    public bool SuspendSociety(int societyId) =>
        _societyRepo.UpdateStatus(societyId, "Suspended");

    public bool DeleteSociety(int societyId) =>
        _societyRepo.Delete(societyId);

    public IEnumerable<SocietyMember> GetMembers(int societyId) =>
        _memberRepo.GetMembers(societyId);

    public bool RemoveMember(int memberId) =>
        _memberRepo.RemoveMember(memberId);

    public IEnumerable<Society> GetMemberSocieties(int studentId)
    {
        var members = _memberRepo.GetMembers(0);
        var all = _societyRepo.GetActive();
        return all.Where(s => _memberRepo.IsMember(studentId, s.SocietyID));
    }
}
