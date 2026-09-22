using BankAccountSystem.Enums;
using BankAccountSystem.Exceptions;
using BankAccountSystem.Helpers;
using BankAccountSystem.Models;
using BankAccountSystem.Records;
using BankAccountSystem.Services;

string dataDirectory = Path.Combine(
    AppContext.BaseDirectory,
    "Data"
);

Directory.CreateDirectory(dataDirectory);

string auditPath = Path.Combine(
    dataDirectory,
    "audit.log"
);

string ledgerPath = Path.Combine(
    dataDirectory,
    "ledger.json"
);

// ==========================================
// 1. ENUM & RECORD
// ==========================================

Console.WriteLine("===== ENUM & RECORD =====");

var owner = new AccountOwnerRecord(
    1,
    "Mahsati Taghiyeva",
    "AZ123456789"
);

Console.WriteLine($"Owner ID: {owner.Id}");
Console.WriteLine($"Full Name: {owner.FullName}");
Console.WriteLine($"Tax ID: {owner.TaxId}");

Console.WriteLine($"Transaction Type: {TransactionType.Deposit}");
Console.WriteLine($"Currency: {Currency.AZN}");


// ==========================================
// 2. BANK ACCOUNT
// ==========================================

Console.WriteLine();
Console.WriteLine("===== BANK ACCOUNT =====");

var account = new BankAccount(
    owner,
    1000m
);

Console.WriteLine($"Account ID: {account.AccountId}");
Console.WriteLine($"Balance: {account.Balance}");

account.Deposit(500m, Currency.AZN);

account.Withdraw(100m, Currency.AZN);

Console.WriteLine($"Balance after transactions: {account.Balance}");


// ==========================================
// INDEXER
// ==========================================

Console.WriteLine();
Console.WriteLine("===== INDEXER =====");

TransactionDetail firstTransaction = account[0];

Console.WriteLine(firstTransaction);


// ==========================================
// EXPLICIT DECIMAL CAST
// ==========================================

Console.WriteLine();
Console.WriteLine("===== EXPLICIT CAST =====");

decimal balance = (decimal)account;

Console.WriteLine($"Decimal balance: {balance}");


// ==========================================
// IMPLICIT STRING CAST
// ==========================================

Console.WriteLine();
Console.WriteLine("===== IMPLICIT CAST =====");

string summary = account;

Console.WriteLine(summary);


// ==========================================
// DEEP COPY
// ==========================================

Console.WriteLine();
Console.WriteLine("===== DEEP COPY =====");

TransactionDetail original = account[0];

TransactionDetail copy = original.DeepCopy();

copy.Amount = 9999m;

Console.WriteLine($"Original amount: {original.Amount}");
Console.WriteLine($"Copied amount: {copy.Amount}");


// ==========================================
// DEPENDENCY INJECTION
// ==========================================

Console.WriteLine();
Console.WriteLine("===== DEPENDENCY INJECTION =====");

IAuditLogService auditService =
    new FileAuditLogService(auditPath);

var ledgerEngine =
    new LedgerEngine<BankAccount>(auditService);

ledgerEngine.AddAccount(account);

BankAccount foundAccount =
    ledgerEngine.GetAccount(account.AccountId);

Console.WriteLine(
    $"Found account: {foundAccount.AccountId}"
);


// ==========================================
// CUSTOM EXCEPTION
// ==========================================

Console.WriteLine();
Console.WriteLine("===== CUSTOM EXCEPTION =====");

try
{
    account.Withdraw(100000m, Currency.AZN);
}
catch (InsufficientBalanceException ex)
{
    Console.WriteLine(ex.Message);
}

try
{
    ledgerEngine.GetAccount(99999);
}
catch (AccountNotFoundException ex)
{
    Console.WriteLine(ex.Message);
}


// ==========================================
// CONCURRENCY - WITHOUT LOCK
// ==========================================

Console.WriteLine();
Console.WriteLine("===== RACE CONDITION TEST =====");

var raceOwner = new AccountOwnerRecord(
    2,
    "Test User",
    "AZ987654321"
);

var raceAccount =
    new BankAccount(raceOwner, 0m);

decimal transactionAmount = 10m;

int transactionCount = 100;

decimal expected =
    transactionAmount * transactionCount;

List<decimal> amounts =
    Enumerable.Repeat(
        transactionAmount,
        transactionCount
    ).ToList();

await ledgerEngine.ProcessTransactionsConcurrentlyAsync(
    raceAccount,
    amounts,
    false
);

Console.WriteLine(
    $"Expected balance: {expected}"
);

Console.WriteLine(
    $"Without lock: {raceAccount.Balance}"
);


// ==========================================
// CONCURRENCY - WITH LOCK
// ==========================================

var safeOwner = new AccountOwnerRecord(
    3,
    "Safe User",
    "AZ111222333"
);

var safeAccount =
    new BankAccount(safeOwner, 0m);

await ledgerEngine.ProcessTransactionsConcurrentlyAsync(
    safeAccount,
    amounts,
    true
);

Console.WriteLine(
    $"Expected balance: {expected}"
);

Console.WriteLine(
    $"With lock: {safeAccount.Balance}"
);


// ==========================================
// REFLECTION
// ==========================================

Console.WriteLine();
Console.WriteLine("===== REFLECTION =====");

ReflectionHelper.InspectAndInvoke(
    ledgerEngine
);


// ==========================================
// ASYNC JSON
// ==========================================

Console.WriteLine();
Console.WriteLine("===== ASYNC JSON =====");

var dataManager =
    new LedgerDataManager(ledgerPath);

List<TransactionDetail> transactions =
    new List<TransactionDetail>
    {
        new TransactionDetail
        {
            Type = TransactionType.Deposit,
            Amount = 500m,
            Currency = Currency.AZN,
            ToAccountId = account.AccountId,
            Date = DateTime.Now
        },

        new TransactionDetail
        {
            Type = TransactionType.Withdrawal,
            Amount = 100m,
            Currency = Currency.AZN,
            FromAccountId = account.AccountId,
            Date = DateTime.Now
        },

        new TransactionDetail
        {
            Type = TransactionType.Transfer,
            Amount = 200m,
            Currency = Currency.AZN,
            FromAccountId = account.AccountId,
            ToAccountId = safeAccount.AccountId,
            Date = DateTime.Now
        }
    };

await dataManager.SaveTransactionsAsync(
    transactions
);

Console.WriteLine(
    $"Transactions saved to: {ledgerPath}"
);

List<TransactionDetail> loadedTransactions =
    await dataManager.LoadTransactionsAsync();

Console.WriteLine(
    $"Loaded transactions: {loadedTransactions.Count}"
);

foreach (TransactionDetail transaction in loadedTransactions)
{
    Console.WriteLine(transaction);
}

Console.WriteLine();
Console.WriteLine("===== PROGRAM FINISHED =====");