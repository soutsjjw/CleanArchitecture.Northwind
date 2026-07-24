using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Orders.Commands.DeleteOrder;
using CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrderDetail;
using CleanArchitecture.Northwind.Web.Controllers;
using CleanArchitecture.Northwind.Web.ViewModels.Orders;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.FunctionalTests.Controllers;

public class OrdersControllerProtectedIdTests
{
    [Test]
    public void Order_Razor_views_transmit_protected_ids_while_displaying_the_original_id()
    {
        var indexView = File.ReadAllText(GetWebViewPath("Index.cshtml"));
        var deleteConfirmationView = File.ReadAllText(GetWebViewPath("_DeleteConfirmationModal.cshtml"));

        indexView.ShouldContain("asp-route-id=\"@order.ProtectedId\"");
        indexView.ShouldContain("new { id = order.ProtectedId }");
        indexView.ShouldContain("<td>@order.Id</td>");
        deleteConfirmationView.ShouldContain("name=\"id\" value=\"@Model.ProtectedId\"");
        deleteConfirmationView.ShouldContain("刪除訂單 #@Model.Id");
    }

    [Test]
    public async Task Details_with_valid_protected_order_id_sends_detail_query_with_unprotected_integer_id()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(sender => sender.Send(
                It.IsAny<GetOrderDetailQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDetailDto>.Success(new OrderDetailDto { Id = 42 }));

        var dataProtectionService = new Mock<IDataProtectionService>();
        dataProtectionService
            .Setup(service => service.Unprotect("protected-order-42"))
            .Returns("42");

        var controller = CreateController(mediator.Object, dataProtectionService.Object);

        await controller.Details("protected-order-42");

        mediator.Verify(sender => sender.Send(
            It.Is<GetOrderDetailQuery>(query => query.Id == 42),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteConfirmation_with_valid_protected_order_id_sends_detail_query_with_unprotected_integer_id()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(sender => sender.Send(
                It.IsAny<GetOrderDetailQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDetailDto>.Success(new OrderDetailDto { Id = 43 }));

        var dataProtectionService = new Mock<IDataProtectionService>();
        dataProtectionService
            .Setup(service => service.Unprotect("protected-order-43"))
            .Returns("43");
        dataProtectionService
            .Setup(service => service.Protect("43"))
            .Returns("protected-order-43");

        var controller = CreateController(mediator.Object, dataProtectionService.Object);

        var result = await controller.DeleteConfirmation("protected-order-43");

        var partialView = result.ShouldBeOfType<PartialViewResult>();
        partialView.ViewName.ShouldBe("_DeleteConfirmationModal");
        var model = partialView.Model.ShouldBeOfType<OrderDetailViewModel>();
        model.Id.ShouldBe(43);
        model.ProtectedId.ShouldBe("protected-order-43");

        mediator.Verify(sender => sender.Send(
            It.Is<GetOrderDetailQuery>(query => query.Id == 43),
            It.IsAny<CancellationToken>()), Times.Once);
        dataProtectionService.Verify(service => service.Protect("43"), Times.Once);
    }

    [TestCase("tampered")]
    [TestCase("protected-not-an-integer")]
    public async Task Delete_with_invalid_or_non_integer_unprotected_order_id_redirects_without_sending_delete_command(
        string protectedId)
    {
        var mediator = new Mock<IMediator>();
        var dataProtectionService = new Mock<IDataProtectionService>();
        dataProtectionService
            .Setup(service => service.Unprotect("tampered"))
            .Returns((string)null!);
        dataProtectionService
            .Setup(service => service.Unprotect("protected-not-an-integer"))
            .Returns("not-an-integer");

        var controller = CreateController(mediator.Object, dataProtectionService.Object);

        var result = await controller.Delete(protectedId);

        var redirect = result.ShouldBeOfType<RedirectToActionResult>();
        redirect.ActionName.ShouldBe(nameof(OrdersController.Index));
        mediator.Verify(sender => sender.Send(
            It.IsAny<DeleteOrderCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static OrdersController CreateController(
        IMediator mediator,
        IDataProtectionService dataProtectionService)
    {
        var mapper = new Mock<IMapper>();
        mapper
            .Setup(service => service.Map<OrderDetailViewModel>(It.IsAny<object>()))
            .Returns((object source) => new OrderDetailViewModel
            {
                Id = ((OrderDetailDto)source).Id
            });

        var requestServices = new Mock<IServiceProvider>();
        requestServices
            .Setup(services => services.GetService(typeof(IMediator)))
            .Returns(mediator);

        return new OrdersController(
            mapper.Object,
            dataProtectionService,
            Mock.Of<ILogger<OrdersController>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = requestServices.Object
                }
            }
        };
    }

    private static string GetWebViewPath(string fileName)
        => Path.Combine(GetRepositoryRoot(), "src", "Web", "Views", "Orders", fileName);

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CleanArchitecture.Northwind.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
