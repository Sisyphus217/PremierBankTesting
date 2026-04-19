using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PremierBankTesting.Domain.Entities;
using PremierBankTesting.Infrastructure.Persistence;
using Xunit;

namespace PremierBankTesting.Integration.Tests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly TestWebApplicationFactory _factory;
    protected readonly HttpClient Client;
    protected readonly FakeBankApiClient BankApiClient;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
        BankApiClient = factory.BankApiClient;
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        db.Transactions.RemoveRange(db.Transactions);
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();
        BankApiClient.Transactions = [];
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task SeedAsync(IEnumerable<User>? users = null, IEnumerable<Transaction>? transactions = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (users is not null) db.Users.AddRange(users);
        if (transactions is not null) db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    protected async Task<(HttpResponseMessage Response, T? Body)> PostAsync<T>(string url)
    {
        var response = await Client.PostAsync(url, null);
        if (!response.IsSuccessStatusCode) return (response, default);
        var content = await response.Content.ReadAsStringAsync();
        return (response, JsonSerializer.Deserialize<T>(content, JsonOptions));
    }

    protected async Task<HttpResponseMessage> PostAsync(string url) =>
        await Client.PostAsync(url, null);

    protected Task<List<Transaction>> GetAllTransactionsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return Task.FromResult(db.Transactions.ToList());
    }

    protected Task<List<User>> GetAllUsersAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return Task.FromResult(db.Users.ToList());
    }
}