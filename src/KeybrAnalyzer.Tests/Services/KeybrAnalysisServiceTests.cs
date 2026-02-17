using KeybrAnalyzer.Models;
using KeybrAnalyzer.Services;

using Shouldly;

namespace KeybrAnalyzer.Tests.Services;

public class KeybrAnalysisServiceTests
{
	private readonly KeybrAnalysisService _sut = new();

	[Fact]
	public void GetAccuracyStreaksShouldReturnLongestStreaks()
	{
		// Arrange
		var sessions = new List<KeybrSession>
		{
			CreateSession(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Local), 100, 0, 100), // 100%
			CreateSession(new DateTime(2026, 1, 1, 10, 5, 0, DateTimeKind.Local), 100, 0, 100), // 100%
			CreateSession(new DateTime(2026, 1, 1, 10, 10, 0, DateTimeKind.Local), 100, 5, 100), // 95%
			CreateSession(new DateTime(2026, 1, 1, 10, 15, 0, DateTimeKind.Local), 100, 0, 100), // 100%
			CreateSession(new DateTime(2026, 1, 1, 10, 20, 0, DateTimeKind.Local), 100, 0, 100), // 100%
			CreateSession(new DateTime(2026, 1, 1, 10, 25, 0, DateTimeKind.Local), 100, 0, 100), // 100%
		};

		// Act
		var streaks = _sut.GetAccuracyStreaks(sessions, [100.0, 97.0, 95.0]);

		// Assert
		// Deduplicated: each threshold returns one entry that is both Max and Current
		streaks.Count.ShouldBe(3);

		// 100% threshold: longest is last 3 sessions, which is also current
		var s100 = streaks.First(s => s.Threshold == 100.0);
		s100.IsMax.ShouldBeTrue();
		s100.IsCurrent.ShouldBeTrue();
		s100.Lessons.ShouldBe(3);
		s100.StartDate.ShouldBe(new DateTime(2026, 1, 1, 10, 15, 0, DateTimeKind.Local));

		// 95% threshold: all 6 sessions are both max and current
		var s95 = streaks.First(s => s.Threshold == 95.0);
		s95.IsMax.ShouldBeTrue();
		s95.IsCurrent.ShouldBeTrue();
		s95.Lessons.ShouldBe(6);
	}

	[Fact]
	public void GetHistogramDataCalculatesL7Metrics()
	{
		// Arrange
		var maxDate = new DateTime(2026, 2, 15, 12, 0, 0, DateTimeKind.Local);
		var sessions = new List<KeybrSession>
		{
			// Within L7
			new(maxDate.AddDays(-1).ToUniversalTime(), 100, 0, 10, 60000, "code", [new HistogramEntry(65, 1, 0, 200)]), // 'A', Latency 200ms
			new(maxDate.AddDays(-2).ToUniversalTime(), 100, 0, 10, 60000, "code", [new HistogramEntry(65, 1, 0, 400)]), // 'A', Latency 400ms

			// Outside L7
			new(maxDate.AddDays(-10).ToUniversalTime(), 100, 0, 10, 60000, "code", [new HistogramEntry(65, 1, 0, 1000)]), // 'A', Latency 1000ms
		};

		// Act
		var results = _sut.GetHistogramData(sessions, maxDate);

		// Assert
		var aPerf = results.ShouldHaveSingleItem();
		aPerf.Key.ShouldBe("A");

		// L7: (200 + 400) / 2 = 300ms
		aPerf.L7H.ShouldBe(2);
		aPerf.L7Latency.ShouldBe(300);

		// L7 CV:
		// mean = 300
		// variance = ( (200-300)^2 + (400-300)^2 ) / 2 = (10000 + 10000) / 2 = 10000
		// stddev = sqrt(10000) = 100
		// CV = 100 / 300 = 0.333...
		aPerf.L7CV.ShouldBe(0.333333, tolerance: 0.001);

		// All-time metrics for comparison
		aPerf.AllH.ShouldBe(3);
		aPerf.Latency.ShouldBe((200 + 400 + 1000) / 3.0, tolerance: 0.001);

		// P50 / P95 Stall Ratio
		// Sorted latencies: [(200, 1), (400, 1), (1000, 1)]
		// Total hits = 3
		// P50 (index 3*0.5 = 1.5): cumulative hit 2 is at 400ms. P50 = 400.
		// P95 (index 3*0.95 = 2.85): cumulative hit 3 is at 1000ms. P95 = 1000.
		aPerf.P50.ShouldBe(400);
		aPerf.P95.ShouldBe(1000);
		aPerf.StallRatio.ShouldBe(2.5);
	}

	private static KeybrSession CreateSession(DateTime timeStamp, int length, int errors, double speed)
	{
		return new KeybrSession(timeStamp, speed, errors, length, 60000, "generated", []);
	}
}
