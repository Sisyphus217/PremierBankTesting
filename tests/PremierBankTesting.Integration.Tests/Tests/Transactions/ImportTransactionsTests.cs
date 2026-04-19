using System.Net;
using PremierBankTesting.Application.Abstractions.Bank;
using PremierBankTesting.Integration.Tests.Infrastructure;
using Xunit;

namespace PremierBankTesting.Integration.Tests.Tests.Transactions;

public class ImportTransactionsTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string Url = "/api/v1/transactions/import";

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Import_WhenBankHasNewTransactions_SavesAllAndReturnsCount(int count)
    {
        BankApiClient.Transactions = Enumerable.Range(1, count)
            .Select(i => new BankTransaction
            {
                Id = Guid.NewGuid(),
                Amount = i * 100,
                Comment = "Оплата",
                Timestamp = DateTime.UtcNow,
                UserEmail = $"user{i}@test.com"
            })
            .ToList();

        var (response, body) = await PostAsync<ImportResult>(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(count, body!.Imported);
        var saved = await GetAllTransactionsAsync();
        Assert.Equal(count, saved.Count);
    }

    [Fact]
    public async Task Import_WhenBankHasNoTransactions_ReturnsZero()
    {
        BankApiClient.Transactions = [];

        var (response, body) = await PostAsync<ImportResult>(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, body!.Imported);
    }

    [Fact]
    public async Task Import_WhenCalledTwice_IsIdempotentAndDoesNotDuplicate()
    {
        BankApiClient.Transactions =
        [
            new BankTransaction
            {
                Id = Guid.NewGuid(), Amount = 500, Comment = "Списание", Timestamp = DateTime.UtcNow,
                UserEmail = "user@test.com"
            },
        ];

        await PostAsync(Url);
        var (response, body) = await PostAsync<ImportResult>(Url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, body!.Imported);
        Assert.Single(await GetAllTransactionsAsync());
    }

    [Fact]
    public async Task Import_WhenUserNotInDatabase_CreatesUserAutomatically()
    {
        BankApiClient.Transactions =
        [
            new BankTransaction
            {
                Id = Guid.NewGuid(), Amount = 300, Comment = "Подарок", Timestamp = DateTime.UtcNow,
                UserEmail = "newuser@test.com"
            },
        ];

        await PostAsync(Url);

        var users = await GetAllUsersAsync();
        Assert.Single(users);
        Assert.Equal("newuser@test.com", users[0].Email);
    }

    [Theory]
    [InlineData("User@Test.com", "user@test.com")]
    [InlineData("USER@TEST.COM", "user@test.com")]
    [InlineData("uSeR@tEsT.cOm", "user@test.com")]
    [InlineData("user@test.com", "user@test.com")]
    public async Task Import_EmailNormalizesToLowerInvariant(string incomingEmail, string expectedEmail)
    {
        BankApiClient.Transactions =
        [
            new BankTransaction
            {
                Id = Guid.NewGuid(), Amount = 100, Comment = "Оплата", Timestamp = DateTime.UtcNow,
                UserEmail = incomingEmail
            },
        ];

        await PostAsync(Url);

        var users = await GetAllUsersAsync();
        Assert.Single(users);
        Assert.Equal(expectedEmail, users[0].Email);
    }

    [Fact]
    public async Task Import_WhenTwoTransactionsSameEmailDifferentCase_CreatesOnlyOneUser()
    {
        BankApiClient.Transactions =
        [
            new BankTransaction
            {
                Id = Guid.NewGuid(), Amount = 100, Comment = "Оплата", Timestamp = DateTime.UtcNow,
                UserEmail = "User@Test.com"
            },
            new BankTransaction
            {
                Id = Guid.NewGuid(), Amount = 200, Comment = "Пополнение", Timestamp = DateTime.UtcNow,
                UserEmail = "user@test.com"
            },
        ];

        await PostAsync(Url);

        Assert.Single(await GetAllUsersAsync());
        Assert.Equal(2, (await GetAllTransactionsAsync()).Count);
    }

    private record ImportResult(int Imported);
}