using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using FluentAssertions;
using Moq;
using Message = CodeChallenge.Api.Models.Message;

namespace CodeChallenge.Tests;

public class MessageLogicTests
{
    private readonly Mock<IMessageRepository> _repoMock;
    private readonly MessageLogic _logic;

    public MessageLogicTests()
    {
        _repoMock = new Mock<IMessageRepository>(MockBehavior.Strict);

        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<MessageLogic>();
        _logic = new MessageLogic(_repoMock.Object, logger);
    }

    [Fact]
    public async Task CreateMessage_SuccessfulCreation_ReturnsCreatedWithValue()
    {
        var organizationId = Guid.NewGuid();
        var request = new CreateMessageRequest
        {
            Title = "Unique title",
            Content = new string('a', 20)
        };

        _repoMock.Setup(r => r.GetByTitleAsync(organizationId, request.Title))
                 .ReturnsAsync((Message?)null);

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                 .ReturnsAsync((Message m) =>
                 {
                     m.Id = Guid.NewGuid();
                     m.CreatedAt = DateTime.UtcNow;
                     return m;
                 });

        var result = await _logic.CreateMessageAsync(organizationId, request);

        result.Should().BeOfType<Created<Message>>();
        var created = result as Created<Message>;
        created!.Value.Should().NotBeNull();
        created.Value.Id.Should().NotBeEmpty();
        created.Value.Title.Should().Be(request.Title);

        _repoMock.Verify(r => r.GetByTitleAsync(organizationId, request.Title), Times.Once);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Once);
    }

    [Fact]
    public async Task CreateMessage_DuplicateTitle_ReturnsConflict()
    {
        var organizationId = Guid.NewGuid();
        var request = new CreateMessageRequest
        {
            Title = "AlreadyExists",
            Content = new string('b', 20)
        };

        var existing = new Message
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Title = request.Title,
            Content = "old",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetByTitleAsync(organizationId, request.Title))
                 .ReturnsAsync(existing);

        var result = await _logic.CreateMessageAsync(organizationId, request);

        result.Should().BeOfType<Conflict>();
        var conflict = result as Conflict;
        conflict!.Message.Should().Contain("already exists");

        _repoMock.Verify(r => r.GetByTitleAsync(organizationId, request.Title), Times.Once);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Never);
    }

    [Fact]
    public async Task CreateMessage_InvalidContentLength_ReturnsValidationError()
    {
        var organizationId = Guid.NewGuid();
        var request = new CreateMessageRequest
        {
            Title = "Valid title",
        };

        _repoMock.Setup(r => r.GetByTitleAsync(organizationId, request.Title))
                 .ReturnsAsync((Message?)null);

        var result = await _logic.CreateMessageAsync(organizationId, request);

        result.Should().BeOfType<ValidationError>();
        var validation = result as ValidationError;
        validation!.Errors.Should().ContainKey(nameof(request.Content));
        validation.Errors[nameof(request.Content)].Should().NotBeEmpty();

        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMessage_NonExistent_ReturnsNotFound()
    {
        var organizationId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var request = new UpdateMessageRequest
        {
            Title = "New title",
            Content = new string('x', 20),
            IsActive = true
        };

        _repoMock.Setup(r => r.GetByIdAsync(organizationId, id))
                 .ReturnsAsync((Message?)null);

        var result = await _logic.UpdateMessageAsync(organizationId, id, request);

        result.Should().BeOfType<NotFound>();
        var nf = result as NotFound;
        nf!.Message.Should().Contain(id.ToString());

        _repoMock.Verify(r => r.GetByIdAsync(organizationId, id), Times.Once);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Message>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMessage_InactiveMessage_ReturnsValidationError()
    {
        var organizationId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var existing = new Message
        {
            Id = id,
            OrganizationId = organizationId,
            Title = "Old",
            Content = "old content",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        var request = new UpdateMessageRequest
        {
            Title = "New Title",
            Content = new string('z', 20),
            IsActive = false
        };

        _repoMock.Setup(r => r.GetByIdAsync(organizationId, id))
                 .ReturnsAsync(existing);

        var result = await _logic.UpdateMessageAsync(organizationId, id, request);

        result.Should().BeOfType<ValidationError>();
        var ve = result as ValidationError;
        ve!.Errors.Should().ContainKey("IsActive");
        ve.Errors["IsActive"][0].Should().Contain("active");

        _repoMock.Verify(r => r.GetByIdAsync(organizationId, id), Times.Once);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Message>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMessage_NonExistent_ReturnsNotFound()
    {
        var organizationId = Guid.NewGuid();
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(organizationId, id))
                 .ReturnsAsync((Message?)null);

        var result = await _logic.DeleteMessageAsync(organizationId, id);

        result.Should().BeOfType<NotFound>();
        _repoMock.Verify(r => r.GetByIdAsync(organizationId, id), Times.Once);
        _repoMock.Verify(r => r.DeleteAsync(organizationId, id), Times.Never);
    }

    [Fact]
    public async Task DeleteMessage_InactiveMessage_ReturnsValidationError()
    {
        var organizationId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var existing = new Message
        {
            Id = id,
            OrganizationId = organizationId,
            Title = "Old",
            Content = "old content",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _repoMock.Setup(r => r.GetByIdAsync(organizationId, id))
                 .ReturnsAsync(existing);

        var result = await _logic.DeleteMessageAsync(organizationId, id);

        result.Should().BeOfType<ValidationError>();
        var ve = result as ValidationError;
        ve!.Errors.Should().ContainKey("IsActive");
        ve.Errors["IsActive"][0].Should().Contain("deleted");

        _repoMock.Verify(r => r.GetByIdAsync(organizationId, id), Times.Once);
        _repoMock.Verify(r => r.DeleteAsync(organizationId, id), Times.Never);
    }
}
