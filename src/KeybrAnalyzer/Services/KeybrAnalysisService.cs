using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;

namespace KeybrAnalyzer.Services;

public class KeybrAnalysisService : IKeybrAnalysisService
{
	public IReadOnlyList<KeyPerformance> GetHistogramData(IEnumerable<KeybrSession> sessions, DateTime maxDateLocal)
	{
		var latestDate = maxDateLocal.Date;
		var allEntries = sessions.SelectMany(s => s.Histogram.Select(h => new HistogramEntryLocal(h, s.TimeStamp.ToLocalTime()))).ToList();

		return [.. allEntries.GroupBy(e => e.Entry.CodePoint)
			.Select(g => CalculatePerformance(g.Key, [.. g], latestDate))];
	}

	public IReadOnlyList<AccuracyStreak> GetAccuracyStreaks(IEnumerable<KeybrSession> sessions, double[] thresholds)
	{
		ArgumentNullException.ThrowIfNull(sessions);
		ArgumentNullException.ThrowIfNull(thresholds);

		var sortedSessions = sessions.OrderBy(s => s.TimeStamp).ToList();
		var results = new List<AccuracyStreak>();

		foreach (var threshold in thresholds)
		{
			var (longest, active) = CalculateStreaksForThreshold(threshold, sortedSessions);
			if (longest != null && active != null && ReferenceEquals(longest, active))
			{
				results.Add(longest with { IsMax = true, IsCurrent = true });
			}
			else
			{
				if (longest != null)
				{
					results.Add(longest with { IsMax = true });
				}

				if (active != null)
				{
					results.Add(active with { IsCurrent = true });
				}
			}
		}

		return results;
	}

	private static (AccuracyStreak? Longest, AccuracyStreak? Active) CalculateStreaksForThreshold(double threshold, List<KeybrSession> sortedSessions)
	{
		AccuracyStreak? longestStreak = null;
		var currentStreakSessions = new List<KeybrSession>();
		AccuracyStreak? activeStreak = null;

		foreach (var session in sortedSessions)
		{
			var accuracy = (1.0 - ((double)session.Errors / session.Length)) * 100;
			if (accuracy >= threshold)
			{
				currentStreakSessions.Add(session);
			}
			else
			{
				if (currentStreakSessions.Count > 0)
				{
					var streak = CreateStreak(threshold, currentStreakSessions);
					if (longestStreak == null || streak.Lessons > longestStreak.Lessons)
					{
						longestStreak = streak;
					}

					currentStreakSessions.Clear();
				}
			}
		}

		if (currentStreakSessions.Count > 0)
		{
			activeStreak = CreateStreak(threshold, currentStreakSessions);
			if (longestStreak == null || activeStreak.Lessons > longestStreak.Lessons)
			{
				longestStreak = activeStreak;
			}
		}

		return (longestStreak, activeStreak);
	}

	private static KeyPerformance CalculatePerformance(int codePoint, List<HistogramEntryLocal> allEntries, DateTime latestDate)
	{
		var allH = allEntries.Sum(e => e.Entry.HitCount);
		var allM = allEntries.Sum(e => e.Entry.MissCount);
		var totalTime = allEntries.Sum(e => e.Entry.TimeToType * e.Entry.HitCount);
		var latency = allH > 0 ? totalTime / allH : 0;
		var allErr = allEntries.CalculateErr();

		var l7 = allEntries.Where(e => e.TimeLocal.Date > latestDate.AddDays(-7)).ToList();
		var l3 = allEntries.Where(e => e.TimeLocal.Date > latestDate.AddDays(-3)).ToList();
		var l1 = allEntries.Where(e => e.TimeLocal.Date > latestDate.AddDays(-1)).ToList();

		var l7H = l7.Sum(e => e.Entry.HitCount);
		var l7Latency = l7H > 0 ? l7.Sum(e => e.Entry.TimeToType * e.Entry.HitCount) / l7H : 0;
		var l7Err = l7.CalculateErr();

		var cv = allEntries.CalculateCV(latency, allH);
		var l7CV = l7.CalculateCV(l7Latency, l7H);

		var latencies = allEntries.Select(e => (e.Entry.TimeToType, e.Entry.HitCount)).OrderBy(x => x.TimeToType);
		var p50 = latencies.CalculatePercentile(0.5);
		var p95 = latencies.CalculatePercentile(0.95);

		var dailyWpms = Enumerable.Range(0, 7)
			.Select(i => latestDate.AddDays(-6 + i))
			.Select(date => allEntries.Where(e => e.TimeLocal.Date == date).CalculateWpm())
			.ToList();

		var d1 = allEntries.Where(e => e.TimeLocal.Date == latestDate).ToList();
		var d2 = allEntries.Where(e => e.TimeLocal.Date == latestDate.AddDays(-1)).ToList();
		var prev7 = allEntries.Where(e => e.TimeLocal.Date < latestDate && e.TimeLocal.Date >= latestDate.AddDays(-7)).ToList();

		var d1Err = d1.CalculateErr();
		var d2Err = d2.CalculateErr();
		var prev7Err = prev7.CalculateErr();

		return new()
		{
			CodePoint = codePoint,
			Key = codePoint == 32 ? "SPC" : char.ConvertFromUtf32(codePoint),
			AllH = allH,
			AllM = allM,
			Latency = latency,
			AllErr = allErr,
			AllWpm = allH > 0 ? Math.Min(100, 12000 / ((totalTime / allH) + 0.000001)) : 0,

			CV = cv,
			Mastery = KeyMetricCalculator.CalculateMastery(latency, allErr, cv, KeyMetricCalculator.GetCSharpWeight(codePoint)),
			P50 = p50,
			P95 = p95,
			StallRatio = p50 > 0 ? p95 / p50 : 1.0,

			L7H = l7H,
			L7M = l7.Sum(e => e.Entry.MissCount),
			L7Latency = l7Latency,
			L7Err = l7Err,
			L7Wpm = l7.CalculateWpm(),
			L7CV = l7CV,

			L3H = l3.Sum(e => e.Entry.HitCount),
			L3M = l3.Sum(e => e.Entry.MissCount),
			L3Err = l3.CalculateErr(),
			L3Wpm = l3.CalculateWpm(),

			L1H = l1.Sum(e => e.Entry.HitCount),
			L1M = l1.Sum(e => e.Entry.MissCount),
			L1Err = l1.CalculateErr(),
			L1Wpm = l1.CalculateWpm(),

			D1H = d1.Sum(e => e.Entry.HitCount),
			D1M = d1.Sum(e => e.Entry.MissCount),
			D1Err = d1Err,
			D1Wpm = d1.CalculateWpm(),

			D2H = d2.Sum(e => e.Entry.HitCount),
			D2M = d2.Sum(e => e.Entry.MissCount),
			D2Err = d2Err,
			D2Wpm = d2.CalculateWpm(),

			D3H = allEntries.Where(e => e.TimeLocal.Date == latestDate.AddDays(-2)).Sum(e => e.Entry.HitCount),
			D3M = allEntries.Where(e => e.TimeLocal.Date == latestDate.AddDays(-2)).Sum(e => e.Entry.MissCount),
			D3Err = allEntries.Where(e => e.TimeLocal.Date == latestDate.AddDays(-2)).CalculateErr(),
			D3Wpm = allEntries.Where(e => e.TimeLocal.Date == latestDate.AddDays(-2)).CalculateWpm(),

			DailyWpm = dailyWpms,
			Improvement = (d1.Count > 0 && prev7.Count > 0) ? d1Err - prev7Err : 0,
			ImprovementYesterday = (d1.Count > 0 && d2.Count > 0) ? d1Err - d2Err : 0,
			L7Impact = l7H * l7Err / 100.0
		};
	}

	private static AccuracyStreak CreateStreak(double threshold, List<KeybrSession> streakSessions)
	{
		return new AccuracyStreak(
			threshold,
			streakSessions.Count,
			streakSessions.Sum(s => s.Length),
			streakSessions.Max(s => s.Speed) / 5.0,
			streakSessions.Average(s => s.Speed) / 5.0,
			streakSessions.First().TimeStamp.ToLocalTime());
	}
}
