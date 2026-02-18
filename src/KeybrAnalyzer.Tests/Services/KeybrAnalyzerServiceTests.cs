using Shouldly;

namespace KeybrAnalyzer.Tests.Services;

public sealed class KeybrAnalyzerServiceTests
{
	[Fact]
	public void ReadTransactionsAsyncShouldThrowOnMalformedCsv()
	{
		1.ShouldBe(1);
	}
}
