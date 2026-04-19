namespace PremierBankTesting.Application.Abstractions.Bank;

public record BankTransaction
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string Comment { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public string UserEmail { get; init; } = string.Empty;
}
