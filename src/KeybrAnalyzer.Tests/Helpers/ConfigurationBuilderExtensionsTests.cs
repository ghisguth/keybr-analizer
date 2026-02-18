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

	[Fact]
	public void AddIniFileShouldWorkWithCorrectFormat()
	{
		// This test verifies our assumption about INI format for collections
		// Arrange
		var builder = new ConfigurationBuilder();
		var iniPath = Path.Combine(_testRoot, "test.ini");
		var content = """
			[KeybrAnalyzer]
			OpenedKeys:0=abc
			OpenedKeys:1=def
			""";
		File.WriteAllText(iniPath, content);

		// Act
		builder.AddIniFile(iniPath);
		var config = builder.Build();

		// Assert
		config["KeybrAnalyzer:OpenedKeys:0"].ShouldBe("abc");
		config["KeybrAnalyzer:OpenedKeys:1"].ShouldBe("def");
	}

	public void Dispose()
	{
		if (Directory.Exists(_testRoot))
		{
			Directory.Delete(_testRoot, true);
		}
	}
}
