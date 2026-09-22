namespace BankAccountSystem.Services;

public class FileAuditLogService : IAuditLogService
{
    private readonly string _path;

    public FileAuditLogService(string path)
    {
        _path = path;
    }

    public void Log(string message)
    {
        using StreamWriter writer = new StreamWriter(
            _path,
            append: true
        );

        writer.WriteLine(
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}"
        );
    }
}