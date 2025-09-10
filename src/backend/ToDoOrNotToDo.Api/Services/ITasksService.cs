using ToDoOrNotToDo.Api.Data;
using ToDoOrNotToDo.Api.DTOs;

namespace ToDoOrNotToDo.Api.Services;

public interface ITasksService
{
    /// <summary>
    /// Gets all tasks sorted by status (active first), then by creation/completion date
    /// </summary>
    Task<IEnumerable<TaskEntity>> ListAsync();
    
    /// <summary>
    /// Creates a new task with the specified title
    /// </summary>
    /// <param name="title">The title of the task (will be trimmed and validated)</param>
    /// <returns>The created task entity</returns>
    Task<TaskEntity> CreateAsync(string title);
    
    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="id">The ID of the task to update</param>
    /// <param name="title">Optional new title (will be trimmed)</param>
    /// <param name="isCompleted">Optional completion status</param>
    /// <returns>The updated task entity, or null if not found</returns>
    Task<TaskEntity?> UpdateAsync(int id, string? title = null, bool? isCompleted = null);
    
    /// <summary>
    /// Deletes a task by ID
    /// </summary>
    /// <param name="id">The ID of the task to delete</param>
    /// <returns>True if the task was deleted, false if not found (idempotent)</returns>
    Task<bool> DeleteAsync(int id);
}
