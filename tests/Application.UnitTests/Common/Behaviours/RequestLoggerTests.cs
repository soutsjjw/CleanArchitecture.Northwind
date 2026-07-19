using CleanArchitecture.Northwind.Application.Common.Behaviours;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CleanArchitecture.Northwind.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<RegisterUserCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<RegisterUserCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<RegisterUserCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(CreateRegisterUserCommand(), new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<RegisterUserCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(CreateRegisterUserCommand(), new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }

    private static RegisterUserCommand CreateRegisterUserCommand()
        => new()
        {
            Email = "new.user@example.com",
            Password = "Password1!",
            FullName = "New User",
            Title = "Engineer",
            DepartmentId = 1,
            OfficeId = 1
        };
}
