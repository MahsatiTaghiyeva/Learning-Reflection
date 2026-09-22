using BankAccountSystem.Attributes;
using BankAccountSystem.Exceptions;
using BankAccountSystem.Models;

namespace BankAccountSystem.Services;

public class LedgerEngine<T> where T : BankAccount
{
    private readonly IAuditLogService _auditLogService;

    private readonly Dictionary<int, T> _accounts = new();

    public LedgerEngine(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [AuditLoggable]
    public void AddAccount(T account)
    {
        _accounts[account.AccountId] = account;

        _auditLogService.Log(
            $"Account {account.AccountId} added."
        );
    }

    [AuditLoggable]
    public T GetAccount(int accountId)
    {
        if (!_accounts.TryGetValue(accountId, out T? account))
        {
            throw new AccountNotFoundException(
                $"Account {accountId} not found."
            );
        }

        return account;
    }

    [AuditLoggable]
    public void RemoveAccount(int accountId)
    {
        if (!_accounts.Remove(accountId))
        {
            throw new AccountNotFoundException(
                $"Account {accountId} not found."
            );
        }

        _auditLogService.Log(
            $"Account {accountId} removed."
        );
    }

    [AuditLoggable]
    public async Task ProcessTransactionsConcurrentlyAsync(
        T account,
        IEnumerable<decimal> amounts,
        bool useLock)
    {
        if (useLock)
        {
            await Parallel.ForEachAsync(
                amounts,
                async (amount, cancellationToken) =>
                {
                    account.AddWithLock(amount);

                    await Task.CompletedTask;
                });
        }
        else
        {
            await Parallel.ForEachAsync(
                amounts,
                async (amount, cancellationToken) =>
                {
                    account.AddWithoutLock(amount);

                    await Task.CompletedTask;
                });
        }

        _auditLogService.Log(
            $"Concurrent transactions processed for account {account.AccountId}. " +
            $"Lock: {useLock}"
        );
    }
}