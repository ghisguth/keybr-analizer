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
		var codeSess = sessions.Where(s => s.TextType is "code" or "natural").ToList();
		var normalSess = sessions.Where(s => s.TextType == "generated").ToList();

		summaryReporting.PrintHeader();

		if (codeSess.Count > 0 && normalSess.Count > 0)
		{
			var delta = (normalSess.Average(s => s.Speed) - codeSess.Average(s => s.Speed)) / 5.0;
			var color = delta <= 0 ? Ansi.Green : Ansi.Red;
			summaryReporting.PrintHeaderMetric("DOMAIN GAP (CODE vs NATURAL)", $"{color}{delta:F1} WPM{Ansi.Reset}");
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

		if (codeAllSess.Count > 0)
		{
			var codeHist = analysisService.GetHistogramData(codeAllSess, maxDate);
			var data = codeHist.Where(h => h.L7H >= Constants.MinL7Hits).OrderBy(h => h.Mastery).Take(Constants.TopKeysCount);
			performanceReporting.PrintKeyPerformanceTable([.. data], "C# CODE MASTERY (TOP 20)");
		}

		if (options.Value.ShowAllStats)
		{
			if (codeSess.Count > 0)
			{
				var codeHist = analysisService.GetHistogramData(codeSess, maxDate);
				var data = codeHist.Where(h => h.L7H >= Constants.MinL7Hits).OrderBy(h => h.Mastery).Take(Constants.TopKeysCount);
				performanceReporting.PrintKeyPerformanceTable([.. data], "C# CODE ONLY MASTERY (TOP 20)");
			}

			if (genSess.Count > 0)
			{
				var genHist = analysisService.GetHistogramData(genSess, maxDate);
				var data = genHist.Where(h => h.L7H >= Constants.MinL7Hits).OrderBy(h => h.Mastery).Take(Constants.TopKeysCount);
				performanceReporting.PrintKeyPerformanceTable([.. data], "GUIDED MASTERY (TOP 20)");
			}

			if (numbersSess.Count > 0)
			{
				var numbersHist = analysisService.GetHistogramData(numbersSess, maxDate);
				var data = numbersHist.Where(h => h.L7H >= Constants.MinL7Hits).OrderBy(h => h.Mastery).Take(Constants.TopKeysCount);
				performanceReporting.PrintKeyPerformanceTable([.. data], "NUMBERS MASTERY (TOP 20)");
			}

			if (naturalSess.Count > 0)
			{
				var naturalHist = analysisService.GetHistogramData(naturalSess, maxDate);
				var data = naturalHist.Where(h => h.L7H >= Constants.MinL7Hits).OrderBy(h => h.Mastery).Take(Constants.TopKeysCount);
				performanceReporting.PrintKeyPerformanceTable([.. data], "CUSTOM TEXT MASTERY (TOP 20)");
			}
		}
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
