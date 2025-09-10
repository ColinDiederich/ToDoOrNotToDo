using ToDoOrNotToDo.Api.DTOs;

namespace ToDoOrNotToDo.Api.Exceptions;

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }

    public ValidationException(string message, string field, string error) : base(message)
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}
