namespace Money.Api.Definitions.Base;

/// <summary>
/// Расширение для <see cref="WebApplicationBuilder" />
/// </summary>
public static class AppDefinitionExtensions
{
    /// <summary>
    /// Поиск всех определений в проекте и их включение в конвейер.
    /// <br />
    /// Использует <see cref="IServiceCollection" /> для регистрации.
    /// </summary>
    /// <remarks>
    /// При выполнении в среде разработки доступно больше диагностической информации в консоли.
    /// </remarks>
    /// <param name="builder">Экземпляр <see cref="WebApplicationBuilder" />.</param>
    /// <param name="entryPointsAssembly">Типы сборок точек входа.</param>
    public static void AddDefinitions(this WebApplicationBuilder builder, params Type[] entryPointsAssembly)
    {
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<IAppDefinition>>();

        try
        {
            var appDefinitionInfo = builder.Services.BuildServiceProvider().GetService<AppDefinitionCollection>();
            var definitionCollection = appDefinitionInfo ?? new AppDefinitionCollection();

            foreach (var entryPoint in entryPointsAssembly)
            {
                definitionCollection.AddEntryPoint(entryPoint.Name);

                var types = entryPoint.Assembly.ExportedTypes.Where(Predicate);

                var instances = types.Select(Activator.CreateInstance)
                    .Cast<IAppDefinition>()
                    .Where(definition => definition.Enabled)
                    .OrderBy(definition => definition.ServiceOrderIndex)
                    .ToList();

                foreach (var definition in instances)
                {
                    definitionCollection.AddInfo(new(definition, entryPoint.Name, definition.Enabled, definition.Exported));
                }
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("[Найдены точки входа AppDefinitions]: {@Items}", string.Join(", ", definitionCollection.EntryPoints));
            }

            var items = definitionCollection.GetDistinct().ToList();

            foreach (var item in items)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("[AppDefinitions ConfigureServices с порядковым индексом {@ServiceOrderIndex}]: {@AssemblyName}:{@AppDefinitionName} — {EnabledOrDisabled} {ExportEnabled}",
                        item.Definition.ServiceOrderIndex,
                        item.AssemblyName,
                        item.Definition.GetType().Name,
                        item.Enabled ? "включён" : "отключён",
                        item.Exported ? "(экспортируемый)" : string.Empty);
                }

                item.Definition.ConfigureServices(builder);
            }

            builder.Services.AddSingleton(definitionCollection);

            if (logger.IsEnabled(LogLevel.Debug) == false)
            {
                return;
            }

            var skipped = definitionCollection.GetEnabled().Except(items).ToList();

            if (skipped.Count == 0)
            {
                return;
            }

            logger.LogWarning("[AppDefinitions пропущены ConfigureServices]: {Count}", skipped.Count);

            foreach (var item in skipped)
            {
                logger.LogWarning("[AppDefinitions пропущены ConfigureServices]: {@AssemblyName}:{@AppDefinitionName} — {EnabledOrDisabled}",
                    item.AssemblyName,
                    item.Definition.GetType().Name,
                    item.Enabled ? "включён" : "отключён");
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Ошибка при добавлении определений. Тип ошибки: {ExceptionType}, сообщение: {Message}, стек вызовов: {StackTrace}",
                exception.GetType().Name,
                exception.Message,
                exception.StackTrace);

            throw;
        }
    }

    /// <summary>
    /// Поиск всех определений в проекте и их включение в конвейер.
    /// <br />
    /// Использует <see cref="WebApplication" /> для регистрации.
    /// </summary>
    /// <remarks>
    /// При выполнении в среде разработки доступно больше диагностической информации в консоли.
    /// </remarks>
    /// <param name="source">Экземпляр <see cref="WebApplication" />.</param>
    public static void UseDefinitions(this WebApplication source)
    {
        var logger = source.Services.GetRequiredService<ILogger<AppDefinition>>();
        var definitionCollection = source.Services.GetRequiredService<AppDefinitionCollection>();

        var items = definitionCollection.GetDistinct().OrderBy(item => item.Definition.ApplicationOrderIndex).ToList();

        foreach (var item in items)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("[AppDefinitions ConfigureApplication с порядковым индексом {@ApplicationOrderIndex}]: {@AssemblyName}:{@AppDefinitionName} — {EnabledOrDisabled}",
                    item.Definition.ApplicationOrderIndex,
                    item.AssemblyName,
                    item.Definition.GetType().Name,
                    item.Enabled ? "включён" : "отключён");
            }

            item.Definition.ConfigureApplication(source);
        }

        if (logger.IsEnabled(LogLevel.Debug) == false)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("[Применено AppDefinitions]: {Count} из {Total}", items.Count, definitionCollection.GetEnabled().Count());
            }

            return;
        }

        var skipped = definitionCollection.GetEnabled().Except(items).ToList();

        if (skipped.Count == 0)
        {
            logger.LogInformation("[Применено AppDefinitions]: {Count} из {Total}", items.Count, definitionCollection.GetEnabled().Count());
            return;
        }

        logger.LogWarning("[AppDefinitions пропущены ConfigureApplication]: {Count}", skipped.Count);

        foreach (var item in skipped)
        {
            logger.LogWarning("[AppDefinitions пропущены ConfigureApplication]: {@AssemblyName}:{@AppDefinitionName} — {EnabledOrDisabled} {ExportEnabled}",
                item.AssemblyName,
                item.Definition.GetType().Name,
                item.Enabled ? "включён" : "отключён",
                item.Exported ? "(экспортируемый)" : string.Empty);
        }

        logger.LogInformation("[Применено AppDefinitions]: {Count} из {Total}", items.Count, definitionCollection.GetEnabled().Count());
    }

    /// <summary>
    /// Поиск определения AppDefinition в списке типов
    /// </summary>
    /// <param name="type">Тип для проверки</param>
    /// <returns>Результат проверки</returns>
    private static bool Predicate(Type type)
    {
        return type is { IsAbstract: false, IsInterface: false } && typeof(AppDefinition).IsAssignableFrom(type);
    }
}
