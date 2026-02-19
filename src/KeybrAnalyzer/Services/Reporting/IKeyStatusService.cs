namespace KeybrAnalyzer.Services.Reporting;

public interface IKeyStatusService
{
	KeyStatus GetKeyStatus(char c);

	IEnumerable<char> GetUnlockedKeys();

	IEnumerable<char> GetFocusKeys();

	IEnumerable<char> GetLockedKeys();
}
