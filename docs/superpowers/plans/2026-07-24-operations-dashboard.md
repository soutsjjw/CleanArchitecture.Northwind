# Operations Dashboard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在首頁提供每 10 分鐘更新的受保護營運儀表板。

**Architecture:** 以一個 Application 唯讀 Query 投影訂單與商品資料；MVC Controller 將 DTO 映射至 ViewModel，並讓 Razor 與本機 Chart.js 繪製及輪詢。

**Tech Stack:** ASP.NET Core MVC、MediatR、EF Core、NUnit、Moq、Shouldly、LibMan、Chart.js 4.5.0。

## Global Constraints

- 首頁和 JSON 更新端點均同時要求 `Orders:Read:All` 與 `Products:Read:All`，不新增 Dashboard 權限。
- 日期以伺服器本地日曆日及非 null `OrderDate` 判定；營收為 `UnitPrice * Quantity * (1 - Discount)`，不含運費。
- 只讀資料存取使用 `AsNoTracking()`，並傳遞 `CancellationToken`；Controller 不得使用 DbContext。
- Chart.js 必須在 `src/Web/libman.json` 設定成本機檔，禁止 CDN。
- 不新增 migration、NuGet 套件、推播、日期篩選器或鑽研連結。

## File Structure

- Create: `src/Application/Features/Dashboard/Queries/GetOperationsDashboard/GetOperationsDashboardQuery.cs`
- Create: `src/Application/Features/Dashboard/Queries/GetOperationsDashboard/OperationsDashboardDto.cs`
- Create: `src/Application/Features/Dashboard/Queries/GetOperationsDashboard/GetOperationsDashboardQueryHandler.cs`
- Create: `tests/Application.UnitTests/Features/Dashboard/Queries/GetOperationsDashboard/GetOperationsDashboardQueryHandlerTests.cs`
- Create: `src/Web/ViewModels/Home/OperationsDashboardViewModel.cs`
- Modify: `src/Web/Controllers/HomeController.cs`, `src/Web/Views/Home/Index.cshtml`, `src/Web/libman.json`, `src/Web/wwwroot/css/site.css`
- Create: `src/Web/wwwroot/js/home-dashboard.js`
- Create: `tests/Application.FunctionalTests/Controllers/HomeControllerDashboardTests.cs`

### Task 1: 實作並測試 Application 儀表板查詢

**Files:**

- Create: `src/Application/Features/Dashboard/Queries/GetOperationsDashboard/GetOperationsDashboardQuery.cs`
- Create: `src/Application/Features/Dashboard/Queries/GetOperationsDashboard/OperationsDashboardDto.cs`
- Create: `src/Application/Features/Dashboard/Queries/GetOperationsDashboard/GetOperationsDashboardQueryHandler.cs`
- Create: `tests/Application.UnitTests/Features/Dashboard/Queries/GetOperationsDashboard/GetOperationsDashboardQueryHandlerTests.cs`

**Interfaces:**

- Produces: `GetOperationsDashboardQuery : IRequest<Result<OperationsDashboardDto>>`。
- Produces: DTO 摘要欄位、三個出貨分類、低庫存和 Top 客戶清單。

- [ ] **Step 1: 寫入失敗的資料聚合測試**

```csharp
[Test]
public async Task HandleShouldBuildDashboardForCurrentDayAndMonth()
{
    var clock = Mock.Of<IDateTimeService>(x => x.Now == new DateTime(2026, 7, 24, 10, 0, 0));
    var result = await new GetOperationsDashboardQueryHandler(CreateContext(orders, products).Object, clock)
        .Handle(new GetOperationsDashboardQuery(), CancellationToken.None);

    result.Succeeded.ShouldBeTrue();
    result.Data.TodayOrderCount.ShouldBe(1);
    result.Data.TodayRevenue.ShouldBe(180m);
    result.Data.MonthlyOrderCount.ShouldBe(3);
    result.Data.MonthlyRevenue.ShouldBe(430m);
    result.Data.ShippingStatuses.ShouldContain(x => x.Status == DashboardShippingStatus.Overdue && x.Count == 1);
}
```

測資必須另涵蓋上月、空 `OrderDate`、軟刪除、已出貨、RequiredDate 為今日、空 RequiredDate、停售、等於再訂購水準、低庫存，以及營收相同但訂單數不同的客戶。

- [ ] **Step 2: 執行測試確認紅燈**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~GetOperationsDashboardQueryHandlerTests`

Expected: FAIL，Query 和 Handler 尚不存在。

- [ ] **Step 3: 撰寫最小 Query、DTO 和 Handler**

```csharp
public record GetOperationsDashboardQuery : IRequest<Result<OperationsDashboardDto>>;
public enum DashboardShippingStatus { Shipped, Pending, Overdue }

public sealed class OperationsDashboardDto
{
    public int TodayOrderCount { get; init; }
    public decimal TodayRevenue { get; init; }
    public int MonthlyOrderCount { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public IReadOnlyList<ShippingStatusDto> ShippingStatuses { get; init; } = [];
    public IReadOnlyList<LowStockProductDto> LowStockProducts { get; init; } = [];
    public IReadOnlyList<TopCustomerDto> TopCustomers { get; init; } = [];
}
```

Handler 用 `IDateTimeService.Now.Date` 建立 today、monthStart、nextMonthStart；未軟刪除且日期屬於月份的訂單做投影。已出貨為 `Shipped`；未出貨且 RequiredDate 早於 today 為 `Overdue`；其餘（包括空 RequiredDate）為 `Pending`。低庫存查詢必須是：

```csharp
context.Products.AsNoTracking()
    .Where(p => !p.IsDelete && !p.Discontinued && p.UnitsInStock <= p.ReorderLevel)
    .OrderByDescending(p => (p.ReorderLevel ?? 0) - (p.UnitsInStock ?? 0))
    .ThenBy(p => p.ProductName).Take(5)
```

Top 客戶按營收降冪、訂單數降冪、公司名升冪取 5。每個 EF Core async call 均傳入 cancellation token。

- [ ] **Step 4: 執行測試確認綠燈**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~GetOperationsDashboardQueryHandlerTests`

Expected: PASS，並涵蓋月份邊界、折扣、出貨分類、低庫存及同額排序。

- [ ] **Step 5: 提交本任務**

Run: `git add src/Application/Features/Dashboard tests/Application.UnitTests/Features/Dashboard`

Run: `git commit -m "feat: add operations dashboard query"`

### Task 2: 實作並測試首頁資料流與授權

**Files:**

- Create: `src/Web/ViewModels/Home/OperationsDashboardViewModel.cs`
- Modify: `src/Web/Controllers/HomeController.cs`
- Create: `tests/Application.FunctionalTests/Controllers/HomeControllerDashboardTests.cs`

**Interfaces:**

- Consumes: `GetOperationsDashboardQuery` 和 `OperationsDashboardDto`。
- Produces: `Index(CancellationToken)` 和 `DashboardData(CancellationToken)`。

- [ ] **Step 1: 寫入失敗的 Controller 測試**

```csharp
[Test]
public async Task DashboardDataShouldSendQueryAndReturnMappedJson()
{
    var mediator = CreateMediator(OperationsDashboardDtoFixture.Create());
    var result = await CreateController(mediator).DashboardData(CancellationToken.None);

    result.ShouldBeOfType<JsonResult>().Value.ShouldBeOfType<OperationsDashboardViewModel>();
    mediator.Verify(x => x.Send(It.IsAny<GetOperationsDashboardQuery>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

另以原始碼測試斷言兩個 action 同時含有 `[Authorize(Policy = "Orders:Read:All")]` 和 `[Authorize(Policy = "Products:Read:All")]`，且 HomeController class 不再帶 AllowAnonymous。

- [ ] **Step 2: 執行測試確認紅燈**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~HomeControllerDashboardTests`

Expected: FAIL。

- [ ] **Step 3: 實作 Controller 與 Web ViewModel**

移除 class-level AllowAnonymous，但在 `Privacy` 與 `TermsOfService` action 明確加回 AllowAnonymous。兩個 Dashboard action 共同使用以下授權及 Query，且以私有 `Map` 將 DTO 映射成 ViewModel：

```csharp
[HttpGet]
[Authorize(Policy = "Orders:Read:All")]
[Authorize(Policy = "Products:Read:All")]
public async Task<IActionResult> DashboardData(CancellationToken cancellationToken)
{
    var result = await Mediator.Send(new GetOperationsDashboardQuery(), cancellationToken);
    return result.Succeeded
        ? Json(Map(result.Data))
        : Problem(statusCode: StatusCodes.Status500InternalServerError,
            title: "無法取得營運儀表板資料。");
}
```

`Index` 成功時回傳 ViewModel；失敗時導向既有 `Error/Index`，不可回傳例外細節。

- [ ] **Step 4: 執行測試確認綠燈**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~HomeControllerDashboardTests`

Expected: PASS。

- [ ] **Step 5: 提交本任務**

Run: `git add src/Web/Controllers/HomeController.cs src/Web/ViewModels/Home tests/Application.FunctionalTests/Controllers/HomeControllerDashboardTests.cs`

Run: `git commit -m "feat: protect and expose dashboard data"`

### Task 3: 以 LibMan 加入本機 Chart.js 並建立頁面

**Files:**

- Modify: `src/Web/libman.json`, `src/Web/Views/Home/Index.cshtml`, `src/Web/wwwroot/css/site.css`
- Create: `src/Web/wwwroot/js/home-dashboard.js`
- Modify: `tests/Application.FunctionalTests/Controllers/HomeControllerDashboardTests.cs`

**Interfaces:**

- Consumes: 初始 `OperationsDashboardViewModel` JSON 和 `DashboardData` URL。
- Produces: `window.OperationsDashboard.initialize(root)`，固定每 `600000` ms 更新。

- [ ] **Step 1: 寫入失敗的靜態資產測試**

```csharp
[Test]
public void DashboardAssetsShouldBeLocalAndPollEveryTenMinutes()
{
    File.ReadAllText(GetWebPath("libman.json")).ShouldContain("chart.js@4.5.0");
    var view = File.ReadAllText(GetWebPath("Views", "Home", "Index.cshtml"));
    view.ShouldContain("~/lib/chart.js/chart.umd.min.js");
    view.ShouldContain("id=\"shipping-status-chart\"");
    File.ReadAllText(GetWebPath("wwwroot", "js", "home-dashboard.js")).ShouldContain("600000");
}
```

- [ ] **Step 2: 執行測試確認紅燈**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~HomeControllerDashboardTests`

Expected: FAIL。

- [ ] **Step 3: 設定資產並實作 UI**

在 `src/Web/libman.json` 的 `libraries` 加入以下物件，隨後執行 restore：

```json
{
  "provider": "cdnjs",
  "library": "chart.js@4.5.0",
  "destination": "wwwroot/lib/chart.js/",
  "files": ["chart.umd.min.js"]
}
```

首頁以 Bootstrap 卡片呈現今日／本月訂單和營收，並有出貨圓環圖、每日營收長條圖、低庫存及 Top 客戶表格。使用本機 scripts：

```html
<div id="operations-dashboard" data-dashboard-url="@Url.Action(nameof(HomeController.DashboardData), \"Home\")">
  <canvas id="shipping-status-chart" aria-label="本月出貨狀態圖表"></canvas>
  <canvas id="revenue-chart" aria-label="本月每日營收圖表"></canvas>
</div>
<script src="~/lib/chart.js/chart.umd.min.js"></script>
<script src="~/js/home-dashboard.js"></script>
```

前端須以 `fetch(url, { credentials: "same-origin", headers: { Accept: "application/json" } })` 更新，重繪前 destroy 舊 Chart 實例，以 `window.setInterval(refresh, 600000)` 輪詢。失敗時保留最後成功資料，只顯示 `toastr.error("儀表板資料更新失敗，請稍後再試。")`。樣式只限圖表高度、表格容器和窄螢幕間距。

- [ ] **Step 4: 還原資產並執行測試確認綠燈**

Run: `libman restore src/Web/libman.json`

Expected: `src/Web/wwwroot/lib/chart.js/chart.umd.min.js` 存在。

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~HomeControllerDashboardTests`

Expected: PASS。

- [ ] **Step 5: 提交本任務**

Run: `git add src/Web/libman.json src/Web/Views/Home/Index.cshtml src/Web/wwwroot/js/home-dashboard.js src/Web/wwwroot/css/site.css src/Web/wwwroot/lib/chart.js tests/Application.FunctionalTests/Controllers/HomeControllerDashboardTests.cs`

Run: `git commit -m "feat: render operations dashboard"`

### Task 4: 驗證完整變更

**Files:** 無；僅在測試暴露問題時修改其直接原因。

- [ ] **Step 1: 執行受影響測試**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj`

Expected: PASS。

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj`

Expected: PASS；若 Docker／Testcontainers 無法啟動，記錄實際失敗，不得宣稱通過。

- [ ] **Step 2: 建置方案**

Run: `dotnet build CleanArchitecture.Northwind.slnx`

Expected: Build succeeded。

- [ ] **Step 3: 手動檢查**

具兩項 All 權限的帳號可載入與更新；未登入導向登入；只有一項或低於 All 的權限被拒絕。網路面板僅載入 `/lib/chart.js/chart.umd.min.js`，沒有 CDN；空清單顯示「無資料」且沒有 JavaScript error。
