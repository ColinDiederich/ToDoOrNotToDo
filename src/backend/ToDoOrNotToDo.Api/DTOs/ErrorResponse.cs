namespace ToDoOrNotToDo.Api.DTOs;

public class ErrorResponse
{
    public ErrorInfo Error { get; set; } = new();
}

public class ErrorInfo
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Details { get; set; }
}

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string ServerError = "SERVER_ERROR";
}
