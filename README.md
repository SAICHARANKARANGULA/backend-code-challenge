# Backend Developer Code Challenge

## Introduction
Welcome to the Backend Developer Technical Assessment! This test is designed to evaluate your proficiency in building REST APIs using .NET 8, focusing on clean architecture, business logic, and testing practices. We have prepared a set of tasks and questions that cover a spectrum of skills, ranging from fundamental concepts to more advanced topics.

**Note:** This assessment focuses on API development, architecture, and testing. During the interview, we'll discuss your experience with databases, event-driven design, Docker/Kubernetes, and cloud platforms.

## Tasks
Complete the provided tasks to demonstrate your ability to work with .NET 8, ASP.NET Core Web API, and unit testing. Adjust the complexity based on your experience level.

## Questions
Answer the questions to showcase your understanding of the underlying concepts and best practices associated with the technologies in use.

## Time Limit
This assessment is designed to take approximately 1-2 hours to complete. Please manage your time effectively.

## Setup the Repository
Make sure you have .NET 8 SDK installed
- Install dependencies with `dotnet restore`
- Build the project with `dotnet build`
- Run the project with `dotnet run --project CodeChallenge.Api`
- Navigate to `https://localhost:5095/swagger` to see the API documentation

## Prerequisite
Start the test by forking this repository, and complete the following tasks:

---

## Task 1
**Assignment:** Implement a REST API with CRUD operations for messages. Use the provided `IMessageRepository` and models to create a `MessagesController` with these endpoints:
- `GET /api/v1/organizations/{organizationId}/messages` - Get all messages for an organization
- `GET /api/v1/organizations/{organizationId}/messages/{id}` - Get a specific message
- `POST /api/v1/organizations/{organizationId}/messages` - Create a new message
- `PUT /api/v1/organizations/{organizationId}/messages/{id}` - Update a message
- `DELETE /api/v1/organizations/{organizationId}/messages/{id}` - Delete a message

**Question 1:** Describe your implementation approach and the key decisions you made.

**Ans:** 

For Task 1, I focused on building a clean and straightforward REST API using the existing IMessageRepository. Each controller method maps directly to a CRUD operation, returning the correct HTTP status codes (200, 201, 204, 404). The controller contains only request handling and repository calls—no business logic—so the code is easy to understand and prepares the project for later tasks. I also ensured proper route structure and consistent responses for all endpoints.

**Question 2:** What would you improve or change if you had more time?

**Ans:** 

If I had more time, I would improve the structure by introducing a dedicated business logic layer earlier, so validation and business rules don’t sit in the controller. I would also add response DTOs instead of returning domain models directly, improve error responses using standardized formats like ProblemDetails, and add logging and basic tests to make the API more production-ready.

commit the code as task-1

---

## Task 2
**Assignment:** Separate business logic from the controller and add proper validation.
1. Implement `MessageLogic` class (implement `IMessageLogic`)
2. Implement Business Rules:
   - Title must be unique per organization
   - Content must be between 10 and 1000 characters
   - Title is required and must be between 3 and 200 characters
   - Can only update or delete messages that are active (`IsActive = true`)
   - UpdatedAt should be set automatically on updates
3. Return appropriate result types (see `Logic/Results.cs`)
4. Update Controller to use `IMessageLogic` instead of directly using the repository

**Question 3:** How did you approach the validation requirements and why?

**Ans:** 

I moved all validation rules into the MessageLogic class so the controller stays clean and focused on HTTP concerns. The logic layer checks the title and content length, ensures the title is unique per organization, and enforces that only active messages can be updated or deleted. Keeping validation in one place makes the rules easier to maintain, easier to test, and ensures consistent behavior across the entire application.

**Question 4:** What changes would you make to this implementation for a production environment?

**Ans:** 

For a real production system, I would enforce the title uniqueness rule at the database level, use a real database instead of the in-memory repository, and add more robust error handling with standardized responses. I’d also introduce FluentValidation for cleaner rule definitions, add authentication/authorization, and include logging, metrics, and better observability. Finally, I’d add integration tests and concurrency controls to handle real-world usage safely.

commit the code as task-2

---

## Task 3
**Assignment:** Write comprehensive unit tests for your business logic.
1. Create `CodeChallenge.Tests` project (xUnit)
2. Add required packages: xUnit, Moq, FluentAssertions
3. Write Tests for MessageLogic covering these scenarios:
   - Test successful creation of a message
   - Test duplicate title returns Conflict
   - Test invalid content length returns ValidationError
   - Test update of non-existent message returns NotFound
   - Test update of inactive message returns ValidationError
   - Test delete of non-existent message returns NotFound

**Question 5:** Explain your testing strategy and the tools you chose.

**Ans:** 

We focused tests on the business logic layer (MessageLogic) to keep them fast, deterministic, and easy to reason about. Each unit test isolates MessageLogic by mocking IMessageRepository (using Moq) so we only verify domain rules and repository interactions — no real database or HTTP stack is used. Tests are written with xUnit for structure and FluentAssertions for clear, expressive assertions; this combination makes failures easy to read and helps keep tests maintainable. Overall the goal was to cover happy paths and important edge cases (validation, conflicts, not-found) while keeping the test suite quick to run.

**Question 6:** What other scenarios would you test in a real-world application?

**Ans:** 
Beyond the unit tests, I’d add integration tests that run the full stack (API → logic → real database) to validate DB constraints and transaction behavior (e.g., unique index enforcement and concurrency). I’d also add tests for auth/authorization (ensure org-level access is enforced), boundary tests for exact length limits, and concurrency/resilience tests that simulate parallel creates to surface race conditions. Finally, add higher-level end-to-end and load tests (with seeded data) to verify real-world behavior under stress and to ensure observability/logging/metrics are emitted correctly.

commit the code as task-3
