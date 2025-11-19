using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic;

public class MessageLogic : IMessageLogic
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MessageLogic> _logger;

    private const int MaxTitleLength = 100;
    private const int MaxContentLength = 500;

    public MessageLogic(IMessageRepository repository, ILogger<MessageLogic> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
    {
        var validationErrors = ValidateRequest(request);
        if (validationErrors.Count > 0)
            return new ValidationError(validationErrors);

        var existingByTitle = await _repository.GetByTitleAsync(organizationId, request.Title);
        if (existingByTitle is not null)
        {
            return new Conflict($"A message with the title '{request.Title}' already exists for the organization.");
        }

        var message = new Message
        {
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        var created = await _repository.CreateAsync(message);

        return new Created<Message>(created);
    }

    public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
    {
        var validationErrors = ValidateRequest(request);
        if (validationErrors.Count > 0)
            return new ValidationError(validationErrors);

        var existing = await _repository.GetByIdAsync(organizationId, id);
        if (existing is null)
            return new NotFound($"Message with id '{id}' not found for organization '{organizationId}'.");

        if (!existing.IsActive)
            return new Conflict("Cannot update an inactive message.");

        if (!string.Equals(existing.Title, request.Title, StringComparison.OrdinalIgnoreCase))
        {
            var other = await _repository.GetByTitleAsync(organizationId, request.Title);
            if (other is not null && other.Id != existing.Id)
            {
                return new Conflict($"A message with the title '{request.Title}' already exists for the organization.");
            }
        }

        existing.Title = request.Title;
        existing.Content = request.Content;
        existing.IsActive = request.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);

        if (updated is null)
            return new NotFound($"Message with id '{id}' not found during update.");

        return new Updated();
    }

    public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
    {
        var existing = await _repository.GetByIdAsync(organizationId, id);
        if (existing is null)
            return new NotFound($"Message with id '{id}' not found for organization '{organizationId}'.");

        if (!existing.IsActive)
            return new Conflict("Cannot delete an inactive message.");

        var deleted = await _repository.DeleteAsync(organizationId, id);

        if (!deleted)
            return new NotFound($"Message with id '{id}' not found during delete.");

        return new Deleted();
    }

    public Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
    {
        return _repository.GetByIdAsync(organizationId, id);
    }

    public Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
    {
        return _repository.GetAllByOrganizationAsync(organizationId);
    }

    private Dictionary<string, string[]> ValidateRequest(CreateMessageRequest request)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        void AddError(string key, string message)
        {
            if (!errors.TryGetValue(key, out var list))
            {
                list = new List<string>();
                errors[key] = list;
            }
            list.Add(message);
        }

        if (string.IsNullOrWhiteSpace(request.Title))
            AddError(nameof(request.Title), "Title is required.");
        else if (request.Title.Length > MaxTitleLength)
            AddError(nameof(request.Title), $"Title must not exceed {MaxTitleLength} characters.");

        if (request.Content != null && request.Content.Length > MaxContentLength)
            AddError(nameof(request.Content), $"Content must not exceed {MaxContentLength} characters.");

        return errors.ToDictionary(k => k.Key, k => k.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    private Dictionary<string, string[]> ValidateRequest(UpdateMessageRequest request)
    {
        var createReq = new CreateMessageRequest
        {
            Title = request.Title,
            Content = request.Content
        };
        return ValidateRequest(createReq);
    }
}
