using FluentAssertions;
using HomeworkApp.Bll.Services;
using HomeworkApp.Dal.Repositories.Interfaces;
using Moq;
using Xunit;

namespace HomeworkApp.UnitTests.ServicesTests;

public class RateLimitServiceTests
{
    private readonly Mock<IRateLimitRepository> _repositoryMock;
    private readonly RateLimitService _service;
    private const string testIpAddress = "128.128.128.128";

    public RateLimitServiceTests()
    {
        _repositoryMock = new Mock<IRateLimitRepository>();
        _service = new RateLimitService(_repositoryMock.Object);
    }

    [Fact]
    public async Task IsLimitExceeded_LimitNotReached_ReturnsFalse()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.IsLimitExceeded(testIpAddress, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.IsLimitExceeded(testIpAddress, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _repositoryMock
            .Verify(x => x.IsLimitExceeded(testIpAddress, It.IsAny<int>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task IsLimitExceeded_LimitExceeded_ReturnsTrue()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.IsLimitExceeded(testIpAddress, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IsLimitExceeded(testIpAddress, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repositoryMock
            .Verify(x => x.IsLimitExceeded(testIpAddress, It.IsAny<int>(), CancellationToken.None), Times.Once);
    }
    
    [Fact]
    public async Task IsLimitExceeded_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _repositoryMock
            .Setup(x => x.IsLimitExceeded(testIpAddress, It.IsAny<int>(), cancellationTokenSource.Token))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var action = async () => await _service.IsLimitExceeded(testIpAddress, cancellationTokenSource.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
        _repositoryMock
            .Verify(x => x.IsLimitExceeded(testIpAddress, It.IsAny<int>(), cancellationTokenSource.Token), Times.Once);
    }
}