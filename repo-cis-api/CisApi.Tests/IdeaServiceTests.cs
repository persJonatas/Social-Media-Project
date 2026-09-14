using CisApi.Business.DTOs;
using CisApi.Business.Interfaces;
using CisApi.Business.Services;
using CisApi.Data;
using CisApi.Data.Entities;
using MongoDB.Driver;
using Moq;

namespace CisApi.Tests;

public class IdeaServiceTests
{
    private readonly Mock<IIdeaRepository> _ideaRepoMock = new();
    private readonly Mock<IMongoDbContext> _mongoContextMock = new();

    private IdeaService CreateService() =>
        new IdeaService(_ideaRepoMock.Object, _mongoContextMock.Object);

    private Mock<IMongoCollection<Topic>> CreateTopicCollectionMock(bool exists)
    {
        var topicCollection = new Mock<IMongoCollection<Topic>>();
        var asyncCursor = new Mock<IAsyncCursor<Topic>>();

        var topics = exists
            ? new List<Topic> { new Topic { Id = "topic-1" } }
            : new List<Topic>();

        asyncCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists)
            .ReturnsAsync(false);
        asyncCursor.Setup(c => c.Current).Returns(topics);

        topicCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<Topic>>(),
            It.IsAny<FindOptions<Topic, Topic>>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(asyncCursor.Object);

        return topicCollection;
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsIdeaResponse()
    {
        _mongoContextMock.Setup(c => c.Topics).Returns(CreateTopicCollectionMock(true).Object);
        _ideaRepoMock.Setup(r => r.CreateAsync(It.IsAny<Idea>())).ReturnsAsync((Idea i) => i);

        var result = await CreateService().CreateAsync("user-1", new CreateIdeaRequestDTO
        {
            Title = "Minha ideia",
            TopicId = "topic-1"
        });

        Assert.Equal("Minha ideia", result.Title);
        Assert.Equal("topic-1", result.TopicId);
    }

    [Fact]
    public async Task CreateAsync_WithDescription_SetsDescription()
    {
        _mongoContextMock.Setup(c => c.Topics).Returns(CreateTopicCollectionMock(true).Object);
        _ideaRepoMock.Setup(r => r.CreateAsync(It.IsAny<Idea>())).ReturnsAsync((Idea i) => i);

        var result = await CreateService().CreateAsync("user-1", new CreateIdeaRequestDTO
        {
            Title = "Ideia com descricao",
            TopicId = "topic-1",
            Description = "Descricao detalhada"
        });

        Assert.Equal("Descricao detalhada", result.Description);
    }

    [Fact]
    public async Task CreateAsync_TopicNotFound_ThrowsKeyNotFoundException()
    {
        _mongoContextMock.Setup(c => c.Topics).Returns(CreateTopicCollectionMock(false).Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService().CreateAsync("user-1", new CreateIdeaRequestDTO
            {
                Title = "Minha ideia",
                TopicId = "topic-inexistente"
            }));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllIdeas()
    {
        var ideas = new List<Idea>
        {
            new() { Id = "idea-1", Title = "Ideia 1", TopicId = "topic-1", CreatedByUserId = "user-1" },
            new() { Id = "idea-2", Title = "Ideia 2", TopicId = "topic-1", CreatedByUserId = "user-1" }
        };
        _ideaRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(ideas);

        var result = await CreateService().GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_IdeaFound_ReturnsIdeaResponse()
    {
        var idea = new Idea { Id = "idea-1", Title = "Ideia Existente", CreatedByUserId = "user-1", TopicId = "topic-1" };
        _ideaRepoMock.Setup(r => r.GetByIdAsync("idea-1")).ReturnsAsync(idea);

        var result = await CreateService().GetByIdAsync("idea-1");

        Assert.Equal("idea-1", result.Id);
        Assert.Equal("Ideia Existente", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_IdeaNotFound_ThrowsKeyNotFoundException()
    {
        _ideaRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Idea?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService().GetByIdAsync("id-inexistente"));
    }

    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsUpdatedIdea()
    {
        var idea = new Idea { Id = "idea-1", Title = "Original", CreatedByUserId = "user-1", TopicId = "topic-1" };
        _ideaRepoMock.Setup(r => r.GetByIdAsync("idea-1")).ReturnsAsync(idea);
        _ideaRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Idea>())).ReturnsAsync((Idea i) => i);

        var result = await CreateService().UpdateAsync("user-1", "idea-1", new UpdateIdeaRequestDTO { Title = "Atualizado" });

        Assert.Equal("Atualizado", result.Title);
    }

    [Fact]
    public async Task UpdateAsync_NullTitle_KeepsOriginalTitle()
    {
        var idea = new Idea { Id = "idea-1", Title = "Original", Description = "Desc original", CreatedByUserId = "user-1", TopicId = "topic-1" };
        _ideaRepoMock.Setup(r => r.GetByIdAsync("idea-1")).ReturnsAsync(idea);
        _ideaRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Idea>())).ReturnsAsync((Idea i) => i);

        var result = await CreateService().UpdateAsync("user-1", "idea-1", new UpdateIdeaRequestDTO { Title = null, Description = "Nova desc" });

        Assert.Equal("Original", result.Title);
        Assert.Equal("Nova desc", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_NullDescription_KeepsOriginalDescription()
    {
        var idea = new Idea { Id = "idea-1", Title = "Original", Description = "Desc original", CreatedByUserId = "user-1", TopicId = "topic-1" };
        _ideaRepoMock.Setup(r => r.GetByIdAsync("idea-1")).ReturnsAsync(idea);
        _ideaRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Idea>())).ReturnsAsync((Idea i) => i);

        var result = await CreateService().UpdateAsync("user-1", "idea-1", new UpdateIdeaRequestDTO { Title = "Novo titulo", Description = null });

        Assert.Equal("Novo titulo", result.Title);
        Assert.Equal("Desc original", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_IdeaNotFound_ThrowsKeyNotFoundException()
    {
        _ideaRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Idea?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService().UpdateAsync("user-1", "idea-inexistente", new UpdateIdeaRequestDTO { Title = "X" }));
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var idea = new Idea { Id = "idea-1", Title = "Original", CreatedByUserId = "user-1", TopicId = "topic-1" };
        _ideaRepoMock.Setup(r => r.GetByIdAsync("idea-1")).ReturnsAsync(idea);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateService().UpdateAsync("outro-user", "idea-1", new UpdateIdeaRequestDTO { Title = "X" }));
    }

    [Fact]
    public async Task DeleteAsync_ValidData_CallsDeleteRepository()
    {
        var idea = new Idea { Id = "idea-1", Title = "Original", CreatedByUserId = "user-1", TopicId = "topic-1" };
        _ideaRepoMock.Setup(r => r.GetByIdAsync("idea-1")).ReturnsAsync(idea);
        _ideaRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Idea>())).Returns(Task.CompletedTask);

        await CreateService().DeleteAsync("user-1", "idea-1");

        _ideaRepoMock.Verify(r => r.DeleteAsync(idea), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_IdeaNotFound_ThrowsKeyNotFoundException()
    {
        _ideaRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Idea?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService().DeleteAsync("user-1", "idea-inexistente"));
    }

    [Fact]
    public async Task DeleteAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var idea = new Idea { Id = "idea-1", Title = "Original", CreatedByUserId = "user-1", TopicId = "topic-1" };
        _ideaRepoMock.Setup(r => r.GetByIdAsync("idea-1")).ReturnsAsync(idea);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateService().DeleteAsync("outro-user", "idea-1"));
    }

    [Fact]
    public async Task GetByTopicIdAsync_TopicNotFound_ThrowsKeyNotFoundException()
    {
        _mongoContextMock.Setup(c => c.Topics).Returns(CreateTopicCollectionMock(false).Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService().GetByTopicIdAsync("topic-inexistente"));
    }

    [Fact]
    public async Task GetByTopicIdAsync_ValidTopic_ReturnsIdeas()
    {
        _mongoContextMock.Setup(c => c.Topics).Returns(CreateTopicCollectionMock(true).Object);

        var ideas = new List<Idea>
        {
            new() { Id = "idea-1", Title = "Ideia 1", TopicId = "topic-1", CreatedByUserId = "user-1" },
            new() { Id = "idea-2", Title = "Ideia 2", TopicId = "topic-1", CreatedByUserId = "user-1" }
        };
        _ideaRepoMock.Setup(r => r.GetByTopicIdAsync("topic-1")).ReturnsAsync(ideas);

        var result = await CreateService().GetByTopicIdAsync("topic-1");

        Assert.Equal(2, result.Count());
    }
}