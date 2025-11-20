# Assessment Answers (Task 1–3)

## Q1: Describe your implementation approach and key decisions
- Implemented a thin controller layer (MessagesController) responsible for HTTP concerns and routing.
- Separated business logic into `MessageLogic` implementing `IMessageLogic` to keep controllers thin and testable.
- Followed task rules: Title uniqueness per organization, content/title length bounds, UpdatedAt set on updates, only active messages can be updated/deleted.
- Used repository abstraction `IMessageRepository` to keep data layer swappable (in-memory for challenge).
- Returned typed Result records (`Created<T>`, `Updated`, `Deleted`, `ValidationError`, `Conflict`, `NotFound`) so controller can map to HTTP responses easily.

## Q2: What would you improve with more time?
- Add FluentValidation for declarative validation and better error messages.
- Add integration tests using `WebApplicationFactory<T>` to test controller + routing + model binding.
- Replace in-memory repo with EF Core and migrations for realistic persistence.
- Add OpenAPI response schemas and more descriptive examples in Swagger.

## Q3: How did you approach validation requirements and why?
- Centralized validation in `MessageLogic` so controllers remain thin.
- Enforced the required business rules (title uniqueness, length bounds) in the logic layer to ensure consistent behavior both from API and any potential other callers.
- Returned `ValidationError` with field-specific messages to allow clients to present clear error messages.

## Q4: Changes for production environment
- Switch to persistent database (SQL Server/Postgres) and implement EF Core repository.
- Add authentication/authorization on endpoints.
- Harden logging, monitoring, and error handling (structured logging, correlation ids).
- Add input size limits and rate-limiting to protect endpoints.
- Add CI pipelines and containerization (Docker) with deployment manifests.

## Q5: Testing strategy and tools
- Unit tests: xUnit + Moq for repository mocking + FluentAssertions for expressive asserts.
- Integration tests: WebApplicationFactory to exercise the real pipeline (not included due to time).
- Add end-to-end tests in CI (e.g., Playwright or Postman/Newman) for the Swagger endpoints.

## Q6: Other scenarios to test
- Race conditions for title uniqueness under concurrent creates.
- Partial updates, null fields, and invalid JSON payloads.
- Authorization and permission scenarios.
- Boundary cases for min/max lengths and very large payloads.
- Repository failure modes (DB transient errors) and retry policies.
