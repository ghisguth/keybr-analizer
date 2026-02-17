using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using Shouldly;

namespace KeybrAnalyzer.Tests.Services;

public sealed class KeybrDataServiceTests : IDisposable
{
	private readonly ILogger<KeybrDataService> _logger = Substitute.For<ILogger<KeybrDataService>>();
	private readonly IOptions<KeybrAnalyzerOptions> _options = Substitute.For<IOptions<KeybrAnalyzerOptions>>();
	private readonly string _testDir;

	public KeybrDataServiceTests()
	{
		_testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_testDir);
		_options.Value.Returns(new KeybrAnalyzerOptions { SourceDirectory = _testDir });
	}

	[Fact]
	public void GetLatestDataFilePathShouldReturnNewestFile()
	{
		// Arrange
		var oldFile = Path.Combine(_testDir, "typing-data-old.json");
		var newFile = Path.Combine(_testDir, "typing-data-new.json");
		File.WriteAllText(oldFile, "[]");
		File.SetLastWriteTime(oldFile, DateTime.Now.AddDays(-1));
		File.WriteAllText(newFile, "[]");
		File.SetLastWriteTime(newFile, DateTime.Now);

		var sut = new KeybrDataService(_logger, _options);

		// Act
		var result = sut.GetLatestDataFilePath();

		// Assert
		result.ShouldBe(newFile);
	}

	[Fact]
	public async Task LoadLatestSessionsAsyncShouldDeserializeCorrectly()
	{
		// Arrange
		var file = Path.Combine(_testDir, "typing-data-1.json");
		var json = """
		[
		  {
		    "timeStamp": "2026-02-16T12:00:00Z",
		    "speed": 100.0,
		    "errors": 0,
		    "length": 10,
		    "time": 60000,
		    "textType": "code",
		    "histogram": []
		  }
		]
		""";
		await File.WriteAllTextAsync(file, json, TestContext.Current.CancellationToken);

		var sut = new KeybrDataService(_logger, _options);

		// Act
		var result = await sut.LoadLatestSessionsAsync(TestContext.Current.CancellationToken);

		// Assert
		result.ShouldNotBeNull();
		result.Count.ShouldBe(1);
		result[0].Speed.ShouldBe(100.0);
		result[0].TextType.ShouldBe("code");
	}

	public void Dispose()
	{
		if (Directory.Exists(_testDir))
		{
			Directory.Delete(_testDir, true);
		}
	}
}
