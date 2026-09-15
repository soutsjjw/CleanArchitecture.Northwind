# 客戶列表實作計畫

> **供代理工作者使用：** 必須使用子技能 `superpowers:subagent-driven-development`（建議）或 `superpowers:executing-plans`，逐項執行本計畫；步驟以核取方塊（`- [ ]`）追蹤。

**目標：** 建置唯讀 MVC 客戶列表，提供關鍵字與地理篩選、穩定排序、分頁及依權限顯示的導覽。

**架構：** 在 `Application` 新增 CQRS 查詢，透過 `IApplicationDbContext` 對 `Customer` 使用 `AsNoTracking()` 直接投影，回傳列表項目及國家／城市篩選選項。HTTP 繫結、授權與 Razor 互動保留於 `Web`；Controller 將 Application DTO 映射為 Web ViewModel，並以請求的取消權杖呼叫 `ISender`。沿用既有的 `Customers:Read:` 動態權限 Policy，不引入 Customer 歸屬模型或資料庫 schema 變更。

**技術：** ASP.NET Core MVC、MediatR、EF Core、Mapster（僅於既有映射適用時使用）、NUnit、Shouldly、Moq。

**規格來源：** 使用者於 2026-09-16 在 Codex 對話中確認的需求；Repository 沒有獨立規格文件。

## 全域限制

- 不得修改資料庫 schema、Migration、套件、部署設定或 `appsettings*.json`。
- 僅實作唯讀列表；不建立 Customer 詳細、建立、編輯或刪除路由。
- 必須使用 `Policies.Customers_Read`；透過既有角色管理 UI，為 Sales 與 Customer Service 指派 `Customers:Read:All` 權限；既有授權處理器仍使 Administrator 取得存取權。
- 查詢僅使用 Customer 的 CompanyName、ContactName、Country、City、Phone 與作為穩定排序依據的 Id。
- 關鍵字須先去除前後空白，以不分大小寫的部分比對搜尋五個可見欄位。國家與城市則採選取值的完全比對。
- 預設每頁 10 筆、公司名稱升冪；五個可見欄位皆可排序，並以 Id 作為穩定次要排序鍵。
- 國家選項來自所有 Country 非空白的 Customer；城市選項只來自已選國家的 Customer；瀏覽器變更國家時必須清空 City。
- 透過既有表單分頁行為保留篩選、排序、每頁筆數與頁碼。POST 表單必須使用 Anti-Forgery。

---

### 工作 1：Application 客戶列表查詢

**檔案：**

- 建立：`src/Application/Features/Customers/Queries/GetCustomers/GetCustomersQuery.cs`
- 建立：`src/Application/Features/Customers/Queries/GetCustomers/CustomersDto.cs`
- 測試：`tests/Application.UnitTests/Features/Customers/Queries/GetCustomers/GetCustomersQueryHandlerTests.cs`

**介面：**

- 使用：`IApplicationDbContext.Customers`、`PaginatedList<T>`、`Result<T>`。
- 產出：供 MVC 層使用的 `GetCustomersQuery : IRequest<Result<CustomersDto>>`、`CustomerSortField`、`CustomerListItemDto` 與 `CustomersDto`。

- [ ] **步驟 1：撰寫失敗的查詢測試**

  以 `GetOrdersQueryHandlerTests` 既有的非同步 `DbSet<T>` 測試替身模式建立測試 Fixture。以下以手寫資料驗證可觀察行為：

  ```csharp
  [Test]
  public async Task HandleShouldMatchKeywordAcrossVisibleFields()
  {
      var result = await HandleAsync(new GetCustomersQuery
      {
          Keyword = "  555-0100  ", PageNumber = 1, PageSize = 10
      });

      result.Succeeded.ShouldBeTrue();
      result.Data.Customers.Items.Select(x => x.Id).ShouldBe(["ALFKI"]);
  }

  [Test]
  public async Task HandleShouldFilterCitiesBySelectedCountry()
  {
      var result = await HandleAsync(new GetCustomersQuery
      {
          Country = "Germany", City = "Berlin", PageNumber = 1, PageSize = 10
      });

      result.Data.Customers.Items.Select(x => x.Id).ShouldBe(["ALFKI"]);
      result.Data.Countries.ShouldBe(["France", "Germany"]);
      result.Data.Cities.ShouldBe(["Berlin", "Munich"]);
  }

  [Test]
  public async Task HandleShouldUseIdToStabilizeCompanyNameSortAcrossPages()
  {
      var result = await HandleAsync(new GetCustomersQuery
      {
          SortBy = CustomerSortField.CompanyName,
          PageNumber = 1,
          PageSize = 2
      });

      result.Data.Customers.Items.Select(x => x.Id).ShouldBe(["ALFKI", "ANATR"]);
  }
  ```

  撰寫前先記錄各測試要防止的錯誤：漏掉電話搜尋、城市選項混入其他國家，以及同公司名稱跨頁時排序不穩。

- [ ] **步驟 2：執行新測試並確認 RED**

  執行：`dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~Features.Customers.Queries.GetCustomers`

  預期：因 `GetCustomersQuery`、`CustomersDto` 與 Handler 尚不存在而編譯失敗或無法探索測試。

- [ ] **步驟 3：實作最小查詢契約與 Handler**

  在 `GetCustomersQuery.cs` 定義預設值與欄位：

  ```csharp
  public sealed record GetCustomersQuery : IRequest<Result<CustomersDto>>
  {
      public string? Keyword { get; init; }
      public string? Country { get; init; }
      public string? City { get; init; }
      public CustomerSortField? SortBy { get; init; }
      public bool SortDescending { get; init; }
      public int PageNumber { get; init; } = 1;
      public int PageSize { get; init; } = 10;
  }
  ```

  僅投影五個顯示欄位。從所有 Customer 計算不重複且非空白的國家選項；城市選項則在有 `request.Country` 時限縮為該國家。排序與 `PaginatedList.CreateAsync` 前套用關鍵字與完全比對的地理篩選。依既有訂單列表模式搭配 null guard 使用 `ToLower()`／`Contains()`。每個排序欄位以 `ThenBy(customer => customer.Id)` 或對應的遞減排序作為次要鍵；預設為 `OrderBy(customer => customer.CompanyName).ThenBy(customer => customer.Id)`。

- [ ] **步驟 4：執行聚焦測試並確認 GREEN**

  執行：`dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~Features.Customers.Queries.GetCustomers`

  預期：通過關鍵字、相依城市選項、篩選、穩定排序與分頁的斷言。

- [ ] **步驟 5：僅在 GREEN 後重構**

  僅在排序 switch 的重複足以降低可讀性時，萃取小型私有排序 Helper。重新執行聚焦測試命令，維持綠燈。

- [ ] **步驟 6：提交查詢切片**

  ```bash
  git add src/Application/Features/Customers/Queries/GetCustomers tests/Application.UnitTests/Features/Customers/Queries/GetCustomers
  git commit -m "feat: add customer list query"
  ```

### 工作 2：MVC 客戶列表與受授權導覽

**檔案：**

- 建立：`src/Web/Controllers/CustomersController.cs`
- 建立：`src/Web/ViewModels/Customers/CustomerIndexViewModel.cs`
- 建立：`src/Web/Views/Customers/Index.cshtml`
- 修改：`src/Web/Views/Shared/_SideBarPartial.cshtml`
- 測試：`tests/Application.UnitTests/Features/Customers/Queries/GetCustomers/GetCustomersQueryHandlerTests.cs`（迴歸執行）

**介面：**

- 使用：`GetCustomersQuery`、`CustomersDto`、`CustomerListItemDto`、`Policies.Customers_Read`、`IPaginatedList` 與 `_PaginationPartial.cshtml`。
- 產出：`GET /Customers/Index`、受 Anti-Forgery 保護的篩選 POST，以及僅對可讀取者顯示的側欄 Customer 項目。

- [ ] **步驟 1：實作前記錄 MVC 驗收檢查**

  Repository 沒有 MVC Controller 測試專案。以下手動驗收項目即為測試契約，必須於實作後實際執行：

  ```text
  1. 缺少 Customers:Read 的 Principal 會收到禁止存取，且看不到側欄 Customer 連結。
  2. 已授權的 Principal 能看到五個欄位，並可提交關鍵字、國家與城市篩選。
  3. 選擇 Germany 時 City 僅顯示德國城市；變更 Country 時 City 會在送出前清空。
  4. 點擊可排序表頭會將 pageNumber 重設為 1；第二次點擊同欄位則切換方向。
  5. 分頁與每頁筆數調整會保留所有查詢狀態；無結果時顯示 _NoDataPartial。
  ```

- [ ] **步驟 2：實作最小 Controller 與 ViewModel**

  新增套用 `[Authorize(Policy = Policies.Customers_Read)]` 的 Controller。GET Action 繫結 `keyword`、`country`、`city`、`sortBy`、`sortDescending`、`pageNumber` 與 `pageSize`，再透過 `ISender` 及 `CancellationToken` 傳送 `GetCustomersQuery`。POST Action 具備 `[ValidateAntiForgeryToken]`，接受同一組欄位並重用同一查詢建立方法。將 DTO 明確映射為 Customer index ViewModel；不得將 Customer Domain Entity 暴露給 Razor。

- [ ] **步驟 3：依既有表單／分頁慣例建置 Razor 列表**

  使用參考 `Views/Orders/Index.cshtml` 的 Anti-Forgery `method="post"` 表單。從 ViewModel options 渲染 Country 與 City `<select>`、查詢按鈕與重設連結、結果筆數、五個可排序表頭、nullable 欄位的 `-`、`_NoDataPartial` 與 `_PaginationPartial`。沿用 `pagination.js`；送出前將頁碼、每頁筆數、排序欄位與方向寫入 hidden inputs。加入簡短的用戶端 Country change handler，先將 City select 設為空值再送出。

- [ ] **步驟 4：加入授權感知的側欄項目**

  在 `_SideBarPartial.cshtml` 以 `AuthorizationService.AuthorizeAsync(User, Policies.Customers_Read)` 取得 `canViewCustomers`，僅在授權成功時渲染 Customers Index 導覽。View 中不得加入直接角色檢查。

- [ ] **步驟 5：驗證編譯與查詢迴歸**

  ```bash
  dotnet build CleanArchitecture.Northwind.slnx
  dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter FullyQualifiedName~Features.Customers.Queries.GetCustomers
  ```

  預期：建置成功且所有 Customer 查詢測試通過。

- [ ] **步驟 6：執行 MVC 手動驗收檢查**

  以 Administrator 帳號登入，驗證頁面、五個精確欄位、篩選、排序、分頁與空狀態。透過既有角色管理 UI 為 Sales 與 Customer Service 設定 `Customers:Read:All`，再驗證兩者皆可見導覽連結與列表。確認未取得 Customer read 權限的角色既看不到連結，也無法存取端點。

- [ ] **步驟 7：提交 MVC 切片**

  ```bash
  git add src/Web/Controllers/CustomersController.cs src/Web/ViewModels/Customers/CustomerIndexViewModel.cs src/Web/Views/Customers/Index.cshtml src/Web/Views/Shared/_SideBarPartial.cshtml
  git commit -m "feat: add customer list page"
  ```

### 工作 3：最終驗證與交付審查

**檔案：**

- 僅驗證：工作 1–2 所修改的全部檔案

**介面：**

- 使用：完成的查詢、MVC 頁面與動態權限 Policy。
- 產出：功能遵守分層邊界且通過相關驗證的佐證。

- [ ] **步驟 1：檢查最終 diff**

  執行：`git diff --check` 及 `git diff -- src/Application/Features/Customers src/Web/Controllers/CustomersController.cs src/Web/ViewModels/Customers src/Web/Views/Customers src/Web/Views/Shared/_SideBarPartial.cshtml tests/Application.UnitTests/Features/Customers`

  預期：沒有空白錯誤、schema／設定／套件變更，也沒有 Domain Entity 傳入 View。

- [ ] **步驟 2：執行必要驗證套件**

  ```bash
  dotnet build CleanArchitecture.Northwind.slnx
  dotnet test tests/Application.UnitTests/Application.UnitTests.csproj
  ```

  預期：兩個命令皆通過。若完整單元測試無法執行，需報告其精確失敗資訊，並保留聚焦查詢測試結果。

- [ ] **步驟 3：確認提交不包含被忽略的產物**

  執行：`git diff --cached --name-only`

  預期：沒有 `.superpowers/`、`bin/`、`obj/`、測試結果或產生檔。僅在使用者要求提交時才建立提交；否則保持原始碼變更未暫存。

## 自我審查

- 規格覆蓋：工作 1 涵蓋五欄、關鍵字、地理篩選、預設／穩定排序與分頁；工作 2 涵蓋 MVC Presentation、Anti-Forgery、篩選互動、導覽與動態授權；工作 3 驗證分層邊界與結果。
- 完整性檢查：每項工作均具體定義行為、實作邊界，以及驗證命令或手動驗收檢查。
- 型別一致性：`GetCustomersQuery` 回傳 `Result<CustomersDto>`；`CustomersDto` 包含 `PaginatedList<CustomerListItemDto>` 與國家／城市選項；Controller 僅將這些 DTO 映射為 Customer ViewModel。
