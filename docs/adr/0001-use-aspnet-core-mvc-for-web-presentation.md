---
status: accepted
date: 2026-07-26
---

# Use ASP.NET Core MVC for Web Presentation

本專案以 `jasontaylordev/CleanArchitecture` 為基礎，但 Web Presentation 採用 ASP.NET Core MVC，而不是以 Minimal API 作為主要使用者介面。MVC 的 Controller、ViewModel 與 Razor View 留在 `src/Web`，業務流程仍透過 Application 的 Command、Query 與 Handler 執行，以保留 Clean Architecture 邊界並符合伺服器端頁面需求。

## Considered Options

- Minimal API
- ASP.NET Core MVC
- 獨立前端搭配 Web API

## Consequences

Controller 必須保持薄，View 不得直接接收 Domain Entity。若需要 API，可使用 API Controller 與 MVC 並存，但不可建立第二套業務流程。
