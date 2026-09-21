using System.Collections;
using System.Linq.Expressions;
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
using Microsoft.EntityFrameworkCore.Query;

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
        var (controller, protectedCustomerId) = CreateController(sender.Object, authorization.Object);

        var result = await controller.Details(protectedCustomerId, historyPageNumber: 2);

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
        var (controller, protectedCustomerId) = CreateController(sender.Object, authorization.Object);

        var result = await controller.Details(protectedCustomerId);

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
                    new TestAsyncEnumerable<CustomerOrderHistoryItemDto>(
                        Array.Empty<CustomerOrderHistoryItemDto>().AsQueryable().Expression),
                    1,
                    10).GetAwaiter().GetResult()
            }));
        return sender;
    }

    private static (CustomersController Controller, string ProtectedCustomerId) CreateController(
        ISender sender,
        IAuthorizationService authorizationService)
    {
        var provider = new EphemeralDataProtectionProvider();
        var protectedCustomerId = provider
            .CreateProtector("Customers.Details.CustomerId.v1")
            .Protect("ALFKI");

        var httpContext = new DefaultHttpContext();
        var controller = new CustomersController(sender, provider, Mock.Of<IDataProtectionService>(), authorizationService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ControllerActionDescriptor()
            },
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };

        return (controller, protectedCustomerId);
    }

    private sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object? Execute(Expression expression) => inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var result = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!
                .MakeGenericMethod(resultType).Invoke(inner, [expression]);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType).Invoke(null, [result])!;
        }
    }

    private sealed class TestAsyncEnumerable<T>(Expression expression)
        : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;
        public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    }
}
