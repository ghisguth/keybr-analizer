using KeybrAnalyzer.Options;

using Microsoft.Extensions.Options;

namespace KeybrAnalyzer.Services.Reporting;

public class KeyStatusService : IKeyStatusService
{
	private readonly HashSet<char> _unlockedKeys;
	private readonly HashSet<char> _focusKeys;
	private readonly HashSet<char> _lockedKeys;

	public KeyStatusService(IOptions<KeybrAnalyzerOptions> options)
	{
		ArgumentNullException.ThrowIfNull(options);

		_unlockedKeys = [.. options.Value.OpenedKeys.SelectMany(GetKeysFromCollection)];
		_focusKeys = [.. options.Value.FocusKeys.SelectMany(GetKeysFromCollection)];
		_lockedKeys = [.. options.Value.LockedKeys.Values.SelectMany(c => c.SelectMany(GetKeysFromCollection))];
	}

	public KeyStatus GetKeyStatus(char c)
	{
		if (char.IsWhiteSpace(c))
		{
			return KeyStatus.None;
		}

		if (_focusKeys.Contains(c))
		{
			return KeyStatus.Focus;
		}

		if (_unlockedKeys.Contains(c))
		{
			return KeyStatus.Unlocked;
		}

		// Any key not explicitly unlocked or in focus is considered locked,
		// plus keys explicitly in the locked collection.
		return KeyStatus.Locked;
	}

	public IEnumerable<char> GetUnlockedKeys() => _unlockedKeys;

	public IEnumerable<char> GetFocusKeys() => _focusKeys;

	public IEnumerable<char> GetLockedKeys() => _lockedKeys;

	private static IEnumerable<char> GetKeysFromCollection(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return [];
		}

		return s.Contains(' ', StringComparison.Ordinal)
			? s.Split(' ', StringSplitOptions.RemoveEmptyEntries).SelectMany(p => p)
			: s;
	}
}
