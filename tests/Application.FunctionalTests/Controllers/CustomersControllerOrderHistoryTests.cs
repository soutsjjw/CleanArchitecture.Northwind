using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerDetail;
using CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerOrderHistory;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Controllers;
using CleanArchitecture.Northwind.Web.ViewModels.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace CleanArchitecture.Northwind.Application.FunctionalTests.Controllers;

public class CustomersControllerOrderHistoryTests
{
    [Test]
    public async Task Details_when_orders_read_is_authorized_sends_customer_history_query()
    {
        var sender = CreateSender();
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<object?>(), Policies.Orders_Read))
            .ReturnsAsync(AuthorizationResult.Success());
        var controller = CreateController(sender.Object, authorization.Object);

        var result = await controller.Details("protected-customer", historyPageNumber: 2);

        result.ShouldBeOfType<ViewResult>().Model.ShouldBeOfType<CustomerDetailViewModel>()
            .CanViewOrderHistory.ShouldBeTrue();
        sender.Verify(service => service.Send(
            It.Is<GetCustomerOrderHistoryQuery>(query => query.CustomerId == "ALFKI" && query.PageNumber == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Details_when_orders_read_is_not_authorized_does_not_send_customer_history_query()
    {
        var sender = CreateSender();
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<object?>(), Policies.Orders_Read))
            .ReturnsAsync(AuthorizationResult.Failed());
        var controller = CreateController(sender.Object, authorization.Object);

        var result = await controller.Details("protected-customer");

        result.ShouldBeOfType<ViewResult>().Model.ShouldBeOfType<CustomerDetailViewModel>()
            .CanViewOrderHistory.ShouldBeFalse();
        sender.Verify(service => service.Send(
            It.IsAny<GetCustomerOrderHistoryQuery>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Mock<ISender> CreateSender()
    {
        var sender = new Mock<ISender>();
        sender.Setup(service => service.Send(
                It.IsAny<GetCustomerDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CustomerDetailDto>.Success(new CustomerDetailDto(
                "ALFKI", "Alfreds Futterkiste", null, null, null, null, null, null, null, null, null)));
        sender.Setup(service => service.Send(
                It.IsAny<GetCustomerOrderHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CustomerOrderHistoryDto>.Success(new CustomerOrderHistoryDto
            {
                Orders = PaginatedList<CustomerOrderHistoryItemDto>.CreateAsync(
                    Array.Empty<CustomerOrderHistoryItemDto>().AsQueryable(), 1, 10).GetAwaiter().GetResult()
            }));
        return sender;
    }

    private static CustomersController CreateController(ISender sender, IAuthorizationService authorizationService)
    {
        var protector = new Mock<IDataProtector>();
        protector.Setup(service => service.Unprotect("protected-customer")).Returns("ALFKI");
        var provider = new Mock<IDataProtectionProvider>();
        provider.Setup(service => service.CreateProtector("Customers.Details.CustomerId.v1")).Returns(protector.Object);

        var httpContext = new DefaultHttpContext();
        return new CustomersController(sender, provider.Object, Mock.Of<IDataProtectionService>(), authorizationService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ControllerActionDescriptor()
            },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };
    }
}
