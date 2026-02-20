using KeybrAnalyzer.Services;

using Shouldly;

namespace KeybrAnalyzer.Tests.Services;

public sealed class KeyMetricCalculatorTests
{
	[Theory]
	[InlineData(200, 0, 0, 1.0, 5.0)] // 1000 / (200 * 1 * 1 * 1) = 5
	[InlineData(200, 100, 0, 1.0, 2.5)] // 1000 / (200 * 2 * 1 * 1) = 2.5
	[InlineData(0, 0, 0, 1.0, 0)]
	public void CalculateMasteryShouldReturnCorrectScore(double latency, double errorRate, double cv, double weight, double expected)
	{
		KeyMetricCalculator.CalculateMastery(latency, errorRate, cv, weight).ShouldBe(expected, 0.001);
	}

	[Theory]
	[InlineData('{', 2.0)]
	[InlineData('}', 2.0)]
	[InlineData('(', 2.0)]
	[InlineData(')', 2.0)]
	[InlineData('[', 2.0)]
	[InlineData(']', 2.0)]
	[InlineData(';', 1.5)]
	[InlineData('.', 1.5)]
	[InlineData(',', 1.5)]
	[InlineData('_', 1.5)]
	[InlineData('\"', 1.5)]
	[InlineData('\'', 1.5)]
	[InlineData('=', 1.3)]
	[InlineData('<', 1.3)]
	[InlineData('>', 1.3)]
	[InlineData('+', 1.3)]
	[InlineData('-', 1.3)]
	[InlineData('*', 1.3)]
	[InlineData('/', 1.3)]
	[InlineData('&', 1.3)]
	[InlineData('|', 1.3)]
	[InlineData('!', 1.3)]
	[InlineData('a', 1.0)]
	public void GetCSharpWeightShouldReturnCorrectWeight(char c, double expected)
	{
		KeyMetricCalculator.GetCSharpWeight(c).ShouldBe(expected);
	}
}
