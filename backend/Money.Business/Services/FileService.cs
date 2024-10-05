using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Money.Business.Configs;
using Money.Business.Enums;
using File = Money.Business.Models.File;

namespace Money.Business.Services;

public class FileService(IOptionsSnapshot<FilesStorageConfig> config)
{
    private readonly FilesStorageConfig _config = config.Value;
    public async Task<File> Upload(IFormFile file)
    {
        var fileExt = Path.GetExtension(file.FileName);
        var fileName = Guid.NewGuid() + fileExt;
        var source = Path.Combine(_config.Path, fileName);

        await using (var fileStream = new FileStream(source, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
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