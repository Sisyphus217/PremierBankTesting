using PremierBankTesting.Application.Abstractions.Bank;

namespace PremierBankTesting.Infrastructure.Bank;

internal sealed class BankApiClient : IBankApiClient
{
    public Task<IReadOnlyList<BankTransaction>> GetRecentTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        IReadOnlyList<BankTransaction> transactions =
        [
            new()
            {
                Id = new Guid("a1b2c3d4-0001-0000-0000-000000000000"), Amount = 1000, Comment = "Пополнение",
                Timestamp = now.AddDays(-1), UserEmail = "ivanov@test.com"
            },
            new()
            {
                Id = new Guid("a1b2c3d4-0002-0000-0000-000000000000"), Amount = 2000, Comment = "Оплата",
                Timestamp = now.AddDays(-1), UserEmail = "unknown@test.com"
            },
            new()
            {
                Id = new Guid("a1b2c3d4-0003-0000-0000-000000000000"), Amount = -500, Comment = "Списание",
                Timestamp = now.AddDays(-2), UserEmail = "unknown@test.com"
            },
            new()
            {
                Id = new Guid("a1b2c3d4-0004-0000-0000-000000000000"), Amount = 1000, Comment = "Оплата",
                Timestamp = now.AddDays(-5), UserEmail = "ivanov@test.com"
            },
            new()
            {
                Id = new Guid("a1b2c3d4-0005-0000-0000-000000000000"), Amount = 1500, Comment = "Пополнение",
                Timestamp = now.AddDays(-10), UserEmail = "petrov@test.com"
            },
            new()
            {
                Id = new Guid("a1b2c3d4-0006-0000-0000-000000000000"), Amount = 500, Comment = "Подарок",
                Timestamp = now.AddDays(-15), UserEmail = "ivanov@test.com"
            },
        ];
        return Task.FromResult(transactions);
    }
}
