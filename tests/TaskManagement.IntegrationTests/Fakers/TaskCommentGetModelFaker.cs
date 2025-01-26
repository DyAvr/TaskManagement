using AutoBogus;
using Bogus;
using HomeworkApp.Dal.Models;
using HomeworkApp.IntegrationTests.Creators;

namespace HomeworkApp.IntegrationTests.Fakers;

public static class TaskCommentGetModelFaker
{
    private static readonly object Lock = new();

    private static readonly Faker<TaskCommentGetModel> Faker = new AutoFaker<TaskCommentGetModel>()
        .RuleFor(x => x.TaskId, _ => Create.RandomId());

    public static TaskCommentGetModel[] Generate(int count = 1)
    {
        lock (Lock)
        {
            return Faker.Generate(count).ToArray();
        }
    }

    public static TaskCommentGetModel WithTaskId(
        this TaskCommentGetModel src, 
        long taskId)
        => src with { TaskId = taskId };
    
    public static TaskCommentGetModel WithIncludeDeleted(
        this TaskCommentGetModel src, 
        bool includeDeleted)
        => src with { IncludeDeleted = includeDeleted };    
}