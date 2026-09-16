using CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerDetail;
using CleanArchitecture.Northwind.Application.FunctionalTests.Infrastructure;
using CleanArchitecture.Northwind.Domain.Entities;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.FunctionalTests.Features.Customers;

public class GetCustomerDetailQueryTranslationTests : TestBase
{
    [Test]
    public async Task HandleShouldReturnTheRequestedCustomersContactAndAddressDetails()
    {
        await TestApp.AddAsync(new Customer
        {
            Id = "ALFKI",
            CompanyName = "Alfreds Futterkiste",
            ContactName = "Maria Anders",
            ContactTitle = "Sales Representative",
            Address = "Obere Str. 57",
            City = "Berlin",
            Region = "Berlin",
            PostalCode = "12209",
            Country = "Germany",
            Phone = "030-0074321",
            Fax = "030-0076545"
        });

        var result = await TestApp.SendAsync(new GetCustomerDetailQuery("ALFKI"));

        result.Succeeded.ShouldBeTrue();
        result.Data.ShouldBe(new CustomerDetailDto(
            "ALFKI",
            "Alfreds Futterkiste",
            "Maria Anders",
            "Sales Representative",
            "Obere Str. 57",
            "Berlin",
            "Berlin",
            "12209",
            "Germany",
            "030-0074321",
            "030-0076545"));
    }
}
