using System.Collections.ObjectModel;

namespace KeybrAnalyzer.Services.Reporting;

public class TierGroupModel
{
	public string Label { get; init; } = string.Empty;

	public Collection<KeyStatusModel> Keys { get; init; } = [];
}
