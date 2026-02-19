using KeybrAnalyzer.Options;

using Microsoft.Extensions.Options;

namespace KeybrAnalyzer.Services.Reporting;

public class TrainingStateService(
	IOptions<KeybrAnalyzerOptions> options,
	IKeyStatusService keyStatusService)
	: ITrainingStateService
{
	public TrainingStateModel GetTrainingState()
	{
		return new TrainingStateModel
		{
			UnlockedGroups = [.. options.Value.OpenedKeys.Select(GetGroup)],
			FocusGroups = [.. options.Value.FocusKeys.Select(GetGroup)],
			LockedTiers = [.. options.Value.LockedKeys.Select(kvp => new TierGroupModel
			{
				Label = kvp.Key,
				Keys = [.. GetKeys(kvp.Value.SelectMany(s => s))]
			})]
		};
	}

	private KeyGroupModel GetGroup(string s)
	{
		return new KeyGroupModel
		{
			Keys = [.. GetKeys(s)]
		};
	}

	private IEnumerable<KeyStatusModel> GetKeys(IEnumerable<char> chars)
	{
		foreach (var c in chars)
		{
			yield return new KeyStatusModel
			{
				Key = c,
				Status = keyStatusService.GetKeyStatus(c)
			};
		}
	}
}
