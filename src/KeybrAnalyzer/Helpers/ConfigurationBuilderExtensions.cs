namespace KeybrAnalyzer.Helpers;

public static class ConfigurationBuilderExtensions
{
	public static IConfigurationBuilder AddKeybrConfiguration(this IConfigurationBuilder builder, string[]? args = null)
	{
		ArgumentNullException.ThrowIfNull(builder);

		// Order of priority (last one wins):
		// 1. AppData Roaming (config.ini, then config.json)
		// 2. AppData Local (config.ini, then config.json)
		// 3. User Home (config.ini, then config.json)
		// 4. Environment Variables
		// 5. Command Line Arguments
		var searchDirectories = new List<string>
		{
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "keybranalyzer"),
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "keybranalyzer"),
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "keybranalyzer")
		};

		foreach (var dir in searchDirectories.Where(d => !string.IsNullOrWhiteSpace(d)).Distinct())
		{
			if (Directory.Exists(dir))
			{
				builder.AddIniFile(Path.Combine(dir, "config.ini"), optional: true, reloadOnChange: true);
				builder.AddJsonFile(Path.Combine(dir, "config.json"), optional: true, reloadOnChange: true);
			}
		}

		builder.AddEnvironmentVariables();

		if (args != null)
		{
			builder.AddCommandLine(args);
		}

		return builder;
	}
}
