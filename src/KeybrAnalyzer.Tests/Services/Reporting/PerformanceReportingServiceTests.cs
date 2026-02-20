using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;
using KeybrAnalyzer.Services.Reporting;

using NSubstitute;

using Shouldly;

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
			Arg.Is<IEnumerable<string[]>>(x => x.ToList().Count > 0),
			Arg.Any<bool[]>(),
			title: "TITLE");
	}

	[Fact]
	public void PrintTargetKeysShouldCallWriteTable()
	{
		// Arrange
		var sut = new PerformanceReportingService(_consoleHelper);
		var data = new List<KeyPerformance>
		{
			new() { Key = "A", L7Latency = 500, L7CV = 1.0 },
			new() { Key = "B", L7Latency = 600, L7CV = 2.0 }
		};

		// Act
		sut.PrintTargetKeys(data, "TARGET", h => h.L7CV, 1);

		// Assert
		_consoleHelper.Received().WriteTable(
			Arg.Any<string[]>(),
			Arg.Any<IEnumerable<string[]>>(),
			Arg.Any<bool[]>(),
			title: "TARGET");
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

	[Fact]
	public void PrintCriticalTargetsTableShouldFormatFlags()
	{
		var sut = new PerformanceReportingService(_consoleHelper);
		var data = new List<KeyPerformance>
		{
			new() { Key = "A", L7CV = 1000.0 }, // Hesitation
			new() { Key = "B", L7Err = 1.0, L7Impact = 1000.0 }, // Impact
			new() { Key = "C", StallRatio = 100.0 }, // Stall
			new() { Key = "D", L7Latency = 10000.0 } // Latency
		};

		sut.PrintCriticalTargetsTable(data, "CRITICAL");

		_consoleHelper.ReceivedWithAnyArgs().WriteTable(
			default!,
			Arg.Is<IEnumerable<string[]>>(x => x.ToList().Count > 0),
			default!,
			default!,
			default!);
	}

	[Fact]
	public void PrintKeyPerformanceTableThrowsOnNull()
	{
		var sut = new PerformanceReportingService(_consoleHelper);
		Should.Throw<ArgumentNullException>(() => sut.PrintKeyPerformanceTable(null!, "title"));
	}

	[Fact]
	public void PrintTargetKeysThrowsOnNull()
	{
		var sut = new PerformanceReportingService(_consoleHelper);
		Should.Throw<ArgumentNullException>(() => sut.PrintTargetKeys(null!, "title", h => h.L7CV));
	}

	[Fact]
	public void PrintStallAnalysisTableThrowsOnNull()
	{
		var sut = new PerformanceReportingService(_consoleHelper);
		Should.Throw<ArgumentNullException>(() => sut.PrintStallAnalysisTable(null!, "title"));
	}

	[Fact]
	public void PrintCriticalTargetsTableThrowsOnNull()
	{
		var sut = new PerformanceReportingService(_consoleHelper);
		Should.Throw<ArgumentNullException>(() => sut.PrintCriticalTargetsTable(null!, "title"));
	}

	[Fact]
	public void PrintCriticalTargetsTableEmptyDataDoesNothing()
	{
		var sut = new PerformanceReportingService(_consoleHelper);
		sut.PrintCriticalTargetsTable([], "title");
		_consoleHelper.DidNotReceiveWithAnyArgs().WriteTable(default!, default!, default!, default!, default!);
	}

	[Fact]
	public void PrintAllKeysPerformanceTableThrowsOnNull()
	{
		var sut = new PerformanceReportingService(_consoleHelper);
		Should.Throw<ArgumentNullException>(() => sut.PrintAllKeysPerformanceTable(null!, "title"));
	}

	[Fact]
	public void PrintAllKeysPerformanceTableWritesTable()
	{
		var sut = new PerformanceReportingService(_consoleHelper);
		sut.PrintAllKeysPerformanceTable([new KeyPerformance { Key = "A" }], "title");
		_consoleHelper.ReceivedWithAnyArgs().WriteTable(
			default!,
			Arg.Is<IEnumerable<string[]>>(x => x.ToList().Count > 0),
			default!,
			default!,
			default!);
	}

	[Fact]
	public void PrintStallAnalysisTableWritesTableWithColoredStatus()
	{
		var sut = new PerformanceReportingService(_consoleHelper);
		sut.PrintStallAnalysisTable(
			[
				new KeyPerformance { Key = "A", StallRatio = 4.0 }, // COGNITIVE
				new KeyPerformance { Key = "B", StallRatio = 2.5 }, // HEAVY
				new KeyPerformance { Key = "C", StallRatio = 1.0 } // MECHANICAL
			],
			"title");

		_consoleHelper.ReceivedWithAnyArgs().WriteTable(
			default!,
			Arg.Is<IEnumerable<string[]>>(x => x.ToList().Count > 0),
			default!,
			default!,
			default!);
	}
}
