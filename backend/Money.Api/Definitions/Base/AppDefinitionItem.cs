namespace Money.Api.Definitions.Base;

/// <summary>
/// Информация о <see cref="IAppDefinition" />
/// </summary>
/// <param name="Definition">Экземпляр определения приложения</param>
/// <param name="AssemblyName">Имя сборки, содержащей определение</param>
/// <param name="Enabled">Включено ли определение в процесс</param>
/// <param name="Exported">Можно ли экспортировать определение</param>
public sealed record AppDefinitionItem(IAppDefinition Definition, string AssemblyName, bool Enabled, bool Exported);
