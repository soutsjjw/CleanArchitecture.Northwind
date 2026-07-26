---
status: accepted
date: 2026-07-26
---

# Use Mapster for Object Mapping

本專案以 Mapster 作為既有的物件映射工具，不混用 AutoMapper，也不為單一功能新增其他映射套件。統一映射方式可避免平行設定、註冊差異與維護者無法判斷應採何種模式；少量欄位、UI 格式化或含重要判斷的轉換仍應使用明確程式碼，不把行為隱藏在映射設定中。

## Considered Options

- AutoMapper
- Mapster
- 全部手動映射

## Consequences

Domain／Application DTO 的映射設定留在可看見相關型別的層；Application DTO 到 MVC ViewModel 的映射留在 Web；EF Core 查詢只有在可正確轉譯時才使用投影映射。
