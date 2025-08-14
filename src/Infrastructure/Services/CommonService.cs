using CleanArchitecture.Northwind.Application.Common.DTOs;
using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public class CommonService : ICommonService
{
    private readonly IApplicationDbContext _context;

    public CommonService(IApplicationDbContext context)
    {
        _context = context;
    }

    public IEnumerable<SelectListItemDto> GetDepartmentOptions()
    {
        var list = _context.Departments.ToList().Select(x => new SelectListItemDto
        {
            Value = x.DepartmentId.ToString(),
            Text = x.DeptName
        });

        return list;
    }

    public IEnumerable<OptionItemDto> GetOfficeOptions()
    {
        var list = _context.Offices.ToList().Select(x => new OptionItemDto
        {
            Value = x.OfficeId.ToString(),
            Text = x.OfficeName,
            ParentValue = x.DepartmentId.ToString()
        });

        return list;
    }
}
