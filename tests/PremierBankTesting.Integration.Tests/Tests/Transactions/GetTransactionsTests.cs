using PremierBankTesting.Contracts.Models.Transactions;
using PremierBankTesting.Contracts.Paginations;
using PremierBankTesting.Domain.Entities;
using PremierBankTesting.Integration.Tests.Infrastructure;
using Xunit;

namespace PremierBankTesting.Integration.Tests.Tests.Transactions;

public class GetTransactionsTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string BaseUrl = "/api/v1/transactions";

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task GetTransactions_FilterByIsProcessed_ReturnsOnlyMatchingTransactions(bool filterValue,
        bool expectedIsProcessed)
    {
        var user = User.Create("user@test.com");
        var processed = Transaction.Create(Guid.NewGuid(), 100, "Оплата", DateTime.UtcNow, user.Id);
        var unprocessed = Transaction.Create(Guid.NewGuid(), 200, "Пополнение", DateTime.UtcNow, user.Id);
        processed.MarkAsProcessed();
        await SeedAsync([user], [processed, unprocessed]);

        var result = await GetAsync<PagedResult<TransactionModel>>($"{BaseUrl}?isProcessed={filterValue}");

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(expectedIsProcessed, result.Items[0].IsProcessed);
    }

    [Theory]
    [InlineData(10, 1, 3, 3, 4)]
    [InlineData(10, 2, 3, 3, 4)]
    [InlineData(10, 4, 3, 1, 4)]
    [InlineData(5, 1, 5, 5, 1)]
    [InlineData(5, 1, 10, 5, 1)]
    [InlineData(7, 2, 3, 3, 3)]
    public async Task GetTransactions_Pagination_ReturnsCorrectSlice(
        int totalSeeded, int page, int pageSize, int expectedItemsOnPage, int expectedTotalPages)
    {
        var user = User.Create("user@test.com");
        var transactions = Enumerable.Range(1, totalSeeded)
            .Select(i => Transaction.Create(Guid.NewGuid(), i * 100, "Оплата", DateTime.UtcNow.AddMinutes(-i), user.Id))
            .ToList();
        await SeedAsync([user], transactions);

        var result = await GetAsync<PagedResult<TransactionModel>>($"{BaseUrl}?page={page}&pageSize={pageSize}");

        Assert.NotNull(result);
        Assert.Equal(totalSeeded, result.TotalCount);
        Assert.Equal(expectedItemsOnPage, result.Items.Count);
        Assert.Equal(expectedTotalPages, result.TotalPages);
        Assert.Equal(page, result.Page);
    }

    [Fact]
    public async Task GetTransactions_WhenEmpty_ReturnsEmptyList()
    {
        var result = await GetAsync<PagedResult<TransactionModel>>(BaseUrl);

        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetTransactions_OrderedByTimestampDescending()
    {
        var user = User.Create("user@test.com");
        var oldest = Transaction.Create(Guid.NewGuid(), 100, "Оплата", DateTime.UtcNow.AddDays(-3), user.Id);
        var middle = Transaction.Create(Guid.NewGuid(), 200, "Пополнение", DateTime.UtcNow.AddDays(-2), user.Id);
        var newest = Transaction.Create(Guid.NewGuid(), 300, "Подарок", DateTime.UtcNow.AddDays(-1), user.Id);
        await SeedAsync([user], [oldest, middle, newest]);

        var result = await GetAsync<PagedResult<TransactionModel>>(BaseUrl);

        Assert.NotNull(result);
        Assert.Equal(newest.Id, result.Items[0].Id);
        Assert.Equal(middle.Id, result.Items[1].Id);
        Assert.Equal(oldest.Id, result.Items[2].Id);
    }
}