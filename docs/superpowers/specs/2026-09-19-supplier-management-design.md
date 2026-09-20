# 第二期供應商管理設計

## 目標

提供具權限控管的 Supplier 管理功能，讓營運人員能查詢、建立、編輯、啟用、停用與刪除未被 Product 引用的供應商。

## 已確認規則

- 列表顯示公司名稱、聯絡人、電話、國家、啟用狀態、供應商品數，支援關鍵字、啟用狀態與分頁；預設顯示所有未軟刪除資料，依公司名稱、識別值排序。
- 新增與編輯維護現有完整 Supplier 欄位；`HomePage` 可空，否則為去除空白後的絕對 HTTP／HTTPS URL，外連使用 `target="_blank" rel="noopener noreferrer"`。
- 未軟刪除的公司名稱不分大小寫唯一；軟刪除名稱可重用。
- 任何 Product 引用的 Supplier 不得刪除，只能停用；停用後既有關聯保留，並由既有 Product 表單與 Command 驗證排除。
- 本期無獨立詳細頁。所有 POST 採 Anti-Forgery；Controller 以功能分隔的 Data Protection protected Id 還原識別值，並分別套用 Suppliers Create、Update、Delete policy。
- 不新增 Migration、套件或部署設定。

## 架構

Application 以 Supplier 專屬 CQRS Query／Command 與 Handler 實作資料規則；Web Controller 僅做 MVC 映射、保護識別值與導頁。Infrastructure 現有 Supplier、Product 關聯及 DbContext 已足夠，不需變更。
