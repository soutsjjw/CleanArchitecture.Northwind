# 營運儀表板設計

**日期：** 2026-07-24  
**範圍：** `Home` 首頁營運儀表板

## 目標

將既有空白首頁改為營運儀表板，讓具備全域訂單與商品讀取權限的使用者快速掌握訂單、營收、出貨、庫存與主要客戶。

## 授權與存取

- 首頁與其資料更新端點必須同時要求既有的 `Orders:Read:All` 與 `Products:Read:All` 權限。
- 未登入使用者依 ASP.NET Core Cookie 驗證流程導向登入頁。
- 已登入但權限不足者由既有授權機制拒絕存取。
- 不新增 `Dashboard` 專屬權限；全域儀表板資料的可見性以既有全域訂單與商品讀取權限表達。
- 系統管理員與管理員角色延續既有授權處理器的通過行為。

## 架構與資料流

採用單一唯讀 `GetOperationsDashboardQuery`：

```text
Browser
  -> HomeController
  -> ISender.Send(GetOperationsDashboardQuery)
  -> Application Query Handler
  -> IApplicationDbContext EF Core projection
  -> Dashboard DTO
  -> Home ViewModel / JSON
  -> Razor cards, tables, Chart.js charts
```

- `Application` 定義 Query、DTO 與 Handler；Handler 使用 `IApplicationDbContext` 的可翻譯 EF Core projection，read-only 查詢使用 `AsNoTracking()`。
- `Web` 的 Controller 只處理授權、呼叫 Query，以及 DTO 至首頁 ViewModel／JSON 的映射。
- Razor 不直接查詢資料庫；Chart.js 只處理已由伺服器提供的資料。
- 首頁首次以 Razor ViewModel 繪製；同一份 Query 也供受保護的 GET 更新端點使用。

## 指標定義

所有日期以伺服器目前本地日期與當月區間判定，訂單歸屬以 `OrderDate` 為準。`OrderDate` 為空的訂單不納入今日或本月指標。

| 區塊 | 定義 | 呈現 |
| --- | --- | --- |
| 今日訂單 | `OrderDate` 為今日的訂單筆數 | 摘要卡片 |
| 今日營收 | 今日訂單明細的 `UnitPrice × Quantity × (1 - Discount)` 總和，不含運費 | 摘要卡片 |
| 本月訂單 | `OrderDate` 位於本月的訂單筆數 | 摘要卡片 |
| 本月營收 | 本月訂單明細的折後金額總和，不含運費 | 摘要卡片 |
| 出貨狀態 | 本月訂單依下列三類分組：`ShippedDate` 有值為已出貨；未出貨且未超過 `RequiredDate` 為待出貨；未出貨且已超過 `RequiredDate` 為已逾期 | Chart.js 圓環圖 |
| 低庫存商品 | 未停售且 `UnitsInStock <= ReorderLevel` 的商品，依 `(ReorderLevel - UnitsInStock)` 由大到小，取前 5 名 | 表格：商品名稱、現有庫存、再訂購水準、缺口 |
| Top 客戶 | 本月折後營收最高的前 5 名客戶；同額時訂單數較多者優先 | 表格：公司名稱、訂單數、營收 |

出貨狀態中 `RequiredDate` 為空且尚未出貨的訂單，歸為「待出貨」，避免將沒有承諾出貨日的資料誤標為逾期。

## 前端與更新

- 使用 LibMan 下載 Chart.js 至 `wwwroot/lib`，不使用 CDN。
- 使用既有 Bootstrap、Font Awesome 與全域版型；新增的頁面樣式僅限於儀表板需要的佈局與圖表高度。
- 首次載入後，瀏覽器每 10 分鐘呼叫同一控制器的唯讀更新端點。
- 更新成功時替換摘要卡片與表格內容、銷毀並重建／更新 Chart.js 實例；失敗時保留最後成功畫面並以既有 toast 顯示一般性錯誤，不揭露例外細節。
- 不增加輪詢以外的即時推播、篩選器、鑽研連結或資料寫入行為。

## 測試與驗證

- Application 單元測試：今日／本月界線、折後營收、三種出貨分類、空 `RequiredDate`、低庫存篩選與排序、Top 客戶排名與同額排序。
- MVC／整合測試：未登入導向登入；缺少任一全域讀取權限時拒絕；同時具備兩個權限時首頁與更新端點可用。
- 驗證期間執行受影響測試專案與 `dotnet build`；若新增或調整跨層註冊，執行完整方案建置。

## 不在本次範圍

- 新增資料庫欄位、Migration、套件版本調整或新的專屬儀表板權限。
- 即時推播、可設定日期區間、匯出、鑽研頁面與可編輯儀表板小工具。
