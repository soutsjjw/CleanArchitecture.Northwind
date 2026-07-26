# Architecture Decision Records

本檔是方便審閱的合併版本。Repository 內的 canonical ADR 應分別存放於 `docs/adr/0001-*.md`、`docs/adr/0002-*.md` 等依序編號檔案。

## ADR-0001：Web Presentation 使用 ASP.NET Core MVC

**Status:** Accepted  
**Date:** 2026-07-26

本專案以 `jasontaylordev/CleanArchitecture` 為基礎，但 Web Presentation 採用 ASP.NET Core MVC，而不是以 Minimal API 作為主要使用者介面。MVC 的 Controller、ViewModel 與 Razor View 留在 `src/Web`，業務流程仍透過 Application 的 Command、Query 與 Handler 執行，以保留 Clean Architecture 邊界並符合伺服器端頁面需求。

**Considered options:** Minimal API、ASP.NET Core MVC、另建獨立前端。  
**Consequences:** Controller 必須保持薄；View 不得直接接收 Domain Entity；若需要 API，可使用 API Controller 與 MVC 並存，但不可建立第二套業務流程。

---

## ADR-0002：不使用 .NET Aspire

**Status:** Accepted  
**Date:** 2026-07-26

本專案不保留或重新導入 `.NET Aspire` 的 `AppHost`、`ServiceDefaults`、`TestAppHost`、hosting 套件或相關啟動模型。現有開發與部署方式不需要 Aspire 所提供的應用程式協調層，保留它會增加專案、啟動與部署心智負擔；Web 專案應可使用一般 `dotnet` 工作流程直接建置與啟動。

**Considered options:** 保留 Aspire、僅保留 ServiceDefaults、完整移除 Aspire。  
**Consequences:** 遙測、Health Check 與外部服務設定必須由既有 Web／Infrastructure 組態負責；未來重新導入 Aspire 前必須建立新的 ADR，說明部署需求與遷移成本。

---

## ADR-0003：物件映射使用 Mapster

**Status:** Accepted  
**Date:** 2026-07-26

本專案以 Mapster 作為既有的物件映射工具，不混用 AutoMapper，也不為單一功能新增其他映射套件。統一映射方式可避免平行設定、註冊差異與維護者無法判斷應採何種模式；少量欄位、UI 格式化或含重要判斷的轉換仍應使用明確程式碼，不把行為隱藏在映射設定中。

**Considered options:** AutoMapper、Mapster、全部手動映射。  
**Consequences:** Domain／Application DTO 的映射設定留在可看見相關型別的層；Application DTO 到 MVC ViewModel 的映射留在 Web；EF Core 查詢只有在可正確轉譯時才使用投影映射。

---

## ADR-0004：Data Protection 留在 Web 邊界

**Status:** Accepted  
**Date:** 2026-07-26

ASP.NET Core Data Protection 用於保護 Controller 與 View 之間不應直接暴露或可被竄改的識別值，屬於 Web Presentation concern。`Application` 與 `Domain` 只接收還原後的 `int`、`Guid` 或 Value Object，不依賴 `IDataProtector`、purpose 字串或受保護資料格式，避免 Web 安全機制滲入核心層。

**Considered options:** 在每個 Controller 直接處理、在 Web 建立共用封裝、把保護服務放入 Application 或 Infrastructure。  
**Consequences:** 多處共用時應在 Web 建立明確封裝；purpose 必須依功能分隔；Production key ring 必須持久化；Data Protection 不取代授權、Anti-Forgery、輸入驗證、時效控制或防重放機制。
