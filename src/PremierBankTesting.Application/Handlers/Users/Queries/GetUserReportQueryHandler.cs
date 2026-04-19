using MediatR;
using Microsoft.EntityFrameworkCore;
using PremierBankTesting.Application.Abstractions.Persistence;
using PremierBankTesting.Contracts.Models.Users;
using PremierBankTesting.Contracts.Users.Requests.Queries;

namespace PremierBankTesting.Application.Handlers.Users.Queries;

internal sealed class GetUserReportQueryHandler : IRequestHandler<GetUserReportQuery, IReadOnlyList<UserReportModel>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public GetUserReportQueryHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<UserReportModel>> Handle(GetUserReportQuery request, CancellationToken cancellationToken)
    {
        var from = _timeProvider.GetUtcNow().AddMonths(-1).UtcDateTime;

        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.IsProcessed && t.Timestamp >= from)
            .Join(
                _context.Users,
                t => t.UserId,
                u => u.Id,
                (t, u) => new { u.Email, t.Amount })
            .GroupBy(x => x.Email)
            .OrderByDescending(g => g.Sum(x => x.Amount))
            .Select(g => new UserReportModel(g.Key, g.Sum(x => x.Amount), g.Count()))
            .ToListAsync(cancellationToken);
    }
}
