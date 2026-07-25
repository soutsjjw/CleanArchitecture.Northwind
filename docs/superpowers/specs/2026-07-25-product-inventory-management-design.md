# 第一期商品與庫存管理設計

**日期：** 2026-07-25  
**範圍：** 商品、分類、商品圖片與第一期庫存作業；不含訂單、採購與出貨交易。

## 目標

提供可維護的商品與分類主檔管理，並建立可稽核的庫存分類帳。使用者可以調整或盤點庫存；每次變動都必須在同一交易中更新商品庫存與新增不可變更的歷程。既有 Northwind 資料以目前庫存建立期初紀錄，不回溯舊訂單。

## 範圍

### 本期包含

- 商品列表、搜尋、篩選、詳情、新增、編輯、停用／恢復與條件式軟刪除。
- 商品單一主圖片的上傳、替換、移除與受授權輸出。
- 分類列表、新增、編輯、停用／恢復與條件式軟刪除。
- 新增商品時選擇既有且啟用的供應商；供應商 CRUD 延至第二期。
- 期初庫存、手動調整、盤點、分頁庫存歷程與樂觀並行控制。
- 低庫存規則沿用 `UnitsInStock <= ReorderLevel`；停用商品不納入低庫存儀表板。

### 本期不包含

- 訂單建立、確認、取消、退貨或出貨的庫存異動。
- 採購單、收貨、收貨更正與採購退貨。
- 供應商維護頁面、供應商圖片或分類圖片上傳。
- 多張商品圖片、外部檔案儲存、匯出與報表。

## 主檔規則

### 商品

- 延用 `Product.Discontinued` 表示停用；停用商品不得在未來訂單中被選用，恢復啟用後可再次選用。
- 未被訂單明細或庫存歷程引用的商品可軟刪除，並自一般清單與選項中排除。
- 已被訂單明細或庫存歷程引用的商品禁止刪除，只能停用或恢復。
- 商品可重新指派至另一個啟用中的分類或供應商。
- 商品新增或編輯時，分類與供應商必須存在、未軟刪除且為啟用狀態。

### 分類與供應商

- `Category` 與 `Supplier` 都新增 `IsActive`，既有資料以啟用狀態回填。
- 被商品引用的分類或供應商禁止刪除，只能停用；停用後不出現在商品新增／重新指派的選項中。
- 停用不解除既有商品關聯；商品仍可改指派至另一筆啟用主檔。
- 第一期間供應商僅供讀取與選擇；第二期才提供供應商 CRUD。

## 資料模型與 Migration

### Product

- 新增單一主圖片資料與圖片 MIME type；不信任使用者輸入的檔名或路徑。
- 新增 `RowVersion` 作為 EF Core rowversion／樂觀並行 token。
- 保持既有 `UnitsInStock` 為現行庫存快取值，欄位型別仍為 `short`。

### InventoryTransaction

新增可稽核且不提供修改／刪除操作的實體與資料表。欄位至少包含：

- `Id`、`ProductId`。
- `TransactionType`：`OpeningBalance`、`ManualAdjustment`、`Stocktake`；未來可擴充訂單與採購類型。
- `QuantityBefore`、`QuantityDelta`、`QuantityAfter`。
- `Reason`。
- 可為空的來源單據型別與來源識別值，供未來訂單與採購流程關聯。
- `Created` 與 `CreatedBy`，沿用既有審計機制。

Migration 新增上述欄位與資料表，並針對每筆既有商品新增一筆 `OpeningBalance`：前後庫存皆等於目前 `UnitsInStock`、差異為零、操作者為固定系統識別值。Migration 不修改現有商品庫存，也不從歷史訂單回推庫存。

## 庫存交易規則

### 手動調整

使用者輸入正數或負數的調整量與必填原因。系統以商品目前庫存加上調整量計算新庫存。

### 盤點

使用者輸入實際盤點量與必填原因。系統以「實際盤點量減目前庫存」計算差異。

### 原子性與衝突

每次調整或盤點必須在同一資料庫交易中完成：

1. 讀取未停用、未軟刪除的商品及其 `RowVersion`。
2. 驗證使用者送出的版本、結果庫存不小於零，且所有數值都在 `short` 範圍內。
3. 更新 `Product.UnitsInStock`。
4. 新增對應的 `InventoryTransaction`。
5. 儲存變更；EF Core 並行衝突時回傳一般性衝突訊息，要求使用者重新整理後重試。

任一步失敗都不得保留部分更新或部分歷程。庫存歷程不建立 Update 或 Delete Command；未來訂單、採購、更正與退貨都新增反向或新增交易，不修改舊紀錄。

## Web 與安全

- Controller 只負責 MVC ViewModel 與 Application Command／Query 的映射；不得直接操作 DbContext。
- 商品識別碼與任何修改操作使用既有 Data Protection 邏輯；還原失敗即中止操作。
- 所有瀏覽器狀態變更 POST 都使用 Anti-Forgery。
- 商品圖片限制單一檔案，接受 JPEG、PNG、WebP，並驗證副檔名、Content-Type、檔案簽名與大小。輸出圖片需經受授權的 MVC Action，不公開實體檔案路徑。
- 圖片為可選；編輯時可替換或移除。商品與分類表單驗證失敗時不得遺失其他已輸入欄位。

## 權限與 UI

- 沿用 `Products` 與 `Categories` 的 Read、Create、Update、Delete policy。
- 新增 `Inventory:Read` 保護歷程查閱，新增 `Inventory:Create` 保護手動調整與盤點。
- 供應商選項須具備既有 `Suppliers:Read` 權限；第一期不提供供應商修改動作。
- 導覽與按鈕可依權限隱藏，但 Controller Action 的 policy 是實際安全邊界。
- 商品詳情提供庫存歷程、調整與盤點入口；無權限時不顯示對應操作。

## 架構與資料流

```text
Browser
  -> Web Controller / ViewModel
  -> ISender.Send(Command or Query)
  -> Application Handler / Validator
  -> IApplicationDbContext transaction
  -> Product current stock + InventoryTransaction
  -> Result / DTO
  -> Web redirect, form error, or view
```

Application 定義商品、分類、庫存操作的 Command、Query、DTO、Validator 與 Handler。Infrastructure 只實作 EF Core 設定、Migration 與資料庫交易；Web 不依賴 Infrastructure 實作。讀取清單與歷程使用可翻譯 projection 與 `AsNoTracking()`。

## 測試與驗證

- Application 單元測試：商品與分類輸入驗證、停用／恢復、關聯引用的刪除限制、啟用選項篩選、調整與盤點計算、負庫存與溢位拒絕、並行衝突與歷程不可變更。
- Web／功能測試：授權、受保護識別碼、Anti-Forgery、圖片格式與大小驗證、表單錯誤、操作成功的重新導向及歷程可見性。
- Migration／整合測試：既有商品只產生一筆期初庫存紀錄，且不改變 `UnitsInStock`。
- 完成前執行受影響測試、相關專案建置與完整方案建置；資料模型與 Migration 變更需執行完整 `dotnet build` 及相關完整測試。

## 後續階段的既定規則

- 訂單確認時扣庫存；已確認訂單可取消並回補，已出貨訂單不可取消；退貨依實際明細入庫。
- 採購單未收貨前可取消；已收貨不可取消。錯誤以收貨更正處理，實體退貨以採購退貨處理。
- 庫存不足時拒絕訂單；訂單與採購異動沿用同一 `InventoryTransaction` 分類帳及樂觀並行規則。
