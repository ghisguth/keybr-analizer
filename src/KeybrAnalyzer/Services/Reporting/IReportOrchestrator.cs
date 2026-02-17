using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services.Reporting;

public interface IReportOrchestrator
{
	void GenerateFullReport(IReadOnlyList<KeybrSession> sessions, IReadOnlyList<KeyPerformance> histogram);
}
