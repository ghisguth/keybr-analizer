using System.Collections.ObjectModel;

using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services.Reporting;

public class SummaryReportingService(IConsoleHelper consoleHelper) : ISummaryReportingService
{
	public void PrintHeader()
	{
		consoleHelper.WriteLine($"{Ansi.Bold}{Ansi.Blue}");
		consoleHelper.WriteLine(@" _  _ ____ _   _ ___  ____    ____ _  _ ____ _    _   _ ___ ____ ____ ");
		consoleHelper.WriteLine(@" |_/  |___  \_/  |__] |__/    |__| |\ | |__| |     \_/   /  |___ |__/ ");
		consoleHelper.WriteLine(@" | \_ |___   |   |__] |  \    |  | | \| |  | |___   |   /__ |___ |  \ ");
		consoleHelper.WriteLine($"{Ansi.Reset}");
	}

	public void PrintSessionSummary(IEnumerable<KeybrSession> sessions, string title)
	{
		ArgumentNullException.ThrowIfNull(sessions);

		var list = sessions.ToList();
		if (list.Count == 0)
		{
			return;
		}

		consoleHelper.WriteTitle(title);
		consoleHelper.WriteLine($"{Ansi.Bold}Time:{Ansi.Reset} {(list.Sum(s => s.Time) / 1000.0).ToTimeStr()}");
		consoleHelper.WriteLine($"{Ansi.Bold}Lessons:{Ansi.Reset} {list.Count:N0}");
		consoleHelper.WriteLine($"{Ansi.Bold}Top speed:{Ansi.Reset} {(list.Max(s => s.Speed) / 5.0).FormatWpm()}");
		consoleHelper.WriteLine($"{Ansi.Bold}Average speed:{Ansi.Reset} {(list.Average(s => s.Speed) / 5.0).FormatWpm()}");
		consoleHelper.WriteLine($"{Ansi.Bold}Top accuracy:{Ansi.Reset} {(list.Max(s => 1.0 - ((double)s.Errors / s.Length)) * 100).FormatAccuracy()}");
		consoleHelper.WriteLine($"{Ansi.Bold}Average accuracy:{Ansi.Reset} {(list.Average(s => 1.0 - ((double)s.Errors / s.Length)) * 100).FormatAccuracy()}");
		consoleHelper.WriteLine();
	}

	public void PrintGeneralStats(IEnumerable<KeybrSession> sessions, string label)
	{
		ArgumentNullException.ThrowIfNull(sessions);

		var list = sessions.ToList();
		if (list.Count == 0)
		{
			return;
		}

		string[] headers = ["Sessions", "Time", "Avg Speed", "Accuracy"];
		string[][] rows =
		[
			[
				$"{Ansi.Bold}{list.Count}{Ansi.Reset}",
				(list.Sum(s => s.Time) / 1000.0).ToTimeStr(),
				(list.Average(s => s.Speed) / 5.0).FormatWpm(),
				(list.Average(s => 1.0 - ((double)s.Errors / s.Length)) * 100).FormatAccuracy()
			]
		];
		consoleHelper.WriteTable(headers, rows, [false, false, true, true], title: label);
	}

	public void PrintMilestones(IEnumerable<KeybrSession> sessions)
	{
		ArgumentNullException.ThrowIfNull(sessions);

		var list = sessions.ToList();
		if (list.Count == 0)
		{
			return;
		}

		var best = list.OrderByDescending(s => s.Speed).First();
		string[] headers = ["METRIC", "VALUE", "DESCRIPTION"];
		string[][] rows =
		[
			["Total Typed", $"{list.Sum(s => s.Length):N0}", "Total characters typed"],
			["Total Mistakes", $"{list.Sum(s => s.Errors):N0}", "Total errors made"],
			["Total Training", $"{list.Sum(s => s.Time) / 1000.0 / 3600.0:F1} hours", "Total time spent"],
			["Personal Best", (best.Speed / 5.0).FormatWpm(), $"Achieved on {best.TimeStamp.ToLocalTime():yyyy-MM-dd HH:mm}"],
			["Avg Session", $"{list.Average(s => s.Length):F0} chars", "Average length of one lesson"]
		];
		consoleHelper.WriteTable(headers, rows, [false, true, false], title: "MILESTONES & CAREER RECORDS");
	}

	public void PrintHeaderMetric(string label, string value)
	{
		consoleHelper.WriteLine($"{Ansi.Bold}{label}:{Ansi.Reset} {value}");
		consoleHelper.WriteLine();
	}

	public void PrintTrainingState(IEnumerable<string> opened, IEnumerable<string> focus, IDictionary<string, Collection<string>> locked)
	{
		ArgumentNullException.ThrowIfNull(opened);
		ArgumentNullException.ThrowIfNull(focus);
		ArgumentNullException.ThrowIfNull(locked);

		consoleHelper.WriteTitle("TRAINING PROGRESS SUMMARY");
		consoleHelper.WriteLine($"{Ansi.Bold}Unlocked keys{Ansi.Reset} {Ansi.Green}✓{Ansi.Reset}:");
		foreach (var s in opened)
		{
			consoleHelper.WriteLine($"  {Ansi.Green}{s}{Ansi.Reset}");
		}

		consoleHelper.WriteLine();
		consoleHelper.WriteLine($"{Ansi.Bold}Focus keys (Structural){Ansi.Reset} {Ansi.Yellow}→{Ansi.Reset}:");
		foreach (var s in focus)
		{
			consoleHelper.WriteLine($"  {Ansi.Yellow}{s}{Ansi.Reset}");
		}

		foreach (var (tier, keys) in locked)
		{
			consoleHelper.WriteLine();
			consoleHelper.WriteLine($"{Ansi.Bold}Locked: {tier}{Ansi.Reset} {Ansi.Red}🔒{Ansi.Reset}:");
			foreach (var s in keys)
			{
				consoleHelper.WriteLine($"  {Ansi.Red}{s}{Ansi.Reset}");
			}
		}

		consoleHelper.WriteLine();
	}
}
