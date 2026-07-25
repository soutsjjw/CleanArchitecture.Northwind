# 訂單管理一期補強 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 支援產品搜尋、安全拒絕重複軟刪除、依權限顯示刪除按鈕，並保存列表返回狀態。

**Architecture:** Application Handler 處理產品搜尋與軟刪除狀態；Web Controller/View 保存 route values 並依既有授權服務呈現 UI。保留既有受保護 ID、Anti-Forgery 與 Action 授權。

**Tech Stack:** ASP.NET Core MVC、MediatR、EF Core、NUnit、Moq、Shouldly。

## Global Constraints

- 不新增套件、Migration、資料表欄位或設定。
- Application 不依賴 MVC 或 Infrastructure 實作；Web 不直接操作 DbContext。
- 每項 production code 前先寫測試並確認其失敗。

---

### Task 1: 產品名稱及產品編號搜尋

**Files:**
- Modify: `src/Application/Features/Orders/Queries/GetOrders/GetOrdersQueryHandler.cs:44-55`
- Modify: `tests/Application.UnitTests/Features/Orders/Queries/GetOrders/GetOrdersQueryHandlerTests.cs`

**Interfaces:**
- Consumes: `GetOrdersQuery.Keyword`、`Order.OrderDetails`、`OrderDetail.ProductId`、`OrderDetail.Product.ProductName`。
- Produces: `Handle` 在產品名稱或產品編號符合 Keyword 時包含該訂單。

- [ ] **Step 1: 寫產品名稱搜尋的失敗測試**

```csharp
[Test]
public async Task HandleShouldFindOrderByProductName()
{
    var result = await handler.Handle(new GetOrdersQuery
    {
        Keyword = "chai", PageNumber = 1, PageSize = 10
    }, CancellationToken.None);

    result.Data.Orders.Items.Select(order => order.Id).ShouldBe([10248]);
}
```

- [ ] **Step 2: 確認產品名稱測試失敗**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~GetOrdersQueryHandlerTests.HandleShouldFindOrderByProductName"`

Expected: FAIL，產品名稱尚未列入 Keyword 條件。

- [ ] **Step 3: 寫產品編號搜尋的失敗測試並確認失敗**

```csharp
[Test]
public async Task HandleShouldFindOrderByProductId()
{
    var result = await handler.Handle(new GetOrdersQuery
    {
        Keyword = "11", PageNumber = 1, PageSize = 10
    }, CancellationToken.None);

    result.Data.Orders.Items.Select(order => order.Id).ShouldBe([10248]);
}
```

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~GetOrdersQueryHandlerTests.HandleShouldFindOrderByProductId"`

Expected: FAIL，產品編號尚未列入 Keyword 條件。

- [ ] **Step 4: 寫最小查詢實作**

```csharp
order.OrderDetails.Any(detail =>
    detail.ProductId.ToString().Contains(keyword) ||
    detail.Product.ProductName.ToLower().Contains(keyword))
```

- [ ] **Step 5: 確認兩項搜尋測試通過**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~GetOrdersQueryHandlerTests.HandleShouldFindOrderBy"`

Expected: PASS。

### Task 2: 拒絕重複刪除

**Files:**
- Modify: `src/Application/Features/Orders/Commands/DeleteOrder/DeleteOrderCommandHandler.cs:7-20`
- Modify: `tests/Application.UnitTests/Features/Orders/Commands/DeleteOrder/DeleteOrderCommandHandlerTests.cs`

**Interfaces:**
- Consumes: `DeleteOrderCommand.Id`、`Order.IsDelete`。
- Produces: 已軟刪除訂單回傳失敗，且不呼叫 `SaveChangesAsync`。

- [ ] **Step 1: 寫已刪訂單的失敗測試**

```csharp
[Test]
public async Task HandleShouldRejectAlreadyDeletedOrder()
{
    var order = new Order { Id = 10248, IsDelete = true };
    var result = await handler.Handle(new DeleteOrderCommand { Id = order.Id }, CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}
```

- [ ] **Step 2: 確認測試失敗**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~DeleteOrderCommandHandlerTests.HandleShouldRejectAlreadyDeletedOrder"`

Expected: FAIL，既有 Handler 對已刪訂單回傳成功。

- [ ] **Step 3: 寫最小刪除實作**

```csharp
if (order is null || order.IsDelete)
{
    return await Result.FailureAsync("找不到訂單。");
}
```

- [ ] **Step 4: 確認刪除 Handler 測試通過**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~DeleteOrderCommandHandlerTests"`

Expected: PASS。

### Task 3: UI 權限與返回列表狀態

**Files:**
- Modify: `src/Web/Controllers/OrdersController.cs`
- Modify: `src/Web/ViewModels/Orders/OrderDetailViewModel.cs`
- Modify: `src/Web/Views/Orders/Index.cshtml`
- Modify: `src/Web/Views/Orders/Details.cshtml`
- Modify: `tests/Application.FunctionalTests/Controllers/OrdersControllerProtectedIdTests.cs`

**Interfaces:**
- Consumes: `IAuthorizationService.AuthorizeAsync(User, Policies.Orders_Delete)` 與 `OrderIndexViewModel` 的查詢條件。
- Produces: 取得刪除權限才輸出刪除按鈕；詳情返回連結回復 Keyword、日期、狀態、排序與分頁。

- [ ] **Step 1: 寫 Razor 結構失敗測試**

```csharp
[Test]
public void OrderViewsUseDeletePolicyAndPreserveListRouteValues()
{
    var indexView = File.ReadAllText(GetWebViewPath("Index.cshtml"));
    var detailView = File.ReadAllText(GetWebViewPath("Details.cshtml"));

    indexView.ShouldContain("AuthorizeAsync(User, Policies.Orders_Delete)");
    detailView.ShouldContain("asp-route-keyword");
    detailView.ShouldContain("asp-route-pageNumber");
}
```

- [ ] **Step 2: 確認測試失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests.OrderViewsUseDeletePolicyAndPreserveListRouteValues"`

Expected: FAIL，現有 View 未檢查刪除 policy 且未傳遞列表狀態。

- [ ] **Step 3: 寫最小 Web 實作**

```csharp
@inject IAuthorizationService AuthorizationService
@{ var canDeleteOrders = await AuthorizationService.AuthorizeAsync(User, Policies.Orders_Delete); }
@if (canDeleteOrders.Succeeded)
{
    <button type="button" class="btn btn-sm btn-outline-danger js-open-delete-modal"
            data-url="@Url.Action("DeleteConfirmation", "Orders", new { id = order.ProtectedId })">刪除</button>
}
```

`Details` Action 接收可選的列表查詢參數，寫入 Detail ViewModel；Index 連結傳入目前條件，Details 返回連結傳回相同 route values。

- [ ] **Step 4: 確認 View/Controller 測試通過**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests"`

Expected: PASS。

### Task 4: 整合驗證

**Files:**
- Verify only: Tasks 1–3 涉及檔案。

**Interfaces:**
- Consumes: Tasks 1–3。
- Produces: 可建置且相關訂單測試通過的補強功能。

- [ ] **Step 1: 檢查變更格式**

Run: `git diff --check`

Expected: exit code 0。

- [ ] **Step 2: 執行訂單測試**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~Features.Orders"` 和 `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests"`

Expected: PASS。

- [ ] **Step 3: 建置受影響專案**

Run: `dotnet build src/Application/Application.csproj` 和 `dotnet build src/Web/Web.csproj`

Expected: exit code 0，且無新增編譯錯誤。

- [ ] **Step 4: 提交程式與測試**

Run: `git add src/Application/Features/Orders src/Web/Controllers/OrdersController.cs src/Web/ViewModels/Orders src/Web/Views/Orders tests/Application.UnitTests/Features/Orders tests/Application.FunctionalTests/Controllers/OrdersControllerProtectedIdTests.cs`，接著 `git commit -m "feat: harden order management"`。
