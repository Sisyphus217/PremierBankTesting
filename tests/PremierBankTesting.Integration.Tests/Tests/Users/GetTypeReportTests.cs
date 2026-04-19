using PremierBankTesting.Contracts.Models.Transactions;
using PremierBankTesting.Domain.Entities;
using PremierBankTesting.Integration.Tests.Infrastructure;
using Xunit;

namespace PremierBankTesting.Integration.Tests.Tests.Users;

public class GetTypeReportTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string Url = "/api/v1/reports/types";

    // amount1, amount2 — суммы двух транзакций одного типа; expectedTotal = amount1 + amount2
    [Theory]
    [InlineData(100,  200,  "Оплата",     300)]
    [InlineData(500,  1000, "Пополнение", 1500)]
    [InlineData(1,    99,   "Подарок",    100)]
    [InlineData(1000, 2500, "Перевод",    3500)]
    public async Task GetTypeReport_GroupsTransactionsByComment(int amount1, int amount2, string comment, int expectedTotal)
    {
        var user = User.Create("user@test.com");
        var t1 = Transaction.Create(Guid.NewGuid(), amount1, comment, DateTime.UtcNow, user.Id);
        var t2 = Transaction.Create(Guid.NewGuid(), amount2, comment, DateTime.UtcNow, user.Id);
        t1.MarkAsProcessed(); t2.MarkAsProcessed();
        await SeedAsync([user], [t1, t2]);

        var result = await GetAsync<List<TransactionTypeReportModel>>(Url);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(expectedTotal, result[0].TotalAmount);
        Assert.Equal(2, result[0].TransactionCount);
    }

    [Theory]
    [InlineData(100,  999)]
    [InlineData(1,    50000)]
    [InlineData(9999, 10000)]
    public async Task GetTypeReport_ExcludesUnprocessedTransactions(int processedAmount, int unprocessedAmount)
    {
        var user        = User.Create("user@test.com");
        var processed   = Transaction.Create(Guid.NewGuid(), processedAmount,   "Оплата", DateTime.UtcNow, user.Id);
        var unprocessed = Transaction.Create(Guid.NewGuid(), unprocessedAmount, "Оплата", DateTime.UtcNow, user.Id);
        processed.MarkAsProcessed();
        await SeedAsync([user], [processed, unprocessed]);

        var result = await GetAsync<List<TransactionTypeReportModel>>(Url);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(processedAmount, result[0].TotalAmount);
        Assert.Equal(1, result[0].TransactionCount);
    }

    // smallAmount, bigType — ожидаем big первым в списке
    [Theory]
    [InlineData(100,   "Малая",   5000,   "Большая")]
    [InlineData(1,     "Первая",  99999,  "Вторая")]
    [InlineData(1000,  "A",       1001,   "B")]
    [InlineData(500,   "Списание", 50000, "Пополнение")]
    public async Task GetTypeReport_OrderedByTotalAmountDescending(
        int smallAmount, string smallType, int bigAmount, string bigType)
    {
        var user   = User.Create("user@test.com");
        var tSmall = Transaction.Create(Guid.NewGuid(), smallAmount, smallType, DateTime.UtcNow, user.Id);
        var tBig   = Transaction.Create(Guid.NewGuid(), bigAmount,   bigType,   DateTime.UtcNow, user.Id);
        tSmall.MarkAsProcessed(); tBig.MarkAsProcessed();
        await SeedAsync([user], [tSmall, tBig]);

        var result = await GetAsync<List<TransactionTypeReportModel>>(Url);

        Assert.NotNull(result);
        Assert.Equal(bigType,   result[0].Type);
        Assert.Equal(smallType, result[1].Type);
    }

    [Fact]
    public async Task GetTypeReport_WhenNoProcessedTransactions_ReturnsEmptyList()
    {
        var result = await GetAsync<List<TransactionTypeReportModel>>(Url);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
