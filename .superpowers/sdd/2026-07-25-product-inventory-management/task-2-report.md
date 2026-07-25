# Task 2 Report: Migration 與期初庫存回填

## 變更

- 新增 `20260725102928_AddProductInventoryManagement` Migration、Designer 與 model snapshot。
- Migration 僅建立產品圖片與 rowversion、分類／供應商 `IsActive` 欄位，以及 `InventoryTransactions` 資料表與索引。
- `Up` 以 SQL 對既有 `Products` 各插入一筆 `OpeningBalance`：庫存前後值皆使用 `COALESCE(UnitsInStock, 0)`、差異為 `0`、建立者為 `system:initial-balance`。
- 新增真實 SQL Server Testcontainer 整合測試：先升級到前一版 Migration、以舊 schema 插入庫存為 `17` 與 `NULL` 的產品、套用新 Migration，再驗證異動資料與第二次 `MigrateAsync` 不會重複建立期初資料。`NULL` 庫存的期初異動前後值皆為 `0`。

## 驗證

| 命令 | 結果 |
| --- | --- |
| `C:\tmp\cnw-ef-tools\dotnet-ef.exe migrations script 20250818154756_AddLastPasswordChangedDateColumn 20260725102928_AddProductInventoryManagement --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web\Web.csproj --no-build` | 通過；產生 SQL 僅含本任務 schema 操作與期初 `INSERT`。 |
| `dotnet build tests\Application.FunctionalTests\Application.FunctionalTests.csproj --no-restore` | 通過（0 errors、既有與 Testcontainers obsolete warnings）。 |
| `dotnet build tests\Application.FunctionalTests\Application.FunctionalTests.csproj --no-restore`（fix round） | 通過（0 errors、既有與 Testcontainers obsolete warnings）。 |
| `vstest.console.exe ... /InIsolation /TestCaseFilter:FullyQualifiedName~InitialInventoryMigrationTests /Logger:trx;LogFileName=initial-inventory-null-stock-isolated.trx /ResultsDirectory:C:\tmp\cnw-pim-test-results` | 通過；1/1 通過，17.4537 秒。實際啟動 SQL Server Testcontainer，驗證 `NULL` 庫存回填為 0 與重跑冪等性。 |

## 自審

- 已以 `git diff --check` 檢查，沒有 whitespace error。
- 已確認 Migration `Up`／`Down` 不含 `TodoItems`、`TodoLists`、`AspNetRoleClaims` 或其他無關 schema 操作。
- Scaffold 時發現 snapshot 原本與現行模型已有 Todo／RoleClaim 漂移；使用者已接受同步保留這些 snapshot 漂移的風險，條件是 Migration 的 `Up`／`Down` 不得對其執行 DDL。已確認符合。

## 提交

- `git commit -m "feat: add product inventory migration"`
- `git commit -m "test: cover null opening inventory balance"`

## 疑慮

- 已接受的 snapshot Todo／RoleClaim 漂移仍存在；本次 Migration 刻意不對這些資料表執行 DDL。
