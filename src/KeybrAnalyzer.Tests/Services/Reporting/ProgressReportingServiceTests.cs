using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Models;
using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.Options;

using NSubstitute;

namespace KeybrAnalyzer.Tests.Services.Reporting;

public sealed class ProgressReportingServiceTests
{
	private readonly IConsoleHelper _consoleHelper = Substitute.For<IConsoleHelper>();
	private readonly IOptions<KeybrAnalyzerOptions> _options = Substitute.For<IOptions<KeybrAnalyzerOptions>>();

	public ProgressReportingServiceTests()
	{
		_options.Value.Returns(new KeybrAnalyzerOptions());
	}

	[Fact]
	public void PrintDailyProgressTableShouldCallWriteTable()
	{
		// Arrange
		var sut = new ProgressReportingService(_consoleHelper, _options);
		var sessions = new List<KeybrSession>
		{
			new(DateTime.Now, 100, 0, 10, 1000, "code", [])
		};

		// Act
		sut.PrintDailyProgressTable(sessions);

		// Assert
		_consoleHelper.Received().WriteTable(
			Arg.Any<string[]>(),
			Arg.Any<IEnumerable<string[]>>(),
			Arg.Any<bool[]>(),
			title: "DAILY PROGRESS TABLE");
	}

	[Fact]
	public void PrintTopImprovedKeysShouldHandleEmptyData()
	{
		// Arrange
		var sut = new ProgressReportingService(_consoleHelper, _options);

		// Act
		sut.PrintTopImprovedKeys(new List<KeyPerformance>());

		// Assert
		_consoleHelper.DidNotReceive().WriteTable(Arg.Any<string[]>(), Arg.Any<IEnumerable<string[]>>(), Arg.Any<bool[]>(), title: Arg.Any<string>());
	}
}
