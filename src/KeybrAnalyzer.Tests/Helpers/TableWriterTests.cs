using KeybrAnalyzer.Helpers;

using Shouldly;

namespace KeybrAnalyzer.Tests.Helpers;

public sealed class TableWriterTests
{
	[Fact]
	public void WriteTableShouldProduceOutput()
	{
		// Arrange
		using var sw = new StringWriter();
		var sut = new TableWriter(sw);
		string[] headers = ["Col1", "Col2"];
		string[][] rows = [["Val1", "Val2"]];

		// Act
		sut.WriteTable(headers, rows);

		// Assert
		var output = sw.ToString();
		output.ShouldContain("Col1");
		output.ShouldContain("Col2");
		output.ShouldContain("Val1");
		output.ShouldContain("Val2");
		output.ShouldContain("┌"); // Box drawing char
		output.ShouldContain("└");
	}

	[Fact]
	public void WriteTableWithTitleShouldIncludeTitle()
	{
		// Arrange
		using var sw = new StringWriter();
		var sut = new TableWriter(sw);
		string[] headers = ["H1"];
		string[][] rows = [["V1"]];

		// Act
		sut.WriteTable(headers, rows, title: "MY SPECIAL TABLE");

		// Assert
		var output = sw.ToString();
		output.ShouldContain("MY SPECIAL TABLE");
	}
}
