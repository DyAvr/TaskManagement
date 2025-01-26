using FluentAssertions;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Creators;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using Xunit;
using TaskStatus = HomeworkApp.Dal.Enums.TaskStatus;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskRepositoryTests
{
    private readonly ITaskRepository _repository;

    public TaskRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskRepository;
    }

    [Fact]
    public async Task Add_Task_Success()
    {
        // Arrange
        const int count = 5;

        var tasks = TaskEntityV1Faker.Generate(count);
        
        // Act
        var results = await _repository.Add(tasks, default);

        // Asserts
        results.Should().HaveCount(count);
        results.Should().OnlyContain(x => x > 0);
    }
    
    [Fact]
    public async Task Get_SingleTask_Success()
    {
        // Arrange
        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId);
        
        // Act
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);
        
        // Asserts
        results.Should().HaveCount(1);
        var task = results.Single();

        task.Should().BeEquivalentTo(expectedTask);
    }
    
    [Fact]
    public async Task AssignTask_Success()
    {
        // Arrange
        var assigneeUserId = Create.RandomId();
        
        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId)
            .WithAssignedToUserId(assigneeUserId);
        var assign = AssignTaskModelFaker.Generate()
            .First()
            .WithTaskId(expectedTaskId)
            .WithAssignToUserId(assigneeUserId);
        
        // Act
        await _repository.Assign(assign, default);
        
        // Asserts
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);
        
        results.Should().HaveCount(1);
        var task = results.Single();
        
        expectedTask = expectedTask with {Status = assign.Status};
        task.Should().BeEquivalentTo(expectedTask);
    }

    [Fact]
    public async Task GetSubTasksInStatus_WithHierarchicalTasks_Success()
    {
        // Arrange
        var parentTask = TaskEntityV1Faker.Generate()
            .First()
            .WithStatus(TaskStatus.Done);
        var parentTaskId = (await _repository.Add(new[] { parentTask }, default)).Single();
        
        var childTask = TaskEntityV1Faker.Generate()
            .First()
            .WithStatus(TaskStatus.InProgress)
            .WithParentTaskId(parentTaskId);
        var childTaskId = (await _repository.Add(new[] { childTask }, default)).Single();
        
        var subChildTasks = TaskEntityV1Faker.Generate(2);
        subChildTasks[0] = subChildTasks[0]
            .WithStatus(TaskStatus.Done)
            .WithParentTaskId(childTaskId);
        subChildTasks[1] = subChildTasks[1]
            .WithStatus(TaskStatus.Canceled)
            .WithParentTaskId(childTaskId);
        await _repository.Add(subChildTasks, default);
        
        var targetStatuses = new[] { TaskStatus.Done, TaskStatus.Canceled };
        
        // Act
        var result = await _repository.GetSubTasksInStatus(parentTaskId, targetStatuses, CancellationToken.None);
        
        // Asserts
        result.Should().NotBeEmpty().And.NotContain(t => t.TaskId == parentTask.Id);
        result.Should().OnlyContain(t => targetStatuses.Contains(t.Status));
        result
            .All(t => 
                t.ParentTaskIds.Length == 3 
                && t.ParentTaskIds[0] == parentTaskId 
                && t.ParentTaskIds[1] == childTaskId 
                && t.ParentTaskIds[2] == t.TaskId)
            .Should().BeTrue();
    }
}
