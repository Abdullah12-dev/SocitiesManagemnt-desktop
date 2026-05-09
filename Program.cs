using SocietiesManagementSystem.Data;
using SocietiesManagementSystem.UI.Forms.Shared;

namespace SocietiesManagementSystem;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Catch any unhandled exception and show it so nothing fails silently
        Application.ThreadException += (_, e) =>
            MessageBox.Show(e.Exception.ToString(), "Unhandled Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            MessageBox.Show(e.ExceptionObject?.ToString(), "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        ApplicationConfiguration.Initialize();

        if (!DatabaseConnection.Instance.TestConnection())
        {
            MessageBox.Show(
                "Cannot connect to the database.\n\n" +
                "Please ensure SQL Server is running and accessible.\n\n" +
                "Connection string (edit in Properties\\Settings.cs):\n" +
                "Server=.\\SQLEXPRESS;Database=SocietiesManagementSystem;\nTrusted_Connection=True;TrustServerCertificate=True",
                "Database Connection Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Application.Run(new LoginForm());
    }
}