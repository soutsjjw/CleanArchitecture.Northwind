請依 AGENTS.md 修補安全問題。

問題：
[填入掃描報告或風險]

要求：
- 防止 Mass Assignment
- 不允許未知欄位寫入 entity
- 使用 DTO 控制輸入
- 不移除 validation / auth
- 不暴露敏感欄位
- 不修改 Nginx / Docker
- 不得停用既有 validation / auth / model binding protection / filters / middleware
- 優先採用既有 DTO、Validator、Filter、Binder、Middleware 模式修補
- 不可為了通過掃描而改變既有商業行為，除非需求明確要求
- 若為掃描誤報，需明確說明原因與保留風險
- 不可變更 public routes / request DTO / response schema / configuration keys（除非需求明確要求）
- 不可新增 NuGet 套件（除非需求明確要求）
- 不可主動新增 migration（除非需求明確要求）
- 不可用關閉檢查、放寬驗證、繞過授權的方式修補問題
- 修補後仍需維持既有功能可用

請輸出：
1. 風險分析
2. 修補策略
3. 修改檔案
4. 修改程式碼
5. 驗證方式（AppScan / ZAP）
6. 殘餘風險或誤報說明（若有）
7. 為何此修補不會破壞既有商業行為
