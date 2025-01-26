using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;

namespace HomeworkApp.Dal.Repositories.Interfaces;

public interface ITaskCommentRepository
{
    Task<long> Add(TaskCommentEntityV1 taskComment, CancellationToken token);
    Task Update(TaskCommentEntityV1 taskComment, CancellationToken token);
    Task SetDeleted(long taskCommentId, CancellationToken token);
    Task<TaskCommentEntityV1[]> Get(TaskCommentGetModel query, CancellationToken token);
}