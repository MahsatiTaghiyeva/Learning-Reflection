using BankAccountSystem.Enums;

namespace BankAccountSystem.Models;

public class TransactionDetail
{
    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public Currency Currency { get; set; }

    public int FromAccountId { get; set; }

    public int ToAccountId { get; set; }

    public DateTime Date { get; set; }

    public TransactionDetail DeepCopy()
    {
        return new TransactionDetail
        {
            Type = Type,
            Amount = Amount,
            Currency = Currency,
            FromAccountId = FromAccountId,
            ToAccountId = ToAccountId,
            Date = Date
        };
    }

    public override string ToString()
    {
        return $"{Date:yyyy-MM-dd HH:mm:ss} | " +
               $"{Type} | {Amount} {Currency} | " +
               $"From: {FromAccountId} | To: {ToAccountId}";
    }
}