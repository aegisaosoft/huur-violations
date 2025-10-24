# Huur Violations API

A .NET 9 ASP.NET Core Web API project for managing rental violations.

## Features

- RESTful API for managing violations
- CORS configured for frontend integration (localhost:3000)
- Swagger/OpenAPI documentation
- Built with .NET 9

## Getting Started

### Prerequisites

- .NET 9 SDK
- Visual Studio 2022 or VS Code

### Running the Application

1. Navigate to the project directory:
   ```bash
   cd HuurViolations.Api
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. The API will be available at:
   - HTTPS: `https://localhost:7183`
   - HTTP: `http://localhost:7183`
   - Swagger UI: `https://localhost:7183/swagger`

### API Endpoints

- `GET /api/violations` - Get all violations
- `GET /api/violations/{id}` - Get a specific violation
- `POST /api/violations` - Create a new violation
- `PUT /api/violations/{id}` - Update a violation
- `DELETE /api/violations/{id}` - Delete a violation

### CORS Configuration

The API is configured to allow requests from `http://localhost:3000` for frontend integration.

## Project Structure

```
HuurViolations/
├── HuurViolations.Api/          # Main API project
│   ├── Controllers/             # API controllers
│   ├── Properties/              # Launch settings
│   └── Program.cs               # Application entry point
└── HuurViolations.sln          # Solution file
```


