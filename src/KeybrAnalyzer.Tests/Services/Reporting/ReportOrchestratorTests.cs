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

	[Fact]
	public void DomainGapShouldCompareCodeVsProseFromL7Sessions()
	{
		// Arrange
		var now = DateTime.Now;
		var sessions = new List<KeybrSession>
		{
			// 150 CPM = 30 WPM (code)
			new(now, 150, 0, 10, 60000, "code", []),

			// 100 CPM = 20 WPM (natural -> also code)
			new(now, 100, 0, 10, 60000, "natural", []),

			// Avg Code Speed = 125 CPM = 25 WPM
			// 200 CPM = 40 WPM (generated -> prose)
			new(now, 200, 0, 10, 60000, "generated", []),

			// Old sessions (should be ignored)
			new(now.AddDays(-10), 500, 0, 10, 60000, "code", []),
			new(now.AddDays(-10), 100, 0, 10, 60000, "natural", [])
		};

		var sut = new ReportOrchestrator(
			_options,
			_analysisService,
			_summaryReporting,
			_performanceReporting,
			_progressReporting,
			_fatigueReporting,
			_layoutReporting);

		// Act
		sut.GenerateFullReport(sessions, []);

		// Assert
		// Code WPM = 25. Prose WPM = 40. Delta = 25 - 40 = -15.0
		// Total L7 samples: Code=2, Prose=1
		_summaryReporting.Received().PrintHeaderMetric(Arg.Is<string>(s => s.Contains("DOMAIN GAP (CODE vs PROSE)") && s.Contains("C=2") && s.Contains("P=1")), Arg.Is<string>(s => s.Contains("-15.0 WPM")));
	}

	[Fact]
	public void DomainGapShouldNotPrintIfOneTypeIsMissingInL7()
	{
		// Arrange
		var now = DateTime.Now;
		var sessions = new List<KeybrSession>
		{
			new(now, 200, 0, 10, 60000, "code", []),
			new(now, 200, 0, 10, 60000, "natural", []),

			// Missing "generated" (prose)
			new(now.AddDays(-10), 100, 0, 10, 60000, "generated", [])
		};

		var sut = new ReportOrchestrator(
			_options,
			_analysisService,
			_summaryReporting,
			_performanceReporting,
			_progressReporting,
			_fatigueReporting,
			_layoutReporting);

		// Act
		sut.GenerateFullReport(sessions, []);

		// Assert
		_summaryReporting.DidNotReceive().PrintHeaderMetric(Arg.Is<string>(s => s.Contains("DOMAIN GAP")), Arg.Any<string>());
	}
}
