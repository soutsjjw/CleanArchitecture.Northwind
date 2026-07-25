# 訂單管理一期補強設計

## 目標

補足已確認的一期訂單管理需求：以產品名稱或產品編號搜尋訂單、避免重複軟刪除、依刪除權限顯示操作按鈕，並在詳情返回列表時保留目前查詢條件。

## 架構與資料流

`GetOrdersQueryHandler` 保持在 Application 層，以既有 EF Core 可翻譯的查詢投影和 `OrderDetails` 關聯加入產品名稱與產品編號條件。Web Controller 只負責將列表查詢條件以 route values 傳到詳情與返回連結；不把 MVC 型別傳進 Application。

刪除仍由 `DeleteOrderCommandHandler` 執行，但查詢目標時同時排除 `IsDelete`，使已刪除訂單與不存在訂單具有一致結果。Razor View 透過既有 `IAuthorizationService` 與 `Policies.Orders_Delete` 決定是否輸出刪除按鈕；Action 上既有授權屬性保留，作為真正的安全邊界。

## 錯誤與安全

產品搜尋沿用既有 Keyword 的 trim 與不分大小寫邏輯。軟刪除訂單不會出現在列表、詳情或重複刪除操作中。受保護訂單 ID、Anti-Forgery 與刪除授權不變；UI 隱藏不取代 Action 授權。

## 測試

Application 單元測試新增產品名稱與產品編號搜尋、重複刪除拒絕。Controller/Razor 測試驗證回列表 route values 與刪除按鈕的權限條件。既有訂單測試全數執行。

## 範圍外

不新增客戶、員工或貨運商的聯絡資料；不改動資料庫 schema、Migration、套件、全域 Data Protection 設定，亦不將列表 POST 重構為 GET。
