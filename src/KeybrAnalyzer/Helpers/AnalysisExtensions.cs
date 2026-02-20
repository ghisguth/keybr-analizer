using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Helpers;

public static class AnalysisExtensions
{
	public static double CalculateErr(this IEnumerable<HistogramEntryLocal> entries)
	{
		var list = entries.ToList();
		var h = list.Sum(e => e.Entry.HitCount);
		var m = list.Sum(e => e.Entry.MissCount);
		return (h + m > 0) ? (double)m / (h + m) * 100 : 0;
	}

	public static double CalculateWpm(this IEnumerable<HistogramEntryLocal> entries)
	{
		var list = entries.ToList();
		var h = list.Sum(e => e.Entry.HitCount);
		var t = list.Sum(e => e.Entry.TimeToType * e.Entry.HitCount);
		return (h > 0) ? 12000 / ((t / h) + 0.000001) : 0;
	}

	public static double CalculateCV(this IEnumerable<HistogramEntryLocal> entries, double avgLatency, int totalHits)
	{
		if (totalHits == 0 || avgLatency == 0)
		{
			return 0;
		}

		var sumSquaredDiffs = entries.Sum(e => e.Entry.HitCount * Math.Pow(e.Entry.TimeToType - avgLatency, 2));
		var stdDev = Math.Sqrt(sumSquaredDiffs / totalHits);
		return stdDev / avgLatency;
	}

	public static double CalculatePercentile(this IEnumerable<(double TimeToType, int HitCount)> sortedLatencies, double percentile)
	{
		var list = sortedLatencies.ToList();
		if (list.Count == 0)
		{
			return 0;
		}

		var totalHits = list.Sum(x => x.HitCount);
		if (totalHits == 0)
		{
			return 0;
		}

		var targetIndex = totalHits * percentile;
		var currentCount = 0;

		foreach (var (timeToType, hitCount) in list)
		{
			currentCount += hitCount;
			if (currentCount >= targetIndex)
			{
				return timeToType;
			}
		}

		return list[^1].TimeToType;
	}
}
