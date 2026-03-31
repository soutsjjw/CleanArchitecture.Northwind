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
- ports / volumes / networks
- security headers

限制：
- 僅允許修改應用程式程式碼（Domain / Application / Infrastructure / MVC）
- 不得修改任何部署、網路、安全標頭、容器與環境設定
- 若真正修復需要部署層變更，僅列出建議，不直接修改
- 不得以部署調整取代程式修正
- 不可新增 NuGet 套件
- 不可變更 public API contract（除非需求明確要求）
- 優先採用最小修改
- 不可弱化現有安全控制
- 若部署為真正根因，必須清楚標示「程式端可做」與「部署端需配合」的邊界

如果問題可能來自部署，請只說明原因，不要修改設定。

請輸出：
1. 問題理解
2. 可在應用程式層處理的修正方案
3. 變更檔案
4. 修改程式碼
5. 驗證方式
6. 若仍需部署層配合，請列出建議但不要修改
7. 風險與邊界說明
