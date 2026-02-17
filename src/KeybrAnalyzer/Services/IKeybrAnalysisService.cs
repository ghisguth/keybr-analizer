using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services;

public interface IKeybrAnalysisService
{
	IReadOnlyList<KeyPerformance> GetHistogramData(IEnumerable<KeybrSession> sessions, DateTime maxDateLocal);

	IReadOnlyList<AccuracyStreak> GetAccuracyStreaks(IEnumerable<KeybrSession> sessions, double[] thresholds);
}
