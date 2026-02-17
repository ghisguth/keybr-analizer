namespace KeybrAnalyzer.Helpers;

public static class FormattingExtensions
{
	public static string FormatError(this double val)
	{
		var color = val switch
		{
			< 2.0 => Ansi.Green,
			< 5.0 => Ansi.Yellow,
			_ => Ansi.Red
		};
		return $"{color}{val:F2}%{Ansi.Reset}";
	}

	public static string FormatAccuracy(this double val)
	{
		var color = val switch
		{
			>= 98.0 => Ansi.Green,
			>= 95.0 => Ansi.Yellow,
			_ => Ansi.Red
		};
		return $"{color}{val,6:F2}%{Ansi.Reset}";
	}

	public static string FormatWpm(this double val)
	{
		return $"{Ansi.Cyan}⚡{val,6:F2}{Ansi.Reset}";
	}

	public static string ToTimeStr(this double totalSeconds)
	{
		var timeSpan = TimeSpan.FromSeconds(totalSeconds);
		return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
	}
}
