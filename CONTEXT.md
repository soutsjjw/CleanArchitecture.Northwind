# Northwind

Northwind 描述客戶向公司購買商品、公司向供應商取得商品，以及員工管理銷售訂單、出貨責任與營運資料的業務語言。本文件只定義專案內應一致使用的領域詞彙，不記錄實作技術或架構決策。

## Language

### Identity and responsibility

**User**:
可登入系統並被授予 Role 的身分。User 不等同於 Employee，也不等同於 Customer。
_Avoid_: Account、Login Account、Customer User

**Role**:
授予 User 的營運責任集合；目前標準角色名稱為 Administrator、Sales、Warehouse、Purchase、Finance 與 Customer Service。
_Avoid_: Permission Group、User Type

**Employee**:
Northwind 內部人員，可負責客戶、Sales Order 或 Territory。除非有明確關聯，Employee 不代表可登入系統的 User。
_Avoid_: User、Staff Account

### Business parties

**Customer**:
向 Northwind 購買 Product 並建立 Sales Order 的個人或組織。
_Avoid_: User、Account、Client、Buyer

**Supplier**:
向 Northwind 提供 Product 的外部組織。
_Avoid_: Vendor、Provider

**Shipper**:
負責將 Sales Order 送往 Customer 的運送服務提供者。
_Avoid_: Supplier、Carrier、Delivery Company

### Product catalog

**Category**:
用來歸類 Product 的商業分類。
_Avoid_: Product Type、Group

**Product**:
由 Supplier 提供、可被加入 Sales Order 的銷售品項。
_Avoid_: Item、SKU（除非明確指庫存代碼）

**Discontinued Product**:
已停止提供新銷售，但歷史 Sales Order 仍可能引用的 Product。
_Avoid_: Deleted Product、Inactive Item

### Sales

**Sales Order**:
Customer 向 Northwind 提出的商品購買交易，包含訂購、出貨與運費資訊。
_Avoid_: Purchase Order、Transaction、Order（語意可能與採購混淆時）

**Sales Order Line**:
Sales Order 中的一個 Product 明細，記錄成交單價、數量與折扣。
_Avoid_: Order Item、Line Item、Order Detail

**Order Date**:
Sales Order 被建立或正式接受的日期。
_Avoid_: Created Date、Purchase Date

**Required Date**:
Customer 期望收到 Sales Order 的日期。
_Avoid_: Due Date、Delivery Deadline

**Shipped Date**:
Sales Order 實際交付給 Shipper 的日期。
_Avoid_: Delivery Date、Completed Date

**Freight**:
Sales Order 的運送費用。
_Avoid_: Shipping Price、Delivery Fee

**Ship To**:
Sales Order 的收件名稱與地址；它可以與 Customer 的主要資料不同。
_Avoid_: Customer Address、Billing Address

### Geography and assignment

**Region**:
由多個 Territory 組成的高階營運地理範圍。
_Avoid_: Area、Zone

**Territory**:
隸屬於一個 Region，並可指派給 Employee 的營運區域。
_Avoid_: Region、District

**Employee Territory Assignment**:
Employee 與 Territory 之間的責任關係；一名 Employee 可被指派多個 Territory。
_Avoid_: Employee Region、Territory Ownership

### Customer classification

**Customer Demographic**:
用來描述 Customer 類型或特徵的分類。
_Avoid_: Customer Role、Customer Segment（除非已明確採用為另一概念）

**Customer Demographic Assignment**:
Customer 與 Customer Demographic 之間的分類關係。
_Avoid_: Customer Type、Customer Tag

### Traceability

**Audit Record**:
描述誰在何時對營運資料執行何種動作的追蹤紀錄。
_Avoid_: Activity History、Change History（未指出追蹤目的時）
