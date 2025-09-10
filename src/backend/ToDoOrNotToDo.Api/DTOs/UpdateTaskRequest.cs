namespace ToDoOrNotToDo.Api.DTOs;

public class UpdateTaskRequest
{
    public int? Id { get; set; }
    public string? Title { get; set; }
    public bool? IsCompleted { get; set; }
}
