using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;
using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.Options;

using NSubstitute;

using Shouldly;

namespace KeybrAnalyzer.Tests.Services.Reporting;

public sealed class FatigueReportingServiceTests
{
	private readonly IConsoleHelper _consoleHelper = Substitute.For<IConsoleHelper>();
	private readonly IOptions<KeybrAnalyzerOptions> _options = Substitute.For<IOptions<KeybrAnalyzerOptions>>();

	public FatigueReportingServiceTests()
	{
		_options.Value.Returns(new KeybrAnalyzerOptions());
	}

	[Fact]
	public void PrintFatigueAnalysisShouldAlwaysCallWriteTableForTodaySession()
	{
		// Arrange
		_options.Value.ShowAllStats = false;
		var sut = new FatigueReportingService(_consoleHelper, _options);
		var sessions = new List<KeybrSession>
		{
			new(DateTime.Now, 100, 0, 10, 1000, "code", [])
		};

		// Act
		sut.PrintFatigueAnalysis(sessions);

		// Assert
		_consoleHelper.Received().WriteTable(
			Arg.Any<string[]>(),
			Arg.Any<IEnumerable<string[]>>(),
			Arg.Any<bool[]>(),
			title: "THIS SESSION FATIGUE ANALYSIS");

		_consoleHelper.DidNotReceive().WriteTable(
			Arg.Any<string[]>(),
			Arg.Any<IEnumerable<string[]>>(),
			Arg.Any<bool[]>(),
			title: "SESSION FATIGUE ANALYSIS");
	}

	[Fact]
	public void PrintFatigueAnalysisShouldCallWriteTableForAllSessionsWhenShowAllStatsIsTrue()
	{
		// Arrange
		_options.Value.ShowAllStats = true;
		var sut = new FatigueReportingService(_consoleHelper, _options);
		var sessions = new List<KeybrSession>
		{
			new(DateTime.Now, 100, 0, 10, 1000, "code", [])
		};

		// Act
		sut.PrintFatigueAnalysis(sessions);

		// Assert
		_consoleHelper.Received().WriteTable(
			Arg.Any<string[]>(),
			Arg.Any<IEnumerable<string[]>>(),
			Arg.Any<bool[]>(),
			title: "THIS SESSION FATIGUE ANALYSIS");

		_consoleHelper.Received().WriteTable(
			Arg.Any<string[]>(),
			Arg.Any<IEnumerable<string[]>>(),
			Arg.Any<bool[]>(),
			title: "SESSION FATIGUE ANALYSIS");
	}

	[Fact]
	public void PrintFatigueAnalysisWithManySessionsShouldRender100PlusBlock()
	{
		// Arrange
		_options.Value.ShowAllStats = true;
		var sut = new FatigueReportingService(_consoleHelper, _options);
		var sessions = Enumerable.Range(0, 105).Select(i => new KeybrSession(DateTime.Now.AddMinutes(i), 100, 0, 10, 1000, "code", [])).ToList();

		// Act
		sut.PrintFatigueAnalysis(sessions);

		// Assert
		_consoleHelper.Received().WriteTable(
			Arg.Any<string[]>(),
			Arg.Is<IEnumerable<string[]>>(rows => rows.Any(r => r[0] == "100+")),
			Arg.Any<bool[]>(),
			title: "THIS SESSION FATIGUE ANALYSIS");
	}

	[Fact]
	public void PrintFatigueIndicatorShouldDetectFatigueOnDownwardSlope()
	{
		// Arrange
		var sut = new FatigueReportingService(_consoleHelper, _options);
		var now = DateTime.Now;

		// Create a series of sessions with decreasing speed
		var sessions = new List<KeybrSession>();
		for (var i = 0; i < 10; i++)
		{
			sessions.Add(new(now.AddMinutes(i), 100 - (i * 5), 0, 10, 1000, "code", []));
		}

		// Act
		sut.PrintFatigueIndicator(sessions);

		// Assert
		_consoleHelper.Received().WriteLine(Arg.Is<string>(s => s.Contains("FATIGUE DETECTED")));
	}

	[Fact]
	public void PrintFatigueAnalysisThrowsOnNull()
	{
		var sut = new FatigueReportingService(_consoleHelper, _options);
		Should.Throw<ArgumentNullException>(() => sut.PrintFatigueAnalysis(null!));
	}

	[Fact]
	public void PrintFatigueAnalysisEmptySessionsDoesNothing()
	{
		var sut = new FatigueReportingService(_consoleHelper, _options);
		sut.PrintFatigueAnalysis([]);
		_consoleHelper.DidNotReceiveWithAnyArgs().WriteTable(default!, default!, default!, default!, default!);
	}

	[Fact]
	public void PrintFatigueIndicatorThrowsOnNull()
	{
		var sut = new FatigueReportingService(_consoleHelper, _options);
		Should.Throw<ArgumentNullException>(() => sut.PrintFatigueIndicator(null!));
	}

	[Fact]
	public void PrintFatigueIndicatorLessThan3SessionsDoesNothing()
	{
		var sut = new FatigueReportingService(_consoleHelper, _options);
		sut.PrintFatigueIndicator([new KeybrSession(DateTime.Now, 100, 0, 10, 1000, "code", [])]);
		_consoleHelper.DidNotReceiveWithAnyArgs().WriteLine(default!);
	}

	[Fact]
	public void PrintFatigueIndicatorZeroSlopeDoesNothing()
	{
		var sut = new FatigueReportingService(_consoleHelper, _options);
		var now = DateTime.Now;
		var sessions = new List<KeybrSession>
		{
			new(now, 100, 0, 10, 1000, "code", []),
			new(now.AddMinutes(1), 100, 0, 10, 1000, "code", []),
			new(now.AddMinutes(2), 100, 0, 10, 1000, "code", [])
		};
		sut.PrintFatigueIndicator(sessions);
		_consoleHelper.Received().WriteLine(Arg.Is<string>(s => s.Contains("0.00 WPM")));
	}

	[Fact]
	public void PrintFatigueIndicatorPositiveSlopePrintsUpwardArrow()
	{
		var sut = new FatigueReportingService(_consoleHelper, _options);
		var now = DateTime.Now;
		var sessions = new List<KeybrSession>
		{
			new(now, 100, 0, 10, 1000, "code", []),
			new(now.AddMinutes(1), 110, 0, 10, 1000, "code", []),
			new(now.AddMinutes(2), 120, 0, 10, 1000, "code", [])
		};
		sut.PrintFatigueIndicator(sessions);
		_consoleHelper.Received().WriteLine(Arg.Is<string>(s => s.Contains('↗')));
	}
}
