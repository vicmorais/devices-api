using Devices.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Devices.Api.Middleware;

public class GlobalExceptionHandler: IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            DeviceNotFoundException ex => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Device Not Found",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5"
            },

            DeviceInUseException ex => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Device In Use",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10"
            },

            InvalidDeviceStateException ex => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Device State",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            },

            ValidationException ex => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                Extensions =
                {
                    ["errors"] = ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        )
                }
            },

            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            }
        };

        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        httpContext.Response.StatusCode = problemDetails.Status ?? 500;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}