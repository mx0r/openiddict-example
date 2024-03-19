using JetBrains.Annotations;

namespace Example.AuthServer.Extensions;

[PublicAPI]
public static class ConfigurationExtensions
{
    public static TOptions Configure<TOptions>(
        this IServiceCollection services, 
        IConfiguration config, string sectionName)
        where TOptions : class, new()
    {
        var options = new TOptions();
        if (config.GetSection(sectionName) is { } section)
        {
            section.Bind(options);
            services.Configure<TOptions>(section);
        }

        return options;
    }
}