using Money.Api.Definitions.Base;
using Money.Business.Configs;

namespace Money.Api.Definitions;

public class FilesStorageDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        var filesStorage = builder.Configuration.GetSection("FilesStorage");

        var filesStorageConfig = filesStorage.Get<FilesStorageConfig>();

        if (filesStorageConfig == null || string.IsNullOrEmpty(filesStorageConfig.Path))
        {
            throw new ApplicationException("FilesStoragePath is missing");
        }

        if (!Directory.Exists(filesStorageConfig.Path))
        {
            Directory.CreateDirectory(filesStorageConfig.Path);
        }

        builder.Services.Configure<FilesStorageConfig>(filesStorage);
    }
}