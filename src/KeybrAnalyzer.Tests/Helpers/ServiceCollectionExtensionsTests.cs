using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shouldly;

namespace KeybrAnalyzer.Tests.Helpers;

public class ServiceCollectionExtensionsTests
{
	[Fact]
	public void AddKeybrAnalyzerOptionsOverridesDefaultsWhenConfigExists()
	{
		// Arrange
		var services = new ServiceCollection();
		var configData = new Dictionary<string, string?>
		{
			["KeybrAnalyzer:OpenedKeys:0"] = "abc",
			["KeybrAnalyzer:FocusKeys:0"] = "def",
			["KeybrAnalyzer:LockedKeys:TestTier:0"] = "ghi"
		};
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configData)
			.Build();

		// Act
		services.AddKeybrAnalyzerOptions(configuration);
		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<KeybrAnalyzerOptions>>().Value;

		// Assert
		options.OpenedKeys.ShouldBe(["abc"]);
		options.FocusKeys.ShouldBe(["def"]);
		options.LockedKeys.ShouldContainKey("TestTier");
		options.LockedKeys["TestTier"].ShouldBe(["ghi"]);
		options.LockedKeys.Count.ShouldBe(1);
	}

	[Fact]
	public void AddKeybrAnalyzerOptionsKeepsDefaultsWhenConfigSectionIsMissing()
	{
		// Arrange
		var services = new ServiceCollection();
		var configuration = new ConfigurationBuilder().Build();

		// Act
		services.AddKeybrAnalyzerOptions(configuration);
		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<KeybrAnalyzerOptions>>().Value;

		// Assert
		options.OpenedKeys.ShouldNotBeEmpty();
		options.FocusKeys.ShouldNotBeEmpty();
		options.LockedKeys.ShouldNotBeEmpty();
		options.OpenedKeys.ShouldContain("abcdefghijklmnopqrstuvwxyz");
	}

	[Fact]
	public void AddKeybrAnalyzerOptionsOverridesOnlySpecificCollections()
	{
		// Arrange
		var services = new ServiceCollection();
		var configData = new Dictionary<string, string?>
		{
			["KeybrAnalyzer:FocusKeys:0"] = "custom-focus"
		};
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(configData)
			.Build();

		// Act
		services.AddKeybrAnalyzerOptions(configuration);
		var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<KeybrAnalyzerOptions>>().Value;

		// Assert
		// Overridden
		options.FocusKeys.ShouldBe(["custom-focus"]);

		// Defaults kept because they were not in config
		options.OpenedKeys.ShouldContain("abcdefghijklmnopqrstuvwxyz");
		options.LockedKeys.ShouldContainKey("Tier 4 (Numbers)");
	}
}
