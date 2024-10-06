using Money.Api.Dto.Payments;
using Money.Business.Enums;

namespace Money.Api.Dto.Files;

/// <summary>
///     Файл.
/// </summary>
public class FileDto
{
    /// <summary>
    ///     Название файла.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Тип файла.
    /// </summary>
    public FileTypes FileType { get; set; }

    /// <summary>
    ///     Фабричный метод для создания DTO файла на основе бизнес-модели.
    /// </summary>
    /// <param name="file">Бизнес-модель файла.</param>
    /// <returns>Новый объект <see cref="FileDto" />.</returns>
    public static FileDto FromBusinessModel(Money.Business.Models.File file)
    {
        return new FileDto
        {
            FileName = file.FileName,
            FileType = file.FileType
        };
    }
}