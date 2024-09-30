using Microsoft.AspNetCore.Mvc;
using Money.Common.Exceptions;

namespace Money.Api.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (EntityExistsException exception)
        {
            await HandleExceptionAsync(context, exception, StatusCodes.Status409Conflict);
        }
        catch (NotFoundException exception)
        {
            await HandleExceptionAsync(context, exception, StatusCodes.Status404NotFound);
        }
        catch (PermissionException exception)
        {
            await HandleExceptionAsync(context, exception, StatusCodes.Status403Forbidden);
        }
        catch (BusinessException exception)
        {
            await HandleExceptionAsync(context, exception, StatusCodes.Status400BadRequest);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            ProblemDetails problemDetails = new()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = $"Server Error: {exception.Message}",
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        ProblemDetails problemDetails = new()
        {
            Status = statusCode,
            Title = exception.Message,
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
