using KeybrAnalyzer.Options;

namespace KeybrAnalyzer.Helpers;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddKeybrAnalyzerOptions(this IServiceCollection services, IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		services.Configure<KeybrAnalyzerOptions>(configuration.GetSection(KeybrAnalyzerOptions.SectionName));

		// Ensure configuration overrides defaults for collections
		services.PostConfigure<KeybrAnalyzerOptions>(options =>
		{
			var section = configuration.GetSection(KeybrAnalyzerOptions.SectionName);
			if (section.Exists())
			{
				// If specific keys are present in config, they will override the defaults
				if (section.GetSection("OpenedKeys").Exists())
				{
					options.OpenedKeys.Clear();
				}

				if (section.GetSection("FocusKeys").Exists())
				{
					options.FocusKeys.Clear();
				}

				if (section.GetSection("LockedKeys").Exists())
				{
					options.LockedKeys.Clear();
				}

				section.Bind(options);
			}
		});

		return services;
	}
}
