using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;
using KeybrAnalyzer.Options;

using Microsoft.Extensions.Options;

namespace KeybrAnalyzer.Services.Reporting;

public class ReportOrchestrator(
	IOptions<KeybrAnalyzerOptions> options,
	IKeybrAnalysisService analysisService,
	ISummaryReportingService summaryReporting,
	IPerformanceReportingService performanceReporting,
	IProgressReportingService progressReporting,
	IFatigueReportingService fatigueReporting,
	IKeyboardLayoutReportingService layoutReporting)
	: IReportOrchestrator
{
	public void GenerateFullReport(IReadOnlyList<KeybrSession> sessions, IReadOnlyList<KeyPerformance> histogram)
	{
		var maxDate = sessions.Max(s => s.TimeStamp.ToLocalTime());

		PrintHeaders(sessions, maxDate);
		PrintMasteryTables(sessions, maxDate, histogram);
		PrintTargetTables(histogram);
		PrintGeneralAnalysis(sessions, histogram);
	}

	private void PrintHeaders(IReadOnlyList<KeybrSession> sessions, DateTime maxDate)
	{
		var todaySess = sessions.Where(s => s.TimeStamp.ToLocalTime().Date == maxDate.Date).ToList();
		var l7Sess = sessions.Where(s => s.TimeStamp.ToLocalTime() >= maxDate.AddDays(-7)).ToList();
		var l7CodeSess = l7Sess.Where(s => s.TextType is "code" or "natural").ToList();
		var l7ProseSess = l7Sess.Where(s => s.TextType == "generated").ToList();

		summaryReporting.PrintHeader();

		if (l7CodeSess.Count > 0 && l7ProseSess.Count > 0)
		{
			var codeWpm = l7CodeSess.Average(s => s.Speed) / 5.0;
			var proseWpm = l7ProseSess.Average(s => s.Speed) / 5.0;
			var totalSamples = l7CodeSess.Count + l7ProseSess.Count;

			var delta = codeWpm - proseWpm;
			var color = delta >= 0 ? Ansi.Green : Ansi.Red;
			summaryReporting.PrintHeaderMetric($"DOMAIN GAP (CODE vs PROSE) [C={l7CodeSess.Count}, P={l7ProseSess.Count}]", $"{color}{delta:F1} WPM{Ansi.Reset}");
		}

		summaryReporting.PrintSessionSummary(todaySess, "STATISTICS FOR TODAY");
		summaryReporting.PrintGeneralStats(l7Sess, "LAST 7 DAYS (TRAILING AVERAGE)");
		fatigueReporting.PrintFatigueIndicator(sessions);
		summaryReporting.PrintTrainingState();

		if (options.Value.ShowAllStats)
		{
			layoutReporting.PrintKeyboardLayout(KeyboardMode.Finger);
			layoutReporting.PrintKeyboardLayout(KeyboardMode.KeyType);
			layoutReporting.PrintKeyboardLayout(KeyboardMode.Status);
		}
	}

	private void PrintMasteryTables(IReadOnlyList<KeybrSession> sessions, DateTime maxDate, IReadOnlyList<KeyPerformance> histogram)
	{
		var focusKeysList = options.Value.FocusKeys.SelectMany(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToList();
		var focusKeysData = histogram.Where(h => focusKeysList.Contains(h.Key)).OrderBy(h => h.Mastery).ToList();
		performanceReporting.PrintKeyPerformanceTable(focusKeysData, "FOCUS KEYS MASTERY (SORTED BY MASTERY)");

		var codeSess = sessions.Where(s => s.TextType == "code").ToList();
		var genSess = sessions.Where(s => s.TextType == "generated").ToList();
		var numbersSess = sessions.Where(s => s.TextType == "numbers").ToList();
		var naturalSess = sessions.Where(s => s.TextType == "natural").ToList();

		var codeAllSess = sessions.Where(s => s.TextType is "code" or "natural").ToList();

		PrintFilteredMasteryTable(codeAllSess, maxDate, "C# CODE MASTERY (TOP 20)");

		if (options.Value.ShowAllStats)
		{
			PrintFilteredMasteryTable(codeSess, maxDate, "C# CODE ONLY MASTERY (TOP 20)");
			PrintFilteredMasteryTable(genSess, maxDate, "GUIDED MASTERY (TOP 20)");
			PrintFilteredMasteryTable(numbersSess, maxDate, "NUMBERS MASTERY (TOP 20)");
			PrintFilteredMasteryTable(naturalSess, maxDate, "CUSTOM TEXT MASTERY (TOP 20)");
		}
	}

	private void PrintFilteredMasteryTable(List<KeybrSession> filteredSessions, DateTime maxDate, string title)
	{
		if (filteredSessions.Count == 0)
		{
			return;
		}

		var hist = analysisService.GetHistogramData(filteredSessions, maxDate);
		var data = hist.Where(h => h.L7H >= Constants.MinL7Hits).OrderBy(h => h.Mastery).Take(Constants.TopKeysCount);
		performanceReporting.PrintKeyPerformanceTable([.. data], title);
	}

	private void PrintTargetTables(IReadOnlyList<KeyPerformance> histogram)
	{
		var l7Data = histogram.Where(h => h.L7H >= Constants.MinL7Hits).ToList();
		performanceReporting.PrintCriticalTargetsTable(l7Data, "CRITICAL TARGETS (N >= 10)");

		if (options.Value.ShowAllStats)
		{
			performanceReporting.PrintTargetKeys(l7Data, "TARGET KEYS: HIGHEST HESITATION", h => h.L7CV, top: Constants.TargetKeysCount);
			performanceReporting.PrintTargetKeys([.. l7Data.Where(h => h.L7Err > Constants.ErrorFloorThreshold)], "TARGET KEYS: HIGHEST ERROR IMPACT", h => h.L7Impact, top: Constants.TargetKeysCount);
			performanceReporting.PrintStallAnalysisTable(l7Data, "TARGET KEYS: COGNITIVE STALLS", top: Constants.TargetKeysCount);
			performanceReporting.PrintTargetKeys(l7Data, "TARGET KEYS: LOWEST SPEED", h => h.L7Latency, top: Constants.TargetKeysCount);
		}
	}

	private void PrintGeneralAnalysis(IReadOnlyList<KeybrSession> sessions, IReadOnlyList<KeyPerformance> histogram)
	{
		progressReporting.PrintDailyProgressTable(sessions);
		progressReporting.PrintTimeOfDayAnalysis(sessions);
		fatigueReporting.PrintFatigueAnalysis(sessions);
		progressReporting.PrintAccuracyStreaks(analysisService.GetAccuracyStreaks(sessions, [100.0, 97.0, 95.0]));
		summaryReporting.PrintMilestones(sessions);
		progressReporting.PrintTopImprovedKeys(histogram);
		progressReporting.PrintTopImprovedKeys(histogram, yesterday: true);

		if (options.Value.ShowAllStats)
		{
			performanceReporting.PrintAllKeysPerformanceTable(histogram.OrderBy(h => h.CodePoint));
		}
	}
}
