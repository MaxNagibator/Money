using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Money.Common.Exceptions;

namespace Money.Api.Middlewares;

public class ExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails? problemDetails = null;
        int? statusCode;

        switch (exception)
        {
            case PermissionException:
                statusCode = StatusCodes.Status403Forbidden;
                break;

            case NotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                break;

            case EntityExistsException:
                statusCode = StatusCodes.Status409Conflict;
                break;

            case BusinessException:
                statusCode = StatusCodes.Status400BadRequest;
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                problemDetails = new()
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Извините, произошла непредвиденная ошибка",
                    Detail = "Мы уже работаем над ее устранением. Пожалуйста, попробуйте снова позже",
                };

                break;
        }

        problemDetails ??= new()
        {
            Status = statusCode,
            Title = exception.Message,
        };

        logger.LogError(exception, "Произошло исключение: {Message}", exception.Message);

        httpContext.Response.StatusCode = statusCode.Value;

        return problemDetailsService.TryWriteAsync(new()
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
        });
    }
}
