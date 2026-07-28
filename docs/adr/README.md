# Architecture Decision Records

本目錄保存 CleanArchitecture.Northwind 的架構決策紀錄。

ADR 用於記錄具備實際取捨、未來不易回復，且若缺少背景會令人困惑的重要技術或架構決策。

## Decision Records

| 編號 | 決策 | 狀態 |
|---|---|---|
| [ADR-0001](0001-use-aspnet-core-mvc-for-web-presentation.md) | Web Presentation 使用 ASP.NET Core MVC | Accepted |
| [ADR-0002](0002-do-not-use-dotnet-aspire.md) | 不使用 .NET Aspire | Accepted |
| [ADR-0003](0003-use-mapster-for-object-mapping.md) | 使用 Mapster 進行物件映射 | Accepted |
| [ADR-0004](0004-keep-data-protection-in-web-boundary.md) | Data Protection 保留在 Web 邊界 | Accepted |

## 狀態

- `Proposed`：提議中，尚未正式採用。
- `Accepted`：已採用。
- `Deprecated`：仍存在，但不建議繼續使用。
- `Superseded`：已被其他 ADR 取代。

## 維護原則

- 已接受的 ADR 原則上不直接改寫其歷史決策。
- 決策改變時，建立新的 ADR，並將舊 ADR 標示為 `Superseded`。
- 不為一般程式碼實作細節建立 ADR。