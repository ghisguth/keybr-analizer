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
		"' \"",
		"( ) [ ] { }"
	];

	public Collection<string> FocusKeys { get; } =
	[
		"( ) [ ] { }"
	];

	public Dictionary<string, Collection<string>> LockedKeys { get; } = new()
	{
		["Tier 1 (Basic Operators)"] = ["_ / + - = * < >"],
		["Tier 2 (Logical Operators)"] = ["! % & |"],
		["Tier 3 (Advanced C#)"] = ["@ # ^ \\ ~ ? ` $"],
		["Tier 4 (Numbers)"] = ["1 2 3 4 5 6 7 8 9 0"]
	};
}
