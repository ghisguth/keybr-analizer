using System.Globalization;

using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.Options;

namespace KeybrAnalyzer.Services;

public class KeybrAnalyzerService(
	IHostApplicationLifetime hostApplicationLifetime,
	ILogger<KeybrAnalyzerService> logger,
	IOptions<KeybrAnalyzerOptions> options,
	IKeybrDataService dataService,
	IKeybrAnalysisService analysisService,
	IReportOrchestrator reportOrchestrator)
	: BackgroundService
{
	public virtual async Task RunLogicAsync(CancellationToken stoppingToken)
	{
		try
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US");
			var sessions = await dataService.LoadLatestSessionsAsync(stoppingToken);
			if (sessions == null || sessions.Count == 0)
			{
				return;
			}

			var maxDate = sessions.Max(s => s.TimeStamp.ToLocalTime());
			var histogram = analysisService.GetHistogramData(sessions, maxDate);

			reportOrchestrator.GenerateFullReport(sessions, histogram);
		}
		catch (Exception e)
		{
			logger.LogError(e, "Error processing sessions");
			throw;
		}
		finally
		{
			hostApplicationLifetime.StopApplication();
		}
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (options.Value.RunOnStartup)
		{
			await RunLogicAsync(stoppingToken);
		}
	}
}
