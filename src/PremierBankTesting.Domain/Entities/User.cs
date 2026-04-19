namespace PremierBankTesting.Domain.Entities;

public class User
{
    protected User() { }

    private User(Guid id, string email)
    {
        Id = id;
        Email = email;
    }

    public static User Create(string email) => new(Guid.NewGuid(), email.ToLowerInvariant());

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
}
