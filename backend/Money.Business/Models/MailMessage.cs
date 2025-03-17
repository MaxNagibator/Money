using System.Diagnostics.CodeAnalysis;

namespace Money.Business.Models;

/// <summary>
/// Электронное письмо.
/// </summary>
/// <param name="email">Адрес электронной почты получателя.</param>
/// <param name="title">Заголовок письма.</param>
/// <param name="body">Тело письма.</param>
[method: SetsRequiredMembers]
public class MailMessage(string email, string title, string body)
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Адрес электронной почты получателя.
    /// </summary>
    public required string Email { get; init; } = email;

    /// <summary>
    /// Заголовок письма.
    /// </summary>
    public required string Title { get; init; } = title;

    /// <summary>
    /// Тело письма.
    /// </summary>
    public required string Body { get; init; } = body;

    /// <summary>
    /// Количество попыток отправки письма.
    /// </summary>
    public int RetryCount { get; set; }
}
