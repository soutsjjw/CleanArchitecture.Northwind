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
