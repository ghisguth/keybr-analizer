using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.Options;

using NSubstitute;

namespace KeybrAnalyzer.Tests.Services.Reporting;

public sealed class KeyboardLayoutReportingServiceTests
{
	private readonly IConsoleHelper _consoleHelper = Substitute.For<IConsoleHelper>();
	private readonly IOptions<KeybrAnalyzerOptions> _options = Substitute.For<IOptions<KeybrAnalyzerOptions>>();

	public KeyboardLayoutReportingServiceTests()
	{
		_options.Value.Returns(new KeybrAnalyzerOptions());
	}

	[Theory]
	[InlineData(KeyboardMode.Finger)]
	[InlineData(KeyboardMode.KeyType)]
	[InlineData(KeyboardMode.Status)]
	public void PrintKeyboardLayoutShouldOutputLayout(KeyboardMode mode)
	{
		// Arrange
		var sut = new KeyboardLayoutReportingService(_consoleHelper, _options);

		// Act
		sut.PrintKeyboardLayout(mode);

		// Assert
		_consoleHelper.Received().WriteTitle(Arg.Is<string>(s => s.Contains("VISUAL PROGRESS")));

		// The layout has many rows, ensure we wrote something
		_consoleHelper.Received().WriteLine(Arg.Any<string>());
	}
}
