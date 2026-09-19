using System.Reflection;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.FunctionalTests.Controllers;

public class SuppliersControllerTests
{
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
}
