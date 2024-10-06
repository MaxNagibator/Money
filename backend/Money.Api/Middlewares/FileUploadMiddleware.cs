using Money.Business.Services;

namespace Money.Api.Middlewares;

public class FileUploadMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, FileService fileService)
    {
        if (context.Request.HasFormContentType && context.Request.Form.Files.Any())
        {
            var form = await context.Request.ReadFormAsync();
            var files = form.Files;

            foreach (var file in files)
            {
                fileService.CheckFileType(file.FileName);
            }
        }

        await next(context);
    }
}