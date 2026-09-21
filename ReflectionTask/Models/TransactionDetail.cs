public class TransactionDetail
{
    public int Id { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public Currency Currency { get; set; }

    public DateTime CreatedAt { get; set; }

    public TransactionDetail DeepCopy()
    {
        return new TransactionDetail
        {
            Id = Id,
            Type = Type,
            Amount = Amount,
            Currency = Currency,
            CreatedAt = CreatedAt
        };
    }
}