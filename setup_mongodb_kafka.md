# Hướng dẫn Setup MongoDB & Kafka + Vị trí MongoDB trong code

## 1️⃣ Cài MongoDB (bằng Docker)

```bash
docker run -d --name mongodb -p 27017:27017 mongo:7
```

Verify:
```bash
docker ps | findstr mongodb
# Hoặc mở browser: http://localhost:27017 → hiện "It looks like you are trying..."
```

> Nếu muốn cài native (không Docker): tải tại https://www.mongodb.com/try/download/community

---

## 2️⃣ Cài Kafka (bằng Docker)

```bash
docker run -d --name kafka -p 9092:9092 -e KAFKA_CFG_PROCESS_ROLES=broker,controller -e KAFKA_CFG_NODE_ID=1 -e KAFKA_CFG_CONTROLLER_QUORUM_VOTERS=1@localhost:9093 -e KAFKA_CFG_LISTENERS=PLAINTEXT://:9092,CONTROLLER://:9093 -e KAFKA_CFG_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092 -e KAFKA_CFG_CONTROLLER_LISTENER_NAMES=CONTROLLER bitnami/kafka:latest
```

Verify:
```bash
docker ps | findstr kafka
```

---

## 3️⃣ Vị trí MongoDB trong code

MongoDB dùng làm **Read Database** (chỉ đọc) trong 3 service:

### ProductService
| File | Vai trò |
|---|---|
| [appsettings.json](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/ProductService/appsettings.json) | Config MongoDB: `CQRS_Product_Read` |
| [MongoDbSettings.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/ProductService/Configurations/MongoDbSettings.cs) | Class chứa config MongoDB |
| [ProductReadRepository.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/ProductService/Data/ProductReadRepository.cs) | **ĐỌC từ MongoDB** — GetAll, GetById... |
| [ProductProjectionSyncService.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/ProductService/Data/ProductProjectionSyncService.cs) | Background sync PostgreSQL → MongoDB |
| [ProductQueryHandler.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/ProductService/Queries/ProductQueryHandler.cs) | Query Handler dùng `IProductReadRepository` (MongoDB) |
| [ProductCommandHandler.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/ProductService/Commands/ProductCommandHandler.cs) | Command Handler dùng `ProductDbContext` (PostgreSQL) |

---

### OrderService
| File | Vai trò |
|---|---|
| [appsettings.json](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/OrderService/appsettings.json) | Config MongoDB: `CQRS_Order_Read` |
| [OrderReadRepository.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/OrderService/Data/OrderReadRepository.cs) | **ĐỌC từ MongoDB** |
| [OrderProjectionSyncService.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/OrderService/Data/OrderProjectionSyncService.cs) | Background sync PostgreSQL → MongoDB |
| [OrderQueryHandler.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/OrderService/Handlers/OrderQueryHandler.cs) | Query dùng MongoDB |

---

### InventoryService
| File | Vai trò |
|---|---|
| [appsettings.json](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/InventoryService/appsettings.json) | Config MongoDB: `CQRS_Inventory_Read` |
| [InventoryReadRepository.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/InventoryService/Data/InventoryReadRepository.cs) | **ĐỌC từ MongoDB** |
| [InventoryProjectionSyncService.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/InventoryService/Data/InventoryProjectionSyncService.cs) | Background sync PostgreSQL → MongoDB |
| [InventoryQueryHandler.cs](file:///d:/PRN232/DEMO_CQRS_PRN392/src/Services/InventoryService/Queries/InventoryQueryHandler.cs) | Query dùng MongoDB |

---

## 4️⃣ Luồng dữ liệu MongoDB

```
Command (POST/PUT/DELETE)
    │
    ▼
PostgreSQL (Write DB) ──── Background Sync (mỗi 2 giây) ────► MongoDB (Read DB)
                                                                    │
                                                                    ▼
                                                              Query (GET) đọc từ đây
```

## 5️⃣ Sau khi cài xong, chạy project

```bash
# Verify tất cả đang chạy
docker ps

# Chạy service
cd src/Services/ProductService
dotnet run --urls "http://localhost:5002"
```
