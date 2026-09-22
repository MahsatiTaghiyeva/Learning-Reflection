using System.Threading;
using BankAccountSystem.Enums;
using BankAccountSystem.Exceptions;
using BankAccountSystem.Records;
namespace BankAccountSystem.Models;

public class BankAccount
{
    private static int _lastAccountId = 0;

    private readonly object _balanceLock = new();

    private decimal _balance;

    private readonly List<TransactionDetail> _transactions = new();

    public int AccountId { get; }

    public AccountOwnerRecord Owner { get; init; }

    public decimal Balance
    {
        get
        {
            lock (_balanceLock)
            {
                return _balance;
            }
        }
    }

    public int TransactionCount
    {
        get
        {
            lock (_balanceLock)
            {
                return _transactions.Count;
            }
        }
    }

    public BankAccount(AccountOwnerRecord owner, decimal initialBalance = 0)
    {
        AccountId = Interlocked.Increment(ref _lastAccountId);

        Owner = owner;

        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative.");

        _balance = initialBalance;
    }

    public void Deposit(decimal amount, Currency currency)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        lock (_balanceLock)
        {
            _balance += amount;

            _transactions.Add(new TransactionDetail
            {
                Type = TransactionType.Deposit,
                Amount = amount,
                Currency = currency,
                ToAccountId = AccountId,
                Date = DateTime.Now
            });
        }
    }

    public void Withdraw(decimal amount, Currency currency)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        lock (_balanceLock)
        {
            if (_balance < amount)
                throw new InsufficientBalanceException(
                    $"Account {AccountId} has insufficient balance.");

            _balance -= amount;

            _transactions.Add(new TransactionDetail
            {
                Type = TransactionType.Withdrawal,
                Amount = amount,
                Currency = currency,
                FromAccountId = AccountId,
                Date = DateTime.Now
            });
        }
    }

    public void AddTransaction(TransactionDetail transaction)
    {
        lock (_balanceLock)
        {
            _transactions.Add(transaction.DeepCopy());
        }
    }

    // Indexer
    public TransactionDetail this[int index]
    {
        get
        {
            lock (_balanceLock)
            {
                if (index < 0 || index >= _transactions.Count)
                    throw new IndexOutOfRangeException();

                return _transactions[index].DeepCopy();
            }
        }
    }

    // Explicit cast: BankAccount -> decimal
    public static explicit operator decimal(BankAccount account)
    {
        return account.Balance;
    }

    // Implicit cast: BankAccount -> string
    public static implicit operator string(BankAccount account)
    {
        return $"Account ID: {account.AccountId}, " +
               $"Owner: {account.Owner.FullName}, " +
               $"Balance: {account.Balance}";
    }

    // Used for concurrency demonstration
    public void AddWithoutLock(decimal amount)
    {
        decimal current = _balance;

        Thread.Sleep(1);

        _balance = current + amount;
    }

    public void AddWithLock(decimal amount)
    {
        lock (_balanceLock)
        {
            decimal current = _balance;

            Thread.Sleep(1);

            _balance = current + amount;
        }
    }
}