using KeybrAnalyzer.Helpers;

using Microsoft.Extensions.Configuration;

using Shouldly;

namespace KeybrAnalyzer.Tests.Helpers;

public sealed class ConfigurationBuilderExtensionsTests : IDisposable
{
	private readonly string _testRoot;

	public ConfigurationBuilderExtensionsTests()
	{
		_testRoot = Path.Combine(Path.GetTempPath(), "KeybrAnalyzerTests_" + Guid.NewGuid().ToString());
		Directory.CreateDirectory(_testRoot);
	}

	[Fact]
	public void AddKeybrConfigurationShouldIncludeEnvironmentVariables()
	{
		// Arrange
		var builder = new ConfigurationBuilder();
		Environment.SetEnvironmentVariable("KeybrAnalyzer__ShowAllStats", "true");

		try
		{
			// Act
			builder.AddKeybrConfiguration();
			var config = builder.Build();

			// Assert
			config["KeybrAnalyzer:ShowAllStats"].ShouldBe("true");
		}
		finally
		{
			Environment.SetEnvironmentVariable("KeybrAnalyzer__ShowAllStats", null);
		}
	}

	[Fact]
	public void AddKeybrConfigurationShouldIncludeCommandLineArguments()
	{
		// Arrange
		var builder = new ConfigurationBuilder();
		string[] args = ["--KeybrAnalyzer:ShowAllStats=true"];

		// Act
		builder.AddKeybrConfiguration(args);
		var config = builder.Build();

		// Assert
		config["KeybrAnalyzer:ShowAllStats"].ShouldBe("true");
	}

	public void Dispose()
	{
		if (Directory.Exists(_testRoot))
		{
			Directory.Delete(_testRoot, true);
		}
	}
}
