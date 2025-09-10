using Microsoft.AspNetCore.Mvc;

namespace ToDoOrNotToDo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
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
