namespace Money.Business.Models;

/// <summary>
/// Файл.
/// </summary>
public class File
{
    /// <summary>
    /// Название файла.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Тип файла.
    /// </summary>
    public FileTypes FileType { get; set; }
}
