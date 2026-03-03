# 🚀 HackerNews Best Stories API

A .NET 8 Web API that retrieves the top **N best stories** from the official Hacker News API, with caching, concurrency control, and full test coverage (unit + integration tests).

---

## 📌 Overview

This API exposes a single endpoint:

```bash
GET /api/beststories?n=10
```

It fetches the top *N* best stories from Hacker News, applies concurrency control, caches responses, and returns them sorted by score in descending order.

The project follows clean architectural separation between:

- Controllers (HTTP layer)
- Services (business logic)
- HTTP Clients (external API communication)
- Configuration via strongly-typed Options
- Unit and Integration Tests

---

## 🛠 Tech Stack

- .NET 8
- ASP.NET Core Web API
- IMemoryCache
- HttpClient
- xUnit
- FluentAssertions
- Moq
- WebApplicationFactory (integration testing)
- Swagger / OpenAPI

---

## ▶️ How to Run

From the repository root:

```bash
dotnet restore
dotnet run --project src/HackerNews.BestStories.Api
```

The API will start locally (typically on `https://localhost:xxxx`).

Swagger UI is available in Development environment.

Example: 
```bash
https://localhost:7181/swagger/index.html
```

---

## Troubleshooting

### HTTPS development certificate issue

If you encounter an HTTPS or SSL certificate error when running the API locally, you may need to trust the .NET development certificate.

Run the following command:

```bash
dotnet dev-certs https --trust
```

Then restart the application.

If the issue persists, you can clean and recreate the certificate:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

---

## 🧪 Running Tests

From the repository root:

```bash
dotnet test
```

Test coverage includes:

- ✅ Unit tests for `BestStoriesService`
- ✅ Unit tests for `HackerNewsClient`
- ✅ Integration test for `/api/beststories` endpoint

---

## 📡 Endpoint

### GET `/api/beststories`

#### Query Parameters

| Parameter | Type | Description |
|------------|--------|------------|
| `n` | int? | Number of stories to retrieve. If omitted, the default value from configuration is used. |

#### Example

```bash
GET /api/beststories?n=5
```

#### Response (200 OK)

```json
[
  {
    "title": "Example Story",
    "uri": "https://example.com",
    "postedBy": "username",
    "time": "2026-03-02T12:00:00Z",
    "score": 150,
    "commentCount": 42
  }
]
```

#### Error Responses

- `400 Bad Request` → Invalid `n`
- `500 Internal Server Error` → Unexpected failure

---

## ⚙️ Configuration

Configured via `appsettings.json`.

```json
{
  "HackerNews": {
    "BaseUrl": "https://hacker-news.firebaseio.com/v0/",
    "RequestTimeoutSeconds": 5
  },
  "BestStories": {
    "DefaultN": 10,
    "MaxN": 200,
    "MaxConcurrency": 10
  },
  "Cache": {
    "BestStoriesTtlSeconds": 60,
    "ItemTtlMinutes": 10
  }
}
```

### Key Settings

- **DefaultN** → Default number of stories when `n` is not provided.
- **MaxN** → Maximum allowed value for `n`.
- **MaxConcurrency** → Limits simultaneous calls to the Hacker News API.
- **RequestTimeoutSeconds** → Prevents long-running external calls.
- **Cache TTLs** → Reduces load and improves performance.

---

## 🧠 Design Decisions

### Concurrency Control

A `SemaphoreSlim` limits concurrent external calls to avoid overwhelming the Hacker News API.

### Caching

- Story IDs are cached separately.
- Individual story responses are cached.
- Improves performance and reduces external calls.

### Best-Effort Behavior

If some story fetches fail due to timeouts or transient errors, the API returns the successfully fetched stories instead of failing the entire request.

This ensures resilience and bounded latency when dealing with external dependencies.

### Separation of Concerns

- Controller handles HTTP concerns only.
- Service layer encapsulates business rules.
- Client handles external communication.
- Configuration is strongly typed and centralized.

---

## 📂 Project Structure

```
src/
  HackerNews.BestStories.Api/

tests/
  HackerNews.BestStories.Tests/
    Unit/
    Integration/
```

---

## 🔮 Possible Future Improvements

- Always guarantee returning exactly N items
- Global exception handling middleware with ProblemDetails
- Dockerfile support
- Rate limiting
- Observability (structured logging / metrics)

---

## 🏁 Final Notes

This project focuses on:

- Clean architecture
- Testability
- Resilience against partial external failures
- Configuration-driven behavior
- Production-oriented design considerations
