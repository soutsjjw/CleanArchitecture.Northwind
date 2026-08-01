# CleanArchitecture.Northwind

Northwind 範例系統採用 Clean Architecture、CQRS、MediatR、EF Core 與 ASP.NET Core MVC 建置。它以 [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) 10.8.0 為基礎，將 Northwind 的產品、庫存、訂單、帳號與權限等業務流程落實於分層架構中。

## 快速開始

建置整個方案：

```bash
dotnet build
```

啟動 Web 應用程式：

```bash
dotnet run --project .\src\Web
```

執行測試：

```bash
dotnet test
```

## 架構導覽

系統的主要請求路徑如下：

```text
Browser → MVC Controller → ISender → Application Command／Query Handler
        → Domain → Infrastructure → Database
```

| 分層 | 位置 | 責任 |
| --- | --- | --- |
| Domain | `src/Domain` | Northwind 的實體、值物件與不變業務規則；不依賴外層技術。 |
| Application | `src/Application` | CQRS 用例、Handler、DTO、Validator 與 MediatR pipeline。 |
| Infrastructure | `src/Infrastructure` | EF Core、Identity、資料存取與外部服務等 Application 抽象的實作。 |
| Web | `src/Web` | ASP.NET Core MVC 的 Controller、ViewModel、Razor View、路由與 DI 組合。 |
| Shared | `src/Shared` | 不含特定業務規則的跨專案共用內容。 |
| Tests | `tests` | Domain、Application、Infrastructure 與功能流程的驗證。 |

### 分層規則

- `Domain` 不依賴任何外層。
- `Application` 僅依賴 `Domain`，以抽象描述需要的能力。
- `Infrastructure` 實作 Application 的抽象，不承擔 MVC 或用例流程。
- `Web` 透過 `ISender` 呼叫用例；Controller 不直接操作 `DbContext` 或 Infrastructure 實作。
- Razor View 使用 ViewModel 或 Application DTO，不直接使用 Domain Entity。

## 新成員建議閱讀順序

1. [CONTEXT.md](CONTEXT.md)：Northwind 領域詞彙與統一用語。
2. `docs/adr/`：了解已記錄的架構取捨。
3. `src/Web/Program.cs`：網站啟動、服務註冊與路由入口。
4. `src/Web/Controllers/OrdersController.cs` 與 `ProductsController.cs`：MVC 如何將 HTTP 請求轉為用例。
5. `src/Application/Common/Behaviours/`：驗證、授權、記錄、效能與例外等橫切關注點。
6. `src/Application/Features/Orders/`：Command、Query、Handler、DTO 與 Validator 的典型用例結構。
7. `src/Domain/Entities/Order.cs`、`Product.cs`、`Customer.cs`：核心業務模型。
8. `src/Infrastructure/Data/ApplicationDbContext.cs`：EF Core 與資料持久化的邊界。
9. `tests/Application.UnitTests/`：Application 用例的測試範例。

## 常見功能位置

| 需求 | 主要位置 |
| --- | --- |
| 產品、分類與庫存 | `src/Application/Features/Products`、`Categories`、`Inventory` |
| 訂單查詢與操作 | `src/Application/Features/Orders`、`src/Web/Controllers/OrdersController.cs` |
| 帳號、會員、角色與雙因素驗證 | `src/Application/Features/Account`、`Member`、`Role`、`Totp` |
| 領域資料模型 | `src/Domain/Entities` |
| 資料庫與初始化 | `src/Infrastructure/Data`、`sql/instnwnd.sql` |
| MVC 畫面與輸入模型 | `src/Web/Controllers`、`Views`、`ViewModels` |

## 修改前先注意

下列區域連接多個用例或技術邊界，變更前應先找出呼叫端並執行相應測試：

- `src/Web/Controllers/AccountController.cs`
- `src/Web/Controllers/OrdersController.cs`
- `src/Web/Controllers/ProductsController.cs`
- `src/Infrastructure/DependencyInjection.cs`
- `src/Infrastructure/Data/ApplicationDbContextInitialiser.cs`
- `src/Infrastructure/Identity/IdentityService.cs`

## Code Styles & Formatting

專案包含 [EditorConfig](https://editorconfig.org/) 設定，以維持不同編輯器間的一致程式碼風格。

## Code Scaffolding

此方案支援建立新的 Command 與 Query。請先切換至 `src/Application/`。

建立 Command：

```bash
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

建立 Query：

```bash
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

若出現 `No templates or subcommands found matching: 'ca-usecase'.`，請先安裝範本：

```bash
dotnet new install Clean.Architecture.Solution.Template::10.8.0
```

## 延伸資源

如需了解原始範本，請參閱 [Clean Architecture project website](https://cleanarchitecture.jasontaylor.dev)。
