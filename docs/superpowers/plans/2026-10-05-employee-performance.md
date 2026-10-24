# 員工與業務績效 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 提供可依日期、Employee、Region 與 Territory 查詢的員工績效排名頁面，顯示訂單數與折後銷售額。

**Architecture:** 在 Application 新增獨立的唯讀 `GetEmployeePerformanceQuery` Feature，直接以 `IApplicationDbContext` 對 Orders 做可翻譯投影與彙總，並將篩選選項與結果 DTO 回傳 Web。Web 新增薄的 MVC Controller、ViewModel 及 Razor 頁面；Controller 只映射並透過 `ISender` 呼叫 Query，使用既有 `Orders:Read:All` policy，不新增權限、schema 或 Infrastructure abstraction。

**Tech Stack:** ASP.NET Core MVC、MediatR、EF Core、FluentValidation、NUnit、Moq、Shouldly。

**Spec:** 本文件的「已確認規格」與 [CONTEXT.md](../../../CONTEXT.md)。

## Global Constraints

- Query 僅讀取資料，使用 `AsNoTracking()`、投影與 `CancellationToken`；Controller 不得存取 `DbContext` 或 Infrastructure implementation。
- 僅計入未軟刪除、已指派 Employee、且 `OrderDate` 符合可選起訖範圍的 Sales Order；起訖日均包含，`OrderDate` 為 null 不納入。
- 折後銷售額為 `UnitPrice * Quantity * (1 - Discount)` 的 Sales Order Line 合計，不納入 Freight。
- Employee、Region、Territory 篩選採交集；地理篩選採查詢當下的 `EmployeeTerritory` 關係，不將 Sales Order 直接歸屬 Territory，也不回推歷史指派。
- 未指定 Employee 時省略零業績員工；明確指定且不存在符合訂單時，回傳該 Employee 的 0 筆、0 元、無名次資料列。
- 排名按折後銷售額降冪；同額採競賽並列名次（`1, 2, 2, 4`），顯示順序再依訂單數降冪、Employee ID 升冪穩定化。
- 頁面與側欄入口使用 `Orders:Read:All`；不新增 policy、NuGet 套件、Migration、圖表、匯出或訂單明細連結。

## 已確認規格

- 新增「員工與業務績效」GET 查詢頁，提供日期起／迄、Employee、Region、Territory 篩選與排名表格。
- 日期任一端皆可省略；兩端皆未填時統計所有符合條件的歷史 Sales Order；起日不得晚於迄日。
- Region／Territory 的意義是篩選被目前指派至該地理範圍的 Employee，再統計那些 Employee 的全部符合期間 Sales Order。
- 初版表格顯示名次、Employee、訂單數、折後銷售額；不加入圖表、匯出或鑽研連結。
- 已於 `CONTEXT.md` 同步 Employee Performance 與 Employee Performance Geographic Filter 詞彙；無符合 ADR 門檻的架構決策。

## Review Focus

- 起日大於迄日應在 Application validator 被拒絕，而不是產生空白或錯誤的彙總。
- 同一 Employee 有多個符合 Region／Territory 的 Employee Territory Assignment 時，訂單與銷售額不得重複計算。
- Region 與 Territory 或 Employee 相互不相符時必須回傳空結果，不可放寬其中一個條件。
- 同額 Employee 必須顯示相同競賽名次；分頁或顯示順序不得讓名次變成列號。
- 明確選擇但無符合訂單的 Employee 必須顯示零業績且 `Rank` 為 null；未選擇 Employee 時零業績者不可出現。

## File Structure

- Create: `src/Application/Features/EmployeePerformance/Queries/GetEmployeePerformance/GetEmployeePerformanceQuery.cs`
- Create: `src/Application/Features/EmployeePerformance/Queries/GetEmployeePerformance/EmployeePerformanceDto.cs`
- Create: `src/Application/Features/EmployeePerformance/Queries/GetEmployeePerformance/GetEmployeePerformanceQueryValidator.cs`
- Create: `src/Application/Features/EmployeePerformance/Queries/GetEmployeePerformance/GetEmployeePerformanceQueryHandler.cs`
- Create: `tests/Application.UnitTests/Features/EmployeePerformance/Queries/GetEmployeePerformance/GetEmployeePerformanceQueryHandlerTests.cs`
- Create: `src/Web/Controllers/EmployeePerformanceController.cs`
- Create: `src/Web/ViewModels/EmployeePerformance/EmployeePerformanceIndexViewModel.cs`
- Create: `src/Web/Views/EmployeePerformance/Index.cshtml`
- Modify: `src/Web/Views/Shared/_SideBarPartial.cshtml`
- Modify: `src/Web/wwwroot/css/site.css`
- Create: `tests/Application.FunctionalTests/Controllers/EmployeePerformanceControllerTests.cs`

### Task 1: 實作並測試 Application 員工績效查詢

**Files:**

- Create: `src/Application/Features/EmployeePerformance/Queries/GetEmployeePerformance/GetEmployeePerformanceQuery.cs`
- Create: `src/Application/Features/EmployeePerformance/Queries/GetEmployeePerformance/EmployeePerformanceDto.cs`
- Create: `src/Application/Features/EmployeePerformance/Queries/GetEmployeePerformance/GetEmployeePerformanceQueryValidator.cs`
- Create: `src/Application/Features/EmployeePerformance/Queries/GetEmployeePerformance/GetEmployeePerformanceQueryHandler.cs`
- Create: `tests/Application.UnitTests/Features/EmployeePerformance/Queries/GetEmployeePerformance/GetEmployeePerformanceQueryHandlerTests.cs`

**Interfaces:**

- Produces: `GetEmployeePerformanceQuery : IRequest<Result<EmployeePerformanceDto>>`，含 `DateTime? OrderedFrom`、`DateTime? OrderedTo`、`int? EmployeeId`、`int? RegionId`、`string? TerritoryId`。
- Produces: `EmployeePerformanceDto`，含目前篩選值、`IReadOnlyList<EmployeePerformanceItemDto> Items`，以及 Employee／Region／Territory 選項。
- Produces: `EmployeePerformanceItemDto(int? Rank, int EmployeeId, string EmployeeName, int OrderCount, decimal NetSales)`；`Rank == null` 表示明確選擇的零業績 Employee。

- [ ] **Step 1: 寫入失敗的 Query Handler 與 Validator 測試**

```csharp
[Test]
public async Task HandleShouldAggregateEligibleOrdersAndAssignCompetitionRanks()
{
    var result = await CreateHandler(orders, employees, regions, territories)
        .Handle(new GetEmployeePerformanceQuery
        {
            OrderedFrom = new DateTime(2026, 7, 1),
            OrderedTo = new DateTime(2026, 7, 31)
        }, CancellationToken.None);

    result.Succeeded.ShouldBeTrue();
    result.Data.Items.ShouldBe([
        new EmployeePerformanceItemDto(1, 1, "Nancy Davolio", 2, 180m),
        new EmployeePerformanceItemDto(2, 2, "Andrew Fuller", 1, 90m),
        new EmployeePerformanceItemDto(2, 3, "Janet Leverling", 1, 90m),
        new EmployeePerformanceItemDto(4, 4, "Margaret Peacock", 1, 40m)
    ]);
}
```

同一測試類別另涵蓋：折扣公式且忽略 Freight、起迄日邊界、未填日期端點、空 `OrderDate`、軟刪除與未指派 Employee；Employee／Region／Territory 交集、同 Employee 多個 Territory 不重複計算、衝突地理條件的空結果、選定零業績 Employee 的 `Rank == null`，以及未選 Employee 時零業績者不出現。Validator 測試斷言起日大於迄日失敗。

- [ ] **Step 2: 執行測試確認紅燈**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~GetEmployeePerformanceQueryHandlerTests`

Expected: FAIL，因 Query、DTO、Validator 與 Handler 尚不存在。

- [ ] **Step 3: 撰寫最小 Query、DTO、Validator 和 Handler**

以 `context.Orders.AsNoTracking()` 為查詢起點，先套用 `!order.IsDelete`、`order.EmployeeId.HasValue` 與可選日期條件。Region／Territory 條件必須透過 `order.Employee!.EmployeeTerritories.Any(...)` 判斷，避免 Join 造成同一 Order 被多個指派重複計入。投影 Employee ID、姓名、每張訂單的明細折後合計後才依 Employee GroupBy。

將聚合結果依 `NetSales` 降冪、`OrderCount` 降冪、`EmployeeId` 升冪排列後，在記憶體依前一個銷售額與目前零基列索引產生競賽名次。若 `EmployeeId` 已指定且彙總沒有資料，從 `context.Employees.AsNoTracking()` 取得該 Employee 的名稱，回傳單一 `Rank: null`、0 筆、0 元資料列；找不到 Employee 則回傳空 Items。另以各 DbSet 的 `AsNoTracking()` 投影並排序篩選選項。每個非同步 EF 操作均傳遞 `cancellationToken`。

- [ ] **Step 4: 執行測試確認綠燈**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~GetEmployeePerformanceQueryHandlerTests`

Expected: PASS，涵蓋 Review Focus 的五項風險與所有已確認聚合規則。

- [ ] **Step 5: 提交本任務**

Run: `git add src/Application/Features/EmployeePerformance tests/Application.UnitTests/Features/EmployeePerformance`

Run: `git commit -m "feat: add employee performance query"`

### Task 2: 實作並測試 MVC 查詢頁與受權限保護的入口

**Files:**

- Create: `src/Web/Controllers/EmployeePerformanceController.cs`
- Create: `src/Web/ViewModels/EmployeePerformance/EmployeePerformanceIndexViewModel.cs`
- Create: `src/Web/Views/EmployeePerformance/Index.cshtml`
- Modify: `src/Web/Views/Shared/_SideBarPartial.cshtml`
- Modify: `src/Web/wwwroot/css/site.css`
- Create: `tests/Application.FunctionalTests/Controllers/EmployeePerformanceControllerTests.cs`

**Interfaces:**

- Consumes: `GetEmployeePerformanceQuery` 與 `EmployeePerformanceDto`。
- Produces: `EmployeePerformanceController.Index([FromQuery] GetEmployeePerformanceQuery query, CancellationToken cancellationToken)`，回傳 `EmployeePerformanceIndexViewModel`。
- Produces: `EmployeePerformanceIndexViewModel`，保留篩選值、選項和 `EmployeePerformanceItemViewModel` 清單；項目保留 nullable `Rank`、Employee 名稱、訂單數與折後銷售額。

- [ ] **Step 1: 寫入失敗的 Controller、側欄和 View 行為測試**

```csharp
[Test]
public void EmployeePerformanceIndexRequiresAllOrdersReadPolicy()
{
    var action = typeof(EmployeePerformanceController).GetMethod(nameof(EmployeePerformanceController.Index));
    action!.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
        .Cast<AuthorizeAttribute>()
        .Select(attribute => attribute.Policy)
        .ShouldContain("Orders:Read:All");
}
```

另以 Controller 測試驗證成功結果映射為 `EmployeePerformanceIndexViewModel`、失敗時不洩漏錯誤細節；以原始碼檢查確認 `_SideBarPartial.cshtml` 只在 `Orders:Read:All` 成功時輸出入口，View 含 GET 表單、四個篩選控制項、名次／Employee／訂單數／折後銷售額欄位、`_NoDataPartial`，且沒有圖表、匯出或訂單明細連結。

- [ ] **Step 2: 執行測試確認紅燈**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~EmployeePerformanceControllerTests`

Expected: FAIL，因 Controller、ViewModel、View 與側欄入口尚不存在。

- [ ] **Step 3: 實作 Web 資料流與頁面**

建立僅含 GET `Index` action 的 `EmployeePerformanceController`，加上 `[Authorize(Policy = "Orders:Read:All")]`，並將 DTO 明確映射到 ViewModel；使用傳入的 `CancellationToken` 呼叫 `ISender.Send`。失敗時導向既有 `Home/Index`，不可將 Result errors 顯示給使用者。

View 採既有 Bootstrap 清單頁慣例：日期 input、Employee／Region／Territory select、查詢按鈕與清除連結皆使用 query string；表格顯示 `-` 作為 null 名次。新增 `employee-performance-table` 至既有深色表頭 CSS selector。側欄以一次 `AuthorizeAsync(User, "Orders:Read:All")` 的結果控制「員工與業務績效」入口，避免要求 Products 權限或重用 Dashboard 的雙權限條件。

- [ ] **Step 4: 執行測試確認綠燈**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~EmployeePerformanceControllerTests`

Expected: PASS，確認 Controller 授權、映射、側欄可見性與必要 UI 元素。

- [ ] **Step 5: 提交本任務**

Run: `git add src/Web/Controllers/EmployeePerformanceController.cs src/Web/ViewModels/EmployeePerformance src/Web/Views/EmployeePerformance src/Web/Views/Shared/_SideBarPartial.cshtml src/Web/wwwroot/css/site.css tests/Application.FunctionalTests/Controllers/EmployeePerformanceControllerTests.cs`

Run: `git commit -m "feat: add employee performance report page"`

### Task 3: 驗證跨層功能

**Files:** 無；僅在測試揭露本計畫檔案中的直接原因時修正。

**Interfaces:**

- Consumes: Task 1 的 Application Query 與 Task 2 的 MVC 呈現。
- Produces: 已驗證的受授權員工績效查詢頁。

- [ ] **Step 1: 執行受影響的 Application 與 MVC 測試**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~GetEmployeePerformanceQueryHandlerTests|FullyQualifiedName~GetOrdersQueryHandlerTests"`

Expected: PASS。

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~EmployeePerformanceControllerTests`

Expected: PASS；若 Testcontainers 或必要外部依賴無法啟動，記錄實際錯誤，不得宣稱通過。

- [ ] **Step 2: 建置方案**

Run: `dotnet build CleanArchitecture.Northwind.slnx`

Expected: `Build succeeded`。

- [ ] **Step 3: 手動驗收**

具 `Orders:Read:All` 的帳號可從側欄開啟頁面；無該權限者看不到入口且 action 被拒絕。確認日期、Employee、Region、Territory 交集篩選；同額排名顯示競賽並列；零業績僅在明確選擇 Employee 時出現無名次的一列；窄螢幕表格仍可水平檢視且無 JavaScript error。
