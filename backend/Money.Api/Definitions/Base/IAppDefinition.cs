namespace Money.Api.Definitions.Base;

/// <summary>
/// Интерфейс абстракции определения приложения
/// </summary>
public interface IAppDefinition
{
    /// <summary>
    /// Индекс порядка для включения в конвейер в методе ConfigureServices(). Значение по умолчанию — 0, поэтому индекс
    /// порядка может быть неопределён.
    /// </summary>
    int ServiceOrderIndex { get; }

    /// <summary>
    /// Индекс порядка для включения в конвейер в методе ConfigureApplication(). Значение по умолчанию — 0, поэтому индекс
    /// порядка может быть неопределён.
    /// </summary>
    int ApplicationOrderIndex { get; }

    /// <summary>
    /// Включение или отключение регистрации в конвейере для текущего определения приложения.
    /// </summary>
    /// <remarks>Значение по умолчанию — <c>True</c></remarks>
    bool Enabled { get; }

    /// <summary>
    /// Включает или отключает экспорт определения как контента для модуля, который может быть экспортирован.
    /// </summary>
    /// <remarks>Значение по умолчанию — <c>False</c></remarks>
    bool Exported { get; }

    /// <summary>
    /// Настройка служб для текущего приложения
    /// </summary>
    /// <param name="builder">Экземпляр <see cref="WebApplicationBuilder" /></param>
    void ConfigureServices(WebApplicationBuilder builder);

    /// <summary>
    /// Настройка приложения для текущего приложения
    /// </summary>
    /// <param name="app">Экземпляр <see cref="WebApplication" /></param>
    void ConfigureApplication(WebApplication app);
}
