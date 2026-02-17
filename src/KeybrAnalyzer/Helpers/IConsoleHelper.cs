namespace KeybrAnalyzer.Helpers;

public interface IConsoleHelper
{
	void WriteTitle(string title);

	void WriteLine(string message = "");

	void Write(string message);

	void WriteTable(string[] headers, IEnumerable<string[]> rows, bool[]? rightAlign = null, int maxColumns = 100, string? title = null);

	string GetSparkline(IEnumerable<double> values, int maxWidth = 60, double? min = null, double? max = null);

	string GetProgressBar(double percentage, int width = 20, string? label = null);
}
