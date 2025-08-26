# CleanArchitecture.Northwind

The project was generated using the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture.Northwind) version 8.0.6.

## Build

Run `dotnet build -tl` to build the solution.

## Run

To run the web application:

```bash
cd .\src\Web\
dotnet watch run
```

Navigate to https://localhost:5001. The application will automatically reload if you change any of the source files.

## Code Styles & Formatting

The template includes [EditorConfig](https://editorconfig.org/) support to help maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. The **.editorconfig** file defines the coding styles applicable to this solution.

## Code Scaffolding

The template includes support to scaffold new commands and queries.

Start in the `.\src\Application\` folder.

Create a new command:

```
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

Create a new query:

```
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

If you encounter the error *"No templates or subcommands found matching: 'ca-usecase'."*, install the template and try again:

```bash
dotnet new install Clean.Architecture.Solution.Template::8.0.6
```

## Test

The solution contains unit, integration, and functional tests.

To run the tests:
```bash
dotnet test
```

## Help
To learn more about the template go to the [project website](https://github.com/jasontaylordev/CleanArchitecture). Here you can find additional guidance, request new features, report a bug, and discuss the template with other users.

## 設置 Swagger 起啟版本

至 `CleanArchitecture.Northwind\src\WebAPI\StartupExtensions\SwaggerExtension.cs` 中的 `UseCustomizedSwagger`，將要預設的起始版本放在前面

## DataBase Migration

To run the Src Folder

```bash
cd .\src\
```

### 目前資料庫的 migration 狀態

```bash
dotnet ef migrations list --project Infrastructure --startup-project Mvc --context ApplicationDbContext
```

### 執行資料庫遷移

```bash
dotnet ef migrations add [MigrationName] --project Infrastructure --startup-project Mvc --context ApplicationDbContext --output-dir Data\Migrations

dotnet ef database update --project Infrastructure --startup-project Mvc --context ApplicationDbContext
```

### 執行資料庫遷移回滾

尚未執行 `database update`

```bash
dotnet ef migrations remove --project Infrastructure --startup-project Mvc --context ApplicationDbContext
```

如果已經執行了 `database update`

```bash
dotnet ef database update Previous --project Infrastructure --startup-project Mvc --context ApplicationDbContext

dotnet ef migrations remove --project Infrastructure --startup-project Mvc --context ApplicationDbContext
```

## 角色 

| 角色 \ 模組             | Customers     | **SalesOrders**                                           | Products                          | Categories | Suppliers | Employees | Territories / Regions | Shippers | CustDemo | EmpTerr | **Audit** |
| ------------------- | ------------- | --------------------------------------------------------- | --------------------------------- | ---------- | --------- | --------- | --------------------- | -------- | -------- | ------- | --------- |
| **Administrator**   | CRUD          | CRUD（刪除=軟刪）                                               | CRUD（含售價/成本/供應商連結）                | CRUD       | CRUD      | CRUD      | CRUD                  | CRUD     | CRUD     | CRUD    | **R**     |
| **Sales**           | C(limited), R | **CRU(own), No D**                                        | R                                 | R          | R         | R         | R                     | R        | R        | R       | –         |
| **Warehouse**       | R             | **U(僅 ShippedDate/ShipVia/Freight/ShipName/ShipAddress)** | R                                 | –          | –         | –         | R                     | R（可選 U）  | –        | –       | –         |
| **Purchase**        | R             | R                                                         | **U(僅成本、供應商連結)**（是否允許 C：建議關閉或走審核） | R          | **CRUD**  | –         | –                     | –        | –        | –       | –         |
| **Finance**         | R             | R                                                         | R                                 | R          | R         | –         | R                     | R        | R        | R       | （可選 R\*)  |
| **CustomerService** | **CRUD**      | R                                                         | R                                 | R          | R         | –         | –                     | –        | **CRUD** | –       | –         |


## Sonarqube

### 1 在 SonarQube 建立專案並產生 Token

1. 在 SonarQube 的 UI 建立一個 Project（取得 **Project Key**）。
2. 到 **My Account → Security** 產生一組 **User Token**（建議無到期或定期輪替）。 ([docs.sonarsource.com][1])

> 之後所有掃描都用這個 Token 作為認證。

---

### 2 安裝 SonarScanner for .NET

在開發機或 CI runner 上安裝 dotnet 全球工具版掃描器：

```bash
dotnet tool install --global dotnet-sonarscanner
```

SonarScanner for .NET 是針對使用 `dotnet/MSBuild` 的專案所推薦的掃描方式；掃描時會用到 **begin → build/test → end** 這段式命令。 ([docs.sonarsource.com][2])

---

### 3 本機最小可行範例（含測試覆蓋率）

假設你在方案根目錄（含 `YourSolution.sln`）執行，且你的 SonarQube 在 `http://sonarqube:9000/`：

**Windows PowerShell / Linux Bash 通用（核心步驟）**

```bash
# 3-1 掃描開始：設定必要屬性
dotnet sonarscanner begin \
  /k:"your-project-key" \
  /n:"Your Display Name" \
  /v:"1.0.0" \
  /d:sonar.host.url="http://sonarqube:9000" \
  /d:sonar.token="YOUR_TOKEN" \
  /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \
  /d:sonar.cs.vstest.reportsPaths="**/*.trx"

# 3-2 編譯
dotnet build --no-incremental

# 3-3 單元測試 + 產生 OpenCover 覆蓋率 & VSTest TRX
# 若測試專案使用 coverlet.msbuild，以下參數即可產生 OpenCover 與 TRX
dotnet test tests/Your.Tests/Your.Tests.csproj \
  --logger "trx;LogFileName=test.trx" \
  /p:CollectCoverage=true \
  /p:CoverletOutput=./TestResults/coverage \
  /p:CoverletOutputFormat=opencover

# 3-4 結束並上傳分析資料
dotnet sonarscanner end /d:sonar.token="YOUR_TOKEN"
```

說明：

* `begin` 期會註冊 MSBuild hook；`end` 期會收集 build、test、coverage 成果並上傳。 ([docs.sonarsource.com][3])
* SonarQube **不會自行產生覆蓋率報告**，你必須用第三方工具（如 coverlet）產生，再用 `sonar.cs.opencover.reportsPaths` 等屬性告訴掃描器路徑。 ([docs.sonarsource.com][4])
* 使用 coverlet 時，請輸出 **OpenCover** 格式，配合 `sonar.cs.opencover.reportsPaths` 最穩定。 ([docs.sonarsource.com][4], [Sonar Community][5])