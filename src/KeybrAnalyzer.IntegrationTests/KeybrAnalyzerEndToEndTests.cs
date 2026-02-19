using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NSubstitute;

namespace KeybrAnalyzer.IntegrationTests;

public sealed class KeybrAnalyzerEndToEndTests : IDisposable
{
	private readonly string _tempDir;
	private readonly IConsoleHelper _mockConsole = Substitute.For<IConsoleHelper>();
	private readonly IHost _host;

	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Host is disposed in Dispose")]
	public KeybrAnalyzerEndToEndTests()
	{
		_tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_tempDir);

		var builder = Host.CreateApplicationBuilder();

		// Configure Options
		builder.Services.Configure<KeybrAnalyzerOptions>(options =>
		{
			options.SourceDirectory = _tempDir;
			options.RunOnStartup = false;
		});

		// Core Services
		builder.Services.AddSingleton<ITableWriter>(new TableWriter(TextWriter.Null));
		builder.Services.AddSingleton(_mockConsole);
		builder.Services.AddSingleton<IKeybrDataService, KeybrDataService>();
		builder.Services.AddSingleton<IKeybrAnalysisService, KeybrAnalysisService>();
		builder.Services.AddSingleton<IKeyStatusService, KeyStatusService>();
		builder.Services.AddSingleton<ITrainingStateService, TrainingStateService>();

		// Reporting Services
		builder.Services.AddSingleton<ISummaryReportingService, SummaryReportingService>();
		builder.Services.AddSingleton<IPerformanceReportingService, PerformanceReportingService>();
		builder.Services.AddSingleton<IProgressReportingService, ProgressReportingService>();
		builder.Services.AddSingleton<IFatigueReportingService, FatigueReportingService>();
		builder.Services.AddSingleton<IKeyboardLayoutReportingService, KeyboardLayoutReportingService>();
		builder.Services.AddSingleton<IReportOrchestrator, ReportOrchestrator>();

		builder.Services.AddSingleton<KeybrAnalyzerService>();

		// Logging
		builder.Logging.ClearProviders();

		_host = builder.Build();
	}

	[Fact]
	public async Task ServiceShouldProcessDataAndRenderReportsAsync()
	{
		// Arrange
		var json = LoadResource("KeybrAnalyzer.IntegrationTests.Resources.sample-data.json");
		var filePath = Path.Combine(_tempDir, "typing-data-sample.json");
		await File.WriteAllTextAsync(filePath, json, TestContext.Current.CancellationToken);

		using var scope = _host.Services.CreateScope();
		var analyzer = scope.ServiceProvider.GetRequiredService<KeybrAnalyzerService>();

		// Act
		await analyzer.RunLogicAsync(TestContext.Current.CancellationToken);

		// Assert
		_mockConsole.Received().WriteLine(Arg.Is<string>(s => s.Contains(" _  _ ____ _   _ ___  ____    ____ _  _ ____ _    _   _ ___ ____ ____ ")));
		_mockConsole.Received().WriteTitle("STATISTICS FOR TODAY");
		_mockConsole.Received().WriteTitle("ACCURACY STREAKS");
		_mockConsole.Received().WriteTitle("TRAINING PROGRESS SUMMARY");

		// Table headers check via WriteTable
		_mockConsole.Received().WriteTable(
			Arg.Is<string[]>(h => h.Contains("METRIC") && h.Contains("VALUE")),
			Arg.Any<IEnumerable<string[]>>(),
			Arg.Any<bool[]>(),
			title: "MILESTONES & CAREER RECORDS");
	}

	public void Dispose()
	{
		_host.Dispose();
		if (Directory.Exists(_tempDir))
		{
			Directory.Delete(_tempDir, true);
		}
	}

	private static string LoadResource(string resourceName)
	{
		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
			?? throw new InvalidOperationException($"Could not find resource: {resourceName}");

		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}
}
