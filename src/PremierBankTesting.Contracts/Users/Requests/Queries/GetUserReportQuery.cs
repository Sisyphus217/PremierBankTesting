using MediatR;
using PremierBankTesting.Contracts.Models.Users;

namespace PremierBankTesting.Contracts.Users.Requests.Queries;

public record GetUserReportQuery : IRequest<IReadOnlyList<UserReportModel>>;
