using FluentAssertions;
using HomeworkApp.Dal.Providers.Interfaces;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskCommentRepositoryTests
{
    private readonly ITaskCommentRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TaskCommentRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskCommentRepository;
        _dateTimeProvider = fixture.DateTimeProvider;
    }

    [Fact]
    public async Task Add_TaskComment_Success()
    {
        // Arrange
        var taskComment = TaskCommentEntityV1Faker.Generate()
            .First();
        
        // Act
        var result = await _repository.Add(taskComment, default);

        // Asserts
        result.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task Update_TaskCommentMessage_Success()
    {
        // Arrange
        const string initialMessage = "initial message";
        const string updatedMessage = "updated message";

        var originalTaskComment  = TaskCommentEntityV1Faker.Generate()
            .First()
            .WithMessage(initialMessage);
        var taskCommentId = await _repository.Add(originalTaskComment, default);

        var updatingTaskComment = originalTaskComment
            .WithId(taskCommentId)
            .WithMessage(updatedMessage);

        // Act
        await _repository.Update(updatingTaskComment, CancellationToken.None);

        // Asserts
        var query = TaskCommentGetModelFaker.Generate()
            .First()
            .WithTaskId(updatingTaskComment.TaskId);
        var results = await _repository.Get(query, CancellationToken.None);
        
        var modifiedAt = _dateTimeProvider.Now();
        var expectedTaskComment = updatingTaskComment
            .WithModifiedAt(modifiedAt);
        results
            .Should().ContainSingle(x => x.Id == taskCommentId)
            .Which
            .Should().BeEquivalentTo(expectedTaskComment);
    }
    
    [Fact]
    public async Task SetDeleted_TaskComment_Success()
    {
        // Arrange
        var originalTaskComment = TaskCommentEntityV1Faker.Generate()
            .First();
        var taskCommentId = await _repository.Add(originalTaskComment, default);
        var createdTaskComment = originalTaskComment
            .WithId(taskCommentId);

        // Act
        await _repository.SetDeleted(taskCommentId, CancellationToken.None);

        // Asserts
        var query = TaskCommentGetModelFaker.Generate()
            .First()
            .WithTaskId(createdTaskComment.TaskId)
            .WithIncludeDeleted(true);
        var results = await _repository.Get(query, CancellationToken.None);
        
        var modifiedAt = _dateTimeProvider.Now();
        results
            .Should().ContainSingle(x => x.Id == taskCommentId)
            .Which
            .DeletedAt.Should().Be(modifiedAt);
    }
    
    [Fact]
    public async Task Get_NonDeletedTaskComments_Success()
    {
        // Arrange
        const long taskId = 1;   
        var taskComment1 = TaskCommentEntityV1Faker.Generate()
            .First()
            .WithAt(DateTimeOffset.UtcNow)
            .WithTaskId(taskId);

        var taskComment2 = TaskCommentEntityV1Faker.Generate()
            .First()
            .WithAt(DateTimeOffset.UtcNow.AddDays(1))
            .WithTaskId(taskId);
        
        var taskComment3 = TaskCommentEntityV1Faker.Generate()
            .First()
            .WithTaskId(taskId);

        var taskComment1Id = await _repository.Add(taskComment1, CancellationToken.None);
        var taskComment2Id = await _repository.Add(taskComment2, CancellationToken.None);
        var taskComment3Id = await _repository.Add(taskComment3, CancellationToken.None);

        await _repository.SetDeleted(taskComment3Id, CancellationToken.None);
        var taskCommentIds = new []{taskComment1Id, taskComment2Id, taskComment3Id};

        var query = TaskCommentGetModelFaker.Generate()
            .First()
            .WithTaskId(taskId)
            .WithIncludeDeleted(false);

        // Act
        var results = await _repository.Get(query, CancellationToken.None);

        // Asserts
        var actualTaskComments = results
            .Where(r => taskCommentIds.Contains(r.Id))
            .ToArray();

        actualTaskComments.Should().HaveCount(2);
        
        // Checking the descending order by creation date 
        actualTaskComments[0].Id.Should().Be(taskComment2Id);
        actualTaskComments[1].Id.Should().Be(taskComment1Id);
    }
}
