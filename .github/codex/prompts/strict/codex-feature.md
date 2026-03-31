請依 AGENTS.md，實作一個新的 MediatR CQRS 功能。

需求：
[填入需求]

規則：
- Command / Query 分離
- Handler 單一職責
- Controller 保持薄
- 商業邏輯在 Application
- DTO 必須存在
- 不回傳 Entity
- 新增檔案與資料夾命名必須遵循既有 feature 結構
- 不可自創新的資料夾分層或架構模式
- 新增程式碼前，先參考鄰近功能的既有寫法
- 若涉及資料庫變更，先說明是否真的需要 migration，不主動產生 migration
- 不可變更既有 public API contract（除非需求明確要求）
- 不可修改 Docker / Nginx / CI/CD / deployment 設定
- 不可新增 NuGet 套件（除非需求明確要求）
- 不可將商業邏輯放入 Controller / View / Infrastructure 邊界之外
- 不可繞過既有 validation / authorization / mapping 慣例

請優先遵循：
- 現有資料夾結構
- 現有命名慣例
- 現有 DI / Mapping / Validation 模式

請輸出：
1. 需求理解
2. 新增哪些檔案
3. 放在哪個資料夾
4. 修改 / 新增程式碼
5. 是否需要測試
6. 是否影響資料庫
7. 驗證方式
8. 風險說明（若有）
9. 為何此做法符合既有結構
