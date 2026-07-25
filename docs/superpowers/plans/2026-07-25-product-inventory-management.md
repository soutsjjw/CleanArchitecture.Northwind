# 第一期商品與庫存管理 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 建立商品與分類 CRUD、單一商品圖片、可稽核的期初／調整／盤點庫存分類帳，以及樂觀並行與權限保護。

**Architecture:** Application 定義 Command、Query、DTO、Validator 與 Handler，透過 `IApplicationDbContext` 存取資料。Infrastructure 負責 EF Core 組態與 Migration；Web 只將 ViewModel 映射為 Application 型別、保護識別碼並呈現 Razor UI。庫存命令在同一個 `SaveChangesAsync` 中更新 `Product.UnitsInStock` 與新增 `InventoryTransaction`，由 Product 的 rowversion 偵測衝突。

**Tech Stack:** .NET 8、ASP.NET Core MVC、MediatR、FluentValidation、EF Core SQL Server、Mapster、NUnit、Moq、Shouldly。

## Global Constraints

- Domain 不依賴 EF Core、MVC、MediatR 或 Data Protection；Application 不依賴 Web 或 Infrastructure。
- 所有瀏覽器狀態變更 Action 都使用 `[ValidateAntiForgeryToken]`；Web 使用既有 `IDataProtectionService` 處理商品／分類識別碼。
- 不新增 NuGet 套件、外部檔案儲存、部署設定或 .NET Aspire。
- 商品圖片只接受 JPEG、PNG、WebP；檢查副檔名、Content-Type、檔案簽名與大小，且不信任檔名或路徑。
- 已被訂單明細或庫存歷程引用的商品不可刪除；已被商品引用的分類／供應商不可刪除；一律改用停用。
- 第一期間供應商僅可讀取與選擇，採購與訂單交易流程不在本計畫內。
- 每個 production 行為先新增失敗測試，再完成最小實作並執行該測試。

---

## File Structure

| 區域 | 主要檔案 | 責任 |
| --- | --- | --- |
| Domain | `Entities/InventoryTransaction.cs`、`Enums/InventoryTransactionType.cs` | 庫存分類帳資料與異動種類 |
| Application | `Features/Products/**`、`Features/Categories/**`、`Features/Inventory/**` | 主檔與庫存用例、驗證、投影 |
| Infrastructure | `Data/Configurations/*`、`Data/Migrations/*` | EF 對應、rowversion、期初資料回填 |
| Web | `Controllers/ProductsController.cs`、`Controllers/CategoriesController.cs`、`ViewModels/Products/**`、`ViewModels/Categories/**`、`Views/Products/**`、`Views/Categories/**` | MVC 表單、圖片、歷程與授權 UI |
| Tests | `tests/Application.UnitTests/Features/{Products,Categories,Inventory}/**`、`tests/Application.FunctionalTests/Controllers/**` | 行為、授權、表單與整合驗證 |

### Task 1: 建立可演進的庫存資料模型與 EF 組態

**Files:**
- Create: `src/Domain/Enums/InventoryTransactionType.cs`
- Create: `src/Domain/Entities/InventoryTransaction.cs`
- Create: `src/Infrastructure/Data/Configurations/InventoryTransactionConfiguration.cs`
- Modify: `src/Domain/Entities/Product.cs`
- Modify: `src/Domain/Entities/Category.cs`
- Modify: `src/Domain/Entities/Supplier.cs`
- Modify: `src/Application/Common/Interfaces/IApplicationDbContext.cs`
- Modify: `src/Infrastructure/Data/ApplicationDbContext.cs`

**Interfaces:**
- Produces `InventoryTransactionType.OpeningBalance`, `ManualAdjustment`, `Stocktake`。
- Produces `DbSet<InventoryTransaction> InventoryTransactions { get; }`。
- Produces `Product.RowVersion`、`Product.Picture`、`Product.PictureContentType`、`Category.IsActive`、`Supplier.IsActive`。

- [ ] **Step 1: 寫模型與組態的失敗測試**

```csharp
[Test]
public void ProductConfigurationShouldUseRowVersionAndInventoryTransactionShouldRequireReason()
{
    typeof(Product).GetProperty(nameof(Product.RowVersion)).ShouldNotBeNull();
    typeof(InventoryTransaction).GetProperty(nameof(InventoryTransaction.Reason)).ShouldNotBeNull();
}
```

- [ ] **Step 2: 執行測試確認失敗**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~InventoryModelTests"`

Expected: FAIL，`Product.RowVersion` 與 `InventoryTransaction` 尚不存在。

- [ ] **Step 3: 建立 Domain 型別與 DbContext 集合**

```csharp
public enum InventoryTransactionType { OpeningBalance, ManualAdjustment, Stocktake }

public sealed class InventoryTransaction : BaseAuditableEntity<int>
{
    public int ProductId { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public short QuantityBefore { get; set; }
    public short QuantityDelta { get; set; }
    public short QuantityAfter { get; set; }
    public string Reason { get; set; } = null!;
    public string? SourceDocumentType { get; set; }
    public int? SourceDocumentId { get; set; }
    public Product Product { get; set; } = null!;
}
```

在 `Product` 加入 `byte[] RowVersion`、可為空的圖片資料與 MIME type；在分類與供應商加入預設為 `true` 的 `IsActive`。將 `InventoryTransactions` 加入兩個 DbContext 介面／實作。

- [ ] **Step 4: 寫 EF Core 組態**

```csharp
builder.Property(x => x.RowVersion).IsRowVersion();
builder.HasIndex(x => new { x.ProductId, x.Created });
builder.Property(x => x.Reason).HasMaxLength(250).IsRequired();
builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId)
       .OnDelete(DeleteBehavior.Restrict);
```

- [ ] **Step 5: 執行模型測試**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~InventoryModelTests"`

Expected: PASS。

### Task 2: 產生 Migration 與期初庫存資料回填

**Files:**
- Create: `src/Infrastructure/Data/Migrations/<timestamp>_AddProductInventoryManagement.cs`
- Create: `src/Infrastructure/Data/Migrations/<timestamp>_AddProductInventoryManagement.Designer.cs`
- Modify: `src/Infrastructure/Data/Migrations/ApplicationDbContextModelSnapshot.cs`
- Test: `tests/Application.FunctionalTests/Features/Inventory/InitialInventoryMigrationTests.cs`

**Interfaces:**
- Consumes Task 1 的模型與組態。
- Produces Product 圖片／rowversion、分類／供應商啟用欄位與 `InventoryTransactions` 資料表。

- [ ] **Step 1: 寫期初回填整合測試**

```csharp
[Test]
public async Task MigrationShouldCreateOneOpeningBalanceWithoutChangingProductStock()
{
    var product = await context.Products.SingleAsync(x => x.Id == productId);
    var transaction = await context.InventoryTransactions.SingleAsync(x => x.ProductId == productId);
    transaction.TransactionType.ShouldBe(InventoryTransactionType.OpeningBalance);
    transaction.QuantityBefore.ShouldBe(product.UnitsInStock);
    transaction.QuantityAfter.ShouldBe(product.UnitsInStock);
    transaction.QuantityDelta.ShouldBe((short)0);
}
```

- [ ] **Step 2: 執行測試確認失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~InitialInventoryMigrationTests"`

Expected: FAIL，尚未建立資料表與期初資料。

- [ ] **Step 3: 產生並檢查 Migration**

Run: `dotnet ef migrations add AddProductInventoryManagement --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/Web.csproj`

在 Migration 的 `Up` 中，以 SQL 從 `Products` 插入一筆期初異動；`QuantityBefore`、`QuantityAfter` 使用 `COALESCE(UnitsInStock, 0)`，`QuantityDelta` 為 0，`CreatedBy` 為 `system:initial-balance`。確認 `IsActive` 預設值為 true。

- [ ] **Step 4: 重新執行期初回填測試**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~InitialInventoryMigrationTests"`

Expected: PASS，且重跑資料庫初始化時不產生重複期初紀錄。

### Task 3: 實作庫存調整、盤點與並行處理

**Files:**
- Create: `src/Application/Features/Inventory/Commands/AdjustInventory/AdjustInventoryCommand.cs`
- Create: `src/Application/Features/Inventory/Commands/AdjustInventory/AdjustInventoryCommandValidator.cs`
- Create: `src/Application/Features/Inventory/Commands/AdjustInventory/AdjustInventoryCommandHandler.cs`
- Create: `src/Application/Features/Inventory/Commands/Stocktake/StocktakeCommand.cs`
- Create: `src/Application/Features/Inventory/Commands/Stocktake/StocktakeCommandValidator.cs`
- Create: `src/Application/Features/Inventory/Commands/Stocktake/StocktakeCommandHandler.cs`
- Test: `tests/Application.UnitTests/Features/Inventory/Commands/InventoryCommandHandlerTests.cs`

**Interfaces:**
- `AdjustInventoryCommand(int ProductId, short QuantityDelta, string Reason, byte[] RowVersion)`。
- `StocktakeCommand(int ProductId, short ActualQuantity, string Reason, byte[] RowVersion)`。
- 成功結果提供最新庫存與最新 rowversion；失敗結果不儲存任何資料。

- [ ] **Step 1: 寫負庫存、盤點差異與停用商品的失敗測試**

```csharp
[TestCase((short)-11)]
[TestCase((short)-1)]
public async Task AdjustShouldRejectNegativeResult(short delta)
{
    var result = await handler.Handle(new AdjustInventoryCommand(productId, delta, "盤損", version), CancellationToken.None);
    result.Succeeded.ShouldBeFalse();
    context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}

[Test]
public async Task StocktakeShouldWriteActualMinusCurrentAsDelta() { /* current 8, actual 11 => +3 */ }
```

- [ ] **Step 2: 執行測試確認失敗**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~InventoryCommandHandlerTests"`

Expected: FAIL，命令與 Handler 尚不存在。

- [ ] **Step 3: 實作共用庫存寫入邏輯**

```csharp
var before = product.UnitsInStock ?? 0;
var after = checked((short)(before + request.QuantityDelta));
if (after < 0) return Result.Failure("庫存不足，無法完成操作。");
context.InventoryTransactions.Add(new InventoryTransaction {
    ProductId = product.Id, TransactionType = type,
    QuantityBefore = before, QuantityDelta = delta, QuantityAfter = after,
    Reason = request.Reason.Trim()
});
product.UnitsInStock = after;
await context.SaveChangesAsync(cancellationToken);
```

在儲存時設定 `product.RowVersion` 的 OriginalValue；捕捉 `DbUpdateConcurrencyException` 並回傳不揭露細節的衝突結果。驗證商品未停用／未刪除、原因非空且長度不超過 250、結果位於 `short` 範圍。

- [ ] **Step 4: 寫並行衝突測試並完成實作**

```csharp
[Test]
public async Task AdjustShouldReturnConflictWhenRowVersionHasChanged()
{
    saveChanges.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ThrowsAsync(new DbUpdateConcurrencyException());
    var result = await handler.Handle(command, CancellationToken.None);
    result.Succeeded.ShouldBeFalse();
    result.Errors.ShouldContain("庫存已被其他使用者更新");
}
```

- [ ] **Step 5: 執行庫存命令測試**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~Features.Inventory.Commands"`

Expected: PASS。

### Task 4: 建立庫存歷程與商品選項 Query

**Files:**
- Create: `src/Application/Features/Inventory/Queries/GetInventoryTransactions/GetInventoryTransactionsQuery.cs`
- Create: `src/Application/Features/Inventory/Queries/GetInventoryTransactions/GetInventoryTransactionsQueryHandler.cs`
- Create: `src/Application/Features/Inventory/Queries/GetInventoryTransactions/InventoryTransactionDto.cs`
- Create: `src/Application/Features/Products/Queries/GetProductOptions/GetProductOptionsQuery.cs`
- Create: `src/Application/Features/Products/Queries/GetProductOptions/GetProductOptionsQueryHandler.cs`
- Test: `tests/Application.UnitTests/Features/Inventory/Queries/GetInventoryTransactionsQueryHandlerTests.cs`

**Interfaces:**
- 歷程 Query 回傳依 `Created`、`Id` 倒序的 `PaginatedList<InventoryTransactionDto>`。
- 商品選項／分類與供應商選項一律排除 `IsDelete` 或 `IsActive == false` 的資料。

- [ ] **Step 1: 寫排序、投影與停用篩選失敗測試**

```csharp
[Test]
public async Task HandleShouldReturnNewestTransactionsFirstWithoutTracking()
{
    var result = await handler.Handle(new GetInventoryTransactionsQuery { ProductId = productId }, CancellationToken.None);
    result.Data.Items.Select(x => x.Id).ShouldBe([newestId, olderId]);
}
```

- [ ] **Step 2: 執行 Query 測試確認失敗**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~GetInventoryTransactionsQueryHandlerTests"`

Expected: FAIL，Query 尚不存在。

- [ ] **Step 3: 實作可翻譯的只讀 projection**

```csharp
return await context.InventoryTransactions.AsNoTracking()
    .Where(x => x.ProductId == request.ProductId)
    .OrderByDescending(x => x.Created).ThenByDescending(x => x.Id)
    .Select(x => new InventoryTransactionDto(x.Id, x.TransactionType, x.QuantityBefore,
        x.QuantityDelta, x.QuantityAfter, x.Reason, x.Created, x.CreatedBy))
    .PaginatedListAsync(request.PageNumber, request.PageSize);
```

- [ ] **Step 4: 執行 Query 測試**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~Features.Inventory.Queries"`

Expected: PASS。

### Task 5: 建立商品與分類 Application CRUD

**Files:**
- Create: `src/Application/Features/Products/Commands/{CreateProduct,UpdateProduct,SetProductDiscontinued,DeleteProduct}/**`
- Create: `src/Application/Features/Products/Queries/{GetProducts,GetProductDetail,GetProductFormOptions}/**`
- Create: `src/Application/Features/Categories/Commands/{CreateCategory,UpdateCategory,SetCategoryActive,DeleteCategory}/**`
- Create: `src/Application/Features/Categories/Queries/{GetCategories,GetCategoryDetail}/**`
- Test: `tests/Application.UnitTests/Features/Products/**`
- Test: `tests/Application.UnitTests/Features/Categories/**`

**Interfaces:**
- Product create/update 只接受原始 `CategoryId`、`SupplierId`、圖片資料／MIME type與產品欄位；不接受 MVC `IFormFile`。
- Delete Handler 先檢查 `OrderDetails` 與 `InventoryTransactions`／`Products` 關聯，再決定可軟刪除或回傳失敗。

- [ ] **Step 1: 寫主檔規則失敗測試**

```csharp
[Test]
public async Task DeleteProductShouldRejectWhenInventoryHistoryExists() { /* failure and no save */ }
[Test]
public async Task CreateProductShouldRejectInactiveCategoryOrSupplier() { /* validation failure */ }
[Test]
public async Task DeleteCategoryShouldRejectWhenProductsExist() { /* failure and no save */ }
```

- [ ] **Step 2: 執行主檔規則測試確認失敗**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~Features.Products|FullyQualifiedName~Features.Categories"`

Expected: FAIL，CRUD Command／Query 尚不存在。

- [ ] **Step 3: 實作 Validators 與 Handlers**

所有名稱與理由使用 trim 後驗證；商品名稱最大 40、分類名稱最大 15、包裝量最大 20。更新商品時驗證分類與供應商仍啟用，並允許合法重新指派。停用／恢復不解除現有關聯。

- [ ] **Step 4: 執行商品與分類單元測試**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~Features.Products|FullyQualifiedName~Features.Categories"`

Expected: PASS。

### Task 6: 實作圖片驗證與 Web ViewModels

**Files:**
- Create: `src/Web/Services/ProductImageValidator.cs`
- Create: `src/Web/ViewModels/Products/{ProductIndexViewModel,ProductEditViewModel,ProductDetailViewModel,InventoryAdjustmentViewModel,StocktakeViewModel}.cs`
- Create: `src/Web/ViewModels/Categories/{CategoryIndexViewModel,CategoryEditViewModel}.cs`
- Test: `tests/Application.FunctionalTests/Controllers/ProductImageValidatorTests.cs`

**Interfaces:**
- `ProductImageValidator.TryValidate(IFormFile? file, out ValidatedProductImage? image, out string? error)`。
- 最大檔案大小應以單一常數定義（2 MiB）；接受 JPEG、PNG、WebP 簽名與 MIME type 的交集。

- [ ] **Step 1: 寫圖片安全驗證失敗測試**

```csharp
[TestCase("image/jpeg", ".jpg", true)]
[TestCase("image/png", ".png", true)]
[TestCase("image/gif", ".gif", false)]
public void TryValidateShouldAcceptOnlyAllowedImageFormats(string contentType, string extension, bool expected) { /* ... */ }
```

- [ ] **Step 2: 執行測試確認失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~ProductImageValidatorTests"`

Expected: FAIL，驗證器尚不存在。

- [ ] **Step 3: 實作驗證器與 ViewModels**

讀取最多 2 MiB 加一個簽名緩衝區；拒絕空檔、超限、MIME／副檔名／簽名不一致的檔案。驗證成功才將 bytes 與正規化 MIME type 交給 Application Command；絕不保存原始檔名。

- [ ] **Step 4: 執行圖片驗證測試**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~ProductImageValidatorTests"`

Expected: PASS。

### Task 7: 實作 Products 與 Categories MVC Controller 和 Razor UI

**Files:**
- Create: `src/Web/Controllers/ProductsController.cs`
- Create: `src/Web/Controllers/CategoriesController.cs`
- Create: `src/Web/Views/Products/{Index,Details,Create,Edit,AdjustInventory,Stocktake}.cshtml`
- Create: `src/Web/Views/Products/_ProductForm.cshtml`
- Create: `src/Web/Views/Categories/{Index,Create,Edit}.cshtml`
- Create: `src/Web/Views/Categories/_CategoryForm.cshtml`
- Modify: `src/Web/Views/Shared/_SideBarPartial.cshtml`
- Test: `tests/Application.FunctionalTests/Controllers/{ProductsControllerTests,CategoriesControllerTests}.cs`

**Interfaces:**
- 所有 GET／POST Action 使用 `Policies.Products_*`、`Policies.Categories_*`；歷程使用 `Inventory:Read:`，調整／盤點使用 `Inventory:Create:`。
- `Image(string id)` 需 `Products:Read:`，且只輸出未軟刪除商品的已驗證圖片。

- [ ] **Step 1: 寫受保護識別碼、Anti-Forgery 與 policy 的失敗測試**

```csharp
[Test]
public void ProductMutatingActionsShouldRequireAntiForgeryAndPolicies()
{
    var source = File.ReadAllText(GetWebControllerPath("ProductsController.cs"));
    source.ShouldContain("[ValidateAntiForgeryToken]");
    source.ShouldContain("Policies.Products_Update");
    source.ShouldContain("Inventory:Create:");
}
```

- [ ] **Step 2: 執行 Web 結構測試確認失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~ProductsControllerTests|FullyQualifiedName~CategoriesControllerTests"`

Expected: FAIL，Controller 與 Views 尚不存在。

- [ ] **Step 3: 實作 Controller 與 Razor 表單**

將保護後 Id、rowversion（Base64）、可選圖片與 UI 驗證置於 ViewModel。POST 還原 Id 後才建立 Command；失敗時把 Application `FieldErrors` 轉為 ModelState，且重新載入啟用分類／供應商選項。使用 multipart form 上傳圖片，所有提交使用 Tag Helper Anti-Forgery。

- [ ] **Step 4: 實作歷程頁面與授權導覽**

商品詳情以 `GetInventoryTransactionsQuery` 顯示歷程；僅在 policy 成功時顯示調整、盤點、停用、恢復與刪除按鈕。側邊欄只在有商品／分類讀取權限時顯示對應連結。

- [ ] **Step 5: 執行 MVC 功能測試**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~ProductsControllerTests|FullyQualifiedName~CategoriesControllerTests"`

Expected: PASS。

### Task 8: 補齊權限、整合與回歸測試

**Files:**
- Modify: `src/Domain/Constants/Policies.cs`
- Modify: `src/Web/Views/Shared/_SideBarPartial.cshtml`
- Create: `tests/Application.FunctionalTests/Controllers/InventoryAuthorizationTests.cs`
- Modify: 受 Task 1–7 影響的既有測試 fixture／測試檔案。

**Interfaces:**
- `Policies.Inventory_Read = "Inventory:Read:"` 與 `Policies.Inventory_Create = "Inventory:Create:"`。
- 現有權限解析器可處理 `Inventory:Read:All` 與 `Inventory:Create:All`，不需新增權限解析套件或平行機制。

- [ ] **Step 1: 寫庫存頁面授權失敗測試**

```csharp
[Test]
public async Task AdjustShouldForbidUserWithoutInventoryCreatePermission()
{
    var response = await client.PostAsync(adjustUrl, validForm);
    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
}
```

- [ ] **Step 2: 執行授權測試確認失敗**

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~InventoryAuthorizationTests"`

Expected: FAIL，尚未建立 inventory policy 與 Action 授權。

- [ ] **Step 3: 加入 policy 常數與權限測試 fixture 資料**

使用既有 permission claim 格式授予測試使用者 `Inventory:Read:All` 或 `Inventory:Create:All`；不可在 Controller 內自行解析 claims。

- [ ] **Step 4: 執行完整相關測試與建置**

Run: `dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~Features.Products|FullyQualifiedName~Features.Categories|FullyQualifiedName~Features.Inventory"`

Expected: PASS。

Run: `dotnet test tests/Application.FunctionalTests/Application.FunctionalTests.csproj --filter "FullyQualifiedName~ProductsControllerTests|FullyQualifiedName~CategoriesControllerTests|FullyQualifiedName~InventoryAuthorizationTests|FullyQualifiedName~ProductImageValidatorTests"`

Expected: PASS。

Run: `dotnet build CleanArchitecture.Northwind.slnx`

Expected: exit code 0，無新增編譯錯誤。

### Task 9: 最終資料庫與回歸驗證

**Files:**
- Verify only: Tasks 1–8 涉及的檔案與 Migration。

**Interfaces:**
- Consumes 所有前述任務。
- Produces 可部署且資料完整的第一期商品與庫存管理。

- [ ] **Step 1: 檢查 Migration 與工作樹格式**

Run: `git diff --check` 和 `dotnet ef migrations has-pending-model-changes --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/Web.csproj`

Expected: 兩者 exit code 0。

- [ ] **Step 2: 執行完整方案測試**

Run: `dotnet test CleanArchitecture.Northwind.slnx`

Expected: exit code 0，沒有失敗測試。

- [ ] **Step 3: 人工檢查安全邊界**

確認所有 POST Action 有 Anti-Forgery、圖片 Action 有 Read policy、Web 沒有 `ApplicationDbContext` 注入、Application 沒有 MVC 或 `IFormFile` 引用、庫存歷程沒有 Update/Delete Command。

- [ ] **Step 4: 提交實作**

Run: `git add src/Domain src/Application src/Infrastructure src/Web tests docs/superpowers/plans/2026-07-25-product-inventory-management.md`，接著 `git commit -m "feat: add product inventory management"`。

## Self-Review

- 規格中的商品／分類 CRUD、供應商唯讀選擇、單一商品圖片、期初庫存、手動調整、盤點、歷程、負庫存拒絕、並行衝突、權限與未來訂單／採購邊界，分別對應到 Task 1–9。
- 本計畫沒有未定義的功能佔位文字；Migration 驗證使用既有的 `Application.FunctionalTests` SQL Server Testcontainers fixture。
- 所有跨任務型別名稱均在 Task 1、Task 3 或 Task 4 首次定義，Web 層只使用已由 Application 產出的 Command、Query 與 DTO。
