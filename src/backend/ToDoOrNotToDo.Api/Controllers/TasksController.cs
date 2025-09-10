using Microsoft.AspNetCore.Mvc;
using ToDoOrNotToDo.Api.DTOs;
using ToDoOrNotToDo.Api.Services;

namespace ToDoOrNotToDo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITasksService _tasksService;

    public TasksController(ITasksService tasksService)
    {
        _tasksService = tasksService;
    }

    /// <summary>
    /// Gets all tasks sorted by status (active first), then by creation/completion date
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
    {
        var tasks = await _tasksService.ListAsync();
        var taskDtos = tasks.Select(t => t.ToDto());
        return Ok(taskDtos);
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var task = await _tasksService.CreateAsync(request.Title);
            var taskDto = task.ToDto();
            return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, taskDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    [HttpPatch]
    public async Task<ActionResult<TaskDto>> UpdateTask([FromBody] UpdateTaskRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!request.IsValid)
        {
            return BadRequest(new { error = "At least one field (title or isCompleted) must be provided for update." });
        }

        try
        {
            var task = await _tasksService.UpdateAsync(request.Id, request.Title, request.IsCompleted);
            
            if (task == null)
            {
                return NotFound(new { error = "Task not found." });
            }

            var taskDto = task.ToDto();
            return Ok(taskDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a task by ID
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var deleted = await _tasksService.DeleteAsync(id);
        
        // Always return 204 No Content (idempotent behavior)
        return NoContent();
    }
}
