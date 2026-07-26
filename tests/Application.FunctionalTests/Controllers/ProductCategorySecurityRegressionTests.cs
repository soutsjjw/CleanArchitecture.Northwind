using System.Security.Claims;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Categories.Commands.DeleteCategory;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.DeleteProduct;
using CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductFormOptions;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Infrastructure.Authorization;
using CleanArchitecture.Northwind.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace CleanArchitecture.Northwind.Web.FunctionalTests.Controllers;

public class ProductCategorySecurityRegressionTests
{
    [TestCase(Policies.Categories_Read)]
    [TestCase(Policies.Suppliers_Read)]
    public async Task Read_policy_resolves_through_permission_policy_provider(
        string policyName)
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        await using var serviceProvider = services.BuildServiceProvider();
        var policyProvider =
            serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(policyName);

        policy.ShouldNotBeNull();
        policy.Requirements.ShouldContain(
            requirement => requirement is PermissionRequirement);
    }

    [Test]
    public async Task Product_details_token_cannot_authorize_product_delete()
    {
        var sender = CreateDeleteSender();
        var provider = new EphemeralDataProtectionProvider();
        var controller = CreateProductsController(sender, provider);
        var token = provider
            .CreateProtector("Products.Details.ItemId.v1")
            .Protect("7");

        var result = await controller.Delete(token, CancellationToken.None);

        result.ShouldBeOfType<NotFoundResult>();
        sender.Verify(
            value => value.Send(
                It.IsAny<DeleteProductCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Category_edit_token_cannot_authorize_product_delete()
    {
        var sender = CreateDeleteSender();
        var provider = new EphemeralDataProtectionProvider();
        var controller = CreateProductsController(sender, provider);
        var token = provider
            .CreateProtector("Categories.Edit.ItemId.v1")
            .Protect("7");

        var result = await controller.Delete(token, CancellationToken.None);

        result.ShouldBeOfType<NotFoundResult>();
        sender.Verify(
            value => value.Send(
                It.IsAny<DeleteProductCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Product_create_denies_before_loading_supplier_options_without_supplier_read()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(value => value.Send(
                It.IsAny<GetProductFormOptionsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductFormOptionsDto>.Success(
                new ProductFormOptionsDto([], [])));
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(value => value.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object?>(),
                Policies.Suppliers_Read))
            .ReturnsAsync(AuthorizationResult.Failed());
        var controller = CreateProductsController(
            sender,
            new EphemeralDataProtectionProvider(),
            authorizationService);

        var result = await controller.Create(CancellationToken.None);

        result.ShouldBeOfType<ForbidResult>();
        sender.Verify(
            value => value.Send(
                It.IsAny<GetProductFormOptionsQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Mock<ISender> CreateDeleteSender()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(value => value.Send(
                It.IsAny<DeleteProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        sender
            .Setup(value => value.Send(
                It.IsAny<DeleteCategoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        return sender;
    }

    private static ProductsController CreateProductsController(
        Mock<ISender> sender,
        IDataProtectionProvider provider,
        Mock<IAuthorizationService>? authorizationService = null)
    {
        authorizationService ??= CreateSuccessfulAuthorizationService();
        return CreateController<ProductsController>(
            sender,
            provider,
            authorizationService);
    }

    private static TController CreateController<TController>(
        Mock<ISender> sender,
        IDataProtectionProvider provider,
        Mock<IAuthorizationService> authorizationService)
        where TController : Controller
    {
        var legacyDataProtection = new Mock<IDataProtectionService>();
        legacyDataProtection
            .Setup(value => value.Unprotect(It.IsAny<string>()))
            .Returns("7");
        legacyDataProtection
            .Setup(value => value.Protect(It.IsAny<string>()))
            .Returns<string>(value => value);

        var constructor = typeof(TController)
            .GetConstructors()
            .Single();
        var arguments = constructor
            .GetParameters()
            .Select(parameter => parameter.ParameterType switch
            {
                var type when type == typeof(ISender) => (object)sender.Object,
                var type when type == typeof(IDataProtectionProvider) =>
                    (object)provider,
                var type when type == typeof(IDataProtectionService) =>
                    (object)legacyDataProtection.Object,
                var type when type == typeof(IAuthorizationService) =>
                    (object)authorizationService.Object,
                _ => throw new InvalidOperationException(
                    $"Unsupported controller dependency: {parameter.ParameterType}")
            })
            .ToArray();

        var controller =
            (TController)constructor.Invoke(arguments);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            },
            ActionDescriptor = new ControllerActionDescriptor()
        };
        return controller;
    }

    private static Mock<IAuthorizationService>
        CreateSuccessfulAuthorizationService()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(value => value.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object?>(),
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());
        return authorizationService;
    }
}
