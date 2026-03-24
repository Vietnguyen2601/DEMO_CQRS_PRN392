# 🏗️ CQRS Microservice Demo — ASP.NET Core + YARP

Dự án demo kiến trúc **Microservices** sử dụng **CQRS pattern** với **MediatR**, **API Gateway YARP**, và **Entity Framework Core InMemory** trên nền tảng **ASP.NET Core 8**.

---

## 📐 Kiến trúc tổng quan

```
Client (Postman / Frontend)
          │
          ▼
   ┌─────────────────────┐
   │   API Gateway       │  :5000  (YARP)
   └─────────┬───────────┘
             │  Routes theo prefix
   ┌──────────┼──────────────────────────────┐
   │          │                              │
   ▼          ▼          ▼          ▼        ▼
:5001      :5002       :5003       :5004   :5005
Identity  Product     Order      Payment  Inventory
Service   Service     Service    Service  Service
```

### Routing qua Gateway

| Prefix tại Gateway | Chuyển đến |
|---|---|
| `/identity/**` | IdentityService `:5001` |
| `/products/**` | ProductService `:5002` |
| `/orders/**` | OrderService `:5003` |
| `/payments/**` | PaymentService `:5004` |
| `/inventory/**` | InventoryService `:5005` |

---

## 📦 Cấu trúc Solution

```
CQRS/
├── CQRS.sln
├── docker-compose.yml
├── README.md
└── src/
    ├── ApiGateway/                    ← YARP Reverse Proxy
    ├── Shared/
    │   └── Contracts/                 ← Shared: ApiResponse<T>
    └── Services/
        ├── IdentityService/           ← Đăng ký, Đăng nhập
        ├── ProductService/            ← Quản lý sản phẩm
        ├── OrderService/              ← Đặt hàng
        ├── PaymentService/            ← Thanh toán
        └── InventoryService/          ← Tồn kho
```

---

## 🎯 CQRS Pattern

Mỗi service được chia rõ thành **Command** (ghi) và **Query** (đọc), sử dụng thư viện **MediatR**.

```
ServiceName/
├── Commands/
│   ├── XxxCommand.cs          ← IRequest<T>
│   └── XxxCommandHandler.cs   ← IRequestHandler<TCommand, TResponse>
├── Queries/
│   ├── GetXxxQuery.cs
│   └── GetXxxQueryHandler.cs
├── Models/
├── Data/
│   └── XxxDbContext.cs        ← EF Core InMemory
├── Controllers/
│   └── XxxController.cs
└── Program.cs
```

### Ví dụ luồng CQRS

```
POST /api/products
      │
      ▼
ProductsController
      │  new CreateProductCommand(Name, Price, ...)
      ▼
_mediator.Send(command)
      │
      ▼
CreateProductCommandHandler.Handle(...)
      │  db.Products.Add(product)
      ▼
Database (InMemory)
```

---

## 🛠️ Tech Stack

| Thành phần | Thư viện / Công nghệ |
|---|---|
| API Gateway | `Yarp.ReverseProxy` |
| CQRS / Mediator | `MediatR` |
| Database (demo) | `Microsoft.EntityFrameworkCore.InMemory` |
| API Docs | `Swashbuckle.AspNetCore` (Swagger) |
| Framework | ASP.NET Core 8 |

---

## 🚀 Hướng dẫn chạy

### Yêu cầu

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 hoặc VS Code + C# Extension

---

### Option 1: Chạy bằng Visual Studio (Khuyến nghị)

1. Mở file `CQRS.sln` bằng **Visual Studio 2022**
2. Chuột phải vào Solution → **Set Startup Projects**
3. Chọn **Multiple startup projects**, set tất cả 6 project bên dưới sang **Start**:
   - `ApiGateway`
   - `IdentityService`
   - `ProductService`
   - `OrderService`
   - `PaymentService`
   - `InventoryService`
4. Nhấn **F5** để chạy

---

### Option 2: Chạy thủ công từng service (CLI)

Mở **6 terminal riêng biệt**, chạy lần lượt:

```bash
# Terminal 1 - API Gateway (port 5000)
cd src/ApiGateway
dotnet run --urls "http://localhost:5000"

# Terminal 2 - Identity Service (port 5001)
cd src/Services/IdentityService
dotnet run --urls "http://localhost:5001"

# Terminal 3 - Product Service (port 5002)
cd src/Services/ProductService
dotnet run --urls "http://localhost:5002"

# Terminal 4 - Order Service (port 5003)
cd src/Services/OrderService
dotnet run --urls "http://localhost:5003"

# Terminal 5 - Payment Service (port 5004)
cd src/Services/PaymentService
dotnet run --urls "http://localhost:5004"

# Terminal 6 - Inventory Service (port 5005)
cd src/Services/InventoryService
dotnet run --urls "http://localhost:5005"
```

---

### Option 3: Docker Compose

```bash
docker-compose up --build
```

---

## 📡 API Endpoints

> Tất cả request đều đi qua **Gateway tại `http://localhost:5000`**
> Có thể gọi thẳng service nếu cần debug (ví dụ `http://localhost:5001`)

### 🔐 Identity Service

| Method | Gateway URL | Mô tả |
|--------|-------------|-------|
| `POST` | `/identity/api/identity/register` | Đăng ký user mới |
| `POST` | `/identity/api/identity/login` | Đăng nhập, nhận token |
| `GET` | `/identity/api/identity/users` | Lấy danh sách users |
| `GET` | `/identity/api/identity/users/{id}` | Lấy user theo Id |

**Body Register:**
```json
{
  "username": "john",
  "email": "john@example.com",
  "password": "123456"
}
```

**Body Login:**
```json
{
  "email": "john@example.com",
  "password": "123456"
}
```

---

### 📦 Product Service

| Method | Gateway URL | Mô tả |
|--------|-------------|-------|
| `POST` | `/products/api/products` | Tạo sản phẩm mới |
| `GET` | `/products/api/products` | Lấy tất cả sản phẩm |
| `GET` | `/products/api/products/{id}` | Lấy sản phẩm theo Id |

**Body Create Product:**
```json
{
  "name": "Laptop Lenovo",
  "description": "Core i7, 16GB RAM",
  "price": 25000000
}
```

---

### 🛒 Order Service

| Method | Gateway URL | Mô tả |
|--------|-------------|-------|
| `POST` | `/orders/api/orders` | Đặt hàng |
| `GET` | `/orders/api/orders` | Lấy tất cả đơn hàng |
| `GET` | `/orders/api/orders/{id}` | Lấy đơn hàng theo Id |

**Body Place Order:**
```json
{
  "userId": "{{userId}}",
  "productId": "{{productId}}",
  "quantity": 2,
  "totalPrice": 50000000
}
```

---

### 💳 Payment Service

| Method | Gateway URL | Mô tả |
|--------|-------------|-------|
| `POST` | `/payments/api/payments` | Xử lý thanh toán |
| `GET` | `/payments/api/payments/{id}` | Lấy trạng thái thanh toán |

**Body Process Payment:**
```json
{
  "orderId": "{{orderId}}",
  "amount": 50000000,
  "method": "CreditCard"
}
```

---

### 📊 Inventory Service

| Method | Gateway URL | Mô tả |
|--------|-------------|-------|
| `POST` | `/inventory/api/inventory` | Thêm/cập nhật tồn kho |
| `GET` | `/inventory/api/inventory/{productId}` | Xem tồn kho theo sản phẩm |
| `GET` | `/inventory/api/inventory` | Xem toàn bộ tồn kho |

**Body Update Stock:**
```json
{
  "productId": "{{productId}}",
  "productName": "Laptop Lenovo",
  "quantity": 100
}
```

---

## 🔍 Swagger UI

Mỗi service có Swagger riêng (khi chạy development mode):

| Service | Swagger URL |
|---|---|
| IdentityService | http://localhost:5001/swagger |
| ProductService | http://localhost:5002/swagger |
| OrderService | http://localhost:5003/swagger |
| PaymentService | http://localhost:5004/swagger |
| InventoryService | http://localhost:5005/swagger |

---

## 💡 Ghi chú cho team

- **InMemory Database**: Dữ liệu sẽ **mất khi restart** service — đây là môi trường demo. Production cần thay bằng SQL Server / PostgreSQL.
- **Token**: `IdentityService` trả về token giả (`fake-jwt-token-for-{userId}`) — Production cần tích hợp JWT thật.
- **Không có authentication tại Gateway**: Demo tập trung vào CQRS. Có thể bổ sung JWT middleware vào YARP sau.
- **CQRS tách biệt Command/Query**: Mọi thay đổi dữ liệu đi qua `Command`, mọi đọc dữ liệu đi qua `Query` — không được gọi lẫn lộn.

---

## 📚 Tài liệu tham khảo

- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [CQRS Pattern - Microsoft](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [EF Core InMemory](https://learn.microsoft.com/en-us/ef/core/providers/in-memory/)
