using System.Globalization;

using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;
using KeybrAnalyzer.Options;

using Microsoft.Extensions.Options;

namespace KeybrAnalyzer.Services.Reporting;

public class FatigueReportingService(IConsoleHelper consoleHelper, IOptions<KeybrAnalyzerOptions> options) : IFatigueReportingService
{
	public void PrintFatigueAnalysis(IEnumerable<KeybrSession> sessions)
	{
		ArgumentNullException.ThrowIfNull(sessions);

		var allSessions = sessions.ToList();
		if (allSessions.Count == 0)
		{
			return;
		}

		// 1. Render "THIS SESSION FATIGUE ANALYSIS" for current day only
		var todayDate = allSessions.Max(s => s.TimeStamp.ToLocalTime().Date);
		var todaySessions = allSessions.Where(s => s.TimeStamp.ToLocalTime().Date == todayDate).ToList();

		if (todaySessions.Count > 0)
		{
			var todayFatigue = todaySessions
				.OrderBy(s => s.TimeStamp)
				.Select((s, i) => new { Index = i, s })
				.GroupBy(x => Math.Min(x.Index / 5, 20))
				.OrderBy(g => g.Key)
				.Select(g => new
				{
					Block = g.Key == 20 ? "100+" : $"{(g.Key * 5) + 1}-{(g.Key * 5) + 5}",
					Wpm = g.Average(x => x.s.Speed) / 5.0,
					Acc = g.Average(x => 1.0 - ((double)x.s.Errors / x.s.Length)) * 100
				});

			string[] todayHeaders = ["LESSON RANGE", "Avg WPM", "Avg Acc", "FATIGUE"];
			var todayRows = todayFatigue.Select(g => new[] { g.Block, g.Wpm.FormatWpm(), g.Acc.FormatAccuracy(), consoleHelper.GetProgressBar(g.Acc, width: 10) });
			consoleHelper.WriteTable(todayHeaders, todayRows, [false, true, true, false], title: "THIS SESSION FATIGUE ANALYSIS");
		}

		// 2. Hide "SESSION FATIGUE ANALYSIS" behind ALL data option
		if (options.Value.ShowAllStats)
		{
			var fatigue = allSessions.GroupBy(s => s.TimeStamp.ToLocalTime().Date)
				.SelectMany(g => g.OrderBy(s => s.TimeStamp).Select((s, i) => new { Index = i, s }))
				.GroupBy(x => Math.Min(x.Index / 5, 20))
				.OrderBy(g => g.Key)
				.Select(g => new
				{
					Block = g.Key == 20 ? "100+" : $"{(g.Key * 5) + 1}-{(g.Key * 5) + 5}",
					Sess = g.Count(),
					Wpm = g.Average(x => x.s.Speed) / 5.0,
					Acc = g.Average(x => 1.0 - ((double)x.s.Errors / x.s.Length)) * 100
				});

			string[] headers = ["LESSON RANGE", "SAMPLES", "Avg WPM", "Avg Acc", "FATIGUE"];
			var rows = fatigue.Select(g => new[] { g.Block, g.Sess.ToString(CultureInfo.InvariantCulture), g.Wpm.FormatWpm(), g.Acc.FormatAccuracy(), consoleHelper.GetProgressBar(g.Acc, width: 10) });
			consoleHelper.WriteTable(headers, rows, [false, true, true, true, false], title: "SESSION FATIGUE ANALYSIS");
		}
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
