using MediatR;

namespace PremierBankTesting.Contracts.Transactions.Requests.Commands;

public record MarkTransactionProcessedCommand(Guid TransactionId) : IRequest<bool>;
