using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.CreateRole;

public record CreateRoleCommand : IRequest<Result>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateRoleDto, CreateRoleCommand>();
        }
    }
}
