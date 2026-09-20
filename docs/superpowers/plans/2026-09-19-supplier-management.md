# 第二期供應商管理 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 交付具完整 CRUD、篩選分頁、授權與安全保護的供應商管理 MVC 功能。

**Architecture:** Application 建立 Supplier 專屬 Query／Command；Web 仿照 Categories 模組進行 MVC 映射與 protected Id 處理；不變更 Infrastructure 或資料庫 schema。

**Tech Stack:** .NET 10、ASP.NET Core MVC、MediatR、EF Core、FluentValidation、NUnit、Moq、Shouldly。

**Spec:** `docs/superpowers/specs/2026-09-19-supplier-management-design.md`

## Global Constraints

- 不建立 Migration、不新增套件，且不變更部署或全域套件設定。
- Application 只依賴既有抽象；Controller 不直接使用 DbContext。
- 每個 POST action 採 Anti-Forgery、對應 `Policies.Suppliers_*` 與用途分隔 protected Id。
- Product 新增或改指派僅能選擇啟用、未軟刪除 Supplier。

## Review Focus

- `HomePage` 非 HTTP／HTTPS 或非絕對 URL 會被拒絕，空白會正規化為 null。
- 公司名稱不分大小寫的重複資料被拒絕，已軟刪除資料不阻擋名稱重用。
- 包含任何 Product 引用時刪除回傳 409，且 Supplier 狀態不變。
- Edit token 不得用於 Delete，還原失敗不送出 Command。
- 每一 Suppliers 權限分別控制端點與 Razor 操作入口。

---

### Task 1: Supplier Application workflows

**Files:**
- Create: `src/Application/Features/Suppliers/Queries/GetSuppliers/GetSuppliersQuery.cs`
- Create: `src/Application/Features/Suppliers/Queries/GetSupplierDetail/GetSupplierDetailQuery.cs`
- Create: `src/Application/Features/Suppliers/Commands/SupplierCommandSupport.cs`
- Create: `src/Application/Features/Suppliers/Commands/CreateSupplier/CreateSupplierCommand.cs`
- Create: `src/Application/Features/Suppliers/Commands/UpdateSupplier/UpdateSupplierCommand.cs`
- Create: `src/Application/Features/Suppliers/Commands/DeleteSupplier/DeleteSupplierCommand.cs`
- Create: `src/Application/Features/Suppliers/Commands/SetSupplierActive/SetSupplierActiveCommand.cs`
- Test: `tests/Application.UnitTests/Features/Suppliers/Queries/SupplierQueryHandlerTests.cs`
- Test: `tests/Application.UnitTests/Features/Suppliers/Commands/SupplierCommandHandlerTests.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext.Suppliers`, `IApplicationDbContext.Products`, `PaginatedList<T>`。
- Produces: Supplier list/detail DTOs 與 Create、Update、Delete、SetActive commands。

- [ ] **Step 1: 寫入失敗的 query tests**

```csharp
[Test]
public async Task SupplierListShouldFilterKeywordAndStateThenProjectProductCount()
{
    var result = await handler.Handle(new GetSuppliersQuery
    {
        Keyword = "alpha", IsActive = true, PageSize = 10
    }, CancellationToken.None);

    result.Data.Items.ShouldBe([
        new SupplierListItemDto(2, "Alpha Co", "Amy", "01", "Taiwan", true, 1)
    ]);
}
```

- [ ] **Step 2: 執行 RED：`dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~SupplierQueryHandlerTests`**

Expected: 失敗原因為尚未存在 Supplier query 型別。

- [ ] **Step 3: 實作最小 query handlers**

```csharp
var suppliers = context.Suppliers.AsNoTracking().Where(x => !x.IsDelete);
if (!string.IsNullOrWhiteSpace(request.Keyword))
{
    var keyword = request.Keyword.Trim();
    suppliers = suppliers.Where(x => x.CompanyName.Contains(keyword)
        || (x.ContactName != null && x.ContactName.Contains(keyword))
        || (x.Phone != null && x.Phone.Contains(keyword))
        || (x.Country != null && x.Country.Contains(keyword)));
}
```

套用狀態篩選、公司名稱與 Id 排序，投影非軟刪除 Product 數量並分頁；詳細查詢投影所有可編輯欄位。

- [ ] **Step 4: 執行 GREEN：`dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~SupplierQueryHandlerTests`**

Expected: PASS。

- [ ] **Step 5: 寫入失敗的 command tests**

```csharp
[Test]
public async Task DeleteSupplierShouldRejectWhenAnyProductReferencesIt()
{
    var result = await handler.Handle(new DeleteSupplierCommand { Id = 3 }, CancellationToken.None);
    result.StatusCode.ShouldBe(409);
    supplier.IsDelete.ShouldBeFalse();
    supplier.IsActive.ShouldBeTrue();
}
```

另測：建立與改名拒絕不分大小寫的未軟刪除重複名稱、已刪除名稱可重用、無引用時軟刪除且停用、網址與欄位長度驗證。

- [ ] **Step 6: 執行 RED：`dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~SupplierCommandHandlerTests`**

Expected: 失敗原因為尚未存在 Supplier command 型別。

- [ ] **Step 7: 實作最小 command handlers、FluentValidation 與共用正規化**

```csharp
var hasProducts = await context.Products.AnyAsync(x => x.SupplierId == request.Id, cancellationToken);
if (hasProducts)
    return Result.Failure("供應商已有商品使用，請改為停用。", 409);
```

查重限於 `!IsDelete` 且比較 trim 後公司名稱的大小寫不敏感值；`HomePage` 以 `Uri.TryCreate` 且 scheme 為 HTTP／HTTPS 驗證；Create 設為啟用，Update 保留狀態，SetActive 指定狀態，Delete 成功時同時設為軟刪除與停用。

- [ ] **Step 8: 執行 GREEN：`dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~SupplierCommandHandlerTests`**

Expected: PASS。

- [ ] **Step 9: 暫存、檢查與提交 Application task**

Run: `git add src/Application/Features/Suppliers tests/Application.UnitTests/Features/Suppliers`

Run: `git diff --cached --name-only`

Run: `git commit -m "feat: add supplier application workflows"`

### Task 2: Supplier MVC interface and security

**Files:**
- Create: `src/Web/Controllers/SuppliersController.cs`
- Create: `src/Web/ViewModels/Suppliers/SupplierIndexViewModel.cs`
- Create: `src/Web/ViewModels/Suppliers/SupplierEditViewModel.cs`
- Create: `src/Web/Views/Suppliers/Index.cshtml`
- Create: `src/Web/Views/Suppliers/Create.cshtml`
- Create: `src/Web/Views/Suppliers/Edit.cshtml`
- Create: `src/Web/Views/Suppliers/_SupplierForm.cshtml`
- Modify: `src/Web/Views/Shared/_SideBarPartial.cshtml`
- Test: `tests/Application.FunctionalTests/Controllers/SuppliersControllerTests.cs`
- Test: `tests/Application.FunctionalTests/Controllers/SupplierSecurityRegressionTests.cs`

**Interfaces:**
- Consumes: Task 1 DTOs／Commands，`Policies.Suppliers_*`，`IDataProtectionProvider`。
- Produces: `/Suppliers` MVC workflow 與僅對 Suppliers_Read 顯示的導航入口。

- [ ] **Step 1: 寫入失敗的 Controller security tests**

```csharp
[Test]
public void SupplierMutatingActionsRequireAntiForgeryAndScopedPolicies()
{
    AssertActionPolicy(nameof(SuppliersController.Create), Policies.Suppliers_Create);
    AssertActionPolicy(nameof(SuppliersController.Edit), Policies.Suppliers_Update);
    AssertActionPolicy(nameof(SuppliersController.Delete), Policies.Suppliers_Delete);
    AssertActionPolicy(nameof(SuppliersController.SetActive), Policies.Suppliers_Update);
}
```

另測 Edit protector 產生的 token 傳至 Delete 時回傳 NotFound，且 `DeleteSupplierCommand` 不會送出。

- [ ] **Step 2: 執行 RED：`dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~SuppliersControllerTests|FullyQualifiedName~SupplierSecurityRegressionTests`**

Expected: 失敗原因為 Supplier controller 與 views 尚未存在。

- [ ] **Step 3: 實作最小 Controller、ViewModel 與 views**

```csharp
private readonly IDataProtector _editIdProtector =
    dataProtectionProvider.CreateProtector("Suppliers.Edit.ItemId.v1");
private readonly IDataProtector _deleteIdProtector =
    dataProtectionProvider.CreateProtector("Suppliers.Delete.ItemId.v1");
private readonly IDataProtector _setActiveIdProtector =
    dataProtectionProvider.CreateProtector("Suppliers.SetActive.ItemId.v1");
```

對照 `CategoriesController` 處理 Result errors、CancellationToken、protected Id 與 redirect。列表使用 `supplier-table`、篩選與 pagination；表單維護完整欄位；操作依細項 policy 顯示且每個 POST 有 Anti-Forgery；外連僅渲染驗證後的 HTTP／HTTPS URL 並包含 `target="_blank" rel="noopener noreferrer"`。

- [ ] **Step 4: 在 `_SideBarPartial.cshtml` 以既有 `canViewSuppliers.Succeeded` 加入 Supplier nav**

```cshtml
@if (canViewSuppliers.Succeeded)
{
    <li><a asp-action="Index" asp-controller="Suppliers"><i class="fas fa-truck"></i>供應商</a></li>
}
```

- [ ] **Step 5: 執行 GREEN：`dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter FullyQualifiedName~SuppliersControllerTests|FullyQualifiedName~SupplierSecurityRegressionTests`**

Expected: PASS。

- [ ] **Step 6: 暫存、檢查與提交 MVC task**

Run: `git add src/Web tests/Application.FunctionalTests`

Run: `git diff --cached --name-only`

Run: `git commit -m "feat: add supplier management mvc interface"`

### Task 3: Product selection regression and complete verification

**Files:**
- Modify only if coverage is missing: `tests/Application.UnitTests/Features/Products/Commands/ProductCommandHandlerTests.cs`
- Modify only if coverage is missing: `tests/Application.UnitTests/Features/Products/Queries/ProductQueryHandlerTests.cs`

**Interfaces:**
- Consumes: completed Supplier active-state behavior and current Product create/update/form-option behavior。
- Produces: regression proof that stopped Suppliers cannot receive new Product assignments.

- [ ] **Step 1: 查核既有 Product 測試，僅在未覆蓋時先寫 RED**

```csharp
[Test]
public async Task UpdateProductShouldRejectSupplierDisabledAfterExistingAssignment()
{
    var result = await handler.Handle(new UpdateProductCommand
    {
        Id = product.Id, SupplierId = inactiveSupplier.Id
    }, CancellationToken.None);
    result.Succeeded.ShouldBeFalse();
    product.SupplierId.ShouldNotBe(inactiveSupplier.Id);
}
```

- [ ] **Step 2: 執行 Product regression：`dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~ProductCommandHandlerTests|FullyQualifiedName~ProductQueryHandlerTests`**

Expected: 現有覆蓋時 PASS；若新測試發現缺口，先確認 RED、最小修正、再 GREEN。

- [ ] **Step 3: 完整建置與測試**

Run: `dotnet build CleanArchitecture.Northwind.slnx`

Expected: exit code 0。

Run: `dotnet test`

Expected: exit code 0，並將既有 warnings 與新失敗分開記錄。

- [ ] **Step 4: 若有 regression-only 變更，暫存、檢查並提交**

Run: `git add tests/Application.UnitTests`

Run: `git diff --cached --name-only`

Run: `git commit -m "test: cover supplier selection regression"`
