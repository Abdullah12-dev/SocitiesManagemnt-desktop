using SocietiesManagementSystem.Data.Repositories;
using SocietiesManagementSystem.Helpers;
using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem.Services;

public class AuthService
{
    private readonly UserRepository _userRepo = new();
    private readonly StudentRepository _studentRepo = new();

    public (bool success, string message, User? user) Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Email and password are required.", null);

        var user = _userRepo.GetByEmail(email.Trim().ToLower());
        if (user == null)
            return (false, "Invalid email or password.", null);

        if (!user.IsActive)
            return (false, "Your account has been deactivated. Contact admin.", null);

        if (!PasswordHelper.Verify(password, user.PasswordHash))
            return (false, "Invalid email or password.", null);

        return (true, "Login successful.", user);
    }

    public (bool success, string message) RegisterStudent(
        string email, string password, string firstName, string lastName,
        string regNumber, string department, int semester, string phone)
    {
        if (!ValidationHelper.IsValidEmail(email))
            return (false, "Invalid email address.");

        if (!ValidationHelper.IsStrongPassword(password))
            return (false, "Password must be at least 8 characters.");

        if (!ValidationHelper.IsValidRegistrationNumber(regNumber))
            return (false, "Invalid registration number format.");

        if (_userRepo.GetByEmail(email.Trim().ToLower()) != null)
            return (false, "An account with this email already exists.");

        var user = new User
        {
            Email        = email.Trim().ToLower(),
            PasswordHash = PasswordHelper.Hash(password),
            Role         = "Student",
            IsActive     = true
        };

        var userId = _userRepo.Insert(user);
        if (userId <= 0)
            return (false, "Failed to create account. Please try again.");

        var student = new Student
        {
            UserID             = userId,
            FirstName          = firstName.Trim(),
            LastName           = lastName.Trim(),
            RegistrationNumber = regNumber.Trim().ToUpper(),
            Department         = department.Trim(),
            Semester           = semester,
            Phone              = phone.Trim()
        };

        _studentRepo.Insert(student);
        return (true, "Account created successfully. You can now log in.");
    }

    public Student? GetStudentProfile(int userId) => _studentRepo.GetByUserId(userId);
}
