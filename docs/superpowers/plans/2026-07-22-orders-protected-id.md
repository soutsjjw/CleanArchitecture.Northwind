# Orders 受保護 ID Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 使用 ASP.NET Core Data Protection 保護 Orders MVC 頁面與 `OrdersController` 間傳輸的訂單 ID，同時保留畫面上的可讀訂單編號。

**Architecture:** 變更限定於 Web 層：ViewModel 分別保存展示用 `Id` 與傳輸用 `ProtectedId`；Controller 在 View 輸出前加密，在接收請求時解密與驗證後才送出 Application Query 或 Command。Application、Domain 與 Infrastructure 的 Orders use case 契約不變。

**Tech Stack:** ASP.NET Core MVC、Razor、Microsoft ASP.NET Core Data Protection、MediatR、Mapster、NUnit、Moq、Shouldly。

## Global Constraints

- 沿用既有 `IDataProtectionService`，不得新增 NuGet 套件。
- 僅保護 URL、AJAX URL 與表單 hidden input；UI 保持顯示原始訂單編號。
- 無效 token 不得傳遞到 Application，且不得回傳解密例外細節。
- 保留既有授權與 POST Anti-Forgery 保護。
- 不變更 Application、Domain、Infrastructure 的 Orders Query／Command 與資料庫模型。

---

### Task 1: 建立 Controller 請求邊界的紅燈測試

**Files:**
- Create: `tests/Application.FunctionalTests/Controllers/OrdersControllerProtectedIdTests.cs`

**Interfaces:**
- Consumes: `IDataProtectionService.Unprotect(string): string`、`OrdersController.Details(string)`、`OrdersController.DeleteConfirmation(string)`、`OrdersController.Delete(string)`。
- Produces: Controller 回歸測試，確認僅經驗證的整數 ID 會送至 MediatR。

- [ ] **Step 1: 建立可重用的 Controller test setup**

```csharp
private static OrdersController CreateController(IMediator mediator, IDataProtectionService protector)
{
    var services = new ServiceCollection().AddSingleton(mediator).BuildServiceProvider();

    return new OrdersController(
        Mock.Of<IMapper>(),
        protector,
        Mock.Of<ILogger<OrdersController>>())
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services }
        }
    };
}
```

- [ ] **Step 2: 寫入合法 token 的失敗測試**

```csharp
[Test]
public async Task Details_with_a_protected_id_sends_the_unprotected_integer_to_the_query()
{
    var protector = new Mock<IDataProtectionService>();
    protector.Setup(x => x.Unprotect("protected-order-42")).Returns("42");
    var mediator = CreateMediatorThatReturnsOrderDetail(42);
    var controller = CreateController(mediator.Object, protector.Object);

    await controller.Details("protected-order-42");

    mediator.Verify(x => x.Send(
        It.Is<GetOrderDetailQuery>(query => query.Id == 42),
        It.IsAny<CancellationToken>()), Times.Once);
}
```

- [ ] **Step 3: 執行，確認因 action 尚接收 `int` 或未解密而失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests.Details_with_a_protected_id"`

Expected: FAIL；編譯錯誤指出 `Details` 尚未接受 `string`，或驗證指出未以 `42` 建立 query。

- [ ] **Step 4: 寫入無效 token 的失敗測試**

```csharp
[TestCase("tampered")]
[TestCase("not-an-integer")]
public async Task Delete_with_an_invalid_protected_id_does_not_send_a_command(string id)
{
    var protector = new Mock<IDataProtectionService>();
    protector.Setup(x => x.Unprotect(id)).Returns(id == "tampered" ? null! : "abc");
    var mediator = new Mock<IMediator>();
    var controller = CreateController(mediator.Object, protector.Object);

    var result = await controller.Delete(id);

    result.ShouldBeOfType<RedirectToActionResult>();
    mediator.Verify(x => x.Send(It.IsAny<DeleteOrderCommand>(), It.IsAny<CancellationToken>()), Times.Never);
}
```

- [ ] **Step 5: 執行，確認無效 token 尚未被阻擋而失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests.Delete_with_an_invalid_protected_id"`

Expected: FAIL；因 action 尚未接受或驗證受保護字串。

### Task 2: 分離 ViewModel 的顯示 ID 與傳輸 ID

**Files:**
- Modify: `src/Web/ViewModels/Orders/OrderIndexViewModel.cs:27-31`
- Modify: `src/Web/ViewModels/Orders/OrderDetailViewModel.cs:3-5`
- Test: `tests/Application.FunctionalTests/Controllers/OrdersControllerProtectedIdTests.cs`

**Interfaces:**
- Consumes: 訂單 DTO 的整數 `Id`。
- Produces: `OrderItemViewModel.ProtectedId` 與 `OrderDetailViewModel.ProtectedId`，型別皆為非空 `string`。

- [ ] **Step 1: 寫入刪除確認模型的失敗測試**

```csharp
[Test]
public async Task Delete_confirmation_provides_a_protected_id_for_the_post_form()
{
    var protector = new Mock<IDataProtectionService>();
    protector.Setup(x => x.Unprotect("protected-order-42")).Returns("42");
    protector.Setup(x => x.Protect("42")).Returns("protected-order-42");
    var controller = CreateController(CreateMediatorThatReturnsOrderDetail(42).Object, protector.Object);

    var result = await controller.DeleteConfirmation("protected-order-42");

    var view = result.ShouldBeOfType<PartialViewResult>();
    view.Model.ShouldBeOfType<OrderDetailViewModel>().ProtectedId.ShouldBe("protected-order-42");
}
```

- [ ] **Step 2: 執行，確認 `ProtectedId` 尚不存在而失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests.Delete_confirmation_provides"`

Expected: FAIL；編譯錯誤指出 `OrderDetailViewModel.ProtectedId` 尚不存在。

- [ ] **Step 3: 在兩個 ViewModel 新增只讀傳輸欄位**

```csharp
public string ProtectedId { get; init; } = string.Empty;
```

將它緊接於各自的 `Id` 後方；不得改變 `Id` 型別或值。

- [ ] **Step 4: 重新執行 Task 2 測試，確認仍因 Controller 未設定欄位而失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests.Delete_confirmation_provides"`

Expected: FAIL；斷言顯示實際 `ProtectedId` 為空字串。

### Task 3: 加密輸出並安全解密 OrdersController 輸入

**Files:**
- Modify: `src/Web/Controllers/OrdersController.cs:1-123`
- Test: `tests/Application.FunctionalTests/Controllers/OrdersControllerProtectedIdTests.cs`

**Interfaces:**
- Consumes: `IDataProtectionService.Protect(string)`、`IDataProtectionService.Unprotect(string)`、兩個 ViewModel 的 `ProtectedId`。
- Produces: `Details(string id)`、`DeleteConfirmation(string id)`、`Delete(string id)`；只在解密並可轉為 `int` 時呼叫 MediatR。

- [ ] **Step 1: 注入既有 Data Protection 抽象**

```csharp
private readonly IDataProtectionService _dataProtectionService;

public OrdersController(
    IMapper mapper,
    IDataProtectionService dataProtectionService,
    ILogger<OrdersController> logger)
    : base(logger)
{
    _mapper = mapper;
    _dataProtectionService = dataProtectionService;
}
```

- [ ] **Step 2: 加入私有還原 helper，並在三個 action 使用**

```csharp
private bool TryGetOrderId(string protectedId, out int orderId)
    => int.TryParse(_dataProtectionService.Unprotect(protectedId), out orderId);
```

失敗時：`Details`、`Delete` 導回 `Index`，`DeleteConfirmation` 回傳 `NotFound()`；不得建立 Query 或 Command。

- [ ] **Step 3: 加入輸出保護 helper 並填入 ViewModel**

```csharp
private string ProtectOrderId(int orderId)
    => _dataProtectionService.Protect(orderId.ToString(CultureInfo.InvariantCulture));
```

補上 `using System.Globalization;`。在 `MapToViewModel` 的每一個 item 與 `MapToDetailViewModel` 結果中設定 `ProtectedId`；展示用 `Id` 保持原值。

- [ ] **Step 4: 執行 Task 1 與 Task 2 測試，確認轉綠**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests"`

Expected: PASS；合法 token 使用 `42`，無效 token 不呼叫 Command，刪除確認模型含受保護 ID。

### Task 4: 讓 Razor 僅傳遞受保護 ID

**Files:**
- Modify: `src/Web/Views/Orders/Index.cshtml:108-111`
- Modify: `src/Web/Views/Orders/_DeleteConfirmationModal.cshtml:7`
- Test: `tests/Application.FunctionalTests/Controllers/OrdersControllerProtectedIdTests.cs`

**Interfaces:**
- Consumes: `OrderItemViewModel.ProtectedId`、`OrderDetailViewModel.ProtectedId`。
- Produces: 詳細連結、刪除確認 AJAX URL、刪除 POST hidden input 使用受保護 ID，顯示欄位維持原始 ID。

- [ ] **Step 1: 寫入 Razor 傳輸欄位的失敗測試**

```csharp
[Test]
public void Order_views_use_protected_id_for_request_values_and_plain_id_for_display()
{
    File.ReadAllText(OrderIndexViewPath).ShouldContain("asp-route-id=\"@order.ProtectedId\"");
    File.ReadAllText(OrderIndexViewPath).ShouldContain("new { id = order.ProtectedId }");
    File.ReadAllText(DeleteModalViewPath).ShouldContain("value=\"@Model.ProtectedId\"");
    File.ReadAllText(OrderIndexViewPath).ShouldContain("<td>@order.Id</td>");
}
```

- [ ] **Step 2: 執行，確認 Razor 尚使用原始 ID 傳輸而失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests.Order_views_use"`

Expected: FAIL；找不到 `ProtectedId` 的 Razor 傳輸標記。

- [ ] **Step 3: 更新三個傳輸點，不改展示欄位**

```cshtml
<a asp-action="Details" asp-route-id="@order.ProtectedId" class="btn btn-sm btn-outline-primary">檢視</a>
data-url="@Url.Action("DeleteConfirmation", "Orders", new { id = order.ProtectedId })"
<input type="hidden" name="id" value="@Model.ProtectedId" />
```

- [ ] **Step 4: 執行受保護 ID 測試，確認轉綠**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests"`

Expected: PASS；Razor 僅以 `ProtectedId` 輸出請求值，Controller 邊界測試持續通過。

### Task 5: 完整驗證

**Files:**
- Verify: `src/Web/Controllers/OrdersController.cs`
- Verify: `src/Web/ViewModels/Orders/OrderIndexViewModel.cs`
- Verify: `src/Web/ViewModels/Orders/OrderDetailViewModel.cs`
- Verify: `src/Web/Views/Orders/Index.cshtml`
- Verify: `src/Web/Views/Orders/_DeleteConfirmationModal.cshtml`
- Verify: `tests/Application.FunctionalTests/Controllers/OrdersControllerProtectedIdTests.cs`

- [ ] **Step 1: 建置完整方案**

Run: `dotnet build CleanArchitecture.Northwind.slnx`

Expected: PASS，零編譯錯誤。

- [ ] **Step 2: 執行受影響的功能測試**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~OrdersControllerProtectedIdTests"`

Expected: PASS，所有受保護 ID 測試通過。

- [ ] **Step 3: 執行完整測試套件**

Run: `dotnet test CleanArchitecture.Northwind.slnx`

Expected: PASS；若整合測試依賴 SQL Server Docker 容器而無法啟動，記錄實際失敗命令、原因、未驗證範圍與可重跑命令。

- [ ] **Step 4: 檢查變更範圍與格式**

Run: `git diff --check; git diff -- src/Web/Controllers/OrdersController.cs src/Web/ViewModels/Orders/OrderIndexViewModel.cs src/Web/ViewModels/Orders/OrderDetailViewModel.cs src/Web/Views/Orders/Index.cshtml src/Web/Views/Orders/_DeleteConfirmationModal.cshtml tests/Application.FunctionalTests/Controllers/OrdersControllerProtectedIdTests.cs`

Expected: `git diff --check` 無輸出，且 diff 僅包含規格要求的 Web 層與測試變更。

- [ ] **Step 5: 提交完成的功能（僅在使用者要求提交時）**

```bash
git add src/Web/Controllers/OrdersController.cs src/Web/ViewModels/Orders/OrderIndexViewModel.cs src/Web/ViewModels/Orders/OrderDetailViewModel.cs src/Web/Views/Orders/Index.cshtml src/Web/Views/Orders/_DeleteConfirmationModal.cshtml tests/Application.FunctionalTests/Controllers/OrdersControllerProtectedIdTests.cs
git commit -m "feat: protect order identifiers in MVC"
```
