using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Money.Business.Configs;
using File = Money.Business.Models.File;

namespace Money.Business.Services;

public class FileService(IOptionsSnapshot<FilesStorageConfig> config)
{
    private static readonly Dictionary<FileTypes, List<string>> SupportedFilesExtensions = new()
    {
        [FileTypes.Word] = [".doc", ".docx"],
        [FileTypes.Excel] = [".xls", ".xlsx"],
    };

    private readonly FilesStorageConfig _config = config.Value;

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

        return new()
        {
            FileName = fileName,
            FileType = fileType,
        };
    }

    public void CheckFileType(string filename)
    {
        var fileExtension = Path.GetExtension(filename).ToLower();

        if (SupportedFilesExtensions.Values.Any(x => x.Contains(fileExtension)) == false)
        {
            throw new UnsupportedFileExtensionException("К сожалению, такой тип файла не поддерживается. Попробуйте выбрать другой файл.");
        }
    }

    private static FileTypes GetFileType(string fileName)
    {
        if (fileName.Contains('.') == false)
        {
            throw new IncorrectFileException("Вы выбрали неправильный файл. Попробуйте выбрать другой.");
        }

        var fileExtension = Path.GetExtension(fileName).ToLower();

        var fileType = SupportedFilesExtensions.Where(kvp => kvp.Value.Contains(fileExtension))
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

        return fileType;
    }
}
