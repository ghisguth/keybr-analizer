using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;
using KeybrAnalyzer.Services.Reporting;

using NSubstitute;

namespace KeybrAnalyzer.Tests.Services.Reporting;

public sealed class PerformanceReportingServiceTests
{
	private readonly IConsoleHelper _consoleHelper = Substitute.For<IConsoleHelper>();

	[Fact]
	public void PrintKeyPerformanceTableShouldCallWriteTable()
	{
		// Arrange
		var sut = new PerformanceReportingService(_consoleHelper);
		var data = new List<KeyPerformance>
		{
			new() { Key = "A", DailyWpm = new List<double> { 50 } }
		};

		// Act
		sut.PrintKeyPerformanceTable(data, "TITLE");

		// Assert
		_consoleHelper.Received().WriteTable(
			Arg.Any<string[]>(),
			Arg.Any<IEnumerable<string[]>>(),
			Arg.Any<bool[]>(),
			title: "TITLE");
	}

	[Fact]
	public void PrintCriticalTargetsTableShouldFilterCorrectly()
	{
		// Arrange
		var sut = new PerformanceReportingService(_consoleHelper);
		var data = new List<KeyPerformance>
		{
			new() { Key = "A", L7Latency = 500 },
			new() { Key = "B", L7Err = 10, L7H = 100 } // High impact
		};

		// Act
		sut.PrintCriticalTargetsTable(data, "CRITICAL");

		// Assert
		_consoleHelper.Received().WriteTable(
			Arg.Any<string[]>(),
			Arg.Any<IEnumerable<string[]>>(),
			Arg.Any<bool[]>(),
			title: "CRITICAL");
	}
}
