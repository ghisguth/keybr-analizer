using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services;

public interface IKeybrDataService
{
	Task<IReadOnlyList<KeybrSession>?> LoadLatestSessionsAsync(CancellationToken cancellationToken);
	string? GetLatestDataFilePath();
}
