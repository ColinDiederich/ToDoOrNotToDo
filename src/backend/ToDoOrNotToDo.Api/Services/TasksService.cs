using Microsoft.EntityFrameworkCore;
using ToDoOrNotToDo.Api.Data;

namespace ToDoOrNotToDo.Api.Services;

public class TasksService : ITasksService
{
    private readonly AppDbContext _context;

    public TasksService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskEntity>> ListAsync()
    {
        // Get all tasks and sort in memory to handle complex sorting requirements
        var tasks = await _context.Tasks.ToListAsync();
        
        // Active tasks first, then completed tasks
        var activeTasks = tasks
            .Where(t => !t.IsCompleted)
            .OrderBy(t => t.CreatedAt) // CreatedAt ASC
            .ThenBy(t => t.Id); // Id ASC
        
        var completedTasks = tasks
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.CompletedAt) // CompletedAt DESC
            .ThenByDescending(t => t.Id); // Id DESC
        
        return activeTasks.Concat(completedTasks);
    }

    public async Task<TaskEntity> CreateAsync(string title)
    {
        var now = DateTime.UtcNow;
        var task = new TaskEntity
        {
            Title = title,
            IsCompleted = false,
            CreatedAt = now,
            UpdatedAt = now,
            CompletedAt = null
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        
        return task;
    }

    public async Task<TaskEntity?> UpdateAsync(int id, string? title = null, bool? isCompleted = null)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return null;
        }

        var originalTitle = task.Title;
        var originalIsCompleted = task.IsCompleted;

        // Track if any changes were made
        bool hasChanges = false;

        // Update title if provided
        if (!string.IsNullOrWhiteSpace(title))
        {
            var trimmedTitle = title.Trim();
            if (trimmedTitle != originalTitle)
            {
                task.Title = trimmedTitle;
                hasChanges = true;
            }
        }

        // Update completion status if provided
        if (isCompleted.HasValue)
        {
            if (isCompleted.Value != originalIsCompleted)
            {
                task.IsCompleted = isCompleted.Value;
                hasChanges = true;

                // Handle completion status change
                if (isCompleted.Value)
                {
                    // Marking as completed
                    task.CompletedAt = DateTime.UtcNow;
                }
                else
                {
                    // Marking as incomplete
                    task.CompletedAt = null;
                }
            }
        }

        // Only update UpdatedAt if there were actual changes
        if (hasChanges)
        {
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return task;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return false; // Idempotent - treat not found as success
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}
