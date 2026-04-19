namespace PremierBankTesting.Contracts.Models.Users;

public record UserReportModel(string UserEmail, decimal TotalAmount, int TransactionCount);
