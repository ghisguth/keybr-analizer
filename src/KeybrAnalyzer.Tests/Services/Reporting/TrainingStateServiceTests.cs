using KeybrAnalyzer.Options;
using KeybrAnalyzer.Services.Reporting;

using Microsoft.Extensions.Options;

using NSubstitute;

using Shouldly;

namespace KeybrAnalyzer.Tests.Services.Reporting;

public class TrainingStateServiceTests
{
	[Fact]
	public void GetTrainingStateShouldMaintainGroupStructure()
	{
		// Arrange
		var options = new KeybrAnalyzerOptions();
		options.OpenedKeys.Clear();
		options.OpenedKeys.Add("abc");
		options.OpenedKeys.Add("def");

		var keyStatusService = Substitute.For<IKeyStatusService>();
		keyStatusService.GetKeyStatus(Arg.Any<char>()).Returns(KeyStatus.Unlocked);

		var optionsSubstitute = Substitute.For<IOptions<KeybrAnalyzerOptions>>();
		optionsSubstitute.Value.Returns(options);

		var service = new TrainingStateService(optionsSubstitute, keyStatusService);

		// Act
		var state = service.GetTrainingState();

		// Assert
		state.UnlockedGroups.Count.ShouldBe(2);
		state.UnlockedGroups[0].Keys.Count.ShouldBe(3);
		state.UnlockedGroups[0].Keys[0].Key.ShouldBe('a');
		state.UnlockedGroups[1].Keys[0].Key.ShouldBe('d');
	}

	[Fact]
	public void GetTrainingStateShouldReflectStatusFromKeyStatusService()
	{
		// Arrange
		var options = new KeybrAnalyzerOptions();
		options.OpenedKeys.Clear();
		options.OpenedKeys.Add("a b");

		var keyStatusService = Substitute.For<IKeyStatusService>();
		keyStatusService.GetKeyStatus('a').Returns(KeyStatus.Focus);
		keyStatusService.GetKeyStatus('b').Returns(KeyStatus.Unlocked);
		keyStatusService.GetKeyStatus(' ').Returns(KeyStatus.None);

		var optionsSubstitute = Substitute.For<IOptions<KeybrAnalyzerOptions>>();
		optionsSubstitute.Value.Returns(options);

		var service = new TrainingStateService(optionsSubstitute, keyStatusService);

		// Act
		var state = service.GetTrainingState();

		// Assert
		var keys = state.UnlockedGroups[0].Keys;
		keys[0].Key.ShouldBe('a');
		keys[0].Status.ShouldBe(KeyStatus.Focus);
		keys[1].Key.ShouldBe(' ');
		keys[1].Status.ShouldBe(KeyStatus.None);
		keys[2].Key.ShouldBe('b');
		keys[2].Status.ShouldBe(KeyStatus.Unlocked);
	}
}
