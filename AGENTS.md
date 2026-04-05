# Repository Guidelines (中英雙語強化版)

---

## 🌏 Output Language（輸出語言｜最高優先）

- ALL assistant-facing text MUST be in Traditional Chinese.
- This includes:
  - plans
  - progress updates
  - code explanations
  - review comments
  - commit organization summaries
  - final answers
- Do NOT reply in English unless the content is:
  - code
  - shell commands
  - file names
  - branch names
  - log output
  - commit subject lines explicitly requested to stay in English
- If any other instruction conflicts with this rule, this rule wins

## 🎯 Core Principles（核心原則）

- Always follow Clean Architecture（必須遵守分層架構）
- Prefer minimal changes（優先最小修改）
- Do NOT introduce new patterns unless requested（未明確要求不可引入新架構）
- Maintain consistency with existing code（維持既有風格）
- Prioritize correctness over optimization（正確性優先於優化）

---

## 🧱 Project Structure（專案結構）

- Domain → business models only（純業務邏輯）
- Application → use cases, DTOs, validation（用例層）
- Infrastructure → EF Core, services（基礎設施）
- MVC → Controllers & UI（表現層）

❗ Rules（強制規則）:
- Domain MUST NOT depend on any other layer
- Application MUST NOT depend on Infrastructure
- MVC MUST NOT access Infrastructure directly

---

## ⚙️ Coding Conventions（編碼規範）

- Use PascalCase for types（型別使用 PascalCase）
- Use camelCase for variables（變數使用 camelCase）
- Use `_camelCase` for private fields（私有欄位）
- Avoid `var` unless obvious（避免濫用 var）
- Use file-scoped namespace

---

## 🧠 Architecture Conventions（架構規範）

- Controllers should remain thin（Controller 僅負責轉發）
- Controllers MUST use MediatR（必須透過 MediatR）
- Do NOT put business logic in Controllers（不可寫業務邏輯）
- Do NOT expose Domain Entities（不可暴露 Entity）
- Always use DTOs（必須使用 DTO）

---

## 🔄 CQRS / MediatR Rules（強制）

- Commands MUST end with `Command`
- Queries MUST end with `Query`
- Handlers MUST match naming
- One handler per use case（單一責任）

---

## 🧩 Mapping Rules（Mapster / AutoMapper）

- Mapping MUST be centralized（集中管理）
- Do NOT duplicate mapping logic（避免重複 mapping）
- Prefer Mapster if already used（優先使用既有工具）
- Do NOT mix Mapster & AutoMapper in same feature（禁止混用）

---

## 🗄️ EF Core Rules（資料存取）

- Do NOT expose DbContext outside Infrastructure
- Avoid N+1 queries
- Prefer projection (`Select`)
- Do NOT call `.ToList()` too early
- Keep IQueryable in Application, execute in Infrastructure

---

## 🔐 Security Rules（AppScan / ZAP Friendly）

- Do NOT weaken validation（不可降低驗證）
- Prevent Mass Assignment（禁止未知欄位綁定）
- Do NOT expose sensitive data（不可暴露敏感資料）
- Always validate input（必須驗證輸入）
- Use DTO for boundary control

---

## 🚫 DO NOT MODIFY（禁止區域 🔥）

❗ ABSOLUTELY DO NOT TOUCH:

- Dockerfile
- docker-compose.yml / compose.yml
- nginx configuration
- deployment scripts
- CI/CD pipelines

❗ DO NOT CHANGE:

- ports / volumes / networks
- environment variable names
- TLS / CORS / CSP settings

👉 If needed, explain but DO NOT modify（只能說明）

---

## 🧪 Testing Rules（測試規範）

- Domain → Domain.UnitTests
- Application → Application.UnitTests
- Features → Application.FunctionalTests
- Infrastructure → IntegrationTests

---

## 🧾 Commit Rules（提交規範）

- One concern per commit（單一責任）
- Clear message（清楚描述）
- Include impact analysis（影響說明）

---

## 🚀 Common Task Workflow（開發流程）

### Add Feature（新增功能）

1. Add use case in Application
2. Add DTO + Validator
3. Implement Infrastructure
4. Connect Controller
5. Add Tests

---

### DB Change（資料庫變更）

1. Update Entity
2. Review impact
3. Add Migration
4. Run EF command

---

## ⚠️ Error Handling（錯誤處理）

- Do NOT swallow exceptions
- Use application-level exceptions
- Log at boundary (Controller / Middleware)
- Do NOT leak internal details

---

## 🧠 Agent Working Rules（AI 行為規則）

- Prefer minimal diff（最小差異）
- Do NOT refactor unrelated files（不可亂改）
- Inspect nearby code before modifying（先觀察）
- Follow existing patterns（遵循既有設計）

---

## 🧭 Decision Priority（決策優先順序）

1. Correctness（正確性）
2. Minimal change（最小修改）
3. Consistency（一致性）
4. Readability（可讀性）
5. Performance（效能）

---

## 🧪 Validation Checklist（驗證）

- `dotnet build` passes
- Tests pass
- Architecture rules respected
- No unintended side effects

---

## 🧨 Anti-Patterns（禁止）

- ❌ Business logic in Controller
- ❌ Direct DbContext usage in MVC
- ❌ Returning Entity directly
- ❌ Mixing mapping tools
- ❌ Breaking layer dependency

---

## 🧩 Advanced Rules（進階）

- Respect existing DI patterns
- Do NOT introduce global state
- Avoid static service usage
- Prefer interface abstraction

---

## 🧠 Codex Optimization（關鍵優化🔥）

When modifying code:

- Explain BEFORE large changes
- Show summary AFTER changes
- Group related changes
- Avoid over-engineering

---

## 📌 Summary（總結）

This repository enforces:

- Clean Architecture
- Strict layering
- Secure coding
- Minimal impact changes

---

## 🔥 AI Enforcement Rules（STRICT｜Codex 專用強化規則）

> This section overrides general guidance when AI (Codex) is making changes.  
> 當 AI（Codex）進行修改時，此區規則優先。

### 🚫 Absolute Restrictions（絕對禁止）

- Do NOT modify Docker-related files（禁止修改 Docker）
  - Dockerfile
  - docker-compose.yml / compose.yml

- Do NOT modify nginx configuration（禁止修改 Nginx）
- Do NOT modify deployment / CI/CD scripts（禁止修改部署腳本）

- Do NOT change:
  - ports / volumes / networks
  - environment variable names
  - TLS / CORS / CSP / security headers

👉 If changes are required, explain only. DO NOT modify.  
👉 如有需要，僅提供說明，不得直接修改。

---

### ⚙️ Change Scope Control（變更範圍控制）

- Prefer minimal diff（優先最小修改）
- Do NOT refactor unrelated files（禁止修改無關檔案）
- Do NOT perform large refactors unless explicitly requested（未要求不得大改）

- Keep changes localized（變更應局部）
- Preserve existing architecture（維持既有架構）

---

### 🧱 Architecture Protection（架構保護）

- Do NOT break Clean Architecture layering（不可破壞分層）
- MVC MUST NOT access Infrastructure directly
- Application MUST NOT depend on Infrastructure
- Domain MUST remain independent

---

### 🔐 Security Enforcement（安全強制）

- Do NOT weaken validation（不可降低驗證）
- Do NOT allow unknown fields binding（防止 Mass Assignment）
- Do NOT expose sensitive fields（不可暴露敏感資料）
- Always use DTO for input/output boundaries

---

### 🧠 Coding Behavior（AI 行為控制）

When modifying code:

- Inspect nearby code before changes（先觀察既有寫法）
- Follow existing patterns（遵循既有模式）
- Do NOT introduce new frameworks or libraries（不可引入新技術）

---

### 🧪 Safety Checks（修改後檢查）

After changes, ensure:

- Solution builds successfully
- No layer violation introduced
- No unrelated files modified
- Existing behavior not broken

---

### 📌 Output Rules（輸出規則）

- Summarize changes briefly（提供變更摘要）
- Avoid verbose explanation（避免冗長說明）
- Highlight impacted files only（只列出影響檔案）

---

### 🧭 Decision Priority（決策優先）

1. Correctness（正確性）
2. Minimal change（最小修改）
3. Consistency（與既有一致）
4. Readability（可讀性）
5. Performance（效能，非必要不優先）

---


👉 AI must follow rules strictly（必須嚴格遵守）