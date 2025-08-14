using CleanArchitecture.Northwind.Application.Common.DTOs;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface ICommonService
{
    IEnumerable<SelectListItemDto> GetDepartmentOptions();
    IEnumerable<OptionItemDto> GetOfficeOptions();
}
