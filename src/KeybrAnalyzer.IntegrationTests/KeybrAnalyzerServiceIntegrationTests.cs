using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Shouldly;

namespace KeybrAnalyzer.IntegrationTests;

public sealed class KeybrAnalyzerServiceIntegrationTests : IDisposable, IAsyncDisposable
{
	private readonly WebApplicationFactory<Program> _factory = new KeybrAnalyzerTestApplicationFactory();

	[Fact]
	public void AnalyzerServiceShouldBeRegistered()
	{
		using var scope = _factory.Services.CreateScope();
		var analyzer = scope.ServiceProvider.GetService<KeybrAnalyzerService>();
		analyzer.ShouldNotBeNull();
	}

	[Fact]
	public async Task AnalyzerServiceShouldRunWithoutErrorAsync()
	{
		// Arrange
		using var scope = _factory.Services.CreateScope();
		var analyzer = scope.ServiceProvider.GetRequiredService<KeybrAnalyzerService>();

		analyzer.ShouldNotBeNull();

		// Act & Assert
		await Should.NotThrowAsync(async () =>
		{
			await analyzer.RunLogicAsync(TestContext.Current.CancellationToken);
		});
	}

	public void Dispose() => _factory.Dispose();

	public ValueTask DisposeAsync() => _factory.DisposeAsync();

	private sealed class KeybrAnalyzerTestApplicationFactory : WebApplicationFactory<Program>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.Configure(app =>
			{
				app.UseRouting();
			});

			builder.ConfigureServices(services =>
			{
				services.AddRouting();

				services.Configure<KeybrAnalyzerOptions>(options =>
				{
					options.SourceDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
					options.RunOnStartup = false;
				});

				services.Configure<HostOptions>(options =>
				{
					options.ServicesStartConcurrently = false;
					options.ServicesStopConcurrently = false;
				});

				services.AddSingleton<ITableWriter>(new TableWriter(TextWriter.Null));
				services.AddSingleton<IConsoleHelper, ConsoleHelper>();
				services.AddSingleton<IKeybrDataService, KeybrDataService>();
				services.AddSingleton<IKeybrAnalysisService, KeybrAnalysisService>();
				services.AddSingleton<IKeyStatusService, KeyStatusService>();
				services.AddSingleton<ITrainingStateService, TrainingStateService>();

				// Reporting
				services.AddSingleton<ISummaryReportingService, SummaryReportingService>();
				services.AddSingleton<IPerformanceReportingService, PerformanceReportingService>();
				services.AddSingleton<IProgressReportingService, ProgressReportingService>();
				services.AddSingleton<IFatigueReportingService, FatigueReportingService>();
				services.AddSingleton<IKeyboardLayoutReportingService, KeyboardLayoutReportingService>();
				services.AddSingleton<IReportOrchestrator, ReportOrchestrator>();

				services.AddSingleton<KeybrAnalyzerService>();
			});
		}
	}
}
