using System.Net;
using PremierBankTesting.Domain.Entities;
using PremierBankTesting.Integration.Tests.Infrastructure;
using Xunit;

namespace PremierBankTesting.Integration.Tests.Tests.Transactions;

public class MarkAsProcessedTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private static string Url(Guid id) => $"/api/v1/transactions/{id}/mark-as-processed";

    [Theory]
    [InlineData(100, "Оплата")]
    [InlineData(500, "Пополнение")]
    [InlineData(-200, "Списание")]
    [InlineData(99999, "Перевод")]
    [InlineData(0, "Коррекция")]
    public async Task MarkAsProcessed_WhenTransactionExists_Returns204AndSetsIsProcessed(int amount, string comment)
    {
        var user = User.Create("user@test.com");
        var transaction = Transaction.Create(Guid.NewGuid(), amount, comment, DateTime.UtcNow, user.Id);
        await SeedAsync([user], [transaction]);

        var response = await PostAsync(Url(transaction.Id));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True((await GetAllTransactionsAsync())[0].IsProcessed);
    }

    [Fact]
    public async Task MarkAsProcessed_WhenTransactionDoesNotExist_Returns404()
    {
        var response = await PostAsync(Url(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(100, "Оплата")]
    [InlineData(500, "Пополнение")]
    [InlineData(-300, "Списание")]
    public async Task MarkAsProcessed_WhenAlreadyProcessed_Returns204Idempotently(int amount, string comment)
    {
        var user = User.Create("user@test.com");
        var transaction = Transaction.Create(Guid.NewGuid(), amount, comment, DateTime.UtcNow, user.Id);
        transaction.MarkAsProcessed();
        await SeedAsync([user], [transaction]);

        var response = await PostAsync(Url(transaction.Id));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}