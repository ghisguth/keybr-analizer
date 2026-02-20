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

	[Fact]
	public void ConstructorCanBeParameterless()
	{
		var sut = new ConsoleHelper();
		sut.ShouldNotBeNull();
	}

	[Fact]
	public void WriteLineShouldWriteToStream()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		sut.WriteLine("TEST LINE");
		_writer.Received().WriteLine("TEST LINE");
	}

	[Fact]
	public void WriteLineWithNoArgsShouldWriteEmptyLine()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		sut.WriteLine();
		_writer.Received().WriteLine(string.Empty);
	}

	[Fact]
	public void WriteShouldWriteToStream()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		sut.Write("TEST");
		_writer.Received().Write("TEST");
	}

	[Fact]
	public void WriteTableShouldCallTableWriter()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		var headers = new[] { "H1" };
		var rows = new[] { new[] { "R1" } };
		sut.WriteTable(headers, rows, null, 100, "Title");
		_tableWriter.Received().WriteTable(headers, rows, null, 100, "Title");
	}

	[Fact]
	public void GetSparklineEmptyListReturnsEmptyString()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		var result = sut.GetSparkline([]);
		result.ShouldBeEmpty();
	}

	[Fact]
	public void GetSparklineFlatListReturnsConstantBar()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		var result = sut.GetSparkline([1.0, 1.0, 1.0]);
		result.ShouldBe("▄▄▄");
	}

	[Fact]
	public void GetSparklineMoreThanMaxWidthDownsamples()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		var data = Enumerable.Range(0, 100).Select(i => (double)i).ToList();
		var result = sut.GetSparkline(data, maxWidth: 10);
		result.Length.ShouldBe(10);
	}

	[Fact]
	public void GetSparklineVaryingValuesReturnsSparkline()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		var result = sut.GetSparkline([0, 10, 50, 100], min: 0, max: 100);
		result.ShouldNotBeEmpty();
		result.Length.ShouldBe(4);
	}

	[Fact]
	public void GetProgressBarIncludesLabel()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		var result = sut.GetProgressBar(50, width: 10, label: "TestLabel");
		result.ShouldContain("TestLabel");
		result.ShouldContain(Ansi.Red);
	}

	[Fact]
	public void GetProgressBarYellowForOver95()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		var result = sut.GetProgressBar(96, width: 10);
		result.ShouldContain(Ansi.Yellow);
	}

	[Fact]
	public void GetProgressBarClampValues()
	{
		var sut = new ConsoleHelper(_writer, _tableWriter);
		var result1 = sut.GetProgressBar(-10, width: 10);
		var result2 = sut.GetProgressBar(150, width: 10);

		result1.ShouldContain(Ansi.Red);
		result2.ShouldContain(Ansi.Green);
	}
}
