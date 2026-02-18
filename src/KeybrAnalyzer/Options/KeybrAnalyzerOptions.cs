using System.Collections.ObjectModel;

namespace KeybrAnalyzer.Options;

public class KeybrAnalyzerOptions
{
	public const string SectionName = "KeybrAnalyzer";

	public string SourceDirectory { get; set; } = string.Empty;
	public string DataFilePath { get; set; } = string.Empty;
	public bool RunOnStartup { get; set; } = true;
	public bool ShowAllStats { get; set; }

	public Collection<string> OpenedKeys { get; } =
	[
		"abcdefghijklmnopqrstuvwxyz",
		"ABCDEFGHIJKLMNOPQRSTUVWXYZ",
		"; : . ,",
		"_ ' \" ( ) [ ] { }",
		"/ + - = * < >",
		"? ! @ % & | # ~",
		"1 2 3 4 5 7 8 9 0"
	];

	public Collection<string> FocusKeys { get; } =
	[
		"\" ) 4 5 6 8 0"
	];

	public Dictionary<string, Collection<string>> LockedKeys { get; } = new()
	{
		["Tier 3 (Advanced C#)"] = ["^"],
	};
}
