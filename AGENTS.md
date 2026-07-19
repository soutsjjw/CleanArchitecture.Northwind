# AGENTS.md

## 1. 專案定位

本專案以 `jasontaylordev/CleanArchitecture` 為基礎，採用 Clean Architecture、CQRS、MediatR、EF Core 與 ASP.NET Core MVC。

重要假設：

| 項目             | 說明                                                          |
| -------------- | ----------------------------------------------------------- |
| Presentation   | `src/Web` 已由 Minimal API 調整為 ASP.NET Core MVC               |
| Core           | `src/Application` 與 `src/Domain` 是系統核心                      |
| Infrastructure | `src/Infrastructure` 負責 EF Core、Identity、外部服務實作             |
| Shared         | `src/Shared` 僅放跨專案共用且不含業務規則的內容                              |
| Aspire         | 專案已移除 Aspire，不再使用 `AppHost`、`ServiceDefaults`、`TestAppHost` |

所有 Codex 變更必須優先維持 Clean Architecture 邊界，不得為了快速完成而讓 MVC、EF Core、外部服務或基礎設施細節滲入核心層。

## 2. 回覆與變更原則

### 2.1 語言與格式

Codex 回覆必須使用繁體中文。

優先格式：

| 類型   | 要求              |
| ---- | --------------- |
| 說明   | 使用 Markdown     |
| 技術分析 | 表格優先            |
| 實作建議 | 使用 SOP          |
| 程式碼  | 使用正確語言標記        |
| 架構說明 | 必須補充分層、依賴方向、資料流 |

禁止：

| 禁止項目        | 說明                                                                 |
| ----------- | ------------------------------------------------------------------ |
| 虛構檔案        | 不得假設不存在的檔案、類別、方法                                                   |
| 大範圍重構       | 除非任務明確要求                                                           |
| Hacky 解法    | 不得以繞過架構邊界方式完成                                                      |
| 無關修改        | 不得修改與任務無關的格式、命名、套件、設定                                              |
| 未經要求新增套件    | 不得任意新增 NuGet package                                               |
| 未經要求修改部署設定  | 不得任意修改 Docker、CI、環境設定                                              |
| 重新引入 Aspire | 不得新增 `AppHost`、`ServiceDefaults`、`TestAppHost` 或 Aspire hosting 設定 |

## 3. 專案結構

預期主要結構如下：

```text
src/
  Application/
  Domain/
  Infrastructure/
  Shared/
  Web/

tests/
  Application.FunctionalTests/
  Application.UnitTests/
  Domain.UnitTests/
  Infrastructure.IntegrationTests/
  Web.AcceptanceTests/
```

若實際 repo 結構與上述不同，必須以實際檔案為準，不得自行建立平行架構。

若仍殘留下列 Aspire 相關專案或資料夾，除非任務明確要求移除，Codex 不得任意刪除：

```text
src/AppHost/
src/ServiceDefaults/
tests/TestAppHost/
```

但在新增功能、修正錯誤、重構時，不得再依賴或新增 Aspire 相關程式碼。

## 4. 分層責任

| 專案               | 角色                            | 可放內容                                                                                            | 禁止內容                                                                          |
| ---------------- | ----------------------------- | ----------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| `Domain`         | 核心領域模型                        | Entity、Value Object、Domain Event、Enumeration、Domain Rule                                        | EF Core、HTTP、MVC、MediatR Handler、Infrastructure、外部 API、資料庫細節                  |
| `Application`    | Use Case 與流程協調                | Command、Query、Handler、DTO、Validator、Interface、Pipeline Behaviour                                | Controller、ViewModel、DbContext 實作、外部服務實作、Razor、HttpContext                    |
| `Infrastructure` | 技術實作                          | EF Core DbContext、Entity Configuration、Identity、Repository 實作、Email、File Storage、第三方 API Client | MVC Controller、View、業務規則、Use Case 流程                                          |
| `Web`            | ASP.NET Core MVC Presentation | Controller、View、ViewModel、Filter、Middleware、路由、DI 組合、UI 驗證                                      | 商業邏輯、直接操作 DbContext、直接呼叫 Infrastructure Service 實作、把 Domain Entity 直接暴露給 View |
| `Shared`         | 跨專案共用基礎內容                     | 共用常數、共用契約、共用基底型別                                                                                | 特定 Use Case、UI 文字、資料存取、業務規則                                                   |

## 5. 依賴方向硬規則

### 5.1 允許依賴

| From             | Allowed To                                  |
| ---------------- | ------------------------------------------- |
| `Domain`         | 無                                           |
| `Application`    | `Domain`                                    |
| `Infrastructure` | `Application`、`Domain`、必要時 `Shared`         |
| `Web`            | `Application`、`Infrastructure`、必要時 `Shared` |
| `Shared`         | 無業務層依賴                                      |

### 5.2 禁止依賴

| 禁止方向                              | 原因                               |
| --------------------------------- | -------------------------------- |
| `Domain` -> `Application`         | Domain 必須是最內層                    |
| `Domain` -> `Infrastructure`      | 不得知道資料庫、EF Core、外部服務             |
| `Domain` -> `Web`                 | 不得知道 MVC、HTTP、View               |
| `Application` -> `Infrastructure` | Use Case 只能依賴抽象                  |
| `Application` -> `Web`            | Use Case 不得依賴 MVC                |
| `Infrastructure` -> `Web`         | Infrastructure 不得依賴 Presentation |
| `Web` -> EF Core `DbContext`      | Web 不得直接存取資料庫                    |
| `View` -> Domain Entity           | View 不得直接綁定領域模型                  |
| 任一專案 -> Aspire Hosting            | 專案已移除 Aspire，不得重新引入              |

## 6. MVC 架構規則

`Web` 已改為 ASP.NET Core MVC，因此 Presentation 層應遵守下列規則。

### 6.1 Controller 規則

Controller 只能負責：

| 責任                 | 說明                                         |
| ------------------ | ------------------------------------------ |
| 接收 HTTP Request    | Route、QueryString、Form、Body                |
| 建立 Command 或 Query | 將 MVC ViewModel 轉成 Application request     |
| 呼叫 `ISender`       | 使用 MediatR 送出 Command 或 Query              |
| 處理 ModelState      | 回傳 View 或 Redirect                         |
| 回傳結果               | View、RedirectToAction、Json、File、StatusCode |

Controller 禁止：

| 禁止項目                    | 說明                               |
| ----------------------- | -------------------------------- |
| 寫商業邏輯                   | 應放在 Application 或 Domain         |
| 直接注入 DbContext          | 必須透過 Application use case        |
| 直接操作 EF Core Query      | 查詢邏輯應在 Application               |
| 直接呼叫 Infrastructure 實作  | 應依賴 Application 抽象或 Mediator     |
| 回傳 Domain Entity 給 View | 必須使用 ViewModel                   |
| 使用 Service Locator      | 不得手動從 `IServiceProvider` 取服務處理業務 |

建議 Controller 範例：

```csharp
public sealed class TodoListsController : Controller
{
    private readonly ISender _sender;

    public TodoListsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _sender.Send(new GetTodoListsQuery());
        var viewModel = new TodoListsIndexViewModel(result);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTodoListViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        await _sender.Send(new CreateTodoListCommand(viewModel.Title));

        return RedirectToAction(nameof(Index));
    }
}
```

### 6.2 ViewModel 規則

| 類型              | 放置位置                                                               | 用途                 |
| --------------- | ------------------------------------------------------------------ | ------------------ |
| MVC ViewModel   | `src/Web/Models`、`src/Web/ViewModels`、`src/Web/Areas/*/ViewModels` | View 專用資料形狀        |
| Application DTO | `src/Application/**`                                               | Use Case 輸入與輸出     |
| Domain Entity   | `src/Domain/**`                                                    | 領域模型，不得直接給 View 使用 |

ViewModel 規則：

1. ViewModel 可包含 UI 顯示欄位。
2. ViewModel 可包含表單驗證屬性，但不可取代 Application 驗證。
3. ViewModel 不得包含業務規則。
4. ViewModel 不得包裝 EF Core tracking entity。
5. ViewModel 不得直接暴露 Domain Entity。

### 6.3 View 規則

Razor View 只能負責：

| 可做               | 不可做                    |
| ---------------- | ---------------------- |
| 顯示資料             | 查資料庫                   |
| 表單輸入             | 呼叫 Application Service |
| 基本 UI 條件判斷       | 寫商業規則                  |
| 使用 Tag Helper    | 呼叫 Infrastructure      |
| 顯示 ModelState 錯誤 | 修改資料                   |

所有 POST 表單必須使用 Anti-Forgery：

```html
<form asp-action="Create" method="post">
    @Html.AntiForgeryToken()
</form>
```

Controller POST action 必須加上：

```csharp
[ValidateAntiForgeryToken]
```

## 7. CQRS 與 MediatR 規則

### 7.1 Command

Command 用於會改變系統狀態的操作。

| 操作   | 命名                       |
| ---- | ------------------------ |
| 建立   | `CreateXxxCommand`       |
| 修改   | `UpdateXxxCommand`       |
| 刪除   | `DeleteXxxCommand`       |
| 狀態切換 | `ChangeXxxStatusCommand` |
| 匯入   | `ImportXxxCommand`       |
| 匯出要求 | `ExportXxxCommand`       |

### 7.2 Query

Query 用於讀取資料，不得修改狀態。

| 操作   | 命名                   |
| ---- | -------------------- |
| 清單   | `GetXxxsQuery`       |
| 詳細   | `GetXxxDetailQuery`  |
| 下拉選單 | `GetXxxOptionsQuery` |
| 搜尋   | `SearchXxxsQuery`    |

### 7.3 Handler

Handler 規則：

1. Handler 放在 `Application`。
2. Handler 可協調 Use Case。
3. Handler 可呼叫 Application 定義的 Interface。
4. Handler 不得知道 MVC、ViewModel、Controller、TempData。
5. Handler 不得直接依賴 Infrastructure 實作類別。
6. Handler 不得直接處理 HTML 或 Razor。
7. Handler 不得讀取 `HttpContext`，若需要目前使用者資訊，必須透過 Application 抽象，例如 `IUser` 或既有介面。

### 7.4 Validator

FluentValidation 規則：

| 驗證類型          | 放置位置                    |
| ------------- | ----------------------- |
| Use Case 輸入規則 | `Application` Validator |
| UI 顯示格式或必填提示  | MVC ViewModel           |
| 不變的業務規則       | `Domain`                |

不可只在 MVC ViewModel 驗證業務規則。Application 必須保護 Use Case 邊界。

## 8. Application 層規則

Application 是 Use Case 中心，不是 MVC 輔助層。

可放：

| 類型        | 說明                                   |
| --------- | ------------------------------------ |
| Command   | 寫入操作                                 |
| Query     | 讀取操作                                 |
| Handler   | Use Case 執行流程                        |
| DTO       | Use Case 輸出資料                        |
| Validator | Use Case 輸入驗證                        |
| Interface | Infrastructure 抽象                    |
| Behaviour | Validation、Logging、Performance 等橫切關注 |

禁止：

| 禁止項目            | 說明                     |
| --------------- | ---------------------- |
| `Controller`    | Presentation concern   |
| `IActionResult` | MVC concern            |
| `ViewResult`    | MVC concern            |
| `HttpContext`   | Web concern            |
| `TempData`      | MVC concern            |
| `DbContext` 實作  | Infrastructure concern |
| Razor 或 HTML    | Presentation concern   |
| 外部 API SDK 實作   | Infrastructure concern |

## 9. Domain 層規則

Domain 是最核心層。

可放：

| 類型            | 說明                    |
| ------------- | --------------------- |
| Entity        | 有識別性的業務物件             |
| Value Object  | 以值定義的不變物件             |
| Domain Event  | 領域事件                  |
| Enumeration   | 強型別列舉                 |
| Domain Method | 與 Entity 狀態一致性高度相關的行為 |

禁止：

| 禁止項目                   | 說明                        |
| ---------------------- | ------------------------- |
| EF Core attribute      | 不讓 Domain 依賴 ORM          |
| MVC attribute          | 不讓 Domain 依賴 Presentation |
| JSON 序列化設定             | 不讓 Domain 依賴傳輸格式          |
| 資料庫欄位格式                | 屬 Infrastructure          |
| UI 顯示文字                | 屬 Web                     |
| Application DTO        | 屬 Application             |
| Infrastructure Service | 屬 Infrastructure          |

Domain 錯誤建議使用穩定代碼或語意型例外，不要把中文 UI 訊息直接寫入 Domain。

## 10. Infrastructure 層規則

Infrastructure 負責技術實作。

可放：

| 類型                        | 說明                            |
| ------------------------- | ----------------------------- |
| EF Core DbContext         | `IApplicationDbContext` 實作    |
| Entity Configuration      | `IEntityTypeConfiguration<T>` |
| Identity                  | 使用者、角色、權限技術實作                 |
| External Service          | Email、Storage、第三方 API         |
| File Service              | 檔案系統或物件儲存實作                   |
| Repository Implementation | 若專案既有使用 Repository            |

禁止：

| 禁止項目           | 說明                           |
| -------------- | ---------------------------- |
| MVC Controller | Presentation concern         |
| Razor View     | Presentation concern         |
| ViewModel      | Presentation concern         |
| Use Case 流程    | Application concern          |
| 業務規則           | Domain 或 Application concern |
| 直接回傳 UI 訊息     | Web concern                  |

EF Core 規則：

1. 不得將 `DbContext` 暴露給 `Web`。
2. 查詢優先使用 projection。
3. Read-only 查詢優先使用 `AsNoTracking()`。
4. 避免 N+1 query。
5. 不要過早 `.ToList()`。
6. 不要在迴圈中逐筆查詢資料庫。
7. Raw SQL 必須參數化。
8. Migration 只有在任務明確要求或模型變更必要時才新增。

## 11. Web 層 DI 與 Program.cs 規則

MVC Web 應使用：

```csharp
builder.Services.AddControllersWithViews();
```

路由應使用 MVC route：

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

若專案仍保留 API Controller，可同時使用 attribute routing：

```csharp
app.MapControllers();
```

DI 規則：

| 項目                | 規則                                                            |
| ----------------- | ------------------------------------------------------------- |
| Application 註冊    | 由 `Application` 提供擴充方法                                        |
| Infrastructure 註冊 | 由 `Infrastructure` 提供擴充方法                                     |
| Web Controller    | 只注入 `ISender` 或 Web concern service                           |
| Options           | 在 composition root 綁定                                         |
| Secret            | 不得硬編碼                                                         |
| Health Check      | 可放在 Web 或 Infrastructure 註冊，不得依賴 Aspire                       |
| Telemetry         | 可使用標準 ASP.NET Core、OpenTelemetry 或既有設定，不得依賴 `ServiceDefaults` |

禁止在 Controller 中手動建立 Infrastructure 實作：

```csharp
// 禁止
var service = new EmailService();
```

禁止重新引入 Aspire：

```csharp
// 禁止
builder.AddServiceDefaults();
app.MapDefaultEndpoints();
```

若專案已移除 Aspire，`Program.cs` 不得再出現：

```csharp
AddServiceDefaults
MapDefaultEndpoints
Aspire
DistributedApplication
```

## 12. 資料流

標準 MVC 請求資料流：

```text
Browser
  -> Web MVC Controller
  -> ISender.Send(Command 或 Query)
  -> Application Handler
  -> Domain Entity / Value Object / Domain Rule
  -> Application Interface
  -> Infrastructure EF Core / External Service
  -> Database / External System
  -> Application DTO
  -> Web ViewModel
  -> Razor View
  -> Browser
```

禁止資料流：

```text
Controller -> DbContext -> Database
Controller -> Infrastructure Implementation
View -> Domain Entity
Application -> Web
Domain -> Infrastructure
Domain -> Web
任一層 -> Aspire Hosting
```

## 13. 常見需求放置位置

| 需求              | 正確位置                                           | 說明                           |
| --------------- | ---------------------------------------------- | ---------------------------- |
| 新增 MVC 頁面       | `Web/Controllers`、`Web/Views`、`Web/ViewModels` | 只處理 UI                       |
| 新增查詢功能          | `Application` Query + Handler                  | Controller 呼叫 Query          |
| 新增寫入功能          | `Application` Command + Handler + Validator    | Controller 呼叫 Command        |
| 新增 Entity       | `Domain`                                       | 若是核心業務概念                     |
| 新增資料表設定         | `Infrastructure`                               | EF Core Configuration        |
| 新增 Email 發送     | `Application` 介面、`Infrastructure` 實作           | Web 不直接發送                    |
| 新增檔案儲存          | `Application` 介面、`Infrastructure` 實作           | 路徑與儲存細節不進 Domain             |
| 新增 UI 文字        | `Web`                                          | 不進 Domain 或 Application      |
| 新增 Health Check | `Web` 或 `Infrastructure`                       | 不使用 Aspire `ServiceDefaults` |
| 新增測試資源          | 對應測試專案                                         | 不使用 `TestAppHost`            |

## 14. Mapping 規則

Mapping 必須尊重邊界。

| From                             | To              | 位置                                    |
| -------------------------------- | --------------- | ------------------------------------- |
| MVC ViewModel                    | Command         | Controller 或 Web mapping              |
| Query DTO                        | MVC ViewModel   | Controller 或 Web mapping              |
| Domain Entity                    | Application DTO | Application                           |
| Infrastructure Entity Projection | Application DTO | Application Query 或 Infrastructure 實作 |
| Domain Entity                    | MVC ViewModel   | 不建議直接 mapping                         |

規則：

1. 優先沿用專案既有 mapping 工具。
2. 若專案使用 AutoMapper，遵守既有 Profile 與 projection 慣例。
3. 不得為單一小需求引入新的 mapping 套件。
4. 不得把 MVC ViewModel 放到 Application。
5. 不得把 Application DTO 當作 Razor 表單模型，除非專案既有慣例已如此設計且沒有額外 UI 欄位需求。

## 15. 安全規則

MVC 必須注意下列安全要求：

| 風險          | 規則                                           |
| ----------- | -------------------------------------------- |
| CSRF        | POST、PUT、DELETE 表單必須使用 Anti-Forgery          |
| Overposting | 不得直接 bind Domain Entity 或 EF Entity          |
| XSS         | Razor 預設編碼，不得任意使用 `Html.Raw`                 |
| 機敏資料        | 不得輸出 token、password、secret、connection string |
| 錯誤訊息        | 不得把 exception detail 直接顯示給使用者                |
| 授權          | 需要登入或角色限制的頁面必須使用 `[Authorize]` 或既有 policy    |
| 檔案上傳        | 必須驗證副檔名、大小、Content-Type、儲存路徑                 |
| Redirect    | 避免 open redirect，外部 URL 必須驗證                 |
| ModelState  | 驗證失敗必須回到原 View 並顯示錯誤                         |

禁止：

```csharp
// 禁止：直接綁定 Entity，容易 overposting
public async Task<IActionResult> Edit(TodoItem entity)
```

建議：

```csharp
public async Task<IActionResult> Edit(EditTodoItemViewModel viewModel)
```

## 16. 測試規則

測試放置原則：

| 測試類型                 | 專案                                      |
| -------------------- | --------------------------------------- |
| Domain 規則            | `tests/Domain.UnitTests`                |
| Application Handler  | `tests/Application.UnitTests`           |
| Application 流程與資料庫整合 | `tests/Application.FunctionalTests`     |
| Infrastructure 實作    | `tests/Infrastructure.IntegrationTests` |
| MVC 頁面與端到端流程         | `tests/Web.AcceptanceTests`             |

變更後至少執行：

```bash
dotnet build
dotnet test
```

若只改某層，可優先執行對應測試，但最終仍建議執行完整測試。

範例：

```bash
dotnet test tests/Domain.UnitTests
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests
dotnet test tests/Web.AcceptanceTests
```

若測試無法執行，Codex 必須在回覆中明確列出：

| 項目      | 必填               |
| ------- | ---------------- |
| 未執行命令   | 例如 `dotnet test` |
| 未執行原因   | 例如環境缺少 Docker    |
| 風險      | 哪些功能未被驗證         |
| 建議補驗證方式 | 使用者可手動執行的命令      |

## 17. CodeGraph 使用規則

CodeGraph 用於協助 Codex 理解跨檔案、跨類別與跨分層的程式碼關係。它是架構探索與影響分析工具，不是所有任務都必須使用，也不能取代實際檔案確認、建置與測試。

### 17.1 優先使用 CodeGraph 的時機

當任務涉及下列情況時，Codex 應優先使用 CodeGraph 的 `codegraph_explore` 進行初步探索：

| 場景 | 使用目的 |
| --- | --- |
| 專案架構與模組關係 | 理解 `Web`、`Application`、`Domain`、`Infrastructure` 與 `Shared` 的實際互動 |
| HTTP Request 或功能流程 | 追蹤 Controller、Command／Query、Handler、Domain、Infrastructure 到資料庫或外部服務的完整資料流 |
| 呼叫鏈與符號關聯 | 找出某個類別、方法、介面或事件的呼叫端、被呼叫端與實作類別 |
| 跨層功能修改 | 確認變更是否同時影響 MVC、Use Case、Domain Rule、EF Core 或外部服務 |
| 影響範圍分析 | 評估修改、刪除、重新命名或更換介面後可能受影響的功能與測試 |
| 不熟悉的功能或模組 | 在逐檔閱讀前快速找出主要進入點、核心符號及相關檔案 |
| 程式碼審查 | 補充理解變更涉及的呼叫關係、相依性與可能遺漏的影響範圍 |
| 架構違規調查 | 尋找 Controller 直接存取 DbContext、Application 依賴 Infrastructure 等違反分層的關係 |

使用 CodeGraph 時應遵守：

1. 優先使用 `codegraph_explore` 找出相關符號、檔案、呼叫鏈與相依關係。
2. 不得只根據檔名或推測描述架構，必須以 CodeGraph 結果或實際程式碼為依據。
3. CodeGraph 找到目標後，修改前仍須直接讀取相關檔案，確認目前工作樹中的最新內容。
4. CodeGraph 結果不足時，可再使用 `rg`、檔案搜尋或直接讀取檔案補充，不必重複進行無目的的全專案搜尋。
5. CodeGraph 索引過期時，應先更新索引或明確說明限制，不得把過期結果當作目前程式碼狀態。
6. 只有在工具活動中實際呼叫 CodeGraph，才可在回覆中聲稱已使用 CodeGraph。

### 17.2 不需要使用 CodeGraph 的場景

下列任務通常可直接使用對應工具，不必為了形式強制呼叫 CodeGraph：

| 場景 | 建議作法 |
| --- | --- |
| 使用者已指定明確檔案與修改位置 | 直接讀取並修改該檔案，必要時再搜尋局部參照 |
| README、Markdown、註解或文字修正 | 直接編輯文件或指定文字 |
| `appsettings*.json`、`launchSettings.json`、`.editorconfig` 等設定檔檢查 | 直接讀取設定檔並依任務處理，但仍須遵守禁止修改區域 |
| 建置、測試、格式化或靜態分析 | 直接執行 `dotnet build`、`dotnet test`、formatter 或既有分析工具 |
| 編譯錯誤、測試失敗或執行時例外 | 先檢查錯誤訊息、stack trace、測試輸出及直接相關檔案 |
| NuGet 套件版本或弱點處理 | 檢查 `.csproj`、`Directory.Packages.props`、restore 與套件報告；只有分析受影響呼叫範圍時才使用 CodeGraph |
| 小範圍格式、拼字、命名或單一方法內部調整 | 直接修改並執行相關測試；若重新命名公開符號或介面，則應改用 CodeGraph 做影響分析 |
| Razor、CSS、JavaScript、圖片、字型或其他靜態資源調整 | 直接檢查頁面與資源；只有需要追蹤後端資料來源或跨檔案流程時才使用 CodeGraph |
| Git 狀態、diff、commit 範圍與變更清單 | 直接使用 Git；需要理解變更符號的跨層影響時再搭配 CodeGraph |
| 產生或更新 Migration | 依模型差異與 EF Core 工具處理；CodeGraph 僅用於確認相關程式碼影響，不取代 migration 檢查 |

### 17.3 CodeGraph 不可用或資料不足時

若 CodeGraph MCP 未載入、工具呼叫失敗、專案尚未建立索引或索引資料不足，Codex 應：

1. 簡要說明 CodeGraph 不可用或結果不足的原因。
2. 改用 `rg`、檔案搜尋、直接讀取檔案與 Git 等既有工具完成任務。
3. 不得因 CodeGraph 不可用而停止可安全完成的工作。
4. 不得假裝已使用 CodeGraph，或虛構不存在的符號、檔案與呼叫關係。

### 17.4 使用原則摘要

| 問題 | 決策 |
| --- | --- |
| 是否每個 prompt 都必須使用 CodeGraph？ | 否，依任務性質判斷 |
| 何時優先使用？ | 架構、流程、呼叫鏈、相依性、影響範圍與跨層修改 |
| 何時直接讀檔？ | 已知檔案、小範圍修改、設定、文件、錯誤輸出與靜態資源 |
| CodeGraph 是否取代 `rg` 或直接讀檔？ | 否，三者應依需求互補 |
| CodeGraph 是否取代 build 與 test？ | 否，所有變更仍須以實際建置與測試驗證 |

## 18. Codex 實作 SOP

每次修改前必須依序執行下列思考流程。

### 18.1 讀取現況

1. 先依第 17 節判斷是否需要使用 CodeGraph。
2. 涉及架構、流程、呼叫鏈或跨層影響時，優先使用 `codegraph_explore` 找出相關符號與檔案。
3. 搜尋並讀取相關 Controller、View、ViewModel。
4. 搜尋並讀取相關 Command、Query、Handler、Validator。
5. 搜尋並讀取 Domain Entity、Value Object 與現有方法。
6. 搜尋並讀取 Infrastructure 實作與 DbContext 設定。
7. 檢查既有命名、資料夾、測試風格。
8. 檢查是否仍殘留 Aspire 設定，但不得在非相關任務中任意刪除。

### 18.2 判斷放置位置

| 問題                     | 放置層              |
| ---------------------- | ---------------- |
| 是核心業務概念嗎               | `Domain`         |
| 是 Use Case 流程嗎         | `Application`    |
| 是資料庫或外部服務實作嗎           | `Infrastructure` |
| 是畫面、表單、路由、Controller 嗎 | `Web`            |
| 是跨專案且不含業務規則的共用內容嗎      | `Shared`         |
| 是測試資料、測試替身或測試流程嗎       | 對應測試專案           |

### 18.3 實作順序

1. 先補 Domain 行為或規則。
2. 再補 Application Command 或 Query。
3. 再補 Validator。
4. 再補 Infrastructure 實作或 EF Core 設定。
5. 最後補 Web Controller、ViewModel、View。
6. 補上對應測試。
7. 執行 build 與 test。
8. 回報變更摘要、測試結果、風險。

## 19. Aspire 移除後的特別規則

本專案後續不使用 Aspire。

Codex 不得新增或恢復：

| 禁止項目                     | 說明                            |
| ------------------------ | ----------------------------- |
| `src/AppHost`            | 不再使用 Aspire AppHost           |
| `src/ServiceDefaults`    | 不再使用 Aspire ServiceDefaults   |
| `tests/TestAppHost`      | 不再使用 Aspire 測試 Host           |
| `Aspire.Hosting`         | 不得新增 Aspire Hosting 套件        |
| `Aspire.*` 套件            | 除非任務明確要求評估或移除                 |
| `DistributedApplication` | 不得新增 Aspire 編排                |
| `AddServiceDefaults()`   | 不得使用 ServiceDefaults          |
| `MapDefaultEndpoints()`  | 不得使用 ServiceDefaults endpoint |
| Aspire Dashboard         | 不得加入 Aspire Dashboard 設定      |

若任務是「移除 Aspire」，Codex 必須依序檢查：

1. `.sln` 是否仍引用 `AppHost`、`ServiceDefaults`、`TestAppHost`。
2. `src/Web` 是否仍呼叫 `AddServiceDefaults()` 或 `MapDefaultEndpoints()`。
3. `Directory.Packages.props` 是否仍有 Aspire 套件版本。
4. 各 `.csproj` 是否仍引用 Aspire 套件。
5. `launchSettings.json` 是否仍有 Aspire 啟動設定。
6. 測試是否仍依賴 `TestAppHost`。
7. README 或文件是否仍要求使用 Aspire 啟動。
8. CI/CD 是否仍 build 或 test Aspire 專案。

移除 Aspire 後，若需要替代能力，應使用下列方式：

| 原 Aspire 功能                  | 替代方式                                                  |
| ---------------------------- | ----------------------------------------------------- |
| AppHost 編排                   | Docker Compose、Visual Studio 多啟動專案、手動啟動               |
| ServiceDefaults Health Check | 在 `Web` 或 `Infrastructure` 明確註冊 Health Check          |
| Service Discovery            | 使用設定檔、環境變數、反向代理或 DNS                                  |
| Telemetry                    | 使用標準 OpenTelemetry 或既有 logging 設定                     |
| TestAppHost                  | 使用 Testcontainers、WebApplicationFactory 或測試專案 fixture |

## 20. 禁止修改區域

除非任務明確要求，不得修改：

| 區域                         | 原因            |
| -------------------------- | ------------- |
| `Directory.Packages.props` | 影響全域套件版本      |
| `global.json`              | 影響 SDK        |
| Dockerfile / compose       | 影響部署          |
| CI/CD workflow             | 影響建置流程        |
| `appsettings*.json`        | 可能影響環境設定      |
| Migration                  | 會改變資料庫 schema |
| `.editorconfig`            | 影響全專案格式       |

若任務是移除 Aspire，允許修改：

| 區域                         | 條件                                                |
| -------------------------- | ------------------------------------------------- |
| `.sln`                     | 移除 Aspire 專案引用                                    |
| `Directory.Packages.props` | 移除未使用的 Aspire 套件版本                                |
| `.csproj`                  | 移除 Aspire package 或 project reference             |
| `Program.cs`               | 移除 `AddServiceDefaults()`、`MapDefaultEndpoints()` |
| 測試專案                       | 移除 `TestAppHost` 依賴                               |
| 文件                         | 移除 Aspire 啟動說明                                    |

## 21. Pull Request 回覆格式

完成任務後，Codex 回覆必須包含：

```markdown
## 變更摘要

| 類型 | 檔案 | 說明 |
| --- | --- | --- |
| Web | `...` | ... |
| Application | `...` | ... |
| Domain | `...` | ... |
| Infrastructure | `...` | ... |
| Test | `...` | ... |
| Config | `...` | ... |

## 架構判斷

說明本次變更為何放在這些層，並確認沒有破壞依賴方向。

## Aspire 狀態

說明本次是否涉及 Aspire 移除，並列出是否仍有殘留引用。

## 測試結果

| 命令 | 結果 |
| --- | --- |
| `dotnet build` | 通過或失敗 |
| `dotnet test` | 通過或失敗 |

## 風險與後續建議

列出尚未驗證、可能影響、建議補測項目。
```

## 22. 最重要規則

1. `Domain` 不依賴任何外層。
2. `Application` 不依賴 `Infrastructure` 或 `Web`。
3. `Infrastructure` 實作 Application 定義的抽象。
4. `Web` 是 MVC Presentation，只呼叫 Application，不直接處理資料庫。
5. Controller 必須薄，Use Case 必須在 Application。
6. ViewModel 屬於 Web，DTO 屬於 Application，Entity 屬於 Domain。
7. EF Core 屬於 Infrastructure，不得出現在 Controller。
8. 專案已移除 Aspire，不得重新引入 Aspire。
9. 不得為了快速完成破壞 Clean Architecture。
10. 不確定時先搜尋既有慣例，再做最小且可維護的修改。
11. 架構、流程、呼叫鏈與影響分析應優先使用 CodeGraph；已知檔案與小範圍任務不得為了形式強制使用。
