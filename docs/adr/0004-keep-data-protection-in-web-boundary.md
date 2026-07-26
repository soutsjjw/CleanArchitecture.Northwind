---
status: accepted
date: 2026-07-26
---

# Keep Data Protection in the Web Boundary

ASP.NET Core Data Protection 用於保護 Controller 與 View 之間不應直接暴露或可被竄改的識別值，屬於 Web Presentation concern。`Application` 與 `Domain` 只接收還原後的 `int`、`Guid` 或 Value Object，不依賴 `IDataProtector`、purpose 字串或受保護資料格式，避免 Web 安全機制滲入核心層。

## Considered Options

- 在每個 Controller 直接處理
- 在 Web 建立共用封裝
- 把保護服務放入 Application 或 Infrastructure

## Consequences

多處共用時應在 Web 建立明確封裝；purpose 必須依功能分隔；Production key ring 必須持久化。Data Protection 不取代授權、Anti-Forgery、輸入驗證、時效控制或防重放機制。
