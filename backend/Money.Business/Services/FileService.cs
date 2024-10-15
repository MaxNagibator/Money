using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Money.Business.Configs;
using Money.Business.Enums;
using Money.Common.Exceptions;
using File = Money.Business.Models.File;

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
        var fileExt = Path.GetExtension(file.FileName).ToLower();
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
            throw new IncorrectFileException("Вы выбрали неправильный файл. Попробуйте выбрать другой.");
        }

        var fileExt = Path.GetExtension(fileName).ToLower();
        var fileType = _supportedFilesExtensions.Where(kvp => kvp.Value.Contains(fileExt)).Select(kvp => kvp.Key)
            .FirstOrDefault();
        return fileType;
    }

    public void CheckFileType(string filename)
    {
        var fileExt = Path.GetExtension(filename).ToLower();
        if (!_supportedFilesExtensions.Values.Any(x => x.Contains(fileExt)))
        {
            throw new UnsupportedFileExtensionException("К сожалению, такой тип файла не поддерживается. Попробуйте выбрать другой файл.");
        }
    }
}