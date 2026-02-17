using System.Text;

namespace KeybrAnalyzer.Helpers;

public class ConsoleHelper(TextWriter writer, ITableWriter tableWriter) : IConsoleHelper
{
	private readonly TextWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));
	private readonly ITableWriter _tableWriter = tableWriter ?? throw new ArgumentNullException(nameof(tableWriter));

	public ConsoleHelper()
		: this(Console.Out, new TableWriter(Console.Out))
	{
	}

	public void WriteTitle(string title)
	{
		ArgumentNullException.ThrowIfNull(title);
		_writer.WriteLine();
		_writer.WriteLine($"{Ansi.Bold}{Ansi.Cyan}---- {title} ----{Ansi.Reset}");
		_writer.WriteLine();
	}

	public void WriteLine(string message = "") => _writer.WriteLine(message);

	public void Write(string message) => _writer.Write(message);

	public void WriteTable(string[] headers, IEnumerable<string[]> rows, bool[]? rightAlign = null, int maxColumns = 100, string? title = null)
	{
		_tableWriter.WriteTable(headers, rows, rightAlign, maxColumns, title);
	}

	public string GetSparkline(IEnumerable<double> values, int maxWidth = 60, double? min = null, double? max = null)
	{
		var list = values.ToList();
		if (list.Count == 0)
		{
			return string.Empty;
		}

		if (list.Count > maxWidth)
		{
			list = Downsample(list, maxWidth);
		}

		var minVal = min ?? list.Min();
		var maxVal = max ?? list.Max();
		var range = maxVal - minVal;

		if (range < 0.001)
		{
			return new string('▄', list.Count);
		}

		var sparks = new[] { ' ', '▂', '▃', '▄', '▅', '▆', '▇', '█' };
		var result = new StringBuilder();

		foreach (var val in list)
		{
			var clampedVal = Math.Clamp(val, minVal, maxVal);
			var index = (int)((clampedVal - minVal) / range * (sparks.Length - 1));
			result.Append(sparks[index]);
		}

		return result.ToString();
	}

	public string GetProgressBar(double percentage, int width = 20, string? label = null)
	{
		var capped = Math.Clamp(percentage, 0, 100);
		var filledWidth = (int)(capped / 100.0 * width);
		var emptyWidth = width - filledWidth;

		var color = capped switch
		{
			>= 98 => Ansi.Green,
			>= 95 => Ansi.Yellow,
			_ => Ansi.Red
		};

		var bar = new string('█', filledWidth) + new string('░', emptyWidth);
		var result = $"{color}{bar}{Ansi.Reset}";

		if (label != null)
		{
			result += $" {label}";
		}

		return result;
	}

	private static List<double> Downsample(List<double> list, int maxWidth)
	{
		var downsampled = new List<double>();
		var chunkSize = (double)list.Count / maxWidth;
		for (var i = 0; i < maxWidth; i++)
		{
			var start = (int)(i * chunkSize);
			var end = (int)((i + 1) * chunkSize);
			if (end > list.Count)
			{
				end = list.Count;
			}

			if (start < end)
			{
				downsampled.Add(list.GetRange(start, end - start).Average());
			}
		}

		return downsampled;
	}
}
