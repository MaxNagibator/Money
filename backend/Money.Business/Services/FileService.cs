using Microsoft.Extensions.Options;
using Money.Business.Configs;
using Money.Business.Enums;
using File = Money.Business.Models.File;
using Microsoft.AspNetCore.Http;

namespace Money.Business.Services;

public class FileService(IOptionsSnapshot<FilesStorageConfig> config)
{
    private readonly FilesStorageConfig _config = config.Value;

    private static readonly Dictionary<FileTypes, List<string>> _supportedFilesExtensions =
        new Dictionary<FileTypes, List<string>>()
        {
            { FileTypes.Word, [".doc", ".docx"] },
            { FileTypes.Excel, [".xls", ".xlsx"] },
        };

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
        if (!fileName.Contains('.'))
        {
            throw new IncorrectFileException("Неправильный файл!");
        }

        var fileExt = Path.GetExtension(fileName);
        var fileType = _supportedFilesExtensions.Where(kvp => kvp.Value.Contains(fileExt)).Select(kvp => kvp.Key)
            .FirstOrDefault();
        return fileType;
    }
}