# Fintacharts Market Data API

A robust, high-performance ASP.NET Core Web API for fetching, caching, and serving real-time financial market data. This service acts as a middleware between the Fintacharts API and client applications, ensuring high availability, minimizing external API calls, and handling real-time price updates via WebSockets.

## Features

* Real-Time WebSocket Integration: Maintains a persistent connection to the Fintacharts WSS provider to receive live "l1-update" quotes.
* Smart In-Memory Caching: High-frequency data (prices and timestamps) is stored in a thread-safe ConcurrentDictionary to prevent database bottlenecks and provide sub-millisecond read times.
* Lazy Loading & Fallback Mechanism: If a requested asset's price is not available in the real-time cache, the API automatically fetches historical data from the REST API to provide the most recent close price.
* Automatic Token Management: The application transparently handles Fintacharts authentication, token caching, and bearer header injection.
* Containerized Environment: Fully dockerized ecosystem including the API and MS SQL Server, ready to launch with a single command.

## Architecture & Design Decisions

### Why we do not store prices in the database
Financial market data is highly dynamic. Storing live quotes (which can update dozens of times per second) in a relational database like SQL Server introduces severe I/O bottlenecks, database locking issues, and unnecessary disk wear. 

Decision:
* Static Data: We store only the static Instruments data (Id, Symbol, Kind, Provider) in the MS SQL Database. This ensures we have a persistent catalog of available assets.
* Dynamic Data: Prices and Last Update Timestamps are kept strictly in RAM (MarketStateCache). This guarantees maximum throughput and adheres to the principles of high-frequency data architectures.

## Tech Stack

* Framework: .NET 8.0 / ASP.NET Core Web API
* Database: Entity Framework Core 8, MS SQL Server 2022
* Concurrency: ClientWebSocket, SemaphoreSlim, ConcurrentDictionary, BackgroundService
* Testing: xUnit, Moq, FluentAssertions
* Infrastructure: Docker, Docker Compose

## How to Run

The application is fully containerized. The easiest and most reliable way to run the ecosystem is using Docker Compose. This will automatically spin up the MS SQL Server, apply Entity Framework migrations, seed the database with instruments, and start the API.

1. Ensure Docker Desktop (or Docker Engine) is installed and running.
2. Open a terminal in the root directory of the project.
3. Run the following command:
   docker-compose up --build
4. Wait for the database container to become healthy and the API to start (usually takes 10-15 seconds for the first boot).
5. Open your browser and navigate to the Swagger UI:
   http://localhost:8080/swagger

## API Endpoints

Once the application is running, you can interact with the following REST endpoints via Swagger or any HTTP client:

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| GET | /api/assets | Returns a paginated list of all available financial instruments stored in the database. |
| GET | /api/assets/prices | Accepts an array of asset symbols (e.g., ?assets=AAPL&assets=EURUSD). Returns the latest prices from the real-time cache or fallbacks to historical REST fetching if the cache is empty. |

Note: Data seeding happens automatically on the first startup. The API reaches out to Fintacharts, retrieves all instruments, and populates the local SQL database.

## Running Tests

The solution includes a robust suite of Unit Tests focusing token management logic. To run the tests, execute:

dotnet test