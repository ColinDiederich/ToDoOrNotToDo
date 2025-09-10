using ToDoOrNotToDo.Api.DTOs;
using ToDoOrNotToDo.Api.Exceptions;

namespace ToDoOrNotToDo.Api.Services;

public class ValidationService : IValidationService
{
    public void ValidateCreateTaskRequest(CreateTaskRequest request)
    {
        ValidateTitle(request.Title);
    }

    public void ValidateUpdateTaskRequest(UpdateTaskRequest request)
    {
        // Validate Id is provided and valid
        if (!request.Id.HasValue)
        {
            throw new ValidationException("Property validation failed.", "id", "Id is required.");
        }
        if(request.Title != null){
            ValidateTitle(request.Title);
        }
        else if(!request.IsCompleted.HasValue){
            throw new ValidationException("Property validation failed.", "request", "At least one field (title or isCompleted) must be provided for update.");
        }
    }

    public void ValidateTitle(string title){

        var trimmedTitle = title.Trim();
        
        if (string.IsNullOrEmpty(trimmedTitle))
        {
            throw new ValidationException("Property validation failed.", "title", "Title cannot be empty.");
        }
        
        if (trimmedTitle.Length > 100)
        {
            throw new ValidationException("Property validation failed.", "title", "Title cannot exceed 100 characters.");
        }
    }
}
