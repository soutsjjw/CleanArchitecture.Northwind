using CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomers;
using CleanArchitecture.Northwind.Application.FunctionalTests.Infrastructure;
using CleanArchitecture.Northwind.Domain.Entities;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.FunctionalTests.Features.Customers;

public class GetCustomersQueryTranslationTests : TestBase
{
    [Test]
    public async Task HandleShouldTranslateDefaultCompanyNameSortToSql()
    {
        await TestApp.AddAsync(new Customer
        {
            Id = "ALFKI",
            CompanyName = "Alfreds Futterkiste",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = "functional-test"
        });

        var result = await TestApp.SendAsync(new GetCustomersQuery
        {
            PageNumber = 1,
            PageSize = 10
        });

        result.Succeeded.ShouldBeTrue();
        result.Data.Customers.Items.Select(customer => customer.Id).ShouldBe(["ALFKI"]);
    }
}
