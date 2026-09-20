using System.Reflection;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.DeleteSupplier;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.SetSupplierActive;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.UpdateSupplier;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Controllers;
using CleanArchitecture.Northwind.Web.ViewModels.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Moq;

namespace CleanArchitecture.Northwind.Web.FunctionalTests.Controllers;

public class SuppliersControllerTests
{
    [Test]
    public async Task SupplierEditTokenCannotAuthorizeDelete()
    {
        var sender = new Mock<ISender>();
        var provider = new EphemeralDataProtectionProvider();
        var controller = new SuppliersController(sender.Object, provider);
        var editToken = provider.CreateProtector("Suppliers.Edit.ItemId.v1").Protect("7");

        var result = await controller.Delete(editToken, CancellationToken.None);

        result.ShouldBeOfType<NotFoundResult>();
        sender.Verify(value => value.Send(It.IsAny<DeleteSupplierCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task SupplierDeleteShouldPreserveCurrentListPage()
    {
        var sender = new Mock<ISender>();
        sender.Setup(value => value.Send(It.IsAny<DeleteSupplierCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var provider = new EphemeralDataProtectionProvider();
        var controller = new SuppliersController(sender.Object, provider);
        var deleteToken = provider.CreateProtector("Suppliers.Delete.ItemId.v1").Protect("7");

        var result = await controller.Delete(deleteToken, pageNumber: 2, cancellationToken: CancellationToken.None);

        var redirect = result.ShouldBeOfType<RedirectToActionResult>();
        redirect.ActionName.ShouldBe(nameof(SuppliersController.Index));
        redirect.RouteValues!["pageNumber"].ShouldBe(2);
    }

    [Test]
    public async Task SupplierSetActiveShouldPreserveCurrentListPage()
    {
        var sender = new Mock<ISender>();
        sender.Setup(value => value.Send(It.IsAny<SetSupplierActiveCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var provider = new EphemeralDataProtectionProvider();
        var controller = new SuppliersController(sender.Object, provider);
        var activeToken = provider.CreateProtector("Suppliers.SetActive.ItemId.v1").Protect("7");

        var result = await controller.SetActive(activeToken, false, pageNumber: 2, cancellationToken: CancellationToken.None);

        var redirect = result.ShouldBeOfType<RedirectToActionResult>();
        redirect.ActionName.ShouldBe(nameof(SuppliersController.Index));
        redirect.RouteValues!["pageNumber"].ShouldBe(2);
    }

    [Test]
    public async Task SupplierEditShouldPreserveCurrentListPage()
    {
        var sender = new Mock<ISender>();
        sender.Setup(value => value.Send(It.IsAny<UpdateSupplierCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var provider = new EphemeralDataProtectionProvider();
        var controller = new SuppliersController(sender.Object, provider);
        var editToken = provider.CreateProtector("Suppliers.Edit.ItemId.v1").Protect("7");

        var result = await controller.Edit(new SupplierEditViewModel
        {
            ProtectedId = editToken,
            CompanyName = "Updated supplier",
            PageNumber = 2
        }, CancellationToken.None);

        var redirect = result.ShouldBeOfType<RedirectToActionResult>();
        redirect.ActionName.ShouldBe(nameof(SuppliersController.Index));
        redirect.RouteValues!["PageNumber"].ShouldBe(2);
    }

    [Test]
    public void SupplierMutatingActionsRequireAntiForgeryAndScopedPolicies()
    {
        AssertActionPolicy(nameof(SuppliersController.Create), Policies.Suppliers_Create);
        AssertActionPolicy(nameof(SuppliersController.Edit), Policies.Suppliers_Update);
        AssertActionPolicy(nameof(SuppliersController.Delete), Policies.Suppliers_Delete);
        AssertActionPolicy(nameof(SuppliersController.SetActive), Policies.Suppliers_Update);
    }

    private static void AssertActionPolicy(string name, string policy)
    {
        var actions = typeof(SuppliersController).GetMethods().Where(method => method.Name == name).ToArray();
        actions.ShouldNotBeEmpty();
        actions.SelectMany(method => method.GetCustomAttributes<AuthorizeAttribute>()).Select(attribute => attribute.Policy).ShouldContain(policy);
        actions.Where(method => method.GetCustomAttribute<HttpPostAttribute>() != null).ShouldAllBe(method => method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null);
    }

    [Test]
    public void SupplierIndexPostRequiresAntiForgery()
    {
        var postIndex = typeof(SuppliersController).GetMethods()
            .Single(method => method.Name == nameof(SuppliersController.Index)
                && method.GetCustomAttribute<HttpPostAttribute>() != null);

        postIndex.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>().ShouldNotBeNull();
    }
}
