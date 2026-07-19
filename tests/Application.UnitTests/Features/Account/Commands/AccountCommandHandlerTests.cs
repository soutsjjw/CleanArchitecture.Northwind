using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Application.Common.Logging;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.ForgotPassword;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.ResetPassword;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Account.Commands;

public class AccountCommandHandlerTests
{
    private Mock<IApplicationDbContext> _context = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _context = new Mock<IApplicationDbContext>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task RegisterUserShouldDeleteCreatedUserWhenProfileCreationFails()
    {
        const string userId = "created-user-id";
        var userProfileRepository = new Mock<IUserProfileRepository>();
        var userManager = CreateUserManager();
        var handler = new RegisterUserCommandHandler(
            _context.Object,
            _identityService.Object,
            userProfileRepository.Object,
            userManager.Object,
            Mock.Of<ILogger<RegisterUserCommandHandler>>());

        _identityService
            .Setup(x => x.UserRegisterAsync("new.user@example.com", "Password1!"))
            .ReturnsAsync(userId);
        userProfileRepository
            .Setup(x => x.AddUserProfileAsync(It.IsAny<ApplicationUserProfile>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("profile failed"));

        var result = await handler.Handle(new RegisterUserCommand
        {
            Email = "new.user@example.com",
            Password = "Password1!",
            FullName = "New User",
            Title = "Engineer",
            DepartmentId = 10,
            OfficeId = 20
        }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(LoggingEvents.Account.UserProfileCreationFailed);
        _identityService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
        _identityService.Verify(
            x => x.SendConfirmationEmailAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task ForgotPasswordShouldSucceedWithoutSendingEmailWhenEmailDoesNotExist()
    {
        var handler = new ForgotPasswordCommandHandler(
            _context.Object,
            _identityService.Object,
            Mock.Of<ILogger<ForgotPasswordCommandHandler>>());

        _identityService
            .Setup(x => x.GetUserIdAsync("missing@example.com"))
            .ReturnsAsync((string?)null);

        var result = await handler.Handle(new ForgotPasswordCommand
        {
            Email = "missing@example.com",
            Link = "https://app.example.com/Account/ResetPassword"
        }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _identityService.Verify(
            x => x.SendForgotPasswordEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task ResetPasswordShouldRejectPasswordUsedInRecentHistory()
    {
        var handler = new ResetPasswordCommandHandler(
            _context.Object,
            CreateUserManager().Object,
            _identityService.Object);
        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = "user@example.com",
            PasswordHash = "old-password-hash"
        };

        _identityService
            .Setup(x => x.GetUserByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        _identityService
            .Setup(x => x.IsPasswordSameAsLastThree(user, "Password1!"))
            .Returns(true);

        var result = await handler.Handle(new ResetPasswordCommand
        {
            Email = "user@example.com",
            ResetCode = "reset-code",
            NewPassword = "Password1!",
            ConfirmPassword = "Password1!"
        }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("新密碼不可與前三次相同");
        _identityService.Verify(
            x => x.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ResetPasswordShouldRecordPasswordHistoryAfterSuccessfulReset()
    {
        var passwordHistories = new Mock<DbSet<ApplicationUserPasswordHistory>>();
        var handler = new ResetPasswordCommandHandler(
            _context.Object,
            CreateUserManager().Object,
            _identityService.Object);
        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = "user@example.com",
            PasswordHash = "new-password-hash"
        };

        _context
            .Setup(x => x.UserPasswordHistories)
            .Returns(passwordHistories.Object);
        _context
            .Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(1);
        _identityService
            .Setup(x => x.GetUserByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        _identityService
            .Setup(x => x.IsPasswordSameAsLastThree(user, "Password1!"))
            .Returns(false);
        _identityService
            .Setup(x => x.ResetPasswordAsync("user@example.com", "reset-code", "Password1!"))
            .ReturnsAsync(true);

        var result = await handler.Handle(new ResetPasswordCommand
        {
            Email = "user@example.com",
            ResetCode = "reset-code",
            NewPassword = "Password1!",
            ConfirmPassword = "Password1!"
        }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        user.LastPasswordChangedDate.ShouldNotBeNull();
        passwordHistories.Verify(x => x.Add(It.Is<ApplicationUserPasswordHistory>(history =>
            history.UserId == "user-id" &&
            history.PasswordHash == "new-password-hash")), Times.Once);
        _context.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        return new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!,
            null!,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            null!,
            null!,
            null!,
            null!);
    }
}
