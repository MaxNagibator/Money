namespace Money.Api.Definitions.Base;

/// <summary>
/// Информация о коллекции <see cref="IAppDefinition" />
/// </summary>
internal sealed class AppDefinitionCollection
{
    private List<AppDefinitionItem> Items { get; } = [];

    /// <summary>
    /// Добавление найденного элемента в коллекцию
    /// </summary>
    /// <param name="entryPointName">Имя точки входа</param>
    public void AddEntryPoint(string entryPointName)
    {
        EntryPoints.Add(entryPointName);
    }

    /// <summary>
    /// Добавление собранной информации в коллекцию
    /// </summary>
    /// <param name="definition">Элемент определения приложения</param>
    internal void AddInfo(AppDefinitionItem definition)
    {
        var exists = Items.FirstOrDefault(item => item == definition);

        if (exists is null)
        {
            Items.Add(definition);
        }
    }

    /// <summary>
    /// Возвращает отсортированные и включённые элементы
    /// </summary>
    internal IEnumerable<AppDefinitionItem> GetEnabled()
    {
        return Items
            .Where(item => item.Definition.Enabled)
            .OrderBy(item => item.Definition.ServiceOrderIndex);
    }

    /// <summary>
    /// Возвращает уникальные включённые элементы
    /// </summary>
    internal IEnumerable<AppDefinitionItem> GetDistinct()
    {
        return GetEnabled()
            .Select(item => new { item.Definition.GetType().Name, AppDefinitionItem = item })
            .DistinctBy(x => x.Name)
            .Select(x => x.AppDefinitionItem);
    }

    /// <summary>
    /// Найденные имена точек входа
    /// </summary>
    internal List<string> EntryPoints { get; } = [];
}
