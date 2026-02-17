using System.Globalization;

using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;
using KeybrAnalyzer.Options;

using Microsoft.Extensions.Options;

namespace KeybrAnalyzer.Services.Reporting;

public class ProgressReportingService(
	IConsoleHelper consoleHelper,
	IOptions<KeybrAnalyzerOptions> options)
	: IProgressReportingService
{
	public void PrintDailyProgressTable(IEnumerable<KeybrSession> sessions)
	{
		ArgumentNullException.ThrowIfNull(sessions);

		var showAllStats = options.Value.ShowAllStats;
		var headersList = new List<string> { "DATE", "TIME", "SESS", "Avg WPM", "Max WPM", "Avg Acc", "Top Acc" };
		if (showAllStats)
		{
			headersList.Add("WPM History");
		}

		var dailyGroups = sessions.GroupBy(s => s.TimeStamp.ToLocalTime().Date)
			.Select(g => new
			{
				Date = g.Key,
				Time = g.Sum(s => s.Time) / 1000.0,
				Lessons = g.Count(),
				AvgWpm = g.Average(s => s.Speed) / 5.0,
				MaxWpm = g.Max(s => s.Speed) / 5.0,
				AvgAcc = g.Average(s => 1.0 - ((double)s.Errors / s.Length)) * 100,
				MaxAcc = g.Max(s => 1.0 - ((double)s.Errors / s.Length)) * 100,
				Speeds = g.Select(s => s.Speed / 5.0).ToList()
			})
			.OrderBy(g => g.Date).TakeLast(Constants.ProgressTableDays);

		var rows = dailyGroups.Select(g =>
		{
			var row = new List<string>
			{
				g.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
				g.Time.ToTimeStr(),
				g.Lessons.ToString(CultureInfo.InvariantCulture),
				g.AvgWpm.FormatWpm(),
				g.MaxWpm.FormatWpm(),
				g.AvgAcc.FormatAccuracy(),
				g.MaxAcc.FormatAccuracy()
			};
			if (showAllStats)
			{
				row.Add(consoleHelper.GetSparkline(g.Speeds, maxWidth: 60));
			}

			return row.ToArray();
		});

		var alignments = new bool[headersList.Count];
		for (var i = 0; i < alignments.Length; i++)
		{
			alignments[i] = i != 0 && i != (alignments.Length - 1);
		}

		alignments[0] = false;
		alignments[^1] = !showAllStats;

		consoleHelper.WriteTable([.. headersList], rows, alignments, title: "DAILY PROGRESS TABLE");
	}

	public void PrintTimeOfDayAnalysis(IEnumerable<KeybrSession> sessions)
	{
		ArgumentNullException.ThrowIfNull(sessions);

		var hourly = sessions.Select(s => new { s, TimeLocal = s.TimeStamp.ToLocalTime() })
			.GroupBy(x => x.TimeLocal.Hour)
			.OrderBy(g => g.Key)
			.Select(g => new
			{
				Hour = g.Key,
				Sess = g.Count(),
				Wpm = g.Average(x => x.s.Speed) / 5.0,
				Acc = g.Average(x => 1.0 - ((double)x.s.Errors / x.s.Length)) * 100
			});

		string[] headers = ["HOUR", "SAMPLES", "Avg WPM", "Avg Acc"];
		var rows = hourly.Select(g => new[] { $"{g.Hour:D2}:00", g.Sess.ToString(CultureInfo.InvariantCulture), g.Wpm.FormatWpm(), g.Acc.FormatAccuracy() });
		consoleHelper.WriteTable(headers, rows, [false, true, true, true], title: "TIME OF DAY ANALYSIS (Aggregated)");
	}

	public void PrintTopImprovedKeys(IEnumerable<KeyPerformance> data, bool yesterday = false)
	{
		ArgumentNullException.ThrowIfNull(data);

		var title = yesterday ? "TOP IMPROVED / REGRESSED KEYS (TODAY vs YESTERDAY)" : "TOP IMPROVED / REGRESSED KEYS (TODAY vs PREV 7 DAYS)";
		Func<KeyPerformance, double> selector = yesterday ? h => h.ImprovementYesterday : h => h.Improvement;

		var allSignificant = data.Where(h => h.D1H + h.D1M > Constants.MinSignificantHits).ToList();
		if (allSignificant.Count == 0)
		{
			return;
		}

		var improved = allSignificant.OrderBy(selector).Take(Constants.ImprovedRegressedCount);
		var regressed = allSignificant.OrderByDescending(selector).Take(Constants.ImprovedRegressedCount);
		var combined = improved.Union(regressed).OrderBy(selector).ToList();

		string[] headers = ["Key", "Today (H/M)", "Today Err", "Improvement", "Status"];
		var rows = combined.Select(h =>
		{
			var imp = selector(h);
			var color = imp <= 0 ? Ansi.Green : Ansi.Red;
			return new[] { h.Key, $"{h.D1H}/{h.D1M}", h.D1Err.FormatError(), $"{color}{(imp > 0 ? "+" : string.Empty)}{imp:F2}%{Ansi.Reset}", $"{color}{(imp <= 0 ? "IMPROVED" : "REGRESSED")}{Ansi.Reset}" };
		});
		consoleHelper.WriteTable(headers, rows, [false, true, true, true, false], title: title);
	}

	public void PrintAccuracyStreaks(IEnumerable<AccuracyStreak> streaks)
	{
		ArgumentNullException.ThrowIfNull(streaks);

		consoleHelper.WriteTitle("ACCURACY STREAKS");
		foreach (var streak in streaks)
		{
			var bg = streak.IsCurrent ? Ansi.BgGreen : (streak.IsMax ? Ansi.BgBlue : string.Empty);
			var label = streak.IsCurrent ? "CURRENT" : (streak.IsMax ? "MAX" : string.Empty);
			var typeLabel = !string.IsNullOrEmpty(bg) ? $"{bg}{Ansi.Black} {label,-7} {Ansi.Reset} " : "          ";
			consoleHelper.WriteLine($"{typeLabel}{Ansi.Bold}Threshold:{Ansi.Reset}{streak.Threshold.FormatAccuracy()} | {Ansi.Bold}Lessons:{Ansi.Reset} {streak.Lessons,4} | {Ansi.Bold}Chars:{ResetFormat(streak.Characters)} | {Ansi.Bold}Top:{Ansi.Reset} {streak.TopSpeed.FormatWpm()} | {Ansi.Bold}Start:{Ansi.Reset} {streak.StartDate:MM/dd/yy}");
		}

		consoleHelper.WriteLine();
	}

	private static string ResetFormat(long val) => $"{val,6:N0}";
}
