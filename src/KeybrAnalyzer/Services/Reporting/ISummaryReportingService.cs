using System.Collections.ObjectModel;

using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services.Reporting;

public interface ISummaryReportingService
{
	void PrintHeader();

	void PrintSessionSummary(IEnumerable<KeybrSession> sessions, string title);

	void PrintGeneralStats(IEnumerable<KeybrSession> sessions, string label);

	void PrintMilestones(IEnumerable<KeybrSession> sessions);

	void PrintHeaderMetric(string label, string value);

	void PrintTrainingState(IEnumerable<string> opened, IEnumerable<string> focus, IDictionary<string, Collection<string>> locked);
}
