using KeybrAnalyzer.Models;
using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using Shouldly;

namespace KeybrAnalyzer.Tests.Services;

public sealed class KeybrAnalyzerServiceTests : IDisposable
{
	private readonly IHostApplicationLifetime _hostApplicationLifetime = Substitute.For<IHostApplicationLifetime>();
	private readonly ILogger<KeybrAnalyzerService> _logger = Substitute.For<ILogger<KeybrAnalyzerService>>();
	private readonly IOptions<KeybrAnalyzerOptions> _options = Substitute.For<IOptions<KeybrAnalyzerOptions>>();
	private readonly IKeybrDataService _dataService = Substitute.For<IKeybrDataService>();
	private readonly IKeybrAnalysisService _analysisService = Substitute.For<IKeybrAnalysisService>();
	private readonly IReportOrchestrator _reportOrchestrator = Substitute.For<IReportOrchestrator>();

	private readonly KeybrAnalyzerService _sut;

	public KeybrAnalyzerServiceTests()
	{
		_options.Value.Returns(new KeybrAnalyzerOptions());
		_sut = new KeybrAnalyzerService(
			_hostApplicationLifetime,
			_logger,
			_options,
			_dataService,
			_analysisService,
			_reportOrchestrator);
	}

	[Fact]
	public async Task RunLogicAsyncShouldReturnWhenNoSessionsFoundAsync()
	{
		// Arrange
		_dataService.LoadLatestSessionsAsync(Arg.Any<CancellationToken>())
			.Returns((List<KeybrSession>?)null);

		// Act
		await _sut.RunLogicAsync(TestContext.Current.CancellationToken);

		// Assert
		_analysisService.DidNotReceiveWithAnyArgs().GetHistogramData(default!, default);
		_reportOrchestrator.DidNotReceiveWithAnyArgs().GenerateFullReport(default!, default!);
		_hostApplicationLifetime.Received(1).StopApplication();
	}

	[Fact]
	public async Task RunLogicAsyncShouldProcessSessionsSuccessfullyAsync()
	{
		// Arrange
		var sessions = new List<KeybrSession>
		{
			new(DateTime.Now, 100, 0, 10, 60000, "generated", [])
		};
		var histogram = new List<KeyPerformance>();

		_dataService.LoadLatestSessionsAsync(Arg.Any<CancellationToken>())
			.Returns(sessions);
		_analysisService.GetHistogramData(sessions, Arg.Any<DateTime>())
			.Returns(histogram);

		// Act
		await _sut.RunLogicAsync(TestContext.Current.CancellationToken);

		// Assert
		_analysisService.Received(1).GetHistogramData(sessions, Arg.Any<DateTime>());
		_reportOrchestrator.Received(1).GenerateFullReport(sessions, histogram);
		_hostApplicationLifetime.Received(1).StopApplication();
	}

	[Fact]
	public async Task RunLogicAsyncShouldLogErrorAndStopApplicationOnFailureAsync()
	{
		// Arrange
		var exception = new InvalidOperationException("Test exception");
		_dataService.LoadLatestSessionsAsync(Arg.Any<CancellationToken>())
			.Throws(exception);

		// Act & Assert
		var result = await Should.ThrowAsync<InvalidOperationException>(() => _sut.RunLogicAsync(TestContext.Current.CancellationToken));
		result.Message.ShouldBe("Test exception");

		_hostApplicationLifetime.Received(1).StopApplication();
	}

	public void Dispose() => _sut.Dispose();
}
