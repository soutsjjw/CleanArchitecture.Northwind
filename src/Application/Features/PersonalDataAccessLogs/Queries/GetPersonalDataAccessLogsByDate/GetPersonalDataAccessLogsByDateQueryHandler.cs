using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.PersonalDataAccessLogs.Queries.GetPersonalDataAccessLogsByDate;

public class GetPersonalDataAccessLogsByDateQueryHandler : IRequestHandler<GetPersonalDataAccessLogsByDateQuery, Result<IList<PersonalDataAccessLogDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetPersonalDataAccessLogsByDateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IList<PersonalDataAccessLogDto>>> Handle(GetPersonalDataAccessLogsByDateQuery request, CancellationToken cancellationToken)
    {
        var start = request.Date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        var query = await (from p in _context.PersonalDataAccessLogs.AsNoTracking()

                           join viewer in _context.UserProfiles.AsNoTracking() on p.ViewerUserId equals viewer.UserId
                           into viewer_jointable
                           from viewer in viewer_jointable.DefaultIfEmpty()

                           join target in _context.UserProfiles.AsNoTracking() on p.TargetUserId equals target.UserId
                           into target_jointable
                           from target in target_jointable.DefaultIfEmpty()

                           where p.Accessed >= start && p.Accessed < end

                           select new PersonalDataAccessLogDto
                           {
                               Id = p.Id,
                               ViewerUserName = viewer.FullName ?? "",
                               TargetUserName = target.FullName ?? "",
                               Action = p.Action,
                               Accessed = p.Accessed,
                               Description = p.Description ?? "",
                           }).ToListAsync(cancellationToken);

        return Result<IList<PersonalDataAccessLogDto>>.Success(query);
    }
}
