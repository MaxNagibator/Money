namespace Money.Api.Middlewares;

public class FileUploadMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.HasFormContentType && context.Request.Form.Files.Any())
        {
            var form = await context.Request.ReadFormAsync();

            foreach (var file in form.Files)
            {
                FilesService.CheckFileType(file.FileName);
            }
        }

        await next(context);
    }
}
