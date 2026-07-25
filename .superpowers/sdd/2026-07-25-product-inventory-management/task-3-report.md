# Task 3 Report：庫存調整、盤點與並行處理

## 狀態

DONE_WITH_CONCERNS

## 變更

- 新增 `AdjustInventoryCommand`、Validator 與 Handler，支援人工庫存增減。
- 新增 `StocktakeCommand`、Validator 與 Handler，以「實際庫存 - 目前庫存」寫入盤點差異。
- 新增共用 `InventoryCommandExecutor`，讓人工調整與盤點共用相同的商品狀態、原因、庫存範圍、歷程與樂觀並行規則。
- 成功結果回傳最新 `UnitsInStock` 與 EF 儲存後的最新 `RowVersion`。
- 擴充 `IApplicationDbContext.SetOriginalRowVersion`，並在 Infrastructure 使用 EF `Entry(product).Property(...).OriginalValue` 套用用戶端 rowversion。
- 每次操作只呼叫一次 `SaveChangesAsync`，使產品庫存更新與 `InventoryTransaction` 新增位於同一個 EF Core 儲存交易。
- 捕捉 `DbUpdateConcurrencyException`，還原目前 DbContext 中的庫存值、移除未提交的歷程，並只回傳一般衝突訊息 `庫存已被其他使用者更新`。
- 防守性拒絕不存在、停用或軟刪除商品、負庫存、`short` 範圍溢位、空白或超過 250 字元原因，以及缺少 rowversion；所有這些失敗分支均不呼叫 `SaveChangesAsync`。
- 未修改 Web、Migration、套件或部署設定。

## TDD

### RED

先新增 `InventoryCommandHandlerTests`，涵蓋：

- 人工調整造成負庫存。
- 人工調整超過 `short.MaxValue`。
- 商品停用或軟刪除。
- 空白、過長原因與缺少 rowversion。
- 人工調整成功時的庫存、歷程、OriginalValue 與最新結果。
- 盤點差異與負實際庫存。
- rowversion 並行衝突。
- 兩個 Validator 的邊界規則。

主代理執行：

```powershell
dotnet test tests\Application.UnitTests\Application.UnitTests.csproj --filter "FullyQualifiedName~InventoryCommandHandlerTests" --no-restore
```

結果：編譯失敗，`Application.Features.Inventory`、Adjust/Stocktake 命令與 Handler 尚不存在，出現預期的 `CS0234`；失敗原因是功能尚未實作。

本子代理第一次執行同一命令時，曾先被工作樹 `artifacts/obj` 寫入權限阻擋，該環境錯誤未被當成 RED 證據；改由具工作樹權限的主代理重新執行後取得上述有效 RED。

### GREEN

完成最小 production implementation 後，主代理重新執行：

```powershell
dotnet test tests\Application.UnitTests\Application.UnitTests.csproj --filter "FullyQualifiedName~InventoryCommandHandlerTests" --no-restore
```

結果：15/15 通過，0 失敗，耗時 566 ms；僅輸出 Repository 既有 nullable/member-hiding warnings。

### 其他檢查

```powershell
git diff --check
```

結果：通過，沒有 whitespace error；Git 只提示既有工作樹的 LF/CRLF 正規化行為。

提交前的 cached diff 格式檢查亦由主代理確認通過。

## Commit

`d370db9 feat: add inventory adjustment commands`

## 自我審查

- `AdjustInventoryCommand(int ProductId, short QuantityDelta, string Reason, byte[] RowVersion)` 與 `StocktakeCommand(int ProductId, short ActualQuantity, string Reason, byte[] RowVersion)` 符合 brief 公開介面。
- 共用執行器先完成所有輸入、商品狀態與數值邊界檢查，才修改 Product、加入歷程並儲存；一般失敗不會留下待提交異動。
- 人工調整的 `QuantityDelta` 使用請求值；盤點的 `QuantityDelta` 使用 `ActualQuantity - QuantityBefore`，且兩者都驗證 `short` 範圍。
- `UnitsInStock == null` 依 brief 視為 0。
- 用戶端 rowversion 以複本設為 EF OriginalValue；成功結果也以複本回傳最新 rowversion，避免呼叫端直接修改 tracked entity 的陣列。
- 並行例外不揭露資料庫、EF 或例外細節。
- Product 與 InventoryTransaction 由同一個 `IApplicationDbContext`、同一次 `SaveChangesAsync` 提交，沒有跨 DbContext 或先後兩次儲存造成部分成功的路徑。
- 沒有新增平行 Repository/Service 架構；Application 仍只依賴既有 DbContext 抽象，EF-specific OriginalValue 操作留在 Infrastructure 實作。
- 變更範圍只有 Task 3 命令、測試，以及實作 rowversion concurrency 所必需的 Application DbContext 抽象與 Infrastructure 實作。

## 疑慮

- 並行衝突測試依 brief 使用 mock 讓 `SaveChangesAsync` 丟出 `DbUpdateConcurrencyException`；本 Task 未新增真實 SQL Server 的雙 context stale-rowversion 整合測試。因此 EF rowversion metadata 已由 Task 1 測試覆蓋，但從 HTTP/use case 到真實 SQL Server 的完整並行情境仍可在後續 functional test 補強。
- 測試建置仍輸出 Repository 既有 nullable/member-hiding warnings；本 Task 未修改無關程式。
