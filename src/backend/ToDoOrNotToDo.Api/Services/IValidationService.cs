using ToDoOrNotToDo.Api.DTOs;

namespace ToDoOrNotToDo.Api.Services;

public interface IValidationService
{
    void ValidateCreateTaskRequest(CreateTaskRequest request);
    void ValidateUpdateTaskRequest(UpdateTaskRequest request);
}
