using System.ComponentModel.DataAnnotations;

namespace ToDoOrNotToDo.Api.DTOs;

public class UpdateTaskRequest
{
    [Required]
    public int Id { get; set; }
    
    [StringLength(100, MinimumLength = 1)]
    public string? Title { get; set; }
    
    public bool? IsCompleted { get; set; }
    
    public bool IsValid => !string.IsNullOrWhiteSpace(Title) || IsCompleted.HasValue;
}
