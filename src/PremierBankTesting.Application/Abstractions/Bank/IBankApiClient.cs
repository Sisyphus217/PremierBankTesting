namespace PremierBankTesting.Application.Abstractions.Bank;

public interface IBankApiClient
{
    Task<IReadOnlyList<BankTransaction>> GetRecentTransactionsAsync(CancellationToken cancellationToken = default);
}
