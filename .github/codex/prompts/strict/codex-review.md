請依 AGENTS.md 審查以下變更，不直接修改程式碼。

審查內容：
[貼 code / diff / PR 說明]

要求：
- 檢查 Clean Architecture 分層是否被破壞
- 檢查是否有 DTO / Entity 邊界洩漏
- 檢查是否有安全回歸
- 檢查是否有不必要的大改
- 檢查是否碰到 Docker / nginx / 部署相關檔案
- 檢查是否變更 public routes / request DTO / response schema / configuration keys
- 檢查是否引入不必要的 migration / package / framework
- 檢查是否違反既有命名與資料夾結構
- 檢查是否有 validation / auth / binder / middleware 被弱化
- 檢查是否有過度工程化或不一致的寫法

請輸出：
1. 高風險問題
2. 中風險問題
3. 低風險 / 可改善建議
4. 是否建議合併
5. 若不建議合併，請列出最小修正方向
6. 需要特別回歸測試的區域
