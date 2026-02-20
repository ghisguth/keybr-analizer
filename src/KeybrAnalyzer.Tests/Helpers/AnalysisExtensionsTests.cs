using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;

using Shouldly;

namespace KeybrAnalyzer.Tests.Helpers;

public sealed class AnalysisExtensionsTests
{
	[Fact]
	public void CalculateErrShouldHandleEmpty()
	{
		var entries = Enumerable.Empty<HistogramEntryLocal>();
		entries.CalculateErr().ShouldBe(0);
	}

	[Fact]
	public void CalculateErrShouldCalculateCorrectPercentage()
	{
		var entries = new List<HistogramEntryLocal>
		{
			new(new HistogramEntry(65, 80, 20, 200), DateTime.Now)
		};
		entries.CalculateErr().ShouldBe(20.0);
	}

	[Fact]
	public void CalculateWpmShouldCalculateCorrectWpm()
	{
		// 12000 / (Latency + 0.000001)
		// Latency = 200ms
		// WPM = 12000 / 200 = 60
		var entries = new List<HistogramEntryLocal>
		{
			new(new HistogramEntry(65, 10, 0, 200), DateTime.Now)
		};
		entries.CalculateWpm().ShouldBe(60.0, 0.001);
	}

	[Fact]
	public void CalculateWpmShouldNotCapAt100()
	{
		var entries = new List<HistogramEntryLocal>
		{
			new(new HistogramEntry(65, 10, 0, 10), DateTime.Now)
		};

		// Expected 12000 / 10 = 1200 WPM
		entries.CalculateWpm().ShouldBe(1200.0, 0.1);
	}

	[Fact]
	public void CalculateCVShouldReturnZeroOnNoHits()
	{
		var entries = new List<HistogramEntryLocal>
		{
			new(new HistogramEntry(65, 0, 0, 200), DateTime.Now)
		};
		entries.CalculateCV(200, 0).ShouldBe(0);
	}

	[Fact]
	public void CalculateCVShouldCalculateCorrectCoefficientOfVariation()
	{
		var entries = new List<HistogramEntryLocal>
		{
			new(new HistogramEntry(65, 1, 0, 100), DateTime.Now),
			new(new HistogramEntry(65, 1, 0, 300), DateTime.Now)
		};

		// Mean = 200
		// Variance = ((100-200)^2 + (300-200)^2) / 2 = (10000 + 10000) / 2 = 10000
		// StdDev = 100
		// CV = 100 / 200 = 0.5
		entries.CalculateCV(200, 2).ShouldBe(0.5);
	}

	[Fact]
	public void CalculatePercentileShouldReturnCorrectValue()
	{
		var latencies = new List<(double TimeToType, int HitCount)>
		{
			(100, 1),
			(200, 1),
			(300, 1),
			(400, 1)
		};

		// Total hits = 4
		// 50th percentile (index 2) = 200ms
		latencies.CalculatePercentile(0.5).ShouldBe(200);

		// 75th percentile (index 3) = 300ms
		latencies.CalculatePercentile(0.75).ShouldBe(300);
	}
}
