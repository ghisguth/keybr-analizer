using System.Globalization;

using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services.Reporting;

public class FatigueReportingService(IConsoleHelper consoleHelper) : IFatigueReportingService
{
	public void PrintFatigueAnalysis(IEnumerable<KeybrSession> sessions)
	{
		ArgumentNullException.ThrowIfNull(sessions);

		var fatigue = sessions.GroupBy(s => s.TimeStamp.ToLocalTime().Date)
			.SelectMany(g => g.OrderBy(s => s.TimeStamp).Select((s, i) => new { Index = i, s }))
			.GroupBy(x => Math.Min(x.Index / 5, 20))
			.OrderBy(g => g.Key)
			.Select(g => new
			{
				Block = g.Key == 10 ? "50+" : $"{(g.Key * 5) + 1}-{(g.Key * 5) + 5}",
				Sess = g.Count(),
				Wpm = g.Average(x => x.s.Speed) / 5.0,
				Acc = g.Average(x => 1.0 - ((double)x.s.Errors / x.s.Length)) * 100
			});

		string[] headers = ["LESSON RANGE", "SAMPLES", "Avg WPM", "Avg Acc", "FATIGUE"];
		var rows = fatigue.Select(g => new[] { g.Block, g.Sess.ToString(CultureInfo.InvariantCulture), g.Wpm.FormatWpm(), g.Acc.FormatAccuracy(), consoleHelper.GetProgressBar(g.Acc, width: 10) });
		consoleHelper.WriteTable(headers, rows, [false, true, true, true, false], title: "SESSION FATIGUE ANALYSIS");
	}

	public void PrintFatigueIndicator(IEnumerable<KeybrSession> sessions)
	{
		ArgumentNullException.ThrowIfNull(sessions);

		var todayLessons = sessions.GroupBy(s => s.TimeStamp.ToLocalTime().Date).OrderByDescending(g => g.Key).FirstOrDefault();
		if (todayLessons == null || todayLessons.Count() < 3)
		{
			return;
		}

		var data = todayLessons.OrderBy(s => s.TimeStamp).Select((s, i) => new { X = (double)i, Y = s.Speed / 5.0 }).ToList();
		var n = data.Count;
		var den = (n * data.Sum(d => d.X * d.X)) - Math.Pow(data.Sum(d => d.X), 2);
		if (Math.Abs(den) < 0.000001)
		{
			return;
		}

		var slope = ((n * data.Sum(d => d.X * d.Y)) - (data.Sum(d => d.X) * data.Sum(d => d.Y))) / den;
		var color = slope < -0.5 ? Ansi.Red : (slope < 0 ? Ansi.Yellow : Ansi.Green);
		consoleHelper.WriteLine($"{Ansi.Bold}Session Fatigue Indicator:{Ansi.Reset}");
		consoleHelper.WriteLine($"  Slope: {color}{slope:F2} WPM/lesson {(slope < 0 ? "↘" : "↗")}{Ansi.Reset}");
		if (slope < -0.5)
		{
			consoleHelper.WriteLine($"  {Ansi.BgRed}{Ansi.Black}{Ansi.Bold} FATIGUE DETECTED: DIMINISHING RETURNS. STOP TRAINING. {Ansi.Reset}");
		}

		consoleHelper.WriteLine();
	}
}
