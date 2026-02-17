using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services.Reporting;

public interface IPerformanceReportingService
{
	void PrintKeyPerformanceTable(IEnumerable<KeyPerformance> data, string title);

	void PrintTargetKeys(IEnumerable<KeyPerformance> data, string title, Func<KeyPerformance, double> selector, int top = 5);

	void PrintStallAnalysisTable(IEnumerable<KeyPerformance> data, string title, int top = 15);

	void PrintCriticalTargetsTable(IEnumerable<KeyPerformance> data, string title);

	void PrintAllKeysPerformanceTable(IEnumerable<KeyPerformance> data, string title = "ALL KEYS PERFORMANCE");
}
