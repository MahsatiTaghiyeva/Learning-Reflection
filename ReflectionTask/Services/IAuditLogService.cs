namespace BankAccountSystem.Services;

public interface IAuditLogService
{
    void Log(string message);
}