請依照 AGENTS.md 規範進行修改，並嚴格遵守 Clean Architecture 分層。

任務：
[在此填寫需求]

Context：
- .NET 8
- Clean Architecture
- MediatR / CQRS
- MVC + Application + Infrastructure 分層
- 不可破壞既有結構
- 需優先遵循 repository 既有模式與命名

Constraints：
- 最小修改（minimal change）
- 不修改無關檔案
- 不新增 NuGet 套件
- 不引入新架構 / 新框架 / 新資料夾分層
- 不改 Docker / Nginx / 部署設定
- 不降低安全性
- 不變更 public routes / request DTO / response schema / configuration keys（除非需求明確要求）
- 不主動新增 EF Core migration（除非需求明確要求）
- 先分析既有實作模式，再提出修改
- 若需求不明確，採最保守且相容既有行為的做法
- 若有多種做法，優先選擇與現有程式風格最一致者
- 不可弱化 validation / auth / filters / middleware / model binding protections
- 不可因順手優化而擴大修改範圍

Done when：
- build 成功
- 測試可執行
- 不破壞既有行為
- 符合 AGENTS.md
- 未引入分層違規
- 未修改無關檔案

請輸出：
1. 需求理解
2. 修改計畫
3. 變更檔案
4. 修改程式碼
5. 驗證方式
6. 風險說明（若有）
7. 未處理事項（若有）
