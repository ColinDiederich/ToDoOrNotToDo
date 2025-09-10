using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using ToDoOrNotToDo.Api.DTOs;
using ToDoOrNotToDo.Api.Exceptions;

namespace ToDoOrNotToDo.Api.Filters;

public class ErrorHandlingFilter : IExceptionFilter
{
    private readonly ILogger<ErrorHandlingFilter> _logger;

    public ErrorHandlingFilter(ILogger<ErrorHandlingFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var response = new ErrorResponse();

        switch (context.Exception)
        {
            case ValidationException validationEx:
                response.Error.Code = ErrorCodes.ValidationError;
                response.Error.Message = validationEx.Message;
                response.Error.Details = validationEx.Errors;
                context.Result = new BadRequestObjectResult(response);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case NotFoundException notFoundEx:
                response.Error.Code = ErrorCodes.NotFound;
                response.Error.Message = notFoundEx.Message;
                context.Result = new NotFoundObjectResult(response);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            default:
                _logger.LogError(context.Exception, "An unhandled exception occurred");
                response.Error.Code = ErrorCodes.ServerError;
                response.Error.Message = "An internal server error occurred.";
                context.Result = new ObjectResult(response);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        context.ExceptionHandled = true;
    }
}
