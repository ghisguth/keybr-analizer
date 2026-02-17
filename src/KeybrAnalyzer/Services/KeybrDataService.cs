using System.Text.Json;

using KeybrAnalyzer.Models;
using KeybrAnalyzer.Options;

using Microsoft.Extensions.Options;

namespace KeybrAnalyzer.Services;

public class KeybrDataService(
	ILogger<KeybrDataService> logger,
	IOptions<KeybrAnalyzerOptions> options)
	: IKeybrDataService
{
	public async Task<IReadOnlyList<KeybrSession>?> LoadLatestSessionsAsync(CancellationToken cancellationToken)
	{
		var filePath = GetLatestDataFilePath();
		if (filePath == null)
		{
			logger.LogError("Could not find any typing-data*.json files.");
			return null;
		}

		if (logger.IsEnabled(LogLevel.Debug))
		{
			logger.LogDebug("Processing {FilePath}...", filePath);
		}

		var json = await File.ReadAllTextAsync(filePath, cancellationToken);
		return JsonSerializer.Deserialize<List<KeybrSession>>(json);
	}

	public string? GetLatestDataFilePath()
	{
		var searchPaths = new List<string>();

		if (!string.IsNullOrWhiteSpace(options.Value.DataFilePath))
		{
			if (File.Exists(options.Value.DataFilePath))
			{
				return options.Value.DataFilePath;
			}

			searchPaths.Add(options.Value.DataFilePath);
		}

		if (!string.IsNullOrWhiteSpace(options.Value.SourceDirectory))
		{
			searchPaths.Add(options.Value.SourceDirectory);
		}

		searchPaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));

		foreach (var path in searchPaths.Distinct())
		{
			if (!Directory.Exists(path))
			{
				continue;
			}

			var file = Directory.GetFiles(path, "typing-data*.json")
				.OrderByDescending(File.GetLastWriteTime)
				.FirstOrDefault();

			if (file != null)
			{
				return file;
			}
		}

		return null;
	}
}
