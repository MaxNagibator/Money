namespace Money.Api.Middlewares;

public class FileUploadMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, FileService fileService)
    {
        if (context.Request.HasFormContentType && context.Request.Form.Files.Any())
        {
            var form = await context.Request.ReadFormAsync();

            foreach (var file in form.Files)
            {
                fileService.CheckFileType(file.FileName);
            }
        }

        await next(context);
    }
}
