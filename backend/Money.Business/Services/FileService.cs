using Microsoft.Extensions.Options;
using Money.Business.Configs;
using Money.Business.Enums;
using File = Money.Business.Models.File;
using Microsoft.AspNetCore.Http;

namespace Money.Business.Services;

public class FileService(IOptionsSnapshot<FilesStorageConfig> config)
{
    private readonly FilesStorageConfig _config = config.Value;

    public async Task<File> Upload(IFormFile file, CancellationToken cancellationToken = default)
    {
        var fileExt = Path.GetExtension(file.FileName);
        var fileName = Guid.NewGuid() + fileExt;
        var source = Path.Combine(_config.Path, fileName);

        await using (var fileStream = new FileStream(source, FileMode.Create))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        var fileType = GetFileType(fileName);

        return new File
        {
            FileName = fileName,
            FileType = fileType,
        };
    }

    private static FileTypes GetFileType(string fileName)
    {
        var fileType = FileTypes.Unknown;
        if (!fileName.Contains('.'))
        {
            return fileType;
        }

        var fileExt = fileName.Substring(fileName.IndexOf('.')).ToLower();
        fileType = fileExt switch
        {
            ".doc" or ".docx" => FileTypes.Word,
            ".xls" or ".xlsx" => FileTypes.Excel,
            _ => fileType
        };
        return fileType;
    }
}