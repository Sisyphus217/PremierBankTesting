using PremierBankTesting.Application.Abstractions.Bank;

namespace PremierBankTesting.Integration.Tests.Infrastructure;

public class FakeBankApiClient : IBankApiClient
{
    public List<BankTransaction> Transactions { get; set; } = [];

    public Task<IReadOnlyList<BankTransaction>> GetRecentTransactionsAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<BankTransaction>>(Transactions);
}