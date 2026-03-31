請依 AGENTS.md 修補安全問題。

問題：
[填入掃描報告或風險]

要求：
- 防止 Mass Assignment
- 使用 DTO 控制輸入
- 不移除 validation / auth
- 不暴露敏感欄位
- 不修改 Nginx / Docker
- 優先沿用既有 Validator / Filter / Binder / Middleware 模式
- 不可為了通過掃描而改變既有商業行為，除非需求明確要求

請輸出：
1. 風險分析
2. 修補策略
3. 修改檔案
4. 修改程式碼
5. 驗證方式（AppScan / ZAP）
