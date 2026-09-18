# 客戶歷史 Sales Order Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在 Customer 詳情頁提供具分頁的歷史 Sales Order 清單，並以既有受保護訂單詳情路由開啟明細及返回原 Customer。

**Architecture:** 在 Application 新增唯讀 query，使用 `IApplicationDbContext` 投影指定 Customer 的未刪除 Sales Order。Web 在確認 `Orders_Read` 後才送出該 query 並顯示清單；返回導覽只在 Web ViewModel 與路由參數中維護，不把 Data Protection 或 MVC 型別帶入 Application。

**Tech Stack:** ASP.NET Core MVC、MediatR、EF Core、FluentValidation、NUnit、Shouldly。

**Spec:** 使用者已確認的第二期「客戶管理／歷史 Sales Order」共同理解（本對話，2026-09-18）。

## Global Constraints

- 僅顯示 `!IsDelete` 的 Sales Order，且使用 `AsNoTracking()` 與可翻譯 projection。
- 歷史清單與訂單詳情入口只提供給具 `Policies.Orders_Read` 的使用者。
- 所有非同步 Application 與 Controller 呼叫傳遞 `CancellationToken`；不新增套件、Migration 或設定變更。
- Customer 與 Order 識別碼的保護與還原維持在 Web 邊界。

---

### Task 1: Customer 歷史 Sales Order Application Query

**Files:**
- Create: `src/Application/Features/Customers/Queries/GetCustomerOrderHistory/GetCustomerOrderHistoryQuery.cs`
- Create: `src/Application/Features/Customers/Queries/GetCustomerOrderHistory/GetCustomerOrderHistoryQueryValidator.cs`
- Create: `tests/Application.UnitTests/Features/Customers/Queries/GetCustomerOrderHistory/GetCustomerOrderHistoryQueryHandlerTests.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext.Orders`、`PaginatedList<T>`、既有 `OrderShippingStatus`。
- Produces: `GetCustomerOrderHistoryQuery(string CustomerId) : IRequest<Result<CustomerOrderHistoryDto>>`，包含 `PageNumber`、`PageSize`；每筆 `CustomerOrderHistoryItemDto` 提供 `Id`、`OrderDate`、`TotalAmount`、`ShippingStatus`、`ShipperName`。

- [ ] **Step 1: Write the failing test**

```csharp
[Test]
public async Task HandleShouldReturnOnlyTheCustomersActiveOrdersInDescendingOrderDatePages()
{
    var result = await handler.Handle(new GetCustomerOrderHistoryQuery("ALFKI")
    {
        PageNumber = 2,
        PageSize = 1
    }, CancellationToken.None);

    result.Data.Orders.TotalCount.ShouldBe(2);
    result.Data.Orders.Items.Single().Id.ShouldBe(10248);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~GetCustomerOrderHistoryQueryHandlerTests`

Expected: FAIL，因 query 與 handler 尚不存在。

- [ ] **Step 3: Write minimal implementation**

```csharp
var query = context.Orders.AsNoTracking()
    .Where(order => !order.IsDelete && order.CustomerId == request.CustomerId)
    .OrderByDescending(order => order.OrderDate).ThenByDescending(order => order.Id)
    .Select(order => new CustomerOrderHistoryItemDto(/* required fields */));
var orders = await PaginatedList<CustomerOrderHistoryItemDto>.CreateAsync(
    query, request.PageNumber, request.PageSize, cancellationToken);
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~GetCustomerOrderHistoryQueryHandlerTests`

Expected: PASS。

### Task 2: Customer 詳情頁權限與歷史清單

**Files:**
- Modify: `src/Web/Controllers/CustomersController.cs`
- Modify: `src/Web/ViewModels/Customers/CustomerDetailViewModel.cs`
- Modify: `src/Web/Views/Customers/Details.cshtml`
- Create: `tests/Application.FunctionalTests/Controllers/CustomersControllerOrderHistoryTests.cs`

**Interfaces:**
- Consumes: Task 1 query、`IAuthorizationService.AuthorizeAsync(User, Policies.Orders_Read)`、既有 Customer 詳情 Data Protector。
- Produces: `CustomerDetailViewModel` 的 `CanViewOrderHistory`、歷史清單項目與分頁資料；歷史訂單連結提供受保護訂單 Id、受保護 Customer 詳情 Id、目前歷史頁碼。

- [ ] **Step 1: Write the failing test**

```csharp
[Test]
public async Task Details_when_orders_read_is_authorized_sends_customer_history_query()
{
    await controller.Details("protected-customer", historyPageNumber: 2);

    mediator.Verify(sender => sender.Send(
        It.Is<GetCustomerOrderHistoryQuery>(query =>
            query.CustomerId == "ALFKI" && query.PageNumber == 2),
        It.IsAny<CancellationToken>()), Times.Once);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~CustomersControllerOrderHistoryTests`

Expected: FAIL，因 Controller 尚未送出歷史 query。

- [ ] **Step 3: Write minimal implementation**

```csharp
var canViewOrderHistory = (await authorizationService.AuthorizeAsync(
    User, Policies.Orders_Read)).Succeeded;
var history = canViewOrderHistory
    ? await sender.Send(new GetCustomerOrderHistoryQuery(customerId)
      { PageNumber = historyPageNumber, PageSize = historyPageSize }, cancellationToken)
    : null;
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~CustomersControllerOrderHistoryTests`

Expected: PASS；另以 Razor 顯示 `CanViewOrderHistory` 包住表格、顯示日期／金額／狀態／Shipper、`_PaginationPartial` 與訂單詳情入口。

### Task 3: 訂單詳情返回 Customer 導覽

**Files:**
- Modify: `src/Web/Controllers/OrdersController.cs`
- Modify: `src/Web/ViewModels/Orders/OrderDetailViewModel.cs`
- Modify: `src/Web/Views/Orders/Details.cshtml`
- Modify: `tests/Application.FunctionalTests/Controllers/OrdersControllerProtectedIdTests.cs`

**Interfaces:**
- Consumes: Customer 詳情連結的受保護 Id 與 `historyPageNumber`。
- Produces: 自 Customer 歷史清單開啟的訂單詳情，返回 `CustomersController.Details` 並保留 Customer 詳情 Id 和歷史頁碼。

- [ ] **Step 1: Write the failing test**

```csharp
[Test]
public async Task Details_from_customer_history_preserves_customer_return_context()
{
    var result = await controller.Details("protected-order-42",
        customerDetailsId: "protected-customer", historyPageNumber: 2);

    result.ShouldBeOfType<ViewResult>().Model.ShouldBeOfType<OrderDetailViewModel>()
        .CustomerDetailsId.ShouldBe("protected-customer");
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~OrdersControllerProtectedIdTests`

Expected: FAIL，因 ViewModel 尚無 Customer 返回脈絡。

- [ ] **Step 3: Write minimal implementation**

```csharp
return View(MapToDetailViewModel(result.Data, listQuery, customerDetailsId, historyPageNumber));
```

在 Razor 中，當 `CustomerDetailsId` 非空時，以 `Customers/Details`（含 `id` 與 `historyPageNumber`）作為返回連結；否則維持既有訂單清單返回連結。

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~OrdersControllerProtectedIdTests`

Expected: PASS。

### Task 4: 整體驗證

**Files:**
- Verify: `CleanArchitecture.Northwind.slnx`

- [ ] **Step 1: Run affected test projects**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj` and `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj`

Expected: PASS。

- [ ] **Step 2: Build solution**

Run: `dotnet build CleanArchitecture.Northwind.slnx`

Expected: PASS，無編譯錯誤。

