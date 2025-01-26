using Bogus;
using FluentAssertions;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Fixtures;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class RateLimitRepositoryTests
{
    private readonly IRateLimitRepository _repository;

    public RateLimitRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.RateLimitRepository;
    }
    
    [Fact]
    public async Task IsLimitExceeded_SameIpAddress_Success()
    {
        // Arrange
        const int limit = 5;
        var ipAddress = new Faker().Internet.Ip();
        
        // Act
        var results = new List<bool>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(await _repository.IsLimitExceeded(ipAddress, limit, CancellationToken.None));
        }
        
        // Assert
        results.Should().HaveCount(10);
        results.Take(limit).Should().AllSatisfy(result => result.Should().BeFalse());
        results.Skip(limit).Should().AllSatisfy(result => result.Should().BeTrue());
    }
}