using Dapper;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Providers.Interfaces;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Options;

namespace HomeworkApp.Dal.Repositories;

public class TaskCommentRepository : PgRepository, ITaskCommentRepository
{
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public TaskCommentRepository(
        IOptions<DalOptions> dalSettings,
        IDateTimeProvider dateTimeProvider) : base(dalSettings.Value)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<long> Add(TaskCommentEntityV1 taskComment, CancellationToken token)
    {
        const string sqlQuery = @"
insert into task_comments (task_id, author_user_id, message, at) 
values (@TaskId, @AuthorUserId, @Message, @At)
returning id;
";

        await using var connection = await GetConnection();
        return await connection.QuerySingleAsync<long>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    TaskId = taskComment.TaskId,
                    AuthorUserId = taskComment.AuthorUserId,
                    Message = taskComment.Message,
                    At = taskComment.At
                },
                cancellationToken: token));
    }

    public async Task Update(TaskCommentEntityV1 taskComment, CancellationToken token)
    {
        const string sqlQuery = @"
update task_comments
   set message = @Message
     , modified_at = @ModifiedAt
 where id = @Id;
";

        await using var connection = await GetConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    Id = taskComment.Id,
                    Message = taskComment.Message,
                    ModifiedAt = _dateTimeProvider.Now()
                },
                cancellationToken: token));
    }

    public async Task SetDeleted(long taskCommentId, CancellationToken token)
    {
        const string sqlQuery = @"
update task_comments
   set deleted_at = @DeletedAt
 where id = @TaskCommentId;
";

        await using var connection = await GetConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    TaskCommentId = taskCommentId,
                    DeletedAt = _dateTimeProvider.Now()
                },
                cancellationToken: token));
    }

    public async Task<TaskCommentEntityV1[]> Get(TaskCommentGetModel query, CancellationToken token)
    {
        var sqlQuery = @"
select id
     , task_id
     , author_user_id
     , message
     , at
     , modified_at
     , deleted_at
  from task_comments
 where task_id = @TaskId
";

        if (!query.IncludeDeleted)
        {
            sqlQuery += " and deleted_at is null";
        }

        sqlQuery += " order by at desc";

        var cmd = new CommandDefinition(
            sqlQuery,
            new
            {
                TaskId = query.TaskId
            },
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);

        await using var connection = await GetConnection();
        return (await connection.QueryAsync<TaskCommentEntityV1>(cmd))
            .ToArray();
    }
}