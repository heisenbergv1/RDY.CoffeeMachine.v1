# CoffeeMachine API

A .NET Core HTTP API that simulates an internet-connected coffee machine. Implements Clean Architecture principles, unit and integration tests, and integrates with a third-party weather API for extra credit features.

## Overview

This API exposes a single endpoint:

```

GET /brew-coffee

````

Behavior:

1. Returns JSON with a coffee message and ISO-8601 timestamp.
2. Every 5th request per IP returns `503 Service Unavailable`.
3. On April 1st, always returns `418 I'm a teapot`.
4. Extra credit: Returns "Your refreshing iced coffee is ready" when the weather temperature > 30°C.

## Clean Architecture Rationale

The project is structured using **Clean Architecture** principles:

- **Separation of Concerns:**  
  - **Controllers / API Layer:** Handles HTTP requests/responses.  
  - **Application Layer:** Contains MediatR commands/queries and business logic (e.g., `GetCoffeeMessageHandler`).  
  - **Infrastructure Layer:** Handles external services, such as the OpenWeatherMap API client.  

- **Benefits:**  
  - Easier to maintain, test, and swap implementations.  
  - Decouples business logic from framework-specific code (ASP.NET Core).  
  - Supports Dependency Injection and mocking for unit and integration testing.

## Notable .NET 3rd-Party Libraries

| Library                    | Purpose & Rationale                                                                                                              |
| -------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| **MediatR**                | Implements the Command/Query pattern; decouples request handling from controllers, simplifying testability and maintainability.  |
| **FluentAssertions**       | Provides expressive assertions for unit and integration tests. Improves readability and debugging of test failures.              |
| **Moq**                    | Mocking framework used in unit tests to simulate external dependencies like `IWeatherClient`.                                    |
| **Swashbuckle.AspNetCore** | Enables automatic OpenAPI specification generation and interactive Swagger UI for API documentation and manual endpoint testing. |

## Requirements Checklist

**Functional Requirements:**

- [x] GET `/brew-coffee` returns 200 OK with message + ISO-8601 timestamp.
- [x] Every 5th call per IP returns 503 Service Unavailable with empty body.
- [x] April 1st calls always return 418 I'm a teapot with empty body.
- [x] Returns "Your refreshing iced coffee is ready" when weather > 30°C using OpenWeatherMap API.

**Non-Functional Requirements:**

- [x] Implemented in .NET Core 10.0 LTS.
- [x] Includes unit and integration tests for all critical scenarios.

**Extra Credit:**

- [x] Weather-based iced coffee message implemented via `IWeatherClient` and MediatR query handler.

## Setup & Installation

1. Clone the repository:

```bash
git clone git@github.com:heisenbergv1/RDY.CoffeeMachine.git
cd CoffeeMachine.Api
````

2. Restore NuGet packages:

```bash
dotnet restore
```

3. Copy `appsettings.json.example` to `appsettings.json`, then add your OpenWeatherMap API key (https://home.openweathermap.org/api_keys
) to the appropriate configuration section:

```json
{
  "Weather": {
    "ApiKey": "<YOUR_API_KEY>"
  }
}
```

4. Build the project:

```bash
dotnet build
```

## Running the Application

Start the API:

```bash
dotnet run --project CoffeeMachine.Api
```

The API will be available at `http://localhost:5288` or `http://localhost:5288/swagger`.

## Testing

Run all unit and integration tests:

```bash
dotnet test
```

Tests cover:

* IP-based request throttling (every 5th request returns 503)
* April 1st override behavior (418 response)
* Weather-based iced coffee message
* Date/time formatting in ISO-8601
* Integration tests for endpoint response payloads

## Notes

* The IP-based counter uses `ConcurrentDictionary` to ensure thread-safety in concurrent requests.
* Weather API integration uses `HttpClientFactory` and is injected via DI for testability.
* MediatR decouples controllers from business logic for better maintainability and mocking in tests.

