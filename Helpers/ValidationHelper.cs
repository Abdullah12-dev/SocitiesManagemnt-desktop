using System.Text.RegularExpressions;

namespace SocietiesManagementSystem.Helpers;

public static class ValidationHelper
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PhoneRegex =
        new(@"^03\d{9}$", RegexOptions.Compiled);

    private static readonly Regex RegNumRegex =
        new(@"^[A-Z0-9\-/]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());

    public static bool IsValidPhone(string phone) =>
        string.IsNullOrWhiteSpace(phone) || PhoneRegex.IsMatch(phone.Trim());

    public static bool IsValidRegistrationNumber(string regNum) =>
        !string.IsNullOrWhiteSpace(regNum) && RegNumRegex.IsMatch(regNum.Trim());

    public static bool IsStrongPassword(string password) =>
        !string.IsNullOrEmpty(password) && password.Length >= 8;

    public static string? RequiredField(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return $"{fieldName} is required.";
        return null;
    }
}
