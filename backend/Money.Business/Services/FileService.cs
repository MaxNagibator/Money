using Microsoft.Extensions.Options;
using Money.Business.Configs;
using File = Money.Business.Models.File;

namespace Money.Business.Services;

public class FileService(IOptionsSnapshot<FilesStorageConfig> config)
{
    private static readonly Dictionary<FileTypes, List<string>> SupportedFilesExtensions =
        new()
        {
            [FileTypes.Word] = [".doc", ".docx"],
            [FileTypes.Excel] = [".xls", ".xlsx"],
        };

    private readonly FilesStorageConfig _config = config.Value;

    public async Task<File> Upload(IFormFile file, CancellationToken cancellationToken = default)
    {
        string fileExt = Path.GetExtension(file.FileName).ToLower();
        string fileName = Guid.NewGuid() + fileExt;
        string source = Path.Combine(_config.Path, fileName);

        await using (FileStream fileStream = new(source, FileMode.Create))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        FileTypes fileType = GetFileType(fileName);

        return new File
        {
            FileName = fileName,
            FileType = fileType,
        };
    }

    public void CheckFileType(string filename)
    {
        string fileExtension = Path.GetExtension(filename).ToLower();

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

        string fileExtension = Path.GetExtension(fileName).ToLower();

        FileTypes fileType = SupportedFilesExtensions.Where(kvp => kvp.Value.Contains(fileExtension))
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

        return fileType;
    }
}
