namespace KeybrAnalyzer.Helpers;

public static class ConfigurationBuilderExtensions
{
	public static IConfigurationBuilder AddKeybrConfiguration(this IConfigurationBuilder builder, string[]? args = null)
	{
		ArgumentNullException.ThrowIfNull(builder);

		// Order of priority (last one wins):
		// 1. AppData Roaming
		// 2. AppData Local
		// 3. User Home (~/.config/keybranalyzer/config.json)
		// 4. Environment Variables
		// 5. Command Line Arguments
		var searchPaths = new List<string>
		{
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "keybranalyzer", "config.json"),
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "keybranalyzer", "config.json"),
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "keybranalyzer", "config.json")
		};

		foreach (var path in searchPaths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct())
		{
			builder.AddJsonFile(path, optional: true, reloadOnChange: true);
		}

		builder.AddEnvironmentVariables();

		if (args != null)
		{
			builder.AddCommandLine(args);
		}

		return builder;
	}
}
