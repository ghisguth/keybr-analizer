using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Services.Reporting;

using NSubstitute;

namespace KeybrAnalyzer.Tests.Services.Reporting;

public sealed class KeyboardLayoutReportingServiceTests
{
	private readonly IConsoleHelper _consoleHelper = Substitute.For<IConsoleHelper>();
	private readonly IKeyStatusService _keyStatusService = Substitute.For<IKeyStatusService>();

	[Theory]
	[InlineData(KeyboardMode.Finger)]
	[InlineData(KeyboardMode.KeyType)]
	[InlineData(KeyboardMode.Status)]
	public void PrintKeyboardLayoutShouldOutputLayout(KeyboardMode mode)
	{
		// Arrange
		var sut = new KeyboardLayoutReportingService(_consoleHelper, _keyStatusService);

		// Act
		sut.PrintKeyboardLayout(mode);

		// Assert
		_consoleHelper.Received().WriteTitle(Arg.Is<string>(s => s.Contains("VISUAL PROGRESS")));

		// The layout has many rows, ensure we wrote something
		_consoleHelper.Received().WriteLine();
		_consoleHelper.Received().WriteLine(Arg.Any<string>());
	}
}
