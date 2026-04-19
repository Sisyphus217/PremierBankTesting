using PremierBankTesting.Contracts.Models.Users;
using PremierBankTesting.Domain.Entities;
using PremierBankTesting.Integration.Tests.Infrastructure;
using Xunit;

namespace PremierBankTesting.Integration.Tests.Tests.Users;

public class GetUserReportTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string Url = "/api/v1/reports/users";

    [Fact]
    public async Task GetUserReport_WithProcessedTransactionsInLastMonth_ReturnsAggregatedByUser()
    {
        var user1 = User.Create("alice@test.com");
        var user2 = User.Create("bob@test.com");
        var t1 = Transaction.Create(Guid.NewGuid(), 100, "Оплата",      DateTime.UtcNow.AddDays(-5),  user1.Id);
        var t2 = Transaction.Create(Guid.NewGuid(), 300, "Пополнение",  DateTime.UtcNow.AddDays(-10), user1.Id);
        var t3 = Transaction.Create(Guid.NewGuid(), 200, "Оплата",      DateTime.UtcNow.AddDays(-3),  user2.Id);
        t1.MarkAsProcessed(); t2.MarkAsProcessed(); t3.MarkAsProcessed();
        await SeedAsync([user1, user2], [t1, t2, t3]);

        var result = await GetAsync<List<UserReportModel>>(Url);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        var alice = result.First(r => r.UserEmail == "alice@test.com");
        Assert.Equal(400, alice.TotalAmount);
        Assert.Equal(2, alice.TransactionCount);
    }

    [Fact]
    public async Task GetUserReport_ExcludesUnprocessedTransactions()
    {
        var user = User.Create("user@test.com");
        var processed   = Transaction.Create(Guid.NewGuid(), 500, "Оплата",     DateTime.UtcNow.AddDays(-5), user.Id);
        var unprocessed = Transaction.Create(Guid.NewGuid(), 999, "Пополнение", DateTime.UtcNow.AddDays(-2), user.Id);
        processed.MarkAsProcessed();
        await SeedAsync([user], [processed, unprocessed]);

        var result = await GetAsync<List<UserReportModel>>(Url);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(500, result[0].TotalAmount);
        Assert.Equal(1, result[0].TransactionCount);
    }

    // daysOld — сколько дней назад была транзакция (должна не попасть в отчёт)
    [Theory]
    [InlineData(31)]
    [InlineData(45)]
    [InlineData(60)]
    [InlineData(90)]
    [InlineData(365)]
    public async Task GetUserReport_ExcludesTransactionsOlderThanOneMonth(int daysOld)
    {
        var user   = User.Create("user@test.com");
        var recent = Transaction.Create(Guid.NewGuid(), 100,  "Оплата",     DateTime.UtcNow.AddDays(-10), user.Id);
        var old    = Transaction.Create(Guid.NewGuid(), 9999, "Пополнение", DateTime.UtcNow.AddDays(-daysOld), user.Id);
        recent.MarkAsProcessed(); old.MarkAsProcessed();
        await SeedAsync([user], [recent, old]);

        var result = await GetAsync<List<UserReportModel>>(Url);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(100, result[0].TotalAmount);
    }

    [Fact]
    public async Task GetUserReport_WhenNoProcessedTransactions_ReturnsEmptyList()
    {
        var result = await GetAsync<List<UserReportModel>>(Url);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // smallAmount, bigAmount — суммы двух пользователей; ожидаем big первым
    [Theory]
    [InlineData(100,   1000)]
    [InlineData(1,     99999)]
    [InlineData(500,   501)]
    [InlineData(0,     1)]
    public async Task GetUserReport_OrderedByTotalAmountDescending(int smallAmount, int bigAmount)
    {
        var userSmall = User.Create("small@test.com");
        var userBig   = User.Create("big@test.com");
        var tSmall = Transaction.Create(Guid.NewGuid(), smallAmount, "Оплата",     DateTime.UtcNow.AddDays(-1), userSmall.Id);
        var tBig   = Transaction.Create(Guid.NewGuid(), bigAmount,   "Пополнение", DateTime.UtcNow.AddDays(-1), userBig.Id);
        tSmall.MarkAsProcessed(); tBig.MarkAsProcessed();
        await SeedAsync([userSmall, userBig], [tSmall, tBig]);

        var result = await GetAsync<List<UserReportModel>>(Url);

        Assert.NotNull(result);
        Assert.Equal("big@test.com",   result[0].UserEmail);
        Assert.Equal("small@test.com", result[1].UserEmail);
    }
}
