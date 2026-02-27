using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services.Reporting;

public class PerformanceReportingService(IConsoleHelper consoleHelper) : IPerformanceReportingService
{
	public void PrintKeyPerformanceTable(IEnumerable<KeyPerformance> data, string title)
	{
		ArgumentNullException.ThrowIfNull(data);

		string[] headers = ["Key", "N (L7)", "N (L1)", "Latency (L7)", "Err (L7)", "CV (L7)", "Mastery", "L3 Err", "L1 Err", "Trend (WPM)"];
		var rows = data.Select(h =>
		{
			var nonZeroDaily = h.DailyWpm.Where(v => v > 0).ToList();
			var min = nonZeroDaily.Count != 0 ? nonZeroDaily.Min() * 0.9 : 0;
			var max = nonZeroDaily.Count != 0 ? nonZeroDaily.Max() * 1.1 : 100;
			return new[]
			{
				h.Key,
				$"{h.L7H}",
				$"{h.L1H}",
				$"{h.L7Latency:F0}ms",
				h.L7Err.FormatError(),
				$"{h.L7CV:F3}",
				$"{h.Mastery:F1}",
				h.L3Err.FormatError(),
				h.L1Err.FormatError(),
				consoleHelper.GetSparkline(h.DailyWpm, maxWidth: Constants.SparklineWidth, min: min, max: max)
			};
		});
		consoleHelper.WriteTable(headers, rows, [false, true, true, true, true, true, true, true, true, false], title: title);

		consoleHelper.WriteLine($"{Ansi.Bold}Legend:{Ansi.Reset}");
		consoleHelper.WriteLine($"  {Ansi.Bold}Latency{Ansi.Reset} : Average time to type the key (lower is better)");
		consoleHelper.WriteLine($"  {Ansi.Bold}CV{Ansi.Reset}      : Coefficient of Variation (lower is more consistent)");
		consoleHelper.WriteLine($"  {Ansi.Bold}Mastery{Ansi.Reset} : Composite score (Latency, Err%, CV, and C# weight)");
		consoleHelper.WriteLine($"  {Ansi.Bold}Trend{Ansi.Reset}   : Last 7 days WPM (scaled to key's min/max)");
		consoleHelper.WriteLine();
	}

	public void PrintTargetKeys(IEnumerable<KeyPerformance> data, string title, Func<KeyPerformance, double> selector, int top = 5)
	{
		ArgumentNullException.ThrowIfNull(data);

		var ordered = data.OrderByDescending(selector);
		string[] headers = ["Key", "N (L7)", "N (L1)", "Latency", "Error", "CV", "Impact"];
		var rows = ordered.Take(top).Select(h => new[]
		{
			h.Key,
			$"{h.L7H}",
			$"{h.L1H}",
			$"{h.L7Latency:F0}ms",
			h.L7Err.FormatError(),
			$"{h.L7CV:F3}",
			$"{h.L7Impact:F2}"
		});
		consoleHelper.WriteTable(headers, rows, [false, true, true, true, true, true, true], title: title);
	}

	public void PrintStallAnalysisTable(IEnumerable<KeyPerformance> data, string title, int top = 15)
	{
		ArgumentNullException.ThrowIfNull(data);

		var topKeys = data.OrderByDescending(h => h.StallRatio).Take(top).ToList();
		string[] headers = ["Key", "P50 (Med)", "P95", "Stall Ratio", "Status"];
		var rows = topKeys.Select(h =>
		{
			var status = h.StallRatio > 3.0 ? $"{Ansi.Red}COGNITIVE{Ansi.Reset}" : (h.StallRatio > 2.0 ? $"{Ansi.Yellow}HEAVY{Ansi.Reset}" : "MECHANICAL");
			return new[]
			{
				h.Key,
				$"{h.P50:F0}ms",
				$"{h.P95:F0}ms",
				$"{h.StallRatio:F2}",
				status
			};
		});
		consoleHelper.WriteTable(headers, rows, [false, true, true, true, false], title: title);
	}

	public void PrintCriticalTargetsTable(IEnumerable<KeyPerformance> data, string title)
	{
		ArgumentNullException.ThrowIfNull(data);

		var list = data.ToList();
		if (list.Count == 0)
		{
			return;
		}

		var byHesitation = list.OrderByDescending(h => h.L7CV).Take(5).ToList();
		var byImpact = list.Where(h => h.L7Err > Constants.ErrorFloorThreshold).OrderByDescending(h => h.L7Impact).Take(5).ToList();
		var byStall = list.OrderByDescending(h => h.StallRatio).Take(5).ToList();
		var byLatency = list.OrderByDescending(h => h.L7Latency).Take(5).ToList();

		var criticalKeys = byHesitation.Union(byImpact).Union(byStall).Union(byLatency)
			.OrderByDescending(h => h.L7Impact + (h.StallRatio * 10)).ToList();

		string[] headers = ["Key", "N (L7)", "N (L1)", "Latency", "Err", "P95/P50", "Flags"];
		var rows = criticalKeys.Select(h =>
		{
			var flags = new List<string>();
			if (byHesitation.Contains(h))
			{
				flags.Add($"⏳ {Ansi.Cyan}HESIT {Ansi.Reset}");
			}

			if (byImpact.Contains(h))
			{
				flags.Add($"💥 {Ansi.Red}IMPACT{Ansi.Reset}");
			}

			if (byStall.Contains(h))
			{
				flags.Add($"🧊 {Ansi.Yellow}STALL {Ansi.Reset}");
			}

			if (byLatency.Contains(h))
			{
				flags.Add($"🐌 {Ansi.Blue}SLOW  {Ansi.Reset}");
			}

			return new[]
			{
				h.Key,
				$"{h.L7H}",
				$"{h.L1H}",
				$"{h.L7Latency:F0}ms",
				h.L7Err.FormatError(),
				$"{h.StallRatio:F2}",
				string.Join(" ", flags)
			};
		});
		consoleHelper.WriteTable(headers, rows, [false, true, true, true, true, true, false], title: title);

		consoleHelper.WriteLine($"{Ansi.Bold}Legend:{Ansi.Reset}");
		consoleHelper.WriteLine($"  ⏳ {Ansi.Cyan}HESIT {Ansi.Reset} : High Variance (Erratic speed)");
		consoleHelper.WriteLine($"  💥 {Ansi.Red}IMPACT{Ansi.Reset} : High Error Volume (N * Err Rate)");
		consoleHelper.WriteLine($"  🧊 {Ansi.Yellow}STALL {Ansi.Reset} : Cognitive Freeze (P95 vs P50)");
		consoleHelper.WriteLine($"  🐌 {Ansi.Blue}SLOW  {Ansi.Reset} : Low Mechanical Speed (Latency)");
		consoleHelper.WriteLine();
	}

	public void PrintAllKeysPerformanceTable(IEnumerable<KeyPerformance> data, string title = "ALL KEYS PERFORMANCE")
	{
		ArgumentNullException.ThrowIfNull(data);

		string[] headers = ["Key", "Mastery", "CV", "All Err", "All WPM", "L7 Err", "L7 WPM", "L1 Err", "L1 WPM"];
		var rows = data.Select(h => new[]
		{
			h.Key,
			$"{h.Mastery:F1}",
			$"{h.CV:F3}",
			h.AllErr.FormatError(),
			h.AllWpm.FormatWpm(),
			h.L7Err.FormatError(),
			h.L7Wpm.FormatWpm(),
			h.L1Err.FormatError(),
			h.L1Wpm.FormatWpm()
		});
		consoleHelper.WriteTable(headers, rows, [false, true, true, true, true, true, true, true, true], title: title);
	}
}
