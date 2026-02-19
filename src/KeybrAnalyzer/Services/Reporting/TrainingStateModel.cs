using System.Collections.ObjectModel;

namespace KeybrAnalyzer.Services.Reporting;

public class TrainingStateModel
{
	public Collection<KeyGroupModel> UnlockedGroups { get; init; } = [];

	public Collection<KeyGroupModel> FocusGroups { get; init; } = [];

	public Collection<TierGroupModel> LockedTiers { get; init; } = [];
}
