# Task 2 Report: Migration 與期初庫存回填

## 變更

- 新增 `20260725102928_AddProductInventoryManagement` Migration、Designer 與 model snapshot。
- Migration 僅建立產品圖片與 rowversion、分類／供應商 `IsActive` 欄位，以及 `InventoryTransactions` 資料表與索引。
- `Up` 以 SQL 對既有 `Products` 各插入一筆 `OpeningBalance`：庫存前後值皆使用 `COALESCE(UnitsInStock, 0)`、差異為 `0`、建立者為 `system:initial-balance`。
- 新增真實 SQL Server Testcontainer 整合測試：先升級到前一版 Migration、以舊 schema 插入產品、套用新 Migration，再驗證異動資料與第二次 `MigrateAsync` 不會重複建立期初資料。

## 驗證

| 命令 | 結果 |
| --- | --- |
| `C:\tmp\cnw-ef-tools\dotnet-ef.exe migrations script 20250818154756_AddLastPasswordChangedDateColumn 20260725102928_AddProductInventoryManagement --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web\Web.csproj --no-build` | 通過；產生 SQL 僅含本任務 schema 操作與期初 `INSERT`。 |
| `dotnet build tests\Application.FunctionalTests\Application.FunctionalTests.csproj --no-restore` | 通過（0 errors、既有與 Testcontainers obsolete warnings）。 |
| `dotnet test tests\Application.FunctionalTests\Application.FunctionalTests.csproj --filter "FullyQualifiedName~InitialInventoryMigrationTests"` | 未取得最終摘要。原 namespace 會在既有 Web 初始化中失敗：`AuditableEntityInterceptor` 將 `DateTimeOffset` 寫入 `DateTime?`，早於 Migration 測試。 |
| 直接 `vstest.console` 執行隔離 Testcontainer 測試 | 修正前失敗，因以目前 EF 模型在舊 Products schema 寫入尚未存在的 `Picture`／`RowVersion` 欄位；已改為 raw SQL 以舊 schema 插入測試產品。修正後命令已啟動，但依協調指示停止等待測試平台，尚無最終 TRX outcome。 |

## 自審

- 已以 `git diff --check` 檢查，沒有 whitespace error。
- 已確認 Migration `Up`／`Down` 不含 `TodoItems`、`TodoLists`、`AspNetRoleClaims` 或其他無關 schema 操作。
- Scaffold 時發現 snapshot 原本與現行模型已有 Todo／RoleClaim 漂移；依指示保留 EF 產生的 snapshot 漂移，但抑制所有無關且可能破壞資料的 migration operation。

## 提交

- `git commit -m "feat: add product inventory migration"`

## 疑慮

- 隔離整合測試的最終通過結果尚未取得；需要在可完整等待 VSTest/Testcontainers 的環境重新執行該單一測試。
- 既有 FunctionalTests Web fixture 的 audit interceptor 型別轉換錯誤與本任務無關，但會阻止所有使用該 fixture 的整合測試初始化。
