using Money.Business.Enums;
using File = Money.Business.Models.File;

namespace Money.Api.Dto.Files;

/// <summary>
/// Файл.
/// </summary>
public class FileDto
{
    /// <summary>
    /// Название файла.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Тип файла.
    /// </summary>
    public FileTypes FileType { get; set; }

    /// <summary>
    /// Фабричный метод для создания DTO файла на основе бизнес-модели.
    /// </summary>
    /// <param name="file">Бизнес-модель файла.</param>
    /// <returns>Новый объект <see cref="FileDto" />.</returns>
    public static FileDto FromBusinessModel(File file)
    {
        return new()
        {
            FileName = file.FileName,
            FileType = file.FileType,
        };
    }
}
