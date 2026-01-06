# Eshop Products API

Simple ASP.NET Core Web API for managing products in an e-shop.  
The project demonstrates API versioning, Entity Framework Core, database seeding, Swagger documentation, and unit testing with mock data.

---

## Features

- ASP.NET Core Web API
- Entity Framework Core with SQL Server LocalDB
- API versioning (v1, v2)
- Swagger / OpenAPI documentation
- Automatic database creation and migrations
- Unit tests using in-memory SQLite
- Simple product management endpoints

---

## Prerequisites (Windows)

- Windows 10 / 11
- .NET SDK 8.0 or newer
- SQL Server LocalDB  
  (installed automatically with Visual Studio or available via SQL Server Express)
- Git

Verify prerequisites:
```bash
dotnet --version

---

## Running the API

- From repo root, run:

```bash
dotnet run --project .\EshopProducts\EshopProducts.csproj

- Go to https://localhost:7279/index.html

## Running tests

- From repo root, run:

```bash
dotnet test .\EshopProductsTests