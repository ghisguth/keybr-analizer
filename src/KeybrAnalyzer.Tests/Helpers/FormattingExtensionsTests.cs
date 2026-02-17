using KeybrAnalyzer.Helpers;

using Shouldly;

namespace KeybrAnalyzer.Tests.Helpers;

public sealed class FormattingExtensionsTests
{
	[Theory]
	[InlineData(1.5, Ansi.Green)]
	[InlineData(3.5, Ansi.Yellow)]
	[InlineData(6.0, Ansi.Red)]
	public void FormatErrorShouldReturnCorrectColor(double value, string expectedColor)
	{
		var result = value.FormatError();
		result.ShouldContain(expectedColor);
		result.ShouldContain($"{value:F2}%");
		result.ShouldEndWith(Ansi.Reset);
	}

	[Theory]
	[InlineData(99.0, Ansi.Green)]
	[InlineData(96.0, Ansi.Yellow)]
	[InlineData(94.0, Ansi.Red)]
	public void FormatAccuracyShouldReturnCorrectColor(double value, string expectedColor)
	{
		var result = value.FormatAccuracy();
		result.ShouldContain(expectedColor);
		result.ShouldContain("%");
		result.ShouldEndWith(Ansi.Reset);
	}

	[Fact]
	public void FormatWpmShouldReturnCyanWithLightning()
	{
		var result = 60.5.FormatWpm();
		result.ShouldContain(Ansi.Cyan);
		result.ShouldContain("⚡");
		result.ShouldContain("60.50");
	}

	[Theory]
	[InlineData(0, "00:00:00")]
	[InlineData(65, "00:01:05")]
	[InlineData(3665, "01:01:05")]
	[InlineData(360000, "100:00:00")]
	public void ToTimeStrShouldFormatCorrectly(double seconds, string expected)
	{
		seconds.ToTimeStr().ShouldBe(expected);
	}
}
