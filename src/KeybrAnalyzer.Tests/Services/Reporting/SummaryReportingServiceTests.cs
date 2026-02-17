using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;
using KeybrAnalyzer.Services.Reporting;

using NSubstitute;

namespace KeybrAnalyzer.Tests.Services.Reporting;

public sealed class SummaryReportingServiceTests
{
	private readonly IConsoleHelper _consoleHelper = Substitute.For<IConsoleHelper>();

	[Fact]
	public void PrintSessionSummaryShouldCalculateAverages()
	{
		// Arrange
		var sut = new SummaryReportingService(_consoleHelper);
		var sessions = new List<KeybrSession>
		{
			new(DateTime.Now, 50, 0, 10, 1000, "code", []), // 50 speed -> 10 wpm
			new(DateTime.Now, 150, 0, 10, 1000, "code", []) // 150 speed -> 30 wpm
		};

		// Avg wpm = 20
		// Act
		sut.PrintSessionSummary(sessions, "TITLE");

		// Assert
		_consoleHelper.Received().WriteLine(Arg.Is<string>(s => s.Contains("20.00")));
	}

	[Fact]
	public void PrintMilestonesShouldCallWriteTable()
	{
		// Arrange
		var sut = new SummaryReportingService(_consoleHelper);
		var sessions = new List<KeybrSession>
		{
			new(DateTime.Now, 100, 0, 10, 1000, "code", [])
		};

		// Act
		sut.PrintMilestones(sessions);

		// Assert
		_consoleHelper.Received().WriteTable(
			Arg.Any<string[]>(),
			Arg.Any<IEnumerable<string[]>>(),
			Arg.Any<bool[]>(),
			title: Arg.Is<string>(t => t.Contains("MILESTONES")));
	}
}
