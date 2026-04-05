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

## .NET 10 現況

- 專案已正式切換為 `net10.0`
- `global.json` 已固定使用 `.NET 10 SDK`
- 中央套件版本已收斂為 `.NET 10` 對應版本

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

## Package List

```text
專案 'Application' 有以下套件參考
   [net8.0]: 
   最上層套件                                                 已要求      已解析   
   > Ardalis.GuardClauses                                4.6.0    4.6.0 
   > FluentValidation.DependencyInjectionExtensions      11.9.2   11.9.2
   > Mapster                                             10.0.6   10.0.6
   > Mapster.DependencyInjection                         10.0.6   10.0.6
   > Microsoft.EntityFrameworkCore                       8.0.23   8.0.23
   > Otp.NET                                             1.4.0    1.4.0 
   > QRCoder                                             1.6.0    1.6.0 

   可轉移的套件                                                       已解析   
   > FluentValidation                                           11.9.2
   > Mapster.Core                                               10.0.6
   > MediatR                                                    12.4.0
   > MediatR.Contracts                                          2.0.1 
   > Microsoft.AspNetCore.Cryptography.Internal                 8.0.8 
   > Microsoft.AspNetCore.Cryptography.KeyDerivation            8.0.8 
   > Microsoft.AspNetCore.Identity.EntityFrameworkCore          8.0.8 
   > Microsoft.EntityFrameworkCore.Abstractions                 8.0.23
   > Microsoft.EntityFrameworkCore.Analyzers                    8.0.23
   > Microsoft.EntityFrameworkCore.Relational                   8.0.8 
   > Microsoft.Extensions.Caching.Abstractions                  8.0.0 
   > Microsoft.Extensions.Caching.Memory                        8.0.1 
   > Microsoft.Extensions.Configuration.Abstractions            8.0.0 
   > Microsoft.Extensions.DependencyInjection                   8.0.1 
   > Microsoft.Extensions.DependencyInjection.Abstractions      9.0.0 
   > Microsoft.Extensions.Identity.Core                         8.0.8 
   > Microsoft.Extensions.Identity.Stores                       8.0.8 
   > Microsoft.Extensions.Logging                               8.0.1 
   > Microsoft.Extensions.Logging.Abstractions                  8.0.2 
   > Microsoft.Extensions.Options                               8.0.2 
   > Microsoft.Extensions.Primitives                            8.0.0 

專案 'Domain' 有以下套件參考
   [net8.0]: 
   最上層套件                                                    已要求      已解析   
   > MediatR                                                12.4.0   12.4.0
   > Microsoft.AspNetCore.Identity.EntityFrameworkCore      8.0.8    8.0.8 

   可轉移的套件                                                       已解析  
   > MediatR.Contracts                                          2.0.1
   > Microsoft.AspNetCore.Cryptography.Internal                 8.0.8
   > Microsoft.AspNetCore.Cryptography.KeyDerivation            8.0.8
   > Microsoft.EntityFrameworkCore                              8.0.8
   > Microsoft.EntityFrameworkCore.Abstractions                 8.0.8
   > Microsoft.EntityFrameworkCore.Analyzers                    8.0.8
   > Microsoft.EntityFrameworkCore.Relational                   8.0.8
   > Microsoft.Extensions.Caching.Abstractions                  8.0.0
   > Microsoft.Extensions.Caching.Memory                        8.0.0
   > Microsoft.Extensions.Configuration.Abstractions            8.0.0
   > Microsoft.Extensions.DependencyInjection                   8.0.0
   > Microsoft.Extensions.DependencyInjection.Abstractions      8.0.0
   > Microsoft.Extensions.Identity.Core                         8.0.8
   > Microsoft.Extensions.Identity.Stores                       8.0.8
   > Microsoft.Extensions.Logging                               8.0.0
   > Microsoft.Extensions.Logging.Abstractions                  8.0.0
   > Microsoft.Extensions.Options                               8.0.2
   > Microsoft.Extensions.Primitives                            8.0.0

專案 'Infrastructure' 有以下套件參考
   [net8.0]: 
   最上層套件                                                       已要求       已解析    
   > ClosedXML                                                 0.105.0   0.105.0
   > Dapper                                                    2.1.66    2.1.66 
   > MailKit                                                   4.15.1    4.15.1 
   > Microsoft.AspNetCore.Authentication.JwtBearer             8.0.11    8.0.11 
   > Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore      8.0.8     8.0.8  
   > Microsoft.AspNetCore.Identity.EntityFrameworkCore         8.0.8     8.0.8  
   > Microsoft.EntityFrameworkCore.Design                      8.0.23    8.0.23 
   > Microsoft.EntityFrameworkCore.Relational                  8.0.23    8.0.23 
   > Microsoft.EntityFrameworkCore.SqlServer                   8.0.23    8.0.23 
   > Microsoft.EntityFrameworkCore.Tools                       8.0.23    8.0.23 
   > Serilog                                                   4.1.0     4.1.0  

   可轉移的套件                                                       已解析   
   > Ardalis.GuardClauses                                       4.6.0 
   > Azure.Core                                                 1.38.0
   > Azure.Identity                                             1.11.4
   > BouncyCastle.Cryptography                                  2.6.2 
   > ClosedXML.Parser                                           2.0.0 
   > DocumentFormat.OpenXml                                     3.1.1 
   > DocumentFormat.OpenXml.Framework                           3.1.1 
   > ExcelNumberFormat                                          1.1.0 
   > FluentValidation                                           11.9.2
   > FluentValidation.DependencyInjectionExtensions             11.9.2
   > Humanizer.Core                                             2.14.1
   > Mapster                                                    10.0.6
   > Mapster.Core                                               10.0.6
   > Mapster.DependencyInjection                                10.0.6
   > MediatR                                                    12.4.0
   > MediatR.Contracts                                          2.0.1 
   > Microsoft.AspNetCore.Cryptography.Internal                 8.0.8 
   > Microsoft.AspNetCore.Cryptography.KeyDerivation            8.0.8 
   > Microsoft.Bcl.AsyncInterfaces                              6.0.0 
   > Microsoft.CodeAnalysis.Analyzers                           3.3.3 
   > Microsoft.CodeAnalysis.Common                              4.5.0 
   > Microsoft.CodeAnalysis.CSharp                              4.5.0 
   > Microsoft.CodeAnalysis.CSharp.Workspaces                   4.5.0 
   > Microsoft.CodeAnalysis.Workspaces.Common                   4.5.0 
   > Microsoft.Data.SqlClient                                   5.1.7 
   > Microsoft.Data.SqlClient.SNI.runtime                       5.1.2 
   > Microsoft.EntityFrameworkCore                              8.0.23
   > Microsoft.EntityFrameworkCore.Abstractions                 8.0.23
   > Microsoft.EntityFrameworkCore.Analyzers                    8.0.23
   > Microsoft.Extensions.Caching.Abstractions                  8.0.0 
   > Microsoft.Extensions.Caching.Memory                        8.0.1 
   > Microsoft.Extensions.Configuration.Abstractions            8.0.0 
   > Microsoft.Extensions.DependencyInjection                   8.0.1 
   > Microsoft.Extensions.DependencyInjection.Abstractions      9.0.0 
   > Microsoft.Extensions.DependencyModel                       8.0.2 
   > Microsoft.Extensions.Identity.Core                         8.0.8 
   > Microsoft.Extensions.Identity.Stores                       8.0.8 
   > Microsoft.Extensions.Logging                               8.0.1 
   > Microsoft.Extensions.Logging.Abstractions                  8.0.2 
   > Microsoft.Extensions.Options                               8.0.2 
   > Microsoft.Extensions.Primitives                            8.0.0 
   > Microsoft.Identity.Client                                  4.61.3
   > Microsoft.Identity.Client.Extensions.Msal                  4.61.3
   > Microsoft.IdentityModel.Abstractions                       7.1.2 
   > Microsoft.IdentityModel.JsonWebTokens                      7.1.2 
   > Microsoft.IdentityModel.Logging                            7.1.2 
   > Microsoft.IdentityModel.Protocols                          7.1.2 
   > Microsoft.IdentityModel.Protocols.OpenIdConnect            7.1.2 
   > Microsoft.IdentityModel.Tokens                             7.1.2 
   > Microsoft.SqlServer.Server                                 1.0.0 
   > Microsoft.Win32.SystemEvents                               6.0.0 
   > MimeKit                                                    4.15.1
   > Mono.TextTemplating                                        2.2.1 
   > Otp.NET                                                    1.4.0 
   > QRCoder                                                    1.6.0 
   > RBush.Signed                                               4.0.0 
   > SixLabors.Fonts                                            1.0.0 
   > System.ClientModel                                         1.0.0 
   > System.CodeDom                                             4.4.0 
   > System.Collections.Immutable                               6.0.0 
   > System.Composition                                         6.0.0 
   > System.Composition.AttributedModel                         6.0.0 
   > System.Composition.Convention                              6.0.0 
   > System.Composition.Hosting                                 6.0.0 
   > System.Composition.Runtime                                 6.0.0 
   > System.Composition.TypedParts                              6.0.0 
   > System.Configuration.ConfigurationManager                  6.0.1 
   > System.Diagnostics.DiagnosticSource                        6.0.1 
   > System.Drawing.Common                                      6.0.0 
   > System.Formats.Asn1                                        8.0.2 
   > System.IdentityModel.Tokens.Jwt                            7.1.2 
   > System.IO.Packaging                                        8.0.1 
   > System.IO.Pipelines                                        6.0.3 
   > System.Memory                                              4.5.4 
   > System.Memory.Data                                         1.0.2 
   > System.Numerics.Vectors                                    4.5.0 
   > System.Reflection.Metadata                                 6.0.1 
   > System.Runtime.Caching                                     6.0.0 
   > System.Runtime.CompilerServices.Unsafe                     6.0.0 
   > System.Security.AccessControl                              6.0.0 
   > System.Security.Cryptography.Cng                           5.0.0 
   > System.Security.Cryptography.Pkcs                          8.0.1 
   > System.Security.Cryptography.ProtectedData                 6.0.0 
   > System.Security.Permissions                                6.0.0 
   > System.Security.Principal.Windows                          5.0.0 
   > System.Text.Encoding.CodePages                             6.0.0 
   > System.Text.Encodings.Web                                  6.0.1 
   > System.Text.Json                                           4.7.2 
   > System.Threading.Channels                                  6.0.0 
   > System.Threading.Tasks.Extensions                          4.5.4 
   > System.Windows.Extensions                                  6.0.0 

專案 'Application.FunctionalTests' 有以下套件參考
   [net8.0]: 
   最上層套件                                       已要求       已解析    
   > coverlet.collector                        6.0.2     6.0.2  
   > FluentAssertions                          6.12.0    6.12.0 
   > Microsoft.AspNetCore.Mvc.Testing          8.0.11    8.0.11 
   > Microsoft.Data.SqlClient                  5.2.2     5.2.2  
   > Microsoft.EntityFrameworkCore.Sqlite      8.0.23    8.0.23 
   > Microsoft.NET.Test.Sdk                    17.11.0   17.11.0
   > Moq                                       4.20.70   4.20.70
   > nunit                                     3.14.0    3.14.0 
   > NUnit.Analyzers                           3.9.0     3.9.0  
   > NUnit3TestAdapter                         4.5.0     4.5.0  
   > Respawn                                   6.2.1     6.2.1  
   > Testcontainers.MsSql                      4.0.0     4.0.0  

   可轉移的套件                                                               已解析     
   > Ardalis.GuardClauses                                               4.6.0   
   > Azure.Core                                                         1.38.0  
   > Azure.Identity                                                     1.11.4  
   > BouncyCastle.Cryptography                                          2.6.2   
   > Castle.Core                                                        5.1.1   
   > ClosedXML                                                          0.105.0 
   > ClosedXML.Parser                                                   2.0.0   
   > Dapper                                                             2.1.66  
   > Docker.DotNet                                                      3.125.15
   > Docker.DotNet.X509                                                 3.125.15
   > DocumentFormat.OpenXml                                             3.1.1   
   > DocumentFormat.OpenXml.Framework                                   3.1.1   
   > ExcelNumberFormat                                                  1.1.0   
   > FluentValidation                                                   11.9.2  
   > FluentValidation.DependencyInjectionExtensions                     11.9.2  
   > Humanizer                                                          2.14.1  
   > Humanizer.Core                                                     2.14.1  
   > Humanizer.Core.af                                                  2.14.1  
   > Humanizer.Core.ar                                                  2.14.1  
   > Humanizer.Core.az                                                  2.14.1  
   > Humanizer.Core.bg                                                  2.14.1  
   > Humanizer.Core.bn-BD                                               2.14.1  
   > Humanizer.Core.cs                                                  2.14.1  
   > Humanizer.Core.da                                                  2.14.1  
   > Humanizer.Core.de                                                  2.14.1  
   > Humanizer.Core.el                                                  2.14.1  
   > Humanizer.Core.es                                                  2.14.1  
   > Humanizer.Core.fa                                                  2.14.1  
   > Humanizer.Core.fi-FI                                               2.14.1  
   > Humanizer.Core.fr                                                  2.14.1  
   > Humanizer.Core.fr-BE                                               2.14.1  
   > Humanizer.Core.he                                                  2.14.1  
   > Humanizer.Core.hr                                                  2.14.1  
   > Humanizer.Core.hu                                                  2.14.1  
   > Humanizer.Core.hy                                                  2.14.1  
   > Humanizer.Core.id                                                  2.14.1  
   > Humanizer.Core.is                                                  2.14.1  
   > Humanizer.Core.it                                                  2.14.1  
   > Humanizer.Core.ja                                                  2.14.1  
   > Humanizer.Core.ko-KR                                               2.14.1  
   > Humanizer.Core.ku                                                  2.14.1  
   > Humanizer.Core.lv                                                  2.14.1  
   > Humanizer.Core.ms-MY                                               2.14.1  
   > Humanizer.Core.mt                                                  2.14.1  
   > Humanizer.Core.nb                                                  2.14.1  
   > Humanizer.Core.nb-NO                                               2.14.1  
   > Humanizer.Core.nl                                                  2.14.1  
   > Humanizer.Core.pl                                                  2.14.1  
   > Humanizer.Core.pt                                                  2.14.1  
   > Humanizer.Core.ro                                                  2.14.1  
   > Humanizer.Core.ru                                                  2.14.1  
   > Humanizer.Core.sk                                                  2.14.1  
   > Humanizer.Core.sl                                                  2.14.1  
   > Humanizer.Core.sr                                                  2.14.1  
   > Humanizer.Core.sr-Latn                                             2.14.1  
   > Humanizer.Core.sv                                                  2.14.1  
   > Humanizer.Core.th-TH                                               2.14.1  
   > Humanizer.Core.tr                                                  2.14.1  
   > Humanizer.Core.uk                                                  2.14.1  
   > Humanizer.Core.uz-Cyrl-UZ                                          2.14.1  
   > Humanizer.Core.uz-Latn-UZ                                          2.14.1  
   > Humanizer.Core.vi                                                  2.14.1  
   > Humanizer.Core.zh-CN                                               2.14.1  
   > Humanizer.Core.zh-Hans                                             2.14.1  
   > Humanizer.Core.zh-Hant                                             2.14.1  
   > MailKit                                                            4.15.1  
   > Mapster                                                            10.0.6  
   > Mapster.Core                                                       10.0.6  
   > Mapster.DependencyInjection                                        10.0.6  
   > MediatR                                                            12.4.0  
   > MediatR.Contracts                                                  2.0.1   
   > Microsoft.AspNetCore.Authentication.JwtBearer                      8.0.11  
   > Microsoft.AspNetCore.Cryptography.Internal                         8.0.8   
   > Microsoft.AspNetCore.Cryptography.KeyDerivation                    8.0.8   
   > Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore               8.0.8   
   > Microsoft.AspNetCore.Identity.EntityFrameworkCore                  8.0.8   
   > Microsoft.AspNetCore.Mvc.Razor.Extensions                          6.0.0   
   > Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation                  8.0.11  
   > Microsoft.AspNetCore.Razor.Language                                6.0.24  
   > Microsoft.AspNetCore.TestHost                                      8.0.11  
   > Microsoft.Bcl.AsyncInterfaces                                      7.0.0   
   > Microsoft.Build                                                    17.11.48
   > Microsoft.Build.Framework                                          17.11.48
   > Microsoft.CodeAnalysis.Analyzers                                   3.3.4   
   > Microsoft.CodeAnalysis.AnalyzerUtilities                           3.3.0   
   > Microsoft.CodeAnalysis.Common                                      4.8.0   
   > Microsoft.CodeAnalysis.CSharp                                      4.8.0   
   > Microsoft.CodeAnalysis.CSharp.Features                             4.8.0   
   > Microsoft.CodeAnalysis.CSharp.Workspaces                           4.8.0   
   > Microsoft.CodeAnalysis.Elfie                                       1.0.0   
   > Microsoft.CodeAnalysis.Features                                    4.8.0   
   > Microsoft.CodeAnalysis.Razor                                       6.0.24  
   > Microsoft.CodeAnalysis.Scripting.Common                            4.8.0   
   > Microsoft.CodeAnalysis.Workspaces.Common                           4.8.0   
   > Microsoft.CodeCoverage                                             17.11.0 
   > Microsoft.Data.SqlClient.SNI.runtime                               5.2.0   
   > Microsoft.Data.Sqlite.Core                                         8.0.23  
   > Microsoft.DiaSymReader                                             2.0.0   
   > Microsoft.DotNet.Scaffolding.Shared                                8.0.23  
   > Microsoft.EntityFrameworkCore                                      8.0.23  
   > Microsoft.EntityFrameworkCore.Abstractions                         8.0.23  
   > Microsoft.EntityFrameworkCore.Analyzers                            8.0.23  
   > Microsoft.EntityFrameworkCore.Relational                           8.0.23  
   > Microsoft.EntityFrameworkCore.Sqlite.Core                          8.0.23  
   > Microsoft.EntityFrameworkCore.SqlServer                            8.0.23  
   > Microsoft.Extensions.Caching.Abstractions                          8.0.0   
   > Microsoft.Extensions.Caching.Memory                                8.0.1   
   > Microsoft.Extensions.Configuration                                 8.0.0   
   > Microsoft.Extensions.Configuration.Abstractions                    8.0.0   
   > Microsoft.Extensions.Configuration.Binder                          8.0.2   
   > Microsoft.Extensions.Configuration.CommandLine                     8.0.0   
   > Microsoft.Extensions.Configuration.EnvironmentVariables            8.0.0   
   > Microsoft.Extensions.Configuration.FileExtensions                  8.0.1   
   > Microsoft.Extensions.Configuration.Json                            8.0.1   
   > Microsoft.Extensions.Configuration.UserSecrets                     8.0.1   
   > Microsoft.Extensions.DependencyInjection                           8.0.1   
   > Microsoft.Extensions.DependencyInjection.Abstractions              9.0.0   
   > Microsoft.Extensions.DependencyModel                               8.0.2   
   > Microsoft.Extensions.Diagnostics                                   8.0.1   
   > Microsoft.Extensions.Diagnostics.Abstractions                      8.0.1   
   > Microsoft.Extensions.FileProviders.Abstractions                    8.0.0   
   > Microsoft.Extensions.FileProviders.Physical                        8.0.0   
   > Microsoft.Extensions.FileSystemGlobbing                            8.0.0   
   > Microsoft.Extensions.Hosting                                       8.0.1   
   > Microsoft.Extensions.Hosting.Abstractions                          8.0.1   
   > Microsoft.Extensions.Identity.Core                                 8.0.8   
   > Microsoft.Extensions.Identity.Stores                               8.0.8   
   > Microsoft.Extensions.Logging                                       8.0.1   
   > Microsoft.Extensions.Logging.Abstractions                          8.0.2   
   > Microsoft.Extensions.Logging.Configuration                         8.0.1   
   > Microsoft.Extensions.Logging.Console                               8.0.1   
   > Microsoft.Extensions.Logging.Debug                                 8.0.1   
   > Microsoft.Extensions.Logging.EventLog                              8.0.1   
   > Microsoft.Extensions.Logging.EventSource                           8.0.1   
   > Microsoft.Extensions.Options                                       8.0.2   
   > Microsoft.Extensions.Options.ConfigurationExtensions               8.0.0   
   > Microsoft.Extensions.Primitives                                    8.0.0   
   > Microsoft.Identity.Client                                          4.61.3  
   > Microsoft.Identity.Client.Extensions.Msal                          4.61.3  
   > Microsoft.IdentityModel.Abstractions                               7.1.2   
   > Microsoft.IdentityModel.JsonWebTokens                              7.1.2   
   > Microsoft.IdentityModel.Logging                                    7.1.2   
   > Microsoft.IdentityModel.Protocols                                  7.1.2   
   > Microsoft.IdentityModel.Protocols.OpenIdConnect                    7.1.2   
   > Microsoft.IdentityModel.Tokens                                     7.1.2   
   > Microsoft.NET.StringTools                                          17.11.48
   > Microsoft.NETCore.Platforms                                        1.1.0   
   > Microsoft.SqlServer.Server                                         1.0.0   
   > Microsoft.TestPlatform.ObjectModel                                 17.11.0 
   > Microsoft.TestPlatform.TestHost                                    17.11.0 
   > Microsoft.VisualStudio.Azure.Containers.Tools.Targets              1.21.0  
   > Microsoft.VisualStudio.Web.CodeGeneration                          8.0.23  
   > Microsoft.VisualStudio.Web.CodeGeneration.Core                     8.0.23  
   > Microsoft.VisualStudio.Web.CodeGeneration.Design                   8.0.23  
   > Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore      8.0.23  
   > Microsoft.VisualStudio.Web.CodeGeneration.Templating               8.0.23  
   > Microsoft.VisualStudio.Web.CodeGeneration.Utils                    8.0.23  
   > Microsoft.VisualStudio.Web.CodeGenerators.Mvc                      8.0.23  
   > MimeKit                                                            4.15.1  
   > Mono.TextTemplating                                                2.3.1   
   > NETStandard.Library                                                2.0.0   
   > Newtonsoft.Json                                                    13.0.3  
   > NuGet.Common                                                       6.11.0  
   > NuGet.Configuration                                                6.11.0  
   > NuGet.DependencyResolver.Core                                      6.11.0  
   > NuGet.Frameworks                                                   6.11.0  
   > NuGet.LibraryModel                                                 6.11.0  
   > NuGet.Packaging                                                    6.11.0  
   > NuGet.ProjectModel                                                 6.11.0  
   > NuGet.Protocol                                                     6.11.0  
   > NuGet.Versioning                                                   6.11.0  
   > Otp.NET                                                            1.4.0   
   > QRCoder                                                            1.6.0   
   > RBush.Signed                                                       4.0.0   
   > Serilog                                                            4.1.0   
   > Serilog.AspNetCore                                                 8.0.3   
   > Serilog.Extensions.Hosting                                         8.0.0   
   > Serilog.Extensions.Logging                                         8.0.0   
   > Serilog.Formatting.Compact                                         2.0.0   
   > Serilog.Settings.Configuration                                     8.0.4   
   > Serilog.Sinks.Console                                              6.0.0   
   > Serilog.Sinks.Debug                                                2.0.0   
   > Serilog.Sinks.File                                                 6.0.0   
   > Serilog.Sinks.MSSqlServer                                          8.0.0   
   > SharpZipLib                                                        1.4.2   
   > SixLabors.Fonts                                                    1.0.0   
   > SQLitePCLRaw.bundle_e_sqlite3                                      2.1.6   
   > SQLitePCLRaw.core                                                  2.1.6   
   > SQLitePCLRaw.lib.e_sqlite3                                         2.1.6   
   > SQLitePCLRaw.provider.e_sqlite3                                    2.1.6   
   > SSH.NET                                                            2023.0.0
   > SshNet.Security.Cryptography                                       1.3.0   
   > System.Buffers                                                     4.5.1   
   > System.ClientModel                                                 1.0.0   
   > System.CodeDom                                                     5.0.0   
   > System.Collections.Immutable                                       8.0.0   
   > System.Composition                                                 7.0.0   
   > System.Composition.AttributedModel                                 7.0.0   
   > System.Composition.Convention                                      7.0.0   
   > System.Composition.Hosting                                         7.0.0   
   > System.Composition.Runtime                                         7.0.0   
   > System.Composition.TypedParts                                      7.0.0   
   > System.Configuration.ConfigurationManager                          8.0.1   
   > System.Data.DataSetExtensions                                      4.5.0   
   > System.Diagnostics.DiagnosticSource                                6.0.1   
   > System.Diagnostics.EventLog                                        8.0.1   
   > System.Formats.Asn1                                                8.0.2   
   > System.IdentityModel.Tokens.Jwt                                    7.1.2   
   > System.IO.Packaging                                                8.0.1   
   > System.IO.Pipelines                                                8.0.0   
   > System.Memory                                                      4.5.4   
   > System.Memory.Data                                                 1.0.2   
   > System.Numerics.Vectors                                            4.5.0   
   > System.Reflection.Metadata                                         8.0.0   
   > System.Reflection.MetadataLoadContext                              8.0.0   
   > System.Runtime.Caching                                             8.0.0   
   > System.Runtime.CompilerServices.Unsafe                             6.0.0   
   > System.Security.Cryptography.Pkcs                                  8.0.1   
   > System.Security.Cryptography.ProtectedData                         8.0.0   
   > System.Text.Encodings.Web                                          4.7.2   
   > System.Text.Json                                                   8.0.6   
   > System.Threading.Channels                                          7.0.0   
   > System.Threading.Tasks.Extensions                                  4.5.4   
   > Testcontainers                                                     4.0.0   

專案 'Application.UnitTests' 有以下套件參考
   [net8.0]: 
   最上層套件                         已要求       已解析    
   > coverlet.collector          6.0.2     6.0.2  
   > FluentAssertions            6.12.0    6.12.0 
   > Microsoft.NET.Test.Sdk      17.11.0   17.11.0
   > Moq                         4.20.70   4.20.70
   > nunit                       3.14.0    3.14.0 
   > NUnit.Analyzers             3.9.0     3.9.0  
   > NUnit3TestAdapter           4.5.0     4.5.0  

   可轉移的套件                                                       已解析    
   > Ardalis.GuardClauses                                       4.6.0  
   > Azure.Core                                                 1.38.0 
   > Azure.Identity                                             1.11.4 
   > BouncyCastle.Cryptography                                  2.6.2  
   > Castle.Core                                                5.1.1  
   > ClosedXML                                                  0.105.0
   > ClosedXML.Parser                                           2.0.0  
   > Dapper                                                     2.1.66 
   > DocumentFormat.OpenXml                                     3.1.1  
   > DocumentFormat.OpenXml.Framework                           3.1.1  
   > ExcelNumberFormat                                          1.1.0  
   > FluentValidation                                           11.9.2 
   > FluentValidation.DependencyInjectionExtensions             11.9.2 
   > MailKit                                                    4.15.1 
   > Mapster                                                    10.0.6 
   > Mapster.Core                                               10.0.6 
   > Mapster.DependencyInjection                                10.0.6 
   > MediatR                                                    12.4.0 
   > MediatR.Contracts                                          2.0.1  
   > Microsoft.AspNetCore.Authentication.JwtBearer              8.0.11 
   > Microsoft.AspNetCore.Cryptography.Internal                 8.0.8  
   > Microsoft.AspNetCore.Cryptography.KeyDerivation            8.0.8  
   > Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore       8.0.8  
   > Microsoft.AspNetCore.Identity.EntityFrameworkCore          8.0.8  
   > Microsoft.Bcl.AsyncInterfaces                              1.1.1  
   > Microsoft.CodeCoverage                                     17.11.0
   > Microsoft.Data.SqlClient                                   5.1.7  
   > Microsoft.Data.SqlClient.SNI.runtime                       5.1.2  
   > Microsoft.EntityFrameworkCore                              8.0.23 
   > Microsoft.EntityFrameworkCore.Abstractions                 8.0.23 
   > Microsoft.EntityFrameworkCore.Analyzers                    8.0.23 
   > Microsoft.EntityFrameworkCore.Relational                   8.0.23 
   > Microsoft.EntityFrameworkCore.SqlServer                    8.0.23 
   > Microsoft.Extensions.Caching.Abstractions                  8.0.0  
   > Microsoft.Extensions.Caching.Memory                        8.0.1  
   > Microsoft.Extensions.Configuration.Abstractions            8.0.0  
   > Microsoft.Extensions.DependencyInjection                   8.0.1  
   > Microsoft.Extensions.DependencyInjection.Abstractions      9.0.0  
   > Microsoft.Extensions.Identity.Core                         8.0.8  
   > Microsoft.Extensions.Identity.Stores                       8.0.8  
   > Microsoft.Extensions.Logging                               8.0.1  
   > Microsoft.Extensions.Logging.Abstractions                  8.0.2  
   > Microsoft.Extensions.Options                               8.0.2  
   > Microsoft.Extensions.Primitives                            8.0.0  
   > Microsoft.Identity.Client                                  4.61.3 
   > Microsoft.Identity.Client.Extensions.Msal                  4.61.3 
   > Microsoft.IdentityModel.Abstractions                       7.1.2  
   > Microsoft.IdentityModel.JsonWebTokens                      7.1.2  
   > Microsoft.IdentityModel.Logging                            7.1.2  
   > Microsoft.IdentityModel.Protocols                          7.1.2  
   > Microsoft.IdentityModel.Protocols.OpenIdConnect            7.1.2  
   > Microsoft.IdentityModel.Tokens                             7.1.2  
   > Microsoft.NETCore.Platforms                                1.1.0  
   > Microsoft.SqlServer.Server                                 1.0.0  
   > Microsoft.TestPlatform.ObjectModel                         17.11.0
   > Microsoft.TestPlatform.TestHost                            17.11.0
   > Microsoft.Win32.SystemEvents                               6.0.0  
   > MimeKit                                                    4.15.1 
   > NETStandard.Library                                        2.0.0  
   > Newtonsoft.Json                                            13.0.1 
   > Otp.NET                                                    1.4.0  
   > QRCoder                                                    1.6.0  
   > RBush.Signed                                               4.0.0  
   > Serilog                                                    4.1.0  
   > SixLabors.Fonts                                            1.0.0  
   > System.ClientModel                                         1.0.0  
   > System.Configuration.ConfigurationManager                  6.0.1  
   > System.Diagnostics.DiagnosticSource                        6.0.1  
   > System.Diagnostics.EventLog                                6.0.0  
   > System.Drawing.Common                                      6.0.0  
   > System.Formats.Asn1                                        8.0.2  
   > System.IdentityModel.Tokens.Jwt                            7.1.2  
   > System.IO.Packaging                                        8.0.1  
   > System.Memory                                              4.5.4  
   > System.Memory.Data                                         1.0.2  
   > System.Numerics.Vectors                                    4.5.0  
   > System.Reflection.Metadata                                 1.6.0  
   > System.Runtime.Caching                                     6.0.0  
   > System.Runtime.CompilerServices.Unsafe                     6.0.0  
   > System.Security.AccessControl                              6.0.0  
   > System.Security.Cryptography.Cng                           5.0.0  
   > System.Security.Cryptography.Pkcs                          8.0.1  
   > System.Security.Cryptography.ProtectedData                 6.0.0  
   > System.Security.Permissions                                6.0.0  
   > System.Security.Principal.Windows                          5.0.0  
   > System.Text.Encoding.CodePages                             6.0.0  
   > System.Text.Encodings.Web                                  6.0.1  
   > System.Text.Json                                           4.7.2  
   > System.Threading.Tasks.Extensions                          4.5.4  
   > System.Windows.Extensions                                  6.0.0  

專案 'Domain.UnitTests' 有以下套件參考
   [net8.0]: 
   最上層套件                         已要求       已解析    
   > coverlet.collector          6.0.2     6.0.2  
   > FluentAssertions            6.12.0    6.12.0 
   > Microsoft.NET.Test.Sdk      17.11.0   17.11.0
   > nunit                       3.14.0    3.14.0 
   > NUnit.Analyzers             3.9.0     3.9.0  
   > NUnit3TestAdapter           4.5.0     4.5.0  

   可轉移的套件                                                       已解析    
   > MediatR                                                    12.4.0 
   > MediatR.Contracts                                          2.0.1  
   > Microsoft.AspNetCore.Cryptography.Internal                 8.0.8  
   > Microsoft.AspNetCore.Cryptography.KeyDerivation            8.0.8  
   > Microsoft.AspNetCore.Identity.EntityFrameworkCore          8.0.8  
   > Microsoft.CodeCoverage                                     17.11.0
   > Microsoft.EntityFrameworkCore                              8.0.8  
   > Microsoft.EntityFrameworkCore.Abstractions                 8.0.8  
   > Microsoft.EntityFrameworkCore.Analyzers                    8.0.8  
   > Microsoft.EntityFrameworkCore.Relational                   8.0.8  
   > Microsoft.Extensions.Caching.Abstractions                  8.0.0  
   > Microsoft.Extensions.Caching.Memory                        8.0.0  
   > Microsoft.Extensions.Configuration.Abstractions            8.0.0  
   > Microsoft.Extensions.DependencyInjection                   8.0.0  
   > Microsoft.Extensions.DependencyInjection.Abstractions      8.0.0  
   > Microsoft.Extensions.Identity.Core                         8.0.8  
   > Microsoft.Extensions.Identity.Stores                       8.0.8  
   > Microsoft.Extensions.Logging                               8.0.0  
   > Microsoft.Extensions.Logging.Abstractions                  8.0.0  
   > Microsoft.Extensions.Options                               8.0.2  
   > Microsoft.Extensions.Primitives                            8.0.0  
   > Microsoft.NETCore.Platforms                                1.1.0  
   > Microsoft.TestPlatform.ObjectModel                         17.11.0
   > Microsoft.TestPlatform.TestHost                            17.11.0
   > NETStandard.Library                                        2.0.0  
   > Newtonsoft.Json                                            13.0.1 
   > System.Configuration.ConfigurationManager                  4.4.0  
   > System.Reflection.Metadata                                 1.6.0  
   > System.Security.Cryptography.ProtectedData                 4.4.0  

專案 'Infrastructure.IntegrationTests' 有以下套件參考
   [net8.0]: 
   最上層套件                         已要求       已解析    
   > coverlet.collector          6.0.2     6.0.2  
   > Microsoft.NET.Test.Sdk      17.11.0   17.11.0
   > NUnit                       3.14.0    3.14.0 
   > NUnit.Analyzers             3.9.0     3.9.0  
   > NUnit3TestAdapter           4.5.0     4.5.0  

   可轉移的套件                                    已解析    
   > Microsoft.CodeCoverage                  17.11.0
   > Microsoft.NETCore.Platforms             1.1.0  
   > Microsoft.TestPlatform.ObjectModel      17.11.0
   > Microsoft.TestPlatform.TestHost         17.11.0
   > NETStandard.Library                     2.0.0  
   > Newtonsoft.Json                         13.0.1 
   > System.Reflection.Metadata              1.6.0  

專案 'Mvc' 有以下套件參考
   [net8.0]: 
   最上層套件                                                        已要求      已解析   
   > Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation          8.0.11   8.0.11
   > Microsoft.EntityFrameworkCore.Design                       8.0.23   8.0.23
   > Microsoft.VisualStudio.Azure.Containers.Tools.Targets      1.21.0   1.21.0
   > Microsoft.VisualStudio.Web.CodeGeneration.Design           8.0.23   8.0.23
   > Serilog.AspNetCore                                         8.0.3    8.0.3 
   > Serilog.Settings.Configuration                             8.0.4    8.0.4 
   > Serilog.Sinks.Console                                      6.0.0    6.0.0 
   > Serilog.Sinks.File                                         6.0.0    6.0.0 
   > Serilog.Sinks.MSSqlServer                                  8.0.0    8.0.0 

   可轉移的套件                                                               已解析     
   > Ardalis.GuardClauses                                               4.6.0   
   > Azure.Core                                                         1.38.0  
   > Azure.Identity                                                     1.11.4  
   > BouncyCastle.Cryptography                                          2.6.2   
   > ClosedXML                                                          0.105.0 
   > ClosedXML.Parser                                                   2.0.0   
   > Dapper                                                             2.1.66  
   > DocumentFormat.OpenXml                                             3.1.1   
   > DocumentFormat.OpenXml.Framework                                   3.1.1   
   > ExcelNumberFormat                                                  1.1.0   
   > FluentValidation                                                   11.9.2  
   > FluentValidation.DependencyInjectionExtensions                     11.9.2  
   > Humanizer                                                          2.14.1  
   > Humanizer.Core                                                     2.14.1  
   > Humanizer.Core.af                                                  2.14.1  
   > Humanizer.Core.ar                                                  2.14.1  
   > Humanizer.Core.az                                                  2.14.1  
   > Humanizer.Core.bg                                                  2.14.1  
   > Humanizer.Core.bn-BD                                               2.14.1  
   > Humanizer.Core.cs                                                  2.14.1  
   > Humanizer.Core.da                                                  2.14.1  
   > Humanizer.Core.de                                                  2.14.1  
   > Humanizer.Core.el                                                  2.14.1  
   > Humanizer.Core.es                                                  2.14.1  
   > Humanizer.Core.fa                                                  2.14.1  
   > Humanizer.Core.fi-FI                                               2.14.1  
   > Humanizer.Core.fr                                                  2.14.1  
   > Humanizer.Core.fr-BE                                               2.14.1  
   > Humanizer.Core.he                                                  2.14.1  
   > Humanizer.Core.hr                                                  2.14.1  
   > Humanizer.Core.hu                                                  2.14.1  
   > Humanizer.Core.hy                                                  2.14.1  
   > Humanizer.Core.id                                                  2.14.1  
   > Humanizer.Core.is                                                  2.14.1  
   > Humanizer.Core.it                                                  2.14.1  
   > Humanizer.Core.ja                                                  2.14.1  
   > Humanizer.Core.ko-KR                                               2.14.1  
   > Humanizer.Core.ku                                                  2.14.1  
   > Humanizer.Core.lv                                                  2.14.1  
   > Humanizer.Core.ms-MY                                               2.14.1  
   > Humanizer.Core.mt                                                  2.14.1  
   > Humanizer.Core.nb                                                  2.14.1  
   > Humanizer.Core.nb-NO                                               2.14.1  
   > Humanizer.Core.nl                                                  2.14.1  
   > Humanizer.Core.pl                                                  2.14.1  
   > Humanizer.Core.pt                                                  2.14.1  
   > Humanizer.Core.ro                                                  2.14.1  
   > Humanizer.Core.ru                                                  2.14.1  
   > Humanizer.Core.sk                                                  2.14.1  
   > Humanizer.Core.sl                                                  2.14.1  
   > Humanizer.Core.sr                                                  2.14.1  
   > Humanizer.Core.sr-Latn                                             2.14.1  
   > Humanizer.Core.sv                                                  2.14.1  
   > Humanizer.Core.th-TH                                               2.14.1  
   > Humanizer.Core.tr                                                  2.14.1  
   > Humanizer.Core.uk                                                  2.14.1  
   > Humanizer.Core.uz-Cyrl-UZ                                          2.14.1  
   > Humanizer.Core.uz-Latn-UZ                                          2.14.1  
   > Humanizer.Core.vi                                                  2.14.1  
   > Humanizer.Core.zh-CN                                               2.14.1  
   > Humanizer.Core.zh-Hans                                             2.14.1  
   > Humanizer.Core.zh-Hant                                             2.14.1  
   > MailKit                                                            4.15.1  
   > Mapster                                                            10.0.6  
   > Mapster.Core                                                       10.0.6  
   > Mapster.DependencyInjection                                        10.0.6  
   > MediatR                                                            12.4.0  
   > MediatR.Contracts                                                  2.0.1   
   > Microsoft.AspNetCore.Authentication.JwtBearer                      8.0.11  
   > Microsoft.AspNetCore.Cryptography.Internal                         8.0.8   
   > Microsoft.AspNetCore.Cryptography.KeyDerivation                    8.0.8   
   > Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore               8.0.8   
   > Microsoft.AspNetCore.Identity.EntityFrameworkCore                  8.0.8   
   > Microsoft.AspNetCore.Mvc.Razor.Extensions                          6.0.0   
   > Microsoft.AspNetCore.Razor.Language                                6.0.24  
   > Microsoft.Bcl.AsyncInterfaces                                      7.0.0   
   > Microsoft.Build                                                    17.11.48
   > Microsoft.Build.Framework                                          17.11.48
   > Microsoft.CodeAnalysis.Analyzers                                   3.3.4   
   > Microsoft.CodeAnalysis.AnalyzerUtilities                           3.3.0   
   > Microsoft.CodeAnalysis.Common                                      4.8.0   
   > Microsoft.CodeAnalysis.CSharp                                      4.8.0   
   > Microsoft.CodeAnalysis.CSharp.Features                             4.8.0   
   > Microsoft.CodeAnalysis.CSharp.Workspaces                           4.8.0   
   > Microsoft.CodeAnalysis.Elfie                                       1.0.0   
   > Microsoft.CodeAnalysis.Features                                    4.8.0   
   > Microsoft.CodeAnalysis.Razor                                       6.0.24  
   > Microsoft.CodeAnalysis.Scripting.Common                            4.8.0   
   > Microsoft.CodeAnalysis.Workspaces.Common                           4.8.0   
   > Microsoft.Data.SqlClient                                           5.2.2   
   > Microsoft.Data.SqlClient.SNI.runtime                               5.2.0   
   > Microsoft.DiaSymReader                                             2.0.0   
   > Microsoft.DotNet.Scaffolding.Shared                                8.0.23  
   > Microsoft.EntityFrameworkCore                                      8.0.23  
   > Microsoft.EntityFrameworkCore.Abstractions                         8.0.23  
   > Microsoft.EntityFrameworkCore.Analyzers                            8.0.23  
   > Microsoft.EntityFrameworkCore.Relational                           8.0.23  
   > Microsoft.EntityFrameworkCore.SqlServer                            8.0.23  
   > Microsoft.Extensions.Caching.Abstractions                          8.0.0   
   > Microsoft.Extensions.Caching.Memory                                8.0.1   
   > Microsoft.Extensions.Configuration                                 8.0.0   
   > Microsoft.Extensions.Configuration.Abstractions                    8.0.0   
   > Microsoft.Extensions.Configuration.Binder                          8.0.0   
   > Microsoft.Extensions.DependencyInjection                           8.0.1   
   > Microsoft.Extensions.DependencyInjection.Abstractions              9.0.0   
   > Microsoft.Extensions.DependencyModel                               8.0.2   
   > Microsoft.Extensions.Diagnostics.Abstractions                      8.0.0   
   > Microsoft.Extensions.FileProviders.Abstractions                    8.0.0   
   > Microsoft.Extensions.Hosting.Abstractions                          8.0.0   
   > Microsoft.Extensions.Identity.Core                                 8.0.8   
   > Microsoft.Extensions.Identity.Stores                               8.0.8   
   > Microsoft.Extensions.Logging                                       8.0.1   
   > Microsoft.Extensions.Logging.Abstractions                          8.0.2   
   > Microsoft.Extensions.Options                                       8.0.2   
   > Microsoft.Extensions.Options.ConfigurationExtensions               8.0.0   
   > Microsoft.Extensions.Primitives                                    8.0.0   
   > Microsoft.Identity.Client                                          4.61.3  
   > Microsoft.Identity.Client.Extensions.Msal                          4.61.3  
   > Microsoft.IdentityModel.Abstractions                               7.1.2   
   > Microsoft.IdentityModel.JsonWebTokens                              7.1.2   
   > Microsoft.IdentityModel.Logging                                    7.1.2   
   > Microsoft.IdentityModel.Protocols                                  7.1.2   
   > Microsoft.IdentityModel.Protocols.OpenIdConnect                    7.1.2   
   > Microsoft.IdentityModel.Tokens                                     7.1.2   
   > Microsoft.NET.StringTools                                          17.11.48
   > Microsoft.SqlServer.Server                                         1.0.0   
   > Microsoft.VisualStudio.Web.CodeGeneration                          8.0.23  
   > Microsoft.VisualStudio.Web.CodeGeneration.Core                     8.0.23  
   > Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore      8.0.23  
   > Microsoft.VisualStudio.Web.CodeGeneration.Templating               8.0.23  
   > Microsoft.VisualStudio.Web.CodeGeneration.Utils                    8.0.23  
   > Microsoft.VisualStudio.Web.CodeGenerators.Mvc                      8.0.23  
   > MimeKit                                                            4.15.1  
   > Mono.TextTemplating                                                2.3.1   
   > Newtonsoft.Json                                                    13.0.3  
   > NuGet.Common                                                       6.11.0  
   > NuGet.Configuration                                                6.11.0  
   > NuGet.DependencyResolver.Core                                      6.11.0  
   > NuGet.Frameworks                                                   6.11.0  
   > NuGet.LibraryModel                                                 6.11.0  
   > NuGet.Packaging                                                    6.11.0  
   > NuGet.ProjectModel                                                 6.11.0  
   > NuGet.Protocol                                                     6.11.0  
   > NuGet.Versioning                                                   6.11.0  
   > Otp.NET                                                            1.4.0   
   > QRCoder                                                            1.6.0   
   > RBush.Signed                                                       4.0.0   
   > Serilog                                                            4.1.0   
   > Serilog.Extensions.Hosting                                         8.0.0   
   > Serilog.Extensions.Logging                                         8.0.0   
   > Serilog.Formatting.Compact                                         2.0.0   
   > Serilog.Sinks.Debug                                                2.0.0   
   > SixLabors.Fonts                                                    1.0.0   
   > System.ClientModel                                                 1.0.0   
   > System.CodeDom                                                     5.0.0   
   > System.Collections.Immutable                                       8.0.0   
   > System.Composition                                                 7.0.0   
   > System.Composition.AttributedModel                                 7.0.0   
   > System.Composition.Convention                                      7.0.0   
   > System.Composition.Hosting                                         7.0.0   
   > System.Composition.Runtime                                         7.0.0   
   > System.Composition.TypedParts                                      7.0.0   
   > System.Configuration.ConfigurationManager                          8.0.1   
   > System.Data.DataSetExtensions                                      4.5.0   
   > System.Diagnostics.DiagnosticSource                                8.0.0   
   > System.Diagnostics.EventLog                                        8.0.1   
   > System.Formats.Asn1                                                8.0.2   
   > System.IdentityModel.Tokens.Jwt                                    7.1.2   
   > System.IO.Packaging                                                8.0.1   
   > System.IO.Pipelines                                                7.0.0   
   > System.Memory                                                      4.5.4   
   > System.Memory.Data                                                 1.0.2   
   > System.Numerics.Vectors                                            4.5.0   
   > System.Reflection.Metadata                                         8.0.0   
   > System.Reflection.MetadataLoadContext                              8.0.0   
   > System.Runtime.Caching                                             8.0.0   
   > System.Runtime.CompilerServices.Unsafe                             6.0.0   
   > System.Security.Cryptography.Pkcs                                  8.0.1   
   > System.Security.Cryptography.ProtectedData                         8.0.0   
   > System.Text.Encodings.Web                                          4.7.2   
   > System.Text.Json                                                   8.0.6   
   > System.Threading.Channels                                          7.0.0   
   > System.Threading.Tasks.Extensions                                  4.5.4   


```
