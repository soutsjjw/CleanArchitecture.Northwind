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
- 不可變更既有 public API contract（除非需求明確要求）
- 不主動產生 migration
- 不修改 Docker / Nginx / 部署設定

請輸出：
1. 新增哪些檔案
2. 放在哪個資料夾
3. 修改 / 新增程式碼
4. 是否需要測試
5. 是否影響資料庫
6. 驗證方式
