using Money.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Money.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (EntityExistsException exception)
            {
                await HandleExceptionAsync(context, exception, StatusCodes.Status409Conflict);
            }
            catch (NotFoundException exception)
            {
                await HandleExceptionAsync(context, exception, StatusCodes.Status404NotFound);
            }
            catch (IncorrectDataException exception)
            {
                await HandleExceptionAsync(context, exception, StatusCodes.Status400BadRequest);
            }
            catch (PermissionException exception)
            {
                await HandleExceptionAsync(context, exception, StatusCodes.Status403Forbidden);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = $"Server Error: {exception.Message}"
                };

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
        {
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = exception.Message
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
