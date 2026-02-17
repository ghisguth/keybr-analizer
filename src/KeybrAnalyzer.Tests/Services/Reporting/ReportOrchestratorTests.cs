using KeybrAnalyzer.Models;
using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.Options;

using NSubstitute;

namespace KeybrAnalyzer.Tests.Services.Reporting;

public sealed class ReportOrchestratorTests
{
	private readonly IKeybrAnalysisService _analysisService = Substitute.For<IKeybrAnalysisService>();
	private readonly ISummaryReportingService _summaryReporting = Substitute.For<ISummaryReportingService>();
	private readonly IPerformanceReportingService _performanceReporting = Substitute.For<IPerformanceReportingService>();
	private readonly IProgressReportingService _progressReporting = Substitute.For<IProgressReportingService>();
	private readonly IFatigueReportingService _fatigueReporting = Substitute.For<IFatigueReportingService>();
	private readonly IKeyboardLayoutReportingService _layoutReporting = Substitute.For<IKeyboardLayoutReportingService>();
	private readonly IOptions<KeybrAnalyzerOptions> _options = Substitute.For<IOptions<KeybrAnalyzerOptions>>();

	public ReportOrchestratorTests()
	{
		_options.Value.Returns(new KeybrAnalyzerOptions());
	}

	[Fact]
	public void GenerateFullReportShouldInvokeAllSubReports()
	{
		// Arrange
		var sessions = new List<KeybrSession>
		{
			new(DateTime.Now, 100, 0, 10, 60000, "code", [])
		};
		var histogram = new List<KeyPerformance>();

		var sut = new ReportOrchestrator(
			_options,
			_analysisService,
			_summaryReporting,
			_performanceReporting,
			_progressReporting,
			_fatigueReporting,
			_layoutReporting);

		// Act
		sut.GenerateFullReport(sessions, histogram);

		// Assert
		_summaryReporting.Received().PrintHeader();
		_progressReporting.Received().PrintDailyProgressTable(Arg.Any<IEnumerable<KeybrSession>>());
		_summaryReporting.Received().PrintMilestones(Arg.Any<IEnumerable<KeybrSession>>());
	}
}
