using SocietiesManagementSystem.Data.Repositories;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Services;

public class TaskService
{
    private readonly TaskRepository _taskRepo = new();

    public IEnumerable<SocietyTask> GetTasksBySociety(int societyId) =>
        _taskRepo.GetBySociety(societyId);

    public IEnumerable<SocietyTask> GetTasksByStudent(int studentId) =>
        _taskRepo.GetByStudent(studentId);

    public SocietyTask? GetById(int taskId) => _taskRepo.GetById(taskId);

    public (bool success, string message, int taskId) AssignTask(
        int societyId, string title, string description,
        int assignedToStudentId, int assignedByStudentId, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            return (false, "Task title is required.", 0);

        if (dueDate.HasValue && dueDate.Value < DateTime.Now)
            return (false, "Due date cannot be in the past.", 0);

        var task = new SocietyTask
        {
            SocietyID           = societyId,
            Title               = title.Trim(),
            Description         = description.Trim(),
            AssignedToStudentID = assignedToStudentId,
            AssignedByStudentID = assignedByStudentId,
            DueDate             = dueDate
        };

        var id = _taskRepo.Insert(task);
        return id > 0
            ? (true, "Task assigned successfully.", id)
            : (false, "Failed to assign task.", 0);
    }

    public (bool success, string message) UpdateTaskStatus(int taskId, string status)
    {
        return _taskRepo.UpdateStatus(taskId, status)
            ? (true, $"Task marked as {status}.")
            : (false, "Failed to update task.");
    }

    public (bool success, string message) UpdateTask(SocietyTask task)
    {
        if (string.IsNullOrWhiteSpace(task.Title))
            return (false, "Task title is required.");

        return _taskRepo.Update(task)
            ? (true, "Task updated successfully.")
            : (false, "Update failed.");
    }

    public bool DeleteTask(int taskId) => _taskRepo.Delete(taskId);
}
