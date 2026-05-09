namespace SocietiesManagementSystem.Helpers;

public static class TicketHelper
{
    public static string Generate(int eventId, int studentId) =>
        $"TKT-{eventId:D4}-{studentId:D4}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
}
