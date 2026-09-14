using CisApi.Business.DTOs;
using CisApi.Business.Services;
using CisApi.Data;
using CisApi.Data.Entities;
using MongoDB.Driver;
using Moq;

namespace CisApi.Tests;

public class TopicServiceTests
{
    private readonly Mock<IMongoDbContext> _contextMock = new();

    private TopicService CreateService() => new(_contextMock.Object);

    private Mock<IMongoCollection<Topic>> CreateCollectionMock(List<Topic> topics)
    {
        var collection = new Mock<IMongoCollection<Topic>>();
        var cursor = new Mock<IAsyncCursor<Topic>>();

        cursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(topics.Count > 0)
            .ReturnsAsync(false);
        cursor.Setup(c => c.Current).Returns(topics);

        collection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<Topic>>(),
            It.IsAny<FindOptions<Topic, Topic>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor.Object);

        return collection;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTopics()
    {
        var topics = new List<Topic>
        {
            new() { Id = "t1", Title = "Topic 1", CreatedByUserId = "u1" },
            new() { Id = "t2", Title = "Topic 2", CreatedByUserId = "u1" }
        };
        _contextMock.Setup(c => c.Topics).Returns(CreateCollectionMock(topics).Object);

        var result = await CreateService().GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_EmptyCollection_ReturnsEmpty()
    {
        _contextMock.Setup(c => c.Topics).Returns(CreateCollectionMock(new List<Topic>()).Object);

        var result = await CreateService().GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_TopicFound_ReturnsTopicResponse()
    {
        var topic = new Topic { Id = "t1", Title = "Topic 1", CreatedByUserId = "u1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _contextMock.Setup(c => c.Topics).Returns(CreateCollectionMock(new List<Topic> { topic }).Object);

        var result = await CreateService().GetByIdAsync("t1");

        Assert.NotNull(result);
        Assert.Equal("t1", result.Id);
        Assert.Equal("Topic 1", result.Title);
        Assert.Equal("u1", result.CreatedByUserId);
    }

    [Fact]
    public async Task GetByIdAsync_TopicNotFound_ReturnsNull()
    {
        _contextMock.Setup(c => c.Topics).Returns(CreateCollectionMock(new List<Topic>()).Object);

        var result = await CreateService().GetByIdAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTopicAsync_ValidData_ReturnsTopicResponse()
    {
        var collection = new Mock<IMongoCollection<Topic>>();
        collection.Setup(c => c.InsertOneAsync(
            It.IsAny<Topic>(),
            It.IsAny<InsertOneOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.Topics).Returns(collection.Object);

        var result = await CreateService().CreateTopicAsync("u1", new CreateTopicRequest
        {
            Title = "New Topic",
            Description = "Some description"
        });

        Assert.Equal("New Topic", result.Title);
        Assert.Equal("Some description", result.Description);
        Assert.Equal("u1", result.CreatedByUserId);
    }

    [Fact]
    public async Task CreateTopicAsync_NullDescription_ReturnsTopicWithNullDescription()
    {
        var collection = new Mock<IMongoCollection<Topic>>();
        collection.Setup(c => c.InsertOneAsync(
            It.IsAny<Topic>(),
            It.IsAny<InsertOneOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _contextMock.Setup(c => c.Topics).Returns(collection.Object);

        var result = await CreateService().CreateTopicAsync("u1", new CreateTopicRequest
        {
            Title = "No Description Topic"
        });

        Assert.Equal("No Description Topic", result.Title);
        Assert.Null(result.Description);
    }

    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsUpdatedTopic()
    {
        var topic = new Topic { Id = "t1", Title = "Old Title", Description = "Old Desc", CreatedByUserId = "u1" };
        var collection = CreateCollectionMock(new List<Topic> { topic });
        collection.Setup(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Topic>>(),
            It.IsAny<Topic>(),
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));
        _contextMock.Setup(c => c.Topics).Returns(collection.Object);

        var result = await CreateService().UpdateAsync("t1", "u1", new UpdateTopicRequest
        {
            Title = "New Title",
            Description = "New Desc"
        });

        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Desc", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_NullTitle_KeepsOriginalTitle()
    {
        var topic = new Topic { Id = "t1", Title = "Old Title", Description = "Old Desc", CreatedByUserId = "u1" };
        var collection = CreateCollectionMock(new List<Topic> { topic });
        collection.Setup(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Topic>>(),
            It.IsAny<Topic>(),
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));
        _contextMock.Setup(c => c.Topics).Returns(collection.Object);

        var result = await CreateService().UpdateAsync("t1", "u1", new UpdateTopicRequest
        {
            Title = null,
            Description = "New Desc"
        });

        Assert.Equal("Old Title", result.Title);
        Assert.Equal("New Desc", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_NullDescription_KeepsOriginalDescription()
    {
        var topic = new Topic { Id = "t1", Title = "Old Title", Description = "Old Desc", CreatedByUserId = "u1" };
        var collection = CreateCollectionMock(new List<Topic> { topic });
        collection.Setup(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<Topic>>(),
            It.IsAny<Topic>(),
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));
        _contextMock.Setup(c => c.Topics).Returns(collection.Object);

        var result = await CreateService().UpdateAsync("t1", "u1", new UpdateTopicRequest
        {
            Title = "New Title",
            Description = null
        });

        Assert.Equal("New Title", result.Title);
        Assert.Equal("Old Desc", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_TopicNotFound_ThrowsKeyNotFoundException()
    {
        _contextMock.Setup(c => c.Topics).Returns(CreateCollectionMock(new List<Topic>()).Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService().UpdateAsync("nonexistent", "u1", new UpdateTopicRequest { Title = "X" }));
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var topic = new Topic { Id = "t1", Title = "Title", CreatedByUserId = "u1" };
        _contextMock.Setup(c => c.Topics).Returns(CreateCollectionMock(new List<Topic> { topic }).Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateService().UpdateAsync("t1", "other-user", new UpdateTopicRequest { Title = "X" }));
    }

    [Fact]
    public async Task DeleteAsync_ValidData_CallsDeleteOne()
    {
        var topic = new Topic { Id = "t1", Title = "Title", CreatedByUserId = "u1" };
        var collection = CreateCollectionMock(new List<Topic> { topic });
        collection.Setup(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Topic>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteResult.Acknowledged(1));
        _contextMock.Setup(c => c.Topics).Returns(collection.Object);

        await CreateService().DeleteAsync("t1", "u1");

        collection.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<Topic>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_TopicNotFound_ThrowsKeyNotFoundException()
    {
        _contextMock.Setup(c => c.Topics).Returns(CreateCollectionMock(new List<Topic>()).Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateService().DeleteAsync("nonexistent", "u1"));
    }

    [Fact]
    public async Task DeleteAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        var topic = new Topic { Id = "t1", Title = "Title", CreatedByUserId = "u1" };
        _contextMock.Setup(c => c.Topics).Returns(CreateCollectionMock(new List<Topic> { topic }).Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateService().DeleteAsync("t1", "other-user"));
    }
}
