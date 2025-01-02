namespace Money.Api.Definitions.Base;

/// <summary>
/// Базовая реализация для <see cref="IAppDefinition" />
/// </summary>
public abstract class AppDefinition : IAppDefinition
{
    /// <inheritdoc />
    public virtual int ServiceOrderIndex => 0;

    /// <inheritdoc />
    public virtual int ApplicationOrderIndex => 0;

    /// <inheritdoc />
    public virtual bool Enabled => true;

    /// <inheritdoc />
    public virtual bool Exported => false;

    /// <inheritdoc />
    public virtual void ConfigureServices(WebApplicationBuilder builder)
    {
    }

    /// <inheritdoc />
    public virtual void ConfigureApplication(WebApplication app)
    {
    }
}
