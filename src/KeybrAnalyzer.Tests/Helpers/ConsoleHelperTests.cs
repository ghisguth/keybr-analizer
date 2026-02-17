using KeybrAnalyzer.Helpers;

using NSubstitute;

using Shouldly;

namespace KeybrAnalyzer.Tests.Helpers;

public sealed class ConsoleHelperTests
{
	private readonly TextWriter _writer = Substitute.For<TextWriter>();
	private readonly ITableWriter _tableWriter = Substitute.For<ITableWriter>();

	[Fact]
	public void GetProgressBarShouldReturnFormattedString()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);

		var result = sut.GetProgressBar(100, width: 10);

		result.ShouldContain(Ansi.Green);
		result.ShouldContain("██████████");
	}

	[Fact]
	public void WriteTitleShouldWriteToStream()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);

		sut.WriteTitle("TEST TITLE");

		_writer.Received().WriteLine(Arg.Is<string>(s => s.Contains("TEST TITLE")));
	}
}
