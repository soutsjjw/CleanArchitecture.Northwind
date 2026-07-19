using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.Refresh;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UpdateProfile;
using CleanArchitecture.Northwind.Application.Features.Member.Commands.ChangePassword;
using CleanArchitecture.Northwind.Application.Features.Member.Queries.GetProfile;
using CleanArchitecture.Northwind.Application.Features.Role.Commands.CreateRole;
using CleanArchitecture.Northwind.Application.Features.Role.Commands.EditRole;
using CleanArchitecture.Northwind.Application.Features.Role.Commands.RemoveMemberFromRole;
using CleanArchitecture.Northwind.Application.Features.Role.Queries.EditRolePrepare;
using CleanArchitecture.Northwind.Application.Features.User.Commands.UpdateUser;
using CleanArchitecture.Northwind.Application.Features.User.Queries.UserDetail;
using CleanArchitecture.Northwind.Domain.Entities.Identity;

namespace CleanArchitecture.Northwind.Application.Common.Mappings;

public static class MapsterConfiguration
{
    public static void RegisterMappings(TypeAdapterConfig config)
    {
        config.NewConfig<ApplicationUserProfile, ProfileVm>();
        config.NewConfig<ProfileVm, UpdateProfileCommand>();
        config.NewConfig<ChangePasswordDto, ChangePasswordCommand>();
        config.NewConfig<UserDetailDto, UpdateUserCommand>();
        config.NewConfig<CreateRoleDto, CreateRoleCommand>();
        config.NewConfig<EditRolePrepareDto, EditRoleCommand>();
        config.NewConfig<EditRoleItemPrepareDto, EditRoleItemCommand>();
        config.NewConfig<RemoveMemberFromRoleDto, RemoveMemberFromRoleCommand>();
        config.NewConfig<AccessTokenResponse, RefreshVm>();
    }
}
