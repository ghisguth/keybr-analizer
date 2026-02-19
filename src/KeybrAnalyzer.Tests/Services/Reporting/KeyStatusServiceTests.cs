using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.Options;

using NSubstitute;

using Shouldly;

namespace KeybrAnalyzer.Tests.Services.Reporting;

public class KeyStatusServiceTests
{
	[Fact]
	public void GetKeyStatusShouldReturnFocusWhenKeyIsInFocusKeys()
	{
		// Arrange
		var options = new KeybrAnalyzerOptions();
		options.FocusKeys.Clear();
		options.FocusKeys.Add("[ ]");
		var optionsSubstitute = Substitute.For<IOptions<KeybrAnalyzerOptions>>();
		optionsSubstitute.Value.Returns(options);

		var service = new KeyStatusService(optionsSubstitute);

		// Act & Assert
		service.GetKeyStatus('[').ShouldBe(KeyStatus.Focus);
		service.GetKeyStatus(']').ShouldBe(KeyStatus.Focus);
	}

	[Fact]
	public void GetKeyStatusShouldReturnUnlockedWhenKeyIsOpenedButNotFocus()
	{
		// Arrange
		var options = new KeybrAnalyzerOptions();
		options.OpenedKeys.Clear();
		options.OpenedKeys.Add("abc");
		options.FocusKeys.Clear();
		var optionsSubstitute = Substitute.For<IOptions<KeybrAnalyzerOptions>>();
		optionsSubstitute.Value.Returns(options);

		var service = new KeyStatusService(optionsSubstitute);

		// Act & Assert
		service.GetKeyStatus('a').ShouldBe(KeyStatus.Unlocked);
		service.GetKeyStatus('b').ShouldBe(KeyStatus.Unlocked);
		service.GetKeyStatus('c').ShouldBe(KeyStatus.Unlocked);
	}

	[Fact]
	public void GetKeyStatusShouldReturnLockedWhenKeyIsNotOpenedOrFocus()
	{
		// Arrange
		var options = new KeybrAnalyzerOptions();
		options.OpenedKeys.Clear();
		options.FocusKeys.Clear();
		var optionsSubstitute = Substitute.For<IOptions<KeybrAnalyzerOptions>>();
		optionsSubstitute.Value.Returns(options);

		var service = new KeyStatusService(optionsSubstitute);

		// Act & Assert
		service.GetKeyStatus('x').ShouldBe(KeyStatus.Locked);
	}

	[Fact]
	public void GetKeyStatusShouldReturnNoneForWhitespace()
	{
		// Arrange
		var options = new KeybrAnalyzerOptions();
		var optionsSubstitute = Substitute.For<IOptions<KeybrAnalyzerOptions>>();
		optionsSubstitute.Value.Returns(options);

		var service = new KeyStatusService(optionsSubstitute);

		// Act & Assert
		service.GetKeyStatus(' ').ShouldBe(KeyStatus.None);
	}

	[Fact]
	public void GetKeyStatusShouldHandleSpaceSeparatedKeysInOptions()
	{
		// Arrange
		var options = new KeybrAnalyzerOptions();
		options.OpenedKeys.Clear();
		options.OpenedKeys.Add("; : . ,");
		var optionsSubstitute = Substitute.For<IOptions<KeybrAnalyzerOptions>>();
		optionsSubstitute.Value.Returns(options);

		var service = new KeyStatusService(optionsSubstitute);

		// Act & Assert
		service.GetKeyStatus(';').ShouldBe(KeyStatus.Unlocked);
		service.GetKeyStatus(':').ShouldBe(KeyStatus.Unlocked);
		service.GetKeyStatus('.').ShouldBe(KeyStatus.Unlocked);
		service.GetKeyStatus(',').ShouldBe(KeyStatus.Unlocked);
	}

	[Fact]
	public void GetKeyStatusShouldHandleUserReproductionConfig()
	{
		// Arrange
		var options = new KeybrAnalyzerOptions();
		options.OpenedKeys.Clear();
		options.OpenedKeys.Add("abcdefghijklmnopqrstuvwxyz");
		options.OpenedKeys.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
		options.OpenedKeys.Add("; : . ,");
		options.OpenedKeys.Add("_ ' \" ( ) [ ] { }");
		options.OpenedKeys.Add("/ + - = * < >");
		options.OpenedKeys.Add("? ! @ % & | # ~");
		options.OpenedKeys.Add("1 2 3 4 5 7 8 9 0");

		options.FocusKeys.Clear();
		options.FocusKeys.Add("_ - [ ] \" ' * ^ ` $");

		var optionsSubstitute = Substitute.For<IOptions<KeybrAnalyzerOptions>>();
		optionsSubstitute.Value.Returns(options);

		var service = new KeyStatusService(optionsSubstitute);

		// Act & Assert
		service.GetKeyStatus('~').ShouldBe(KeyStatus.Unlocked);
		service.GetKeyStatus('`').ShouldBe(KeyStatus.Focus);
		service.GetKeyStatus('!').ShouldBe(KeyStatus.Unlocked);
		service.GetKeyStatus('1').ShouldBe(KeyStatus.Unlocked);
		service.GetKeyStatus('$').ShouldBe(KeyStatus.Focus);
		service.GetKeyStatus('4').ShouldBe(KeyStatus.Unlocked);
		service.GetKeyStatus('^').ShouldBe(KeyStatus.Focus);
		service.GetKeyStatus('6').ShouldBe(KeyStatus.Locked);
		service.GetKeyStatus('_').ShouldBe(KeyStatus.Focus);
		service.GetKeyStatus('-').ShouldBe(KeyStatus.Focus);
	}
}
