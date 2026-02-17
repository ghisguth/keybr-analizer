using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services.Reporting;

public interface IProgressReportingService
{
	void PrintDailyProgressTable(IEnumerable<KeybrSession> sessions);

	void PrintTimeOfDayAnalysis(IEnumerable<KeybrSession> sessions);

	void PrintTopImprovedKeys(IEnumerable<KeyPerformance> data, bool yesterday = false);

	void PrintAccuracyStreaks(IEnumerable<AccuracyStreak> streaks);
}
