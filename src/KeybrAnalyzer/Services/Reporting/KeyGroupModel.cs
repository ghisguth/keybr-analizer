using System.Collections.ObjectModel;

namespace KeybrAnalyzer.Services.Reporting;

public class KeyGroupModel
{
	public Collection<KeyStatusModel> Keys { get; init; } = [];
}
