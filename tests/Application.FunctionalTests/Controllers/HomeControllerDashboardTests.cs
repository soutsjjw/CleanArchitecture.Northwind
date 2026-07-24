using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;
using CleanArchitecture.Northwind.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.FunctionalTests.Controllers;

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

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CleanArchitecture.Northwind.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
