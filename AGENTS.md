# AGENTS.md

## 1. 專案定位與指令原則

本專案採用 Clean Architecture。實作與修改必須以目前 Repository 的實際結構、既有程式碼與架構決策為準。

本文件是 Repository 級工作規範，主要定義：

- 不可破壞的架構與安全邊界。
- Agent 的探索、修改、驗證與完成原則。
- Skills 的啟用條件、責任邊界與交接方式。
- `CONTEXT.md`、ADR 與其他文件的責任。

規則優先順序：

1. 使用者當前明確要求與系統／安全限制。
2. 目前工作目錄中適用且更具體的 `AGENTS.md`。
3. 本文件。
4. 已安裝 Skill 的一般流程與預設行為。

若 Skill 與本文件衝突，以較高優先順序的規則為準。

實際 Repository 結構、目前工作樹、既有程式碼與測試慣例優先於本文件中的示意名稱。不得虛構不存在的檔案、類別、方法、專案或架構。

本專案目前不使用 .NET Aspire；除非使用者明確要求評估或重新導入，否則不得新增 Aspire 專案、套件或 hosting 設定。

---

## 2. 回覆與修改原則

所有回覆使用繁體中文；程式碼、CLI 命令、路徑、API 名稱與既有識別字維持原樣。

回覆應依任務複雜度調整：

- 簡單問題直接回答結論或下一步。
- 只有比較方案或結構化結果時才使用表格。
- 只有多步驟工作或使用者要求時才展開 SOP。
- 架構說明只在涉及分層、依賴方向或跨層流程時展開。
- 完成工作時優先回報實際修改、實際驗證與尚存風險。

避免：

- 重複已知背景或完整計畫。
- 為了格式完整而產生空白章節。
- 與任務無關的重構、格式化或命名修改。
- 在已有足夠證據後繼續無限制探索。

所有修改採取最小、可維護、可驗證的方式。

---

## 3. Clean Architecture 不可違反邊界

### 3.1 分層責任

| Layer | 主要責任 | 不得包含 |
| --- | --- | --- |
| `Domain` | Entity、Value Object、Domain Event、Domain Rule | EF Core、HTTP、MVC、MediatR Handler、外部 API 或資料庫細節 |
| `Application` | Use Case、Command、Query、Handler、DTO、Validator、抽象介面 | Controller、ViewModel、Razor、`HttpContext`、DbContext 實作、外部服務實作 |
| `Infrastructure` | EF Core、Identity、Repository、File、Email、第三方 API 等技術實作 | MVC Presentation、Use Case 流程與核心業務規則 |
| `Web` | Controller、View、ViewModel、Filter、Middleware、Routing、Composition Root | 核心商業邏輯、直接操作 DbContext、直接使用 Infrastructure 實作 |
| `Shared` | 無業務規則且真正跨專案共用的型別 | Feature-specific Use Case、UI、資料存取或核心業務規則 |

### 3.2 依賴方向

必須維持：

```text
Domain
  ↑
Application
  ↑
Infrastructure

Web → Application
Web → Infrastructure only for composition root
```

具體規則：

- `Domain` 不依賴外層。
- `Application` 只依賴 `Domain` 及必要的無業務依賴共用型別。
- `Application` 不得依賴 `Infrastructure` 或 `Web`。
- `Infrastructure` 實作 `Application` 定義的抽象。
- `Infrastructure` 不得依賴 `Web`。
- `Web` 的業務流程必須透過 `Application`。
- `Web` 可於 composition root 引用 Infrastructure DI 註冊，但 Controller、ViewModel 與 View 不得直接依賴 Infrastructure 實作。
- Controller 不得直接使用 EF Core `DbContext`。
- View 不得直接使用 Domain Entity。

不得為了快速完成需求而讓 MVC、EF Core、外部服務或其他 Infrastructure detail 滲入核心層。

---

## 4. 各層實作原則

### 4.1 Web / MVC

Controller 應：

- 接收與驗證 HTTP 輸入。
- 處理 ModelState、導航與 Presentation concern。
- 將 ViewModel 映射為 Command／Query。
- 透過 `ISender` 呼叫 Application。
- 將 Application DTO 映射為 ViewModel。
- 將 `CancellationToken` 傳入非同步 Application 呼叫。

Controller 不得：

- 直接操作 `DbContext`。
- 直接建立或呼叫 Infrastructure implementation。
- 透過 Service Locator 執行 Use Case。
- 放置核心業務規則。

ViewModel 只承擔 Presentation data 與 UI validation，不得包裝 EF tracking entity 或承擔核心業務規則。

View 只負責呈現、表單輸入、基本 UI 判斷與 ModelState 錯誤呈現，不得查詢或修改資料。

UI 修改應優先沿用 Repository 現有元件、CSS 與頁面慣例，不得建立不必要的平行樣式系統。

### 4.2 Application

Application 是 Use Case 中心。

Command 用於改變狀態；Query 用於讀取資料且不得修改狀態。

Handler：

- 協調 Use Case 與 Domain 行為。
- 可依賴 Application 定義的抽象。
- 不得依賴 Infrastructure implementation。
- 不得知道 Controller、ViewModel、TempData、Razor、HTML 或 `HttpContext`。
- 若需要目前使用者資訊，應透過 Application abstraction。
- 非同步資料存取或外部呼叫應傳遞 `CancellationToken`。

驗證責任：

- Use Case 輸入規則：Application Validator。
- 不變的核心業務規則：Domain。
- 純 UI 格式與 Presentation 提示：Web ViewModel。

不得只依靠 MVC validation 保護 Use Case 邊界。

### 4.3 Domain

Domain：

- 表達核心 business concept 與 invariant。
- 不知道 MVC、EF Core、MediatR、HTTP、資料庫或外部服務。
- Domain error 優先使用穩定代碼、Result 或語意型例外。
- 不直接包含中文 UI message。

### 4.4 Infrastructure / EF Core

資料存取：

- Query 優先 projection。
- Read-only query 優先 `AsNoTracking()`。
- 避免 N+1、過早 `.ToList()` 與迴圈逐筆查詢。
- Raw SQL 必須參數化。
- 非同步查詢、儲存與外部呼叫應傳遞 `CancellationToken`。

Migration 只有在：

- 使用者明確要求；或
- 本次模型變更確實需要 schema 變更

時才新增。

Application 與 Infrastructure 應維持 abstraction / implementation 邊界，不得為了方便讓 Handler 直接依賴 EF Core implementation。

### 4.5 Shared 與 Extensions

`Shared` 只放：

- 無業務規則。
- 無 Presentation 或 Infrastructure dependency。
- 已有真實跨專案共用需求的內容。

不得因「未來可能會共用」提前移入 `Shared`。

Extension method 原則：

- Application-specific behavior 留在 Application。
- EF Core、Identity、資料庫、外部服務相關 extension 留在 Infrastructure。
- HTTP、MVC、Presentation concern 留在 Web。
- 只有真正跨專案且不含業務／技術／UI dependency 時才放 Shared。

---

## 5. Mapping 與 DTO

Mapping 邊界：

| From | To | 建議位置 |
| --- | --- | --- |
| MVC ViewModel | Command／Query | Web |
| Application DTO | MVC ViewModel | Web |
| Domain Entity | Application DTO | Application |
| EF Core Projection | Application DTO | Application Query 或 Infrastructure |
| Domain Entity | MVC ViewModel | 不得直接 mapping |

原則：

- 少量欄位或包含 UI formatting 時，優先顯式 mapping。
- 大量同名欄位、巢狀物件或集合轉換時，可沿用 Repository 已存在的 `MapsterMapper.IMapper`。
- 不得為單一需求導入新的 mapping 套件。
- `ProjectToType` 只有在 EF Core 能正確翻譯且不造成不合理查詢時使用。

Application DTO 預設與其 Command／Query Feature 共置。

`Application/Common/DTOs` 只放：

- 語意穩定。
- 演進方向一致。
- 確實供多個 Use Case 或 Feature 共用。

不同 Use Case 即使欄位類似，只要語意或演進需求不同，就維持各自 DTO，不得為了消除重複強迫共用。

---

## 6. 安全底線

必須遵守：

- Cookie authentication 下會改變狀態的 Browser request 必須使用 Anti-Forgery。
- 不得直接 bind Domain Entity 或 EF Entity，避免 overposting。
- Razor 使用預設 encoding；不得任意使用 `Html.Raw`。
- 不得輸出 password、token、secret、connection string 或未遮罩的個資。
- Log、Exception、Telemetry 不得記錄 secret 或完整受保護資料。
- 不得直接把 exception detail 顯示給終端使用者。
- 受限制操作必須使用適當的 `[Authorize]`、Policy 或資源層級授權。
- File upload 必須驗證副檔名、大小、Content-Type、路徑與檔名。
- Redirect 不得形成 open redirect；外部 URL 必須驗證。
- 驗證失敗必須沿用既有安全 UI flow 回應。

### Data Protection

Data Protection 是 Web concern。

只有當值：

- 不應直接信任 Client；
- 被竄改會改變操作；
- 或需要 purpose／有效期限限制

時才考慮保護。

規則：

- Controller 在 Web boundary 做 protect／unprotect。
- Application 與 Domain 不得依賴 `IDataProtector` 或受保護字串格式。
- ViewModel 使用 `ProtectedId` 等能表達意圖的欄位。
- purpose 必須穩定且限縮於功能範圍。
- Unprotect 失敗、格式錯誤或資料被竄改時中止操作，不洩漏 exception。
- Data Protection 不取代 authorization、resource ownership、Anti-Forgery 或 business validation。
- 有時效的 link 使用 time-limited protector；真正的一次性操作仍須伺服器端 replay protection。

---

## 7. 探索與 CodeGraph

開始修改前：

1. 先讀取使用者指定的檔案。
2. 再讀取與任務直接相關的 implementation、reference 與 test。
3. 檢查 Repository 現有命名、資料夾與實作模式。
4. 不得只依檔名或猜測推定不存在的 architecture。

涉及 Northwind business concept、terminology 或 domain boundary 時，讀取 `CONTEXT.md`。

涉及既有重要架構選擇時，檢查相關 `docs/adr/`。

### CodeGraph

下列情況優先使用 CodeGraph：

- 跨檔案或跨層流程。
- 呼叫鏈或 symbol 關聯不明。
- Public interface 修改。
- 影響範圍不明。
- 不熟悉的 module。
- 架構違規調查。

下列情況通常不需要：

- 已指定明確檔案與修改位置。
- Markdown、README、註解或文字修改。
- 單一設定值。
- 已有明確 root cause 的局部問題。
- Build、test、format 或 Git 操作。

CodeGraph 只協助探索與 impact analysis；修改前仍須直接讀取目前工作樹中的實際檔案。

若 CodeGraph 不可用或索引不足，改用 Repository search、`rg`、Git 與直接讀檔，不得阻塞整個任務，也不得假裝已使用。

---

## 8. CONTEXT.md 與 ADR

### 8.1 CONTEXT.md

Repository 使用根目錄 `CONTEXT.md` 作為 Northwind domain vocabulary 的 canonical source。

`CONTEXT.md` 只記錄：

- Northwind-specific business concept。
- Canonical terminology。
- 容易混用的名稱。
- 必要的 domain distinction。

每個概念保持簡短，通常一至兩句即可。

不得放入：

- ASP.NET Core、EF Core、MediatR、Mapster 等技術。
- 專案結構或部署方式。
- Feature specification。
- Acceptance criteria。
- Implementation plan。
- 架構 trade-off。
- 待辦事項或會議筆記。

若程式碼、使用者描述與 `CONTEXT.md` 對 domain concept 的定義衝突，必須指出差異，不得默默自行選擇。

### 8.2 ADR

ADR 位於：

```text
docs/adr/
```

只有同時符合以下三項條件才建立 ADR：

1. **Hard to reverse**：未來修改有顯著程式、資料、部署、相容性或遷移成本。
2. **Surprising without context**：未留下理由時，維護者很可能質疑這項選擇。
3. **Real trade-off**：確實存在多個可行方案，並基於具體理由做出選擇。

不得為：

- 一般 framework 慣例。
- 容易逆轉的局部 implementation choice。
- Feature acceptance criteria。
- Implementation step。
- 單純因某個 Skill 建議

建立 ADR。

既有架構決策若被取代，新增 ADR 記錄 supersede 關係；不得直接改寫歷史使舊決策看似從未存在。

---

## 9. 修改限制

除非任務明確要求或本次變更確實必要，不得：

- 大範圍重構。
- 新增 NuGet package。
- 修改部署模式。
- 修改 Docker 或 CI/CD。
- 修改 SDK 或全域套件版本。
- 修改 database schema 或新增 Migration。
- 導入新的 framework 或 hosting model。
- 修改與任務無關的 formatter、style 或 configuration。

下列區域修改時須特別保守：

- `Directory.Packages.props`
- `global.json`
- Dockerfile / compose
- CI/CD workflow
- `appsettings*.json`
- Migration
- `.editorconfig`

不得直接修改：

- `bin/`
- `obj/`
- test result output
- compiled artifact
- 工具自動產生的 OpenAPI client 或 generated file

應修改來源、template 或 generation configuration 後重新產生。

---

## 10. 測試與驗證

驗證範圍必須與風險相稱。

最低原則：

- Markdown、README、註解、純文字：檢查內容與格式，通常不需 build。
- 單一方法或單層局部修改：build 受影響 project，執行最相關測試。
- 跨層流程、public interface 或 shared component：執行相關 build、unit test 與 integration / functional test。
- Package、startup、DI、data model 或 schema：執行完整 `dotnet build` 與相關完整測試。
- Pull Request、合併前或使用者明確要求：完整 `dotnet build` 與 `dotnet test`。

常用命令：

```bash
dotnet build
dotnet test
dotnet test tests/Domain.UnitTests
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests
dotnet test tests/Infrastructure.IntegrationTests
```

只有本次工作中實際執行並確認成功的命令，才可宣稱：

- build 成功。
- test 通過。
- lint／format 通過。
- Migration 成功。
- 功能已驗證。

不得依賴：

- 舊的執行結果。
- 推測。
- 「應該可以」。
- reviewer 或 subagent 未附證據的結論。

若必要驗證無法執行，完成回覆必須說明：

- 哪些驗證未執行。
- 原因。
- 因此留下的風險。
- 可由使用者執行的具體命令。

---

## 11. Git 與產生檔案

`.gitignore` 只影響未追蹤檔案，不會自動停止追蹤已提交或 staged 的檔案。

建立 commit 前，必須檢查 staged files，例如：

```bash
git diff --cached --name-only
```

不得使用 `git add -f` 強制加入應被忽略的本機狀態、工具產物或 build output，例如：

- `.superpowers/`
- `bin/`
- `obj/`
- test result
- temporary report

若檔案已被 Git 追蹤，之後才加入 `.gitignore`，應視需要使用：

```bash
git rm --cached -- <path>
```

停止追蹤但保留本機檔案。

只有使用者明確要求時，才能為被忽略路徑建立最小範圍的版本控制例外。

---

## 12. Skills 與工作流程

本節只決定：

- 是否啟用 Skill。
- 哪個 Skill 是目前階段的 workflow owner。
- Skill 之間如何交接。

Skill 的內部 procedure 由各自目前的 `SKILL.md` 管理，不在本文件重複。

不得僅因 Skill 已安裝、名稱相關或存在極低適用可能性，就啟動完整流程。

同一生命週期階段只能有一個主要 workflow owner。Supporting Skill 與品質閘門不得自行升格為第二套完整流程。

### 12.1 Skill routing

| Skill | 啟用條件 |
| --- | --- |
| `grill-me` | 使用者明確輸入 `/grill-me`、`$grill-me`，或明確要求逐題盤問／壓力測試設計 |
| `grill-with-docs` | 使用者明確輸入 `/grill-with-docs`、`$grill-with-docs`，或要求逐題盤問並同步維護 domain 文件 |
| `writing-plans` | 多專案、多階段、public interface、schema、高風險變更；或 `grill-with-docs` 完成交接 |
| `systematic-debugging` | Root cause 不明、第一次修正失敗、測試反覆失敗、現象與理解不一致 |
| `test-driven-development` | 新增或修改明確可測且具中高風險的 behavior |
| `verification-before-completion` | 準備宣稱程式碼、設定、build、test 或功能完成 |
| `requesting-code-review` | 重大功能、複雜跨層修改、安全／權限／schema／public interface 變更或重要合併前 |
| `i-have-adhd` | 使用者明確要求 `$i-have-adhd` 或要求後續回答保持簡短、行動導向 |

一般 Markdown、README、註解、明確局部低風險修改、查詢現有程式行為、單一設定值、明確 build/test/Git 命令通常直接處理，不需額外 Skill。

### 12.2 Supporting Skill

`grilling` 僅作為 `grill-me`／`grill-with-docs` 的 Supporting Skill，除非使用者明確要求，不獨立接管流程。

`domain-modeling` 只在：

- `grill-with-docs` 流程內；或
- 使用者明確要求維護 domain model、`CONTEXT.md` 或 ADR

時啟用。

不得因對話只是提到 domain terminology，就自行修改 domain 文件。

### 12.3 grill-me

`grill-me` 的責任是：

- 找出未決事項。
- 找出矛盾與風險。
- 使用 edge case 壓力測試設計。

若 Repository 已能回答某項事實，不得把相同事實重新詢問使用者。

除非使用者另有要求，`grill-me` 不得：

- 修改 `CONTEXT.md`。
- 建立 ADR。
- 建立 Implementation Plan。
- 開始實作。

### 12.4 grill-with-docs

`grill-with-docs` 負責確認：

- 要做什麼。
- 為什麼做。
- Domain terminology。
- Business rule。
- 重要 edge case。
- 必要 architecture decision。

流程中：

- Repository 或文件可回答的事實不得重新詢問。
- 真正未決的設計問題一次只提出一個。
- 問題應附上 Codex 的建議答案與理由。
- 確認的 domain terminology 應同步到 `CONTEXT.md`。
- 只有符合 ADR 門檻的決策才建立 ADR。

直到使用者明確確認「已達成共同理解」前，不得：

- 開始修改程式碼。
- 修改設定。
- 建立 Migration。
- 產生最終 Implementation Plan。

達成共同理解後：

```text
grill-with-docs
        ↓
CONTEXT.md / 必要 ADR 已同步
        ↓
若原始目標包含實作
        ↓
writing-plans
        ↓
implementation
```

若使用者只要求 grilling、domain 文件或 design confirmation，則停在使用者要求的階段。

### 12.5 writing-plans

`writing-plans` 將已確認需求轉換成可執行的 Repository implementation plan。

由 `grill-with-docs` 交接而來時，`writing-plans` 是必要階段，不得因：

- 修改看似簡單。
- grilling 已談過 implementation。
- Codex 自認知道如何修改。
- 工作預估可在單一 Session 完成

而省略。

由 `grill-with-docs` 交接的 plan 應在同一份文件包含：

#### 已確認規格

只整理已確認內容，例如：

- 問題與目標。
- 預期 behavior／主要 use case。
- 已確認 business rule 與限制。
- Acceptance criteria。
- 重要 edge case。
- Out of Scope。
- 限制 implementation direction 的 ADR 或架構決策。

不得自行新增尚未確認的需求或 business rule。

#### 實作計畫

依目前 Repository 實際狀態描述：

- 受影響 project／component／file。
- Layer boundary 如何維持。
- 預計新增或修改的 behavior。
- Test strategy。
- Implementation order。
- Schema、Migration、public API 或 compatibility impact。
- 每個主要步驟的 verification。

若 planning 時發現會改變：

- Requirement。
- Domain model。
- Public interface。
- Schema。
- Architecture direction。

的 specification gap，不得自行決定；應回到 grilling 釐清後再更新 plan。

Implementation Plan 完成前不得開始修改程式碼、Migration 或設定。

目前不將獨立 specification 文件設為必要流程；除非使用者明確要求或未來確認其具有獨立長期維護價值，否則不額外引入 `to-spec` 作為 mandatory stage。

### 12.6 TDD、Debugging、Verification 與 Review

#### test-driven-development

新增或修改明確可測且具中高風險 behavior 時使用。

測試聚焦 observable behavior 與 Use Case，不得為追求測試數量而綁死 implementation detail。

文件、純文字或極小型低風險修改不強制完整 TDD。

#### systematic-debugging

用於回答「為什麼會壞」。

當 root cause 已有直接證據且修正範圍局部時，可直接修正、補適當測試並進行 verification，不必重新執行完整 debugging ceremony。

不得以猜測取代 root-cause evidence。

#### verification-before-completion

只要準備宣稱工作完成或驗證成功，就必須取得與聲明相符的最新實際證據。

即使任務本身沒有使用其他 Skill，也不得略過必要 verification。

#### requesting-code-review

重大、複雜或高風險修改完成後應考慮獨立 review。

Review 若發現 requirement 或 architecture decision gap，不得只用 code patch 掩蓋，應回到對應的 specification、grilling 或 ADR 階段。

### 12.7 Workflow 原則

一般生命週期：

```text
必要時：
grill-me / grill-with-docs
        ↓
需求與設計收斂
        ↓
必要時：
writing-plans
        ↓
implementation
        │
        ├─ behavior change → test-driven-development
        │
        └─ unknown failure → systematic-debugging
        ↓
verification-before-completion
        ↓
必要時：
requesting-code-review
```

若後續階段發現前一階段其實尚未收斂，應回到適當階段處理，不得由後續 Skill 猜測未確認決策。

未來若安裝另一套與既有 TDD、debugging、planning、verification 或 review workflow 重疊的 Skill，必須先決定 canonical workflow，不得讓兩套完整流程同時自動接管同一階段。

---

## 13. 實作順序

只有任務實際涉及時才使用對應 layer。

典型順序：

```text
Domain rule
    ↓
Application Use Case
    ↓
Infrastructure implementation
    ↓
Web Presentation
```

這不是要求每個功能都必須修改四層。

不得為了讓流程看起來完整而建立不需要的：

- Entity。
- Interface。
- Repository。
- DTO。
- Service。
- Test project。
- Migration。
- Abstraction。

既有簡單實作如果已符合 layer boundary，不得為了套模板而增加不必要 abstraction。

---

## 14. 任務完成回覆

### 小型或局部任務

只需說明：

1. 修改內容或結論。
2. 實際執行的驗證。
3. 必要的未驗證事項或風險。

不存在的項目不要建立空標題。

### 跨檔案、跨層或 Pull Request

依實際需要使用：

- `變更摘要`
- `架構判斷`
- `測試結果`
- `風險與後續建議`

未涉及的區段省略。

任何完成回覆都不得：

- 把未執行的驗證寫成通過。
- 把推測寫成實際結果。
- 隱藏失敗測試或未驗證風險。
- 宣稱不存在的檔案或修改。