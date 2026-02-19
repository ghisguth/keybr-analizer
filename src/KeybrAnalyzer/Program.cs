using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Services;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.Logging.Console;

var builder = Host.CreateApplicationBuilder(args);

// Load custom configuration from home directory, AppData/Local and AppData/Roaming
builder.Configuration.AddKeybrConfiguration(args);

// Logging Defaults (overridable by appsettings.json)
builder.Logging.AddFilter("Default", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Error);

builder.Logging.AddSimpleConsole(options =>
{
	options.SingleLine = true;
	options.IncludeScopes = true;
	options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fffffffzzz ";
	options.UseUtcTimestamp = true;
	options.ColorBehavior = LoggerColorBehavior.Default;
});

builder.Services.AddKeybrAnalyzerOptions(builder.Configuration);

builder.Services.AddSingleton<ITableWriter>(new TableWriter(Console.Out));
builder.Services.AddSingleton<IConsoleHelper, ConsoleHelper>();
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

builder.Services.AddHostedService<KeybrAnalyzerService>();

var host = builder.Build();

host.Run();

public partial class Program
{
}
