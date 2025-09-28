# Car Dealership API

A comprehensive REST API for managing a car dealership system with secure authentication, vehicle inventory management, and purchase processing.

## 🚀 Features

- **Secure Authentication**: JWT-based authentication with OTP validation
- **Role-Based Access Control**: Admin and Customer roles with different permissions
- **Vehicle Management**: Browse, create, and update vehicle inventory
- **Purchase System**: Secure purchase requests with OTP validation
- **Admin Dashboard**: Customer management and purchase approval
- **Database Integration**: SQLite database with Entity Framework Core
- **API Documentation**: Swagger/OpenAPI integration
- **Docker Support**: Containerized deployment
- **Input Validation**: Automatic string trimming and normalization

## 🛠️ Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- SQLite (included with .NET)
- Docker (optional)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd CarDealership.Api
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Run Database Migrations

```bash
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

The API will be available at:

- **HTTP**: `http://localhost:5218`
- **Swagger UI**: `http://localhost:5218/swagger`

## 🐳 Docker Support

### Quick Start with Docker

```bash
# Build the Docker image
docker build -t car-dealership-api .

# Run the container
docker run -p 8080:8080 car-dealership-api
```

Access the API at:

- **HTTP**: `http://localhost:8080`
- **Swagger UI**: `http://localhost:8080/swagger`

## 📚 API Endpoints

### Authentication (`/api/auth`)

- `POST /api/auth/register` - Register new customer
- `POST /api/auth/login` - Login with credentials

### OTP Management (`/api/otp`)

- `POST /api/otp/request` - Request OTP (Register/Login)
- `POST /api/otp/request-purchase` - Request purchase OTP (Customer)
- `POST /api/otp/request-vehicle-update` - Request vehicle update OTP (Admin)

### Vehicle Management (`/api/vehicles`)

- `GET /api/vehicles` - Browse vehicles (Public)
- `GET /api/vehicles/{vehicleId}` - Get vehicle details (Public)
- `POST /api/vehicles` - Add vehicle (Admin)
- `PUT /api/vehicles/{vehicleId}` - Update vehicle (Admin)

### Purchase Management (`/api/purchases`)

- `POST /api/purchases/request` - Request purchase (Customer)
- `GET /api/purchases/history` - Purchase history (Customer)

### Admin Operations (`/api/admin`)

- `GET /api/admin/customers` - List customers (Admin)
- `POST /api/admin/process-sale/{purchaseRequestId}` - Process sale (Admin)

## 🔐 Authentication & Authorization

### JWT Token Structure

- `sub`: User ID
- `email`: User email
- `role`: User role (Admin/Customer)
- `name`: User's full name

### Role-Based Access Control

- **Admin**: Can manage vehicles, view customers, process sales
- **Customer**: Can browse vehicles, make purchase requests, view history

### OTP Security

- OTPs are required for registration, login, and sensitive operations
- Purchase OTPs are bound to specific vehicles
- OTPs expire after 2 minutes
- OTPs are displayed in the server console for development

## 🗄️ Database Schema

### Users Table

- `Id` (Primary Key)
- `Email` (Unique)
- `PasswordHash`
- `Role` (Admin/Customer)
- `FullName`

### Vehicles Table

- `Id` (Primary Key)
- `Make`, `Model`, `Year`, `Price`
- `Color`, `Description`
- `IsAvailable`

### PurchaseRequests Table

- `Id` (Primary Key)
- `VehicleId` (Foreign Key)
- `CustomerId` (Foreign Key)
- `RequestDate`, `Status`

### Sales Table

- `Id` (Primary Key)
- `VehicleId` (Foreign Key)
- `CustomerId` (Foreign Key)
- `SaleDate`, `Price`

## 🧪 Testing the API

### Using Swagger UI

1. Navigate to `http://localhost:5218/swagger`
2. Click "Authorize" and enter your JWT token
3. Test endpoints directly from the UI

## 🎯 Design Decisions

### Architecture

- **Clean Architecture**: Separation of concerns with Controllers, Services, and Data layers
- **Dependency Injection**: All services are registered in the DI container
- **Repository Pattern**: Entity Framework provides data access abstraction

### Security

- **JWT Authentication**: Stateless authentication for scalability
- **OTP Validation**: Additional security layer for sensitive operations
- **Role-Based Authorization**: Fine-grained access control

### Database

- **SQLite**: Lightweight database for development and small deployments
- **Entity Framework Core**: ORM for type-safe database operations
- **Migrations**: Version-controlled database schema changes

### API Design

- **RESTful**: Standard HTTP methods and status codes
- **DTOs**: Data transfer objects for request/response validation
- **Swagger Documentation**: Auto-generated API documentation

