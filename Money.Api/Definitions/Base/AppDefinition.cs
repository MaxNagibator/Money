namespace Money.Api.Definitions.Base;

/// <summary>
///     Базовая реализация для <see cref="IAppDefinition" />
/// </summary>
public abstract class AppDefinition : IAppDefinition
{
    /// <inheritdoc />
    public virtual int ServiceOrderIndex => 0;

    /// <inheritdoc />
    public virtual int ApplicationOrderIndex => 0;

    /// <inheritdoc />
    public virtual bool Enabled { get; protected set; } = true;

    /// <inheritdoc />
    public virtual bool Exported { get; protected set; }

    /// <inheritdoc />
    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    /// <inheritdoc />
    public virtual void ConfigureApplication(IApplicationBuilder app)
    {
    }
}
