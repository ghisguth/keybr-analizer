using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services.Reporting;

public interface IFatigueReportingService
{
	void PrintFatigueAnalysis(IEnumerable<KeybrSession> sessions);

	void PrintFatigueIndicator(IEnumerable<KeybrSession> sessions);
}
