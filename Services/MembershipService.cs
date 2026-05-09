using SocietiesManagementSystem.Data.Repositories;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Services;

public class MembershipService
{
    private readonly MembershipRepository _memberRepo = new();

    public (bool success, string message) ApplyForMembership(int studentId, int societyId)
    {
        if (_memberRepo.IsMember(studentId, societyId))
            return (false, "You are already a member of this society.");

        if (_memberRepo.HasPendingRequest(studentId, societyId))
            return (false, "You already have a pending request for this society.");

        var req = new MembershipRequest { StudentID = studentId, SocietyID = societyId };
        var id = _memberRepo.Insert(req);
        return id > 0
            ? (true, "Membership request submitted successfully.")
            : (false, "Failed to submit request. Please try again.");
    }

    public (bool success, string message) ProcessRequest(int requestId, string action, int headStudentId, string? reason = null)
    {
        var request = _memberRepo.GetById(requestId);
        if (request == null) return (false, "Request not found.");
        if (request.Status != "Pending") return (false, "Request has already been processed.");

        var status = action == "Approve" ? "Approved" : "Rejected";
        _memberRepo.UpdateStatus(requestId, status, headStudentId, reason);

        if (status == "Approved")
            _memberRepo.AddMember(request.StudentID, request.SocietyID);

        return (true, $"Request {status.ToLower()} successfully.");
    }

    public IEnumerable<MembershipRequest> GetRequestsForSociety(int societyId, string? status = null) =>
        _memberRepo.GetBySociety(societyId, status);

    public IEnumerable<MembershipRequest> GetStudentRequests(int studentId) =>
        _memberRepo.GetByStudent(studentId);

    public IEnumerable<MembershipRequest> GetAllRequests() => _memberRepo.GetAll();

    public bool IsMember(int studentId, int societyId) =>
        _memberRepo.IsMember(studentId, societyId);

    public bool HasPending(int studentId, int societyId) =>
        _memberRepo.HasPendingRequest(studentId, societyId);
}
