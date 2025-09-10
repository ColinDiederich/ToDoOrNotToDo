using ToDoOrNotToDo.Api.Data;
using ToDoOrNotToDo.Api.DTOs;

namespace ToDoOrNotToDo.Api.Services;

public static class Mapping
{
    /// <summary>
    /// Maps a TaskEntity to a TaskDto
    /// </summary>
    public static TaskDto ToDto(this TaskEntity entity)
    {
        return new TaskDto
        {
            Id = entity.Id,
            Title = entity.Title,
            IsCompleted = entity.IsCompleted,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CompletedAt = entity.CompletedAt
        };
    }
    
    /// <summary>
    /// Maps a CreateTaskRequest to a TaskEntity (partial mapping - sets defaults)
    /// </summary>
    public static TaskEntity ToEntity(this CreateTaskRequest request)
    {
        var now = DateTime.UtcNow;
        
        return new TaskEntity
        {
            Title = request.Title,
            IsCompleted = false, // Default to not completed
            CreatedAt = now,
            UpdatedAt = now,
            CompletedAt = null
        };
    }
    
    /// <summary>
    /// Updates an existing TaskEntity with values from UpdateTaskRequest
    /// </summary>
    public static void UpdateFromRequest(this TaskEntity entity, UpdateTaskRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            entity.Title = request.Title;
        }
        
        if (request.IsCompleted.HasValue)
        {
            entity.IsCompleted = request.IsCompleted.Value;
            
            // Set CompletedAt if task is being marked as completed
            if (request.IsCompleted.Value && !entity.IsCompleted)
            {
                entity.CompletedAt = DateTime.UtcNow;
            }
            // Clear CompletedAt if task is being marked as incomplete
            else if (!request.IsCompleted.Value && entity.IsCompleted)
            {
                entity.CompletedAt = null;
            }
        }
        
        // Always update the UpdatedAt timestamp
        entity.UpdatedAt = DateTime.UtcNow;
    }
}
