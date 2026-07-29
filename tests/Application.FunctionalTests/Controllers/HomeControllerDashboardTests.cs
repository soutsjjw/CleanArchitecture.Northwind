using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;
using CleanArchitecture.Northwind.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// This controller-only suite intentionally lives outside the Application
// functional-test SetUpFixture namespace so it does not start SQL Testcontainers.
namespace CleanArchitecture.Northwind.Web.FunctionalTests.Controllers;

public class HomeControllerDashboardTests
{
    [Test]
    public void DashboardActionsRequireAllScopeOrdersAndProductsPolicies()
    {
        var source = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "src", "Web", "Controllers", "HomeController.cs"));
        source.ShouldContain("[Authorize(Policy = \"Orders:Read:All\")]");
        source.ShouldContain("[Authorize(Policy = \"Products:Read:All\")]");
        source.ShouldNotContain("[AllowAnonymous]\r\npublic class HomeController");
    }

    [Test]
    public void HomeSeparatesAnonymousCatalogFromAuthorizedDashboard()
    {
        var dashboard = typeof(HomeController).GetMethod(nameof(HomeController.Dashboard));

        var indexGet = typeof(HomeController).GetMethods()
            .Single(method => method.Name == nameof(HomeController.Index)
                && method.GetCustomAttributes(typeof(HttpGetAttribute), inherit: true).Any());
        var indexPost = typeof(HomeController).GetMethods()
            .Single(method => method.Name == nameof(HomeController.Index)
                && method.GetCustomAttributes(typeof(HttpPostAttribute), inherit: true).Any());

        indexGet.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true)
            .ShouldNotBeEmpty();
        indexPost.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true)
            .ShouldNotBeEmpty();
        indexPost.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true)
            .ShouldNotBeEmpty();
        dashboard.ShouldNotBeNull();
        dashboard.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Select(attribute => attribute.Policy)
            .ShouldContain("Orders:Read:All");
        dashboard.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Select(attribute => attribute.Policy)
            .ShouldContain("Products:Read:All");
    }

    [Test]
    public void PublicCatalogViewProvidesPaginationCompatibilityFunctions()
    {
        var view = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(),
            "src",
            "Web",
            "Views",
            "Home",
            "Index.cshtml"));

        view.ShouldContain("window.saveOrdersScrollPosition = window.saveOrdersScrollPosition ?? (() => {});");
        view.ShouldContain("window.submitOrdersSearch = form =>");
    }

    [Test]
    public void PublicCatalogViewShowsLoadingOverlayForSearchAndPagination()
    {
        var view = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(),
            "src",
            "Web",
            "Views",
            "Home",
            "Index.cshtml"));

        view.ShouldContain("id=\"publicProductSearchForm\"");
        view.ShouldContain("$('#loadingOverlay').removeClass('d-none');");
        view.ShouldContain("publicProductSearchForm.addEventListener('submit'");
        view.ShouldContain("window.submitOrdersSearch(event.currentTarget);");
        view.ShouldContain("method=\"post\"");
        view.ShouldContain("@Html.AntiForgeryToken()");
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CleanArchitecture.Northwind.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
