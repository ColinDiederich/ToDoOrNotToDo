using Microsoft.AspNetCore.Mvc;
using ToDoOrNotToDo.Api.DTOs;
using ToDoOrNotToDo.Api.Services;
using ToDoOrNotToDo.Api.Exceptions;

namespace ToDoOrNotToDo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITasksService _tasksService;
    private readonly IValidationService _validationService;

    public TasksController(ITasksService tasksService, IValidationService validationService)
    {
        _tasksService = tasksService;
        _validationService = validationService;
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
        _validationService.ValidateCreateTaskRequest(request);
        var task = await _tasksService.CreateAsync(request.Title);
        var taskDto = task.ToDto();
        return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, taskDto);
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    [HttpPatch]
    public async Task<ActionResult<TaskDto>> UpdateTask([FromBody] UpdateTaskRequest request)
    {
        _validationService.ValidateUpdateTaskRequest(request);
        var task = await _tasksService.UpdateAsync(request.Id.Value, request.Title, request.IsCompleted);
        if (task == null)
        {
            throw new NotFoundException("Task", request.Id.Value);
        }
        var taskDto = task.ToDto();
        return Ok(taskDto);
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
