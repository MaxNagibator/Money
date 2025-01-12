using Money.Api.Middlewares;
using Money.Business.Configs;

namespace Money.Api.Definitions;

public class FilesStorageDefinition : AppDefinition
{
    public override bool Enabled => false; // TODO: ждёт починки https://github.com/MaxNagibator/Money/issues/23
    public override int ApplicationOrderIndex => 2;

    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        var filesStorage = builder.Configuration.GetSection("FilesStorage");

        var filesStorageConfig = filesStorage.Get<FilesStorageConfig>();

        if (string.IsNullOrEmpty(filesStorageConfig?.Path))
        {
            throw new ArgumentException("FilesStoragePath is missing");
        }

        if (Directory.Exists(filesStorageConfig.Path) == false)
        {
            Directory.CreateDirectory(filesStorageConfig.Path);
        }

        builder.Services.Configure<FilesStorageConfig>(filesStorage);
    }

    public override void ConfigureApplication(WebApplication app)
    {
        app.UseMiddleware<FileUploadMiddleware>();
    }
}
