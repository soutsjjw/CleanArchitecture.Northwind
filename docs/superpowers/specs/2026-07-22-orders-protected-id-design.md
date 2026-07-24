# Orders Controller 受保護 ID 設計

## 目標

以 ASP.NET Core Data Protection 保護 `OrdersController` 與 Razor View 之間傳遞的訂單識別碼，避免在 URL、AJAX 請求及表單欄位暴露可直接使用的整數 ID。

## 範圍

- 保護 `Details`、`DeleteConfirmation` 與 `Delete` 的 `id` 傳輸值。
- 訂單清單與明細畫面仍顯示原始訂單編號，僅傳輸值改為受保護字串。
- 沿用既有 `Application.Common.Interfaces.IDataProtectionService`；不新增套件或變更 Application、Domain、Infrastructure 的公開 use case。

## 架構與資料流

變更僅限 Web 層。`OrdersController` 使用既有抽象服務將整數 ID 轉為受保護字串，並在收到請求後還原及驗證，才建立 Application 的 Query 或 Command。

```text
OrderIndex/Detail ViewModel (Id + ProtectedId)
  -> Razor URL / AJAX data-url / hidden input (ProtectedId)
  -> OrdersController string id
  -> IDataProtectionService.Unprotect + int.TryParse
  -> GetOrderDetailQuery / DeleteOrderCommand (int Id)
```

因此 Application 仍只接收原始整數 ID，且不依賴 Web 或 Data Protection 的實作細節。

## 元件設計

1. `OrderItemViewModel` 與 `OrderDetailViewModel` 新增 `ProtectedId`，供 View 產生請求值；既有 `Id` 保持為訂單編號的顯示值。
2. `OrdersController` 注入 `IDataProtectionService`，在清單與刪除確認模型送出 View 前填入 `ProtectedId`。
3. `Details`、`DeleteConfirmation`、`Delete` 接受 `string id`，統一解密並檢查是否為有效整數。
4. `Index.cshtml` 改以 `ProtectedId` 產生詳細頁連結與刪除確認 AJAX URL；`_DeleteConfirmationModal.cshtml` 以它作為 POST hidden input。

## 錯誤處理與安全性

- 無法解密或非整數的 ID 視為不可信輸入，不傳給 Application。
- 詳細頁與刪除 POST 導回清單並顯示一般錯誤訊息；刪除確認 AJAX 請求回應 `NotFound()`。
- 保留既有 `[Authorize]` 與 Anti-Forgery 保護。
- 不將解密例外或保護內容回傳給使用者。

## 測試

- 合法受保護 ID 能還原後呼叫正確的 Query 或 Command。
- 無效或遭竄改的 ID 不會呼叫 MediatR，並產生對應安全回應。
- 清單與刪除確認結果中的 `ProtectedId` 可由 Razor 作為傳輸值使用。

## 非目標

- 不隱藏畫面上顯示的訂單編號。
- 不引入通用 MVC Model Binder 或跨 Controller 的抽象化。
- 不變更資料表、Migration、Application Query/Command 契約或 Domain 模型。
