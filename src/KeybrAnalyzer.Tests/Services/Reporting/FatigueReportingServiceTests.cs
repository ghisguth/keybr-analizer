using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;
using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.Options;

using NSubstitute;

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
}
