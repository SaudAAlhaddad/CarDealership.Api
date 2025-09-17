# Car Dealership API

A demo backend for a car dealership management system.
Built with ASP.NET Core 9, Entity Framework Core, and SQLite.
It supports authentication with JWT tokens, role-based access control (Admin & Customer), and OTP (One-Time Password) verification.

# Features

Authentication & Authorization
- OTP-protected login/register
- JWT tokens for secure API access
- Role-based endpoints (Admin & Customer)

Admin
- View all customers
- Approve/reject purchase requests
- Manage vehicles

Customer
- Browse vehicles
- Request purchase 
- View purchase history

Security
- OTP with expiry and consumption
- Password hashing (SHA256)
- Input trimming & normalization

# Getting Started
You can run this API locally or inside a Docker container.

# 1. Local Run (with .NET SDK)

Clone the repo then
cd CarDealership.Api

Run the API: 
dotnet run

Open Swagger docs in your browser
http://localhost:5080/swagger

# 2. Docker Run

Build the image: 
docker build -t car-dealership-api:latest .

Run the container: 
docker run --name car-api \
  -p 8080:8080 \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__Default="Data Source=/app/data/dealership.db" \
  -v dealership_data:/app/data \
  car-dealership-api:latest

Open Swagger UI
http://localhost:8080/swagger
