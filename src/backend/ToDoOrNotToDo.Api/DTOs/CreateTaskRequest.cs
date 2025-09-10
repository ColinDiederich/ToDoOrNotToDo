using System.ComponentModel.DataAnnotations;

namespace ToDoOrNotToDo.Api.DTOs;

public class CreateTaskRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
}
