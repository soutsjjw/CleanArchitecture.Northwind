# Supplier Management Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 建立可安全檢視、管理與維護供應商聯絡資料、狀態及供應商品的 MVC 功能。

**Architecture:** 保持既有 Clean Architecture 邊界：Application 以投影 DTO 回傳供應商詳情與商品清單；Web Controller 只處理受保護識別值、授權與 ViewModel 映射。所有狀態變更仍經既有 Command 處理，且產品引用限制必須同時受 UI 與 Application 強制。

**Tech Stack:** ASP.NET Core MVC、MediatR、EF Core、Data Protection、NUnit、Moq、Shouldly。

**Spec:** 本次對話中已確認的「第二期功能：供應商管理」共同理解。

## Global Constraints

- 不新增 Migration、NuGet 套件、部署或設定變更。
- 只允許未軟刪除 Supplier 被查詢；停用 Supplier 可檢視但不可用於 Product 新增或改指派。
- 所有 POST 狀態變更必須具 Anti-Forgery、對應 Create／Update／Delete policy 與用途限定的 protected Id。
- 供應商首頁為選填，僅接受 HTTP／HTTPS；外部連結加上 `rel="noopener noreferrer"`。
- 任何 Product（含已軟刪除或停售者）引用 Supplier 時，禁止軟刪除並改以停用處理。
- View 不得接收 Domain Entity，查詢採 `AsNoTracking()` 與可翻譯投影。

## Review Focus

- 已停用而未軟刪除的 Supplier 詳情必須可讀取、明確顯示狀態，且已軟刪除者回傳找不到。
- 供應商品清單必須同時包含停售商品，且商品名稱只在使用者擁有 `Products_Read` 時連往商品詳情。
- 首頁欄位須拒絕 `ftp:`、相對路徑與不合法網址，但接受空白與 HTTP／HTTPS。
- 商品數大於零時，刪除按鈕不可出現，後端仍要回應衝突錯誤以抵擋繞過 UI 的 POST。
- 供應商詳情、編輯、刪除與啟停用各自只能接受對應 Data Protection purpose 所產生的識別值。

---

### Task 1: 擴充供應商詳情查詢與網址驗證

**Files:**
- Modify: `src/Application/Features/Suppliers/Queries/GetSupplierDetail/SupplierDetailDto.cs`
- Create: `src/Application/Features/Suppliers/Queries/GetSupplierDetail/SupplierSuppliedProductDto.cs`
- Modify: `src/Application/Features/Suppliers/Queries/GetSupplierDetail/GetSupplierDetailQueryHandler.cs`
- Modify: `src/Application/Features/Suppliers/Commands/SupplierCommandSupport.cs`
- Test: `tests/Application.UnitTests/Features/Suppliers/Queries/SupplierQueryHandlerTests.cs`
- Test: `tests/Application.UnitTests/Features/Suppliers/Commands/SupplierCommandHandlerTests.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext.Suppliers`、`IApplicationDbContext.Products`、`GetSupplierDetailQuery(int Id)`。
- Produces: `SupplierDetailDto`，含聯絡資料、啟用狀態、商品總數及 `IReadOnlyList<SupplierSuppliedProductDto>`；商品項目含 `Id`、`ProductName`、`QuantityPerUnit`、`UnitPrice`、`Discontinued`。

- [ ] **Step 1: 寫入詳情投影的失敗測試**

```csharp
[Test]
public async Task SupplierDetailShouldIncludeActiveAndDiscontinuedProducts()
{
    var result = await handler.Handle(new GetSupplierDetailQuery(3), CancellationToken.None);

    result.Succeeded.ShouldBeTrue();
    result.Data.Products.Select(product => product.ProductName)
        .ShouldBe(["Active product", "Discontinued product"]);
    result.Data.Products.Single(product => product.ProductName == "Discontinued product")
        .Discontinued.ShouldBeTrue();
}
```

- [ ] **Step 2: 執行測試，確認因詳情尚未含商品清單而失敗**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~SupplierDetailShouldIncludeActiveAndDiscontinuedProducts"`

Expected: FAIL，因 `SupplierDetailDto` 尚無 `Products` 或投影未包含商品。

- [ ] **Step 3: 以單一可翻譯投影加入商品詳情 DTO**

```csharp
public sealed record SupplierSuppliedProductDto(
    int Id, string ProductName, string? QuantityPerUnit,
    decimal? UnitPrice, bool Discontinued);

// GetSupplierDetailQueryHandler 的 Select 內：
value.Products
    .Where(product => !product.IsDelete)
    .OrderBy(product => product.ProductName)
    .Select(product => new SupplierSuppliedProductDto(
        product.Id, product.ProductName, product.QuantityPerUnit,
        product.UnitPrice, product.Discontinued))
    .ToList()
```

同時讓 `ProductCount` 與相同的未軟刪除商品篩選一致；維持 `!value.IsDelete`，使停用 Supplier 可讀取、軟刪除者不可讀取。

- [ ] **Step 4: 重新執行詳情查詢測試，確認通過**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~SupplierDetailShouldIncludeActiveAndDiscontinuedProducts"`

Expected: PASS。

- [ ] **Step 5: 寫入網址規則的失敗測試**

```csharp
[TestCase("ftp://example.test")]
[TestCase("/supplier")]
[TestCase("javascript:alert(1)")]
public async Task CreateSupplierShouldRejectNonHttpHomePage(string homePage)
{
    var result = await handler.Handle(
        new CreateSupplierCommand { CompanyName = "Alpha Co", HomePage = homePage },
        CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.StatusCode.ShouldBe(400);
}

[TestCase("https://example.test")]
[TestCase("http://example.test")]
public async Task CreateSupplierShouldAcceptHttpHomePage(string homePage) { /* assert success */ }
```

- [ ] **Step 6: 執行網址規則測試，確認目前驗證不足時失敗**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~CreateSupplierShouldRejectNonHttpHomePage|FullyQualifiedName~CreateSupplierShouldAcceptHttpHomePage"`

Expected: 目前至少 HTTP/HTTPS 與非 HTTP 網址規則的測試無法全部通過。

- [ ] **Step 7: 在 Application 輸入驗證中只允許絕對 HTTP／HTTPS URI**

```csharp
if (!string.IsNullOrWhiteSpace(homePage)
    && (!Uri.TryCreate(homePage.Trim(), UriKind.Absolute, out var uri)
        || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
{
    return "網站首頁必須是 HTTP 或 HTTPS 網址。";
}
```

讓 Create 與 Update 共用既有 `SupplierCommandSupport.ValidateInput`，不在 Web 複製業務輸入規則。

- [ ] **Step 8: 執行供應商 Application 單元測試**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~Features.Suppliers"`

Expected: PASS。

### Task 2: 建立受保護的供應商詳情 MVC 入口與 ViewModel

**Files:**
- Modify: `src/Web/Controllers/SuppliersController.cs`
- Create: `src/Web/ViewModels/Suppliers/SupplierDetailViewModel.cs`
- Modify: `src/Web/ViewModels/Suppliers/SupplierIndexViewModel.cs`
- Test: `tests/Application.FunctionalTests/Controllers/SuppliersControllerTests.cs`

**Interfaces:**
- Consumes: `GetSupplierDetailQuery`、`SupplierDetailDto`、`Policies.Suppliers_Read`、`Policies.Suppliers_Update`、`Policies.Products_Read`。
- Produces: `SuppliersController.Details(string id, CancellationToken)`，使用 `Suppliers.Details.ItemId.v1`；`SupplierDetailViewModel` 包含顯示欄位、詳情／編輯／刪除／啟停用 protected Id 與商品項目。

- [ ] **Step 1: 寫入詳情 token 隔離的失敗測試**

```csharp
[Test]
public async Task SupplierEditTokenCannotAuthorizeDetails()
{
    var result = await controller.Details(editToken, CancellationToken.None);

    result.ShouldBeOfType<NotFoundResult>();
    sender.Verify(value => value.Send(
        It.IsAny<GetSupplierDetailQuery>(), It.IsAny<CancellationToken>()), Times.Never);
}
```

- [ ] **Step 2: 執行測試，確認 `Details` 尚不存在而失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~SupplierEditTokenCannotAuthorizeDetails"`

Expected: FAIL，因 `SuppliersController.Details` 尚不存在。

- [ ] **Step 3: 實作 read-only Details Action 與 Web mapping**

```csharp
private readonly IDataProtector _details =
    provider.CreateProtector("Suppliers.Details.ItemId.v1");

[HttpGet, Authorize(Policy = Policies.Suppliers_Read)]
public async Task<IActionResult> Details(string id, CancellationToken cancellationToken = default)
{
    if (!TryUnprotect(_details, id, out var supplierId)) return NotFound();
    var result = await sender.Send(new GetSupplierDetailQuery(supplierId), cancellationToken);
    if (!result.Succeeded) return NotFound();
    return View(MapDetails(result.Data));
}
```

`MapDetails` 只在 Web 將 Application DTO 映射成 ViewModel；為每個商品建立既有 `Products.Details.ItemId.v1` purpose 的 protected Id。詳情 action 不依賴 `Products_Read`；由 View 在授權成功時才輸出商品連結。

- [ ] **Step 4: 將清單「公司名稱」改為 Details 連結**

在 `SupplierListItemViewModel` 增加 `DetailsProtectedId`，並於 `Index` mapping 使用 `_details` 產生。僅 `Suppliers_Read` 使用者能進入此清單，因此可安全輸出連結。

- [ ] **Step 5: 重新執行 Controller 測試，確認 token 隔離與既有安全測試通過**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~SuppliersControllerTests"`

Expected: PASS。

### Task 3: 實作詳情頁與刪除限制 UI

**Files:**
- Create: `src/Web/Views/Suppliers/Details.cshtml`
- Modify: `src/Web/Views/Suppliers/Index.cshtml`
- Test: `tests/Application.FunctionalTests/Controllers/SuppliersControllerTests.cs`

**Interfaces:**
- Consumes: `SupplierDetailViewModel` 與 `IAuthorizationService`。
- Produces: 唯讀聯絡資料、狀態、受保護的編輯／啟停用／刪除操作，以及供應商品清單。

- [ ] **Step 1: 寫入反映 UI 契約的失敗測試**

```csharp
[Test]
public void SupplierDetailsActionRequiresReadPolicy()
{
    var action = typeof(SuppliersController).GetMethod(nameof(SuppliersController.Details));
    action!.GetCustomAttributes<AuthorizeAttribute>()
        .Select(attribute => attribute.Policy)
        .ShouldContain(Policies.Suppliers_Read);
}
```

- [ ] **Step 2: 執行測試，確認新增 action 的授權要求通過**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~SupplierDetailsActionRequiresReadPolicy"`

Expected: PASS。

- [ ] **Step 3: 建立 Razor 詳情頁**

頁面以 Razor 預設編碼呈現公司、聯絡人、職稱、拆分地址、電話、傳真與啟用／停用 badge；首頁僅在非空值時輸出：

```razor
<a href="@Model.HomePage" target="_blank" rel="noopener noreferrer">
    @Model.HomePage
</a>
```

依 `Suppliers_Update` 顯示編輯與啟停用 POST 表單（包含 `@Html.AntiForgeryToken()`）；依 `Suppliers_Delete` 且 `ProductCount == 0` 顯示軟刪除表單。商品數大於零時改顯示「已有 X 項商品，請改為停用」提示。

- [ ] **Step 4: 呈現商品清單及最小權限連結**

完整列出未軟刪除商品（含停售）名稱、包裝量、單價、停售狀態。只有 `AuthorizationService.AuthorizeAsync(User, Policies.Products_Read)` 成功時，將名稱連到既有 `ProductsController.Details`；否則輸出純文字，避免擴張商品讀取能力。

- [ ] **Step 5: 同步修正清單頁 UI**

在 `Index.cshtml` 將公司名稱連到詳情；保留編輯、啟停用的既有 Anti-Forgery 表單。把刪除表單包在 `supplier.ProductCount == 0` 條件，非零時改顯示停用提示，保留 Command Handler 的 409 防線。

- [ ] **Step 6: 執行所有供應商 Controller 測試**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~SuppliersControllerTests"`

Expected: PASS。

### Task 4: 完整驗證與交付檢查

**Files:**
- Verify: 所有 Task 1–3 修改檔案。

**Interfaces:**
- Consumes: 完成的 Application 與 Web 功能。
- Produces: 可重現的建置與測試證據。

- [ ] **Step 1: 檢視最終差異，確認無 Migration、設定或無關產物**

Run: `git diff --check; git diff --name-only`

Expected: 無空白錯誤；檔案僅限供應商功能、測試與此計畫。

- [ ] **Step 2: 建置方案**

Run: `dotnet build CleanArchitecture.Northwind.sln`

Expected: PASS，零編譯錯誤。

- [ ] **Step 3: 執行完整測試**

Run: `dotnet test CleanArchitecture.Northwind.sln`

Expected: PASS；若有未由本次變更造成的失敗，逐一記錄名稱與輸出。

- [ ] **Step 4: 完成前的驗證回顧**

確認每個新行為都有先失敗後通過的測試紀錄、所有 POST 仍具 Anti-Forgery、每個 Id 使用正確 protector purpose，且首頁只接受 HTTP／HTTPS。
