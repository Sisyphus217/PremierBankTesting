namespace PremierBankTesting.Domain.Entities;

public class Transaction
{
    protected Transaction() { }

    private Transaction(Guid id, decimal amount, string comment, DateTime timestamp, Guid userId)
    {
        Id = id;
        Amount = amount;
        Comment = comment;
        Timestamp = timestamp;
        IsProcessed = false;
        UserId = userId;
    }

    public static Transaction Create(Guid id, decimal amount, string comment, DateTime timestamp, Guid userId)
        => new(id, amount, comment, timestamp, userId);

    public void MarkAsProcessed() => IsProcessed = true;

    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public bool IsProcessed { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

}
