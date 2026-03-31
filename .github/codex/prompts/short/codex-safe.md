請修改應用程式程式碼，但嚴格禁止修改部署相關內容。

任務：
[填入需求]

禁止修改：
- Dockerfile
- docker-compose / compose.yml
- nginx
- CI/CD
- deployment scripts
- TLS / CORS / CSP
- environment variables

限制：
- 僅允許修改 Domain / Application / Infrastructure / MVC
- 若問題可能來自部署，請只說明原因，不要修改設定
- 優先最小修改
- 不可變更 public API contract（除非需求明確要求）

請輸出：
1. 修正方案
2. 變更檔案
3. 修改程式碼
4. 驗證方式
5. 若需部署層配合，請列出建議
