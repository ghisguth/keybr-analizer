using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;
using KeybrAnalyzer.Services.Reporting;

using NSubstitute;

namespace KeybrAnalyzer.Tests.Services.Reporting;

public sealed class FatigueReportingServiceTests
{
	private readonly IConsoleHelper _consoleHelper = Substitute.For<IConsoleHelper>();

	[Fact]
	public void PrintFatigueAnalysisShouldCallWriteTable()
	{
		// Arrange
		var sut = new FatigueReportingService(_consoleHelper);
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
			title: Arg.Is<string>(t => t.Contains("FATIGUE")));
	}

	[Fact]
	public void PrintFatigueIndicatorShouldDetectFatigueOnDownwardSlope()
	{
		// Arrange
		var sut = new FatigueReportingService(_consoleHelper);
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
