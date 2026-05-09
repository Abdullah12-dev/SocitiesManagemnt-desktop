using SocietiesManagementSystem.Data.Repositories;
using SocietiesManagementSystem.Helpers;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Services;

public class EventService
{
    private readonly EventRepository _eventRepo = new();
    private readonly EventRegistrationRepository _regRepo = new();

    public IEnumerable<Event> GetAllEvents() => _eventRepo.GetAll();

    public IEnumerable<Event> GetUpcomingEvents() => _eventRepo.GetUpcoming();

    public IEnumerable<Event> GetEventsBySociety(int societyId) =>
        _eventRepo.GetBySociety(societyId);

    public IEnumerable<Event> GetPendingEvents() => _eventRepo.GetPending();

    public Event? GetById(int eventId) => _eventRepo.GetById(eventId);

    public (bool success, string message, int eventId) CreateEvent(
        int societyId, string title, string description, DateTime eventDate,
        DateTime? endDate, string venue, int? maxParticipants, string eventType)
    {
        if (string.IsNullOrWhiteSpace(title))
            return (false, "Event title is required.", 0);

        if (eventDate < DateTime.Now)
            return (false, "Event date must be in the future.", 0);

        var ev = new Event
        {
            SocietyID       = societyId,
            Title           = title.Trim(),
            Description     = description.Trim(),
            EventDate       = eventDate,
            EndDate         = endDate,
            Venue           = venue.Trim(),
            MaxParticipants = maxParticipants,
            EventType       = eventType.Trim()
        };

        var id = _eventRepo.Insert(ev);
        return id > 0
            ? (true, "Event created and pending admin approval.", id)
            : (false, "Failed to create event.", 0);
    }

    public (bool success, string message) UpdateEvent(Event ev)
    {
        if (string.IsNullOrWhiteSpace(ev.Title))
            return (false, "Event title is required.");

        return _eventRepo.Update(ev)
            ? (true, "Event updated successfully.")
            : (false, "Update failed.");
    }

    public bool ApproveEvent(int eventId, int adminId) =>
        _eventRepo.UpdateStatus(eventId, "Approved", adminId);

    public bool CancelEvent(int eventId) =>
        _eventRepo.UpdateStatus(eventId, "Cancelled");

    public bool CompleteEvent(int eventId) =>
        _eventRepo.UpdateStatus(eventId, "Completed");

    public (bool success, string message, string ticket) RegisterForEvent(int studentId, int eventId)
    {
        var ev = _eventRepo.GetById(eventId);
        if (ev == null) return (false, "Event not found.", "");
        if (ev.Status != "Approved") return (false, "Event is not open for registration.", "");
        if (ev.EventDate < DateTime.Now) return (false, "Event has already passed.", "");

        if (_regRepo.IsRegistered(studentId, eventId))
            return (false, "You are already registered for this event.", "");

        if (ev.MaxParticipants.HasValue && ev.RegistrationCount >= ev.MaxParticipants.Value)
            return (false, "Event is fully booked.", "");

        var ticket = TicketHelper.Generate(eventId, studentId);
        var reg = new EventRegistration
        {
            EventID    = eventId,
            StudentID  = studentId,
            TicketCode = ticket
        };

        var id = _regRepo.Insert(reg);
        return id > 0
            ? (true, "Registered successfully! Your ticket has been generated.", ticket)
            : (false, "Registration failed. Please try again.", "");
    }

    public IEnumerable<EventRegistration> GetStudentRegistrations(int studentId) =>
        _regRepo.GetByStudent(studentId);

    public IEnumerable<EventRegistration> GetEventAttendees(int eventId) =>
        _regRepo.GetByEvent(eventId);

    public bool IsRegistered(int studentId, int eventId) =>
        _regRepo.IsRegistered(studentId, eventId);

    public bool UpdateAttendance(int registrationId, string status) =>
        _regRepo.UpdateAttendance(registrationId, status);
}
