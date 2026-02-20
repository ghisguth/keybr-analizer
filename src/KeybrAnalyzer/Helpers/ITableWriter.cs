using System.Text.RegularExpressions;

namespace KeybrAnalyzer.Helpers;

public interface ITableWriter
{
	void WriteTable(string[] headers, IEnumerable<string[]> rows, bool[]? rightAlign = null, int maxColumns = 100, string? title = null);
}

public partial class TableWriter(TextWriter writer) : ITableWriter
{
	private static readonly HashSet<int> WideRunes =
	[
		0x26A1, // ⚡
		0x2328, // ⌨️
		0x2705, // ✅
		0x2713, // ✓
		0x1F3AF, // 🎯
		0x1F4CA, // 📊
		0x1F512, // 🔒
		0x23F3, // ⏳
		0x1F4A5, // 💥
		0x1F9CA, // 🧊
		0x1F40C // 🐌
	];

	private readonly TextWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));

	public void WriteTable(string[] headers, IEnumerable<string[]> rows, bool[]? rightAlign = null, int maxColumns = 100, string? title = null)
	{
		ArgumentNullException.ThrowIfNull(headers);
		ArgumentNullException.ThrowIfNull(rows);

		if (headers.Length == 0)
		{
			return;
		}

		var rowList = rows.ToList();
		var allColumnWidths = CalculateColumnWidths(headers, rowList);

		if (headers.Length <= 1 || maxColumns >= headers.Length - 1)
		{
			PrintTablePage(headers, rowList, rightAlign, allColumnWidths, [.. Enumerable.Range(0, headers.Length)], title);
			return;
		}

		PrintChunkedTables(headers, rowList, rightAlign, maxColumns, title, allColumnWidths);
	}

	private static int[] CalculateColumnWidths(string[] headers, List<string[]> rows)
	{
		var widths = headers.Select(GetVisibleLength).ToArray();
		foreach (var row in rows)
		{
			for (var i = 0; i < row.Length; i++)
			{
				if (i < widths.Length)
				{
					widths[i] = Math.Max(widths[i], GetVisibleLength(row[i]));
				}
			}
		}

		return [.. widths.Select(w => w + 2)];
	}

	private static int GetVisibleLength(string text)
	{
		var stripped = StripAnsiCodesRegex().Replace(text, string.Empty);
		var length = 0;
		foreach (var rune in stripped.EnumerateRunes())
		{
			length += rune.Value > 0xFFFF || IsWide(rune.Value) ? 2 : 1;
		}

		return length;
	}

	private static bool IsWide(int value) => WideRunes.Contains(value);

	[GeneratedRegex(@"\u001b\[[0-9;]*m")]
	private static partial Regex StripAnsiCodesRegex();

	private void PrintChunkedTables(string[] headers, List<string[]> rowList, bool[]? rightAlign, int maxColumns, string? title, int[] allColumnWidths)
	{
		var startIndex = 1;
		while (startIndex < headers.Length)
		{
			var columnIndices = new List<int> { 0 };
			for (var i = 0; i < maxColumns && startIndex + i < headers.Length; i++)
			{
				columnIndices.Add(startIndex + i);
			}

			PrintTablePage(headers, rowList, rightAlign, allColumnWidths, [.. columnIndices], title);
			startIndex += maxColumns;

			if (startIndex < headers.Length)
			{
				_writer.WriteLine();
			}
		}
	}

	private void PrintTablePage(string[] allHeaders, List<string[]> allRows, bool[]? allRightAlign, int[] allWidths, int[] columnIndices, string? title)
	{
		string[] pageHeaders = [.. columnIndices.Select(i => allHeaders[i])];
		int[] pageWidths = [.. columnIndices.Select(i => allWidths[i])];
		bool[]? pageRightAlign = allRightAlign == null ? null : [.. columnIndices.Select(i => i < allRightAlign.Length && allRightAlign[i])];

		var totalTableWidth = pageWidths.Sum() + pageWidths.Length + 1;

		if (title != null)
		{
			WriteTitleInternal(title, totalTableWidth);
		}

		DrawLine(pageWidths, "┌", "┬", "┐");
		DrawRow(pageHeaders, pageWidths, null, Ansi.Bold);
		DrawLine(pageWidths, "├", "┼", "┤");

		foreach (var row in allRows)
		{
			string[] pageRow = [.. columnIndices.Select(i => i < row.Length ? row[i] : string.Empty)];
			DrawRow(pageRow, pageWidths, pageRightAlign);
		}

		DrawLine(pageWidths, "└", "┴", "┘");
	}

	private void WriteTitleInternal(string title, int width)
	{
		var dashesTotal = width - title.Length - 2;
		_writer.WriteLine();
		if (dashesTotal < 8)
		{
			_writer.WriteLine($"{Ansi.Bold}{Ansi.Cyan}---- {title} ----{Ansi.Reset}");
		}
		else
		{
			var leftDashes = dashesTotal / 2;
			var rightDashes = dashesTotal - leftDashes;
			_writer.WriteLine($"{Ansi.Bold}{Ansi.Cyan}{new string('-', leftDashes)} {title} {new string('-', rightDashes)}{Ansi.Reset}");
		}

		_writer.WriteLine();
	}

	private void DrawLine(int[] widths, string start, string middle, string end)
	{
		_writer.Write(start);
		for (var i = 0; i < widths.Length; i++)
		{
			_writer.Write(new string('─', widths[i]));
			if (i < widths.Length - 1)
			{
				_writer.Write(middle);
			}
		}

		_writer.WriteLine(end);
	}

	private void DrawRow(string[] row, int[] widths, bool[]? rightAlign, string format = "")
	{
		_writer.Write("│");
		for (var i = 0; i < row.Length; i++)
		{
			var content = row[i];
			var visibleLength = GetVisibleLength(content);
			var padding = widths[i] - visibleLength - 2;
			var alignRight = rightAlign != null && i < rightAlign.Length && rightAlign[i];

			_writer.Write(" ");
			if (alignRight)
			{
				_writer.Write(new string(' ', padding));
				_writer.Write(format + content + Ansi.Reset);
			}
			else
			{
				_writer.Write(format + content + Ansi.Reset);
				_writer.Write(new string(' ', padding));
			}

			_writer.Write(" │");
		}

		_writer.WriteLine();
	}
}
