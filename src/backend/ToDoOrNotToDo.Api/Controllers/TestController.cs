using Microsoft.AspNetCore.Mvc;
using ToDoOrNotToDo.Api.Data;

namespace ToDoOrNotToDo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly AppDbContext _context;

    public TestController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet("sample")]
    public IActionResult GetSample()
    {
        var sample = new SampleDto
        {
            Id = 1,
            Name = "Test Item",
            Description = "This is a test item",
            NullField = null, // This should be omitted from JSON response
            OptionalField = "This field has a value",
            EmptyField = string.Empty
        };

        return Ok(sample);
    }

    [HttpGet("tasks")]
    public IActionResult GetTasks()
    {
        var tasks = _context.Tasks.ToList();
        return Ok(tasks);
    }
}

public class SampleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? NullField { get; set; }
    public string? OptionalField { get; set; }
    public string EmptyField { get; set; } = string.Empty;
}
