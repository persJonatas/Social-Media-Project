using CisApi.Business.DTOs;
using CisApi.Business.Interfaces;
using CisApi.Business.Services;
using CisApi.Data.Entities;
using CisApi.Data.Interfaces;
using Moq;

namespace CisApi.Tests;

public class VoteServiceTests
{
    private readonly Mock<IVoteRepository> _voteRepoMock = new();
    private readonly Mock<IIdeaRepository> _ideaRepoMock = new();

    private VoteService CreateService() =>
        new(_voteRepoMock.Object, _ideaRepoMock.Object);

    [Fact]
    public async Task CastAsync_ValidData_ReturnsVoteResponse()
    {
        var request = new VoteRequestDTO { IdeaId = "idea-1" };
        _ideaRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        _voteRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        _voteRepoMock.Setup(r => r.CastAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Vote { Id = "vote-1", IdeaId = "idea-1", UserId = "user-1", CreatedAt = DateTime.UtcNow });

        var service = CreateService();
        var result = await service.CastAsync(request, "user-1");

        Assert.Equal("vote-1", result.Id);
        Assert.Equal("idea-1", result.IdeaId);
        Assert.Equal("user-1", result.UserId);
    }

    [Fact]
    public async Task CastAsync_IdeaNotFound_ThrowsKeyNotFoundException()
    {
        var request = new VoteRequestDTO { IdeaId = "idea-inexistente" };
        _ideaRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CastAsync(request, "user-1"));
    }

    [Fact]
    public async Task CastAsync_AlreadyVoted_ThrowsInvalidOperationException()
    {
        var request = new VoteRequestDTO { IdeaId = "idea-1" };
        _ideaRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        _voteRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CastAsync(request, "user-1"));
    }

    [Fact]
    public async Task CancelAsync_ValidData_DeletesVote()
    {
        _ideaRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        _voteRepoMock.Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Vote { Id = "vote-1", IdeaId = "idea-1", UserId = "user-1" });

        var service = CreateService();
        await service.CancelAsync("idea-1", "user-1");

        _voteRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Vote>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_IdeaNotFound_ThrowsKeyNotFoundException()
    {
        _ideaRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CancelAsync("idea-inexistente", "user-1"));
    }

    [Fact]
    public async Task CancelAsync_VoteNotFound_ThrowsKeyNotFoundException()
    {
        _ideaRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        _voteRepoMock.Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((Vote?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CancelAsync("idea-1", "user-1"));
    }
}
