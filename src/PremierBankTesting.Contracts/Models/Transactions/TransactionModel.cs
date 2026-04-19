
namespace PremierBankTesting.Contracts.Models.Transactions;

public record TransactionModel(
    Guid Id,
    decimal Amount,
    string Comment,
    DateTime Timestamp,
    bool IsProcessed,
    string UserEmail
);
