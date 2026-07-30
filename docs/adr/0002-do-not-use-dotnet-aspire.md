---
status: accepted
date: 2026-07-26
---

# Do Not Use .NET Aspire

本專案不保留或重新導入 `.NET Aspire` 的 `AppHost`、`ServiceDefaults`、測試用 hosting 專案、hosting 套件或相關啟動模型。現有開發與部署方式不需要 Aspire 所提供的應用程式協調層，保留它會增加專案、啟動與部署心智負擔；Web 專案應可使用一般 `dotnet` 工作流程直接建置與啟動。

## Considered Options

- 保留完整 Aspire
- 僅保留 ServiceDefaults
- 完整移除 Aspire

## Consequences

遙測、Health Check 與外部服務設定由既有 Web／Infrastructure 組態負責。未來重新導入 Aspire 前必須建立新的 ADR，說明部署需求與遷移成本。
