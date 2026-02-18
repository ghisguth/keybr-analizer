using KeybrAnalyzer.Helpers;
using KeybrAnalyzer.Options;

using Microsoft.Extensions.Options;

namespace KeybrAnalyzer.Services.Reporting;

public class KeyboardLayoutReportingService(
	IConsoleHelper consoleHelper,
	IOptions<KeybrAnalyzerOptions> options)
	: IKeyboardLayoutReportingService
{
	public void PrintKeyboardLayout(KeyboardMode mode)
	{
		var title = mode switch
		{
			KeyboardMode.Finger => "VISUAL PROGRESS BY FINGER",
			KeyboardMode.KeyType => "VISUAL PROGRESS BY KEY TYPE",
			KeyboardMode.Status => "VISUAL PROGRESS BY STATUS (Unlocked/Focus/Locked)",
			_ => throw new ArgumentOutOfRangeException(nameof(mode))
		};
		consoleHelper.WriteTitle(title);

		var openedKeys = new HashSet<char>(options.Value.OpenedKeys.SelectMany(GetKeysFromCollection));
		var focusKeys = new HashSet<char>(options.Value.FocusKeys.SelectMany(GetKeysFromCollection));
		var lockedKeys = new HashSet<char>(options.Value.LockedKeys.Values.SelectMany(GetKeysFromCollection));

		foreach (var (shifted, normal) in KeyboardLayoutData.Layout)
		{
			for (var i = 0; i < shifted.Length; i++)
			{
				consoleHelper.Write(FormatKey(shifted[i], openedKeys, focusKeys, lockedKeys, mode));
			}

			consoleHelper.WriteLine();

			for (var i = 0; i < normal.Length; i++)
			{
				consoleHelper.Write(FormatKey(normal[i], openedKeys, focusKeys, lockedKeys, mode));
			}

			consoleHelper.WriteLine();
		}

		var spaceKey = FormatKey("                         SPACE                         ", openedKeys, focusKeys, lockedKeys, mode);
		consoleHelper.WriteLine($"            {spaceKey}");
		consoleHelper.WriteLine();

		PrintLegend(mode);
	}

	private static bool IsSpecialKey(string key)
	{
		var trimmed = key.Trim();
		return trimmed == "SPACE" || trimmed == "TAB" || trimmed == "CAPS" || trimmed == "ENTER" || trimmed == "BACKSPACE" || trimmed.Contains("SHIFT", StringComparison.Ordinal);
	}

	private static string GetFingerColor(char c) => c switch
	{
		'~' or '`' or '1' or '!' or 'q' or 'Q' or 'a' or 'A' or 'z' or 'Z' => Ansi.BgBlue,
		'2' or '@' or 'w' or 'W' or 's' or 'S' or 'x' or 'X' => Ansi.BgYellow,
		'3' or '#' or 'e' or 'E' or 'd' or 'D' or 'c' or 'C' => Ansi.BgRed,
		'4' or '$' or '5' or '%' or 'r' or 'R' or 't' or 'T' or 'f' or 'F' or 'g' or 'G' or 'v' or 'V' or 'b' or 'B' => Ansi.BgGreen,
		' ' => Ansi.BgMagenta,
		'6' or '^' or '7' or '&' or 'y' or 'Y' or 'u' or 'U' or 'h' or 'H' or 'j' or 'J' or 'n' or 'N' or 'm' or 'M' => Ansi.BgCyan,
		'8' or '*' or 'i' or 'I' or 'k' or 'K' or ',' or '<' => Ansi.BgRed,
		'9' or '(' or 'o' or 'O' or 'l' or 'L' or '.' or '>' => Ansi.BgYellow,
		'0' or ')' or '-' or '_' or '=' or '+' or 'p' or 'P' or '[' or '{' or ']' or '}' or '\\' or '|' or ';' or ':' or '\'' or '\"' or '/' or '?' => Ansi.BgBlue,
		_ => string.Empty
	};

	private static string GetKeyTypeColorBg(char c)
	{
		if (char.IsLower(c) || char.IsUpper(c))
		{
			return char.IsLower(c) ? Ansi.BgBlue : Ansi.BgCyan;
		}

		if (char.IsDigit(c))
		{
			return Ansi.BgYellow;
		}

		if (",.?\"'-:;".Contains(c, StringComparison.Ordinal))
		{
			return Ansi.BgRed;
		}

		return Ansi.BgMagenta;
	}

	private static IEnumerable<char> GetKeysFromCollection(IEnumerable<string> collection)
	{
		return collection.SelectMany(GetKeysFromCollection);
	}

	private static IEnumerable<char> GetKeysFromCollection(string s)
	{
		return s.Contains(' ', StringComparison.Ordinal) ? s.Split(' ', StringSplitOptions.RemoveEmptyEntries).SelectMany(p => p) : s;
	}

	private static string FormatSingleChar(char c, HashSet<char> opened, HashSet<char> focus, HashSet<char> locked, KeyboardMode mode)
	{
		var isFocus = focus.Contains(c);
		var isOpened = opened.Contains(c) || isFocus;
		var isLocked = locked.Contains(c) || (!isOpened && !char.IsWhiteSpace(c));

		var bg = mode switch
		{
			KeyboardMode.Status => isFocus ? Ansi.BgYellow : (isOpened ? Ansi.BgGreen : (isLocked ? Ansi.BgRed : Ansi.BgWhite)),
			KeyboardMode.Finger => GetFingerColor(c),
			KeyboardMode.KeyType => GetKeyTypeColorBg(c),
			_ => Ansi.BgWhite
		};

		if (string.IsNullOrEmpty(bg))
		{
			bg = Ansi.BgWhite;
		}

		var fg = Ansi.Black;
		if (mode != KeyboardMode.Status && isLocked)
		{
			fg = Ansi.Dim + Ansi.Black;
		}

		var style = isOpened ? Ansi.Bold : string.Empty;
		return $"{bg}{fg}{style} {c} {Ansi.Reset}";
	}

	private static string FormatKey(string key, HashSet<char> opened, HashSet<char> focus, HashSet<char> locked, KeyboardMode mode)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return key;
		}

		var trimmed = key.Trim();
		if (trimmed.Length == 1 && !IsSpecialKey(trimmed))
		{
			return FormatSingleChar(trimmed[0], opened, focus, locked, mode);
		}

		if (trimmed.StartsWith('[') || trimmed.Contains("SPACE", StringComparison.Ordinal))
		{
			return $"{Ansi.Bold}{key}{Ansi.Reset}";
		}

		return key;
	}

	private void PrintLegend(KeyboardMode mode)
	{
		if (mode == KeyboardMode.Finger)
		{
			consoleHelper.WriteLine($"{Ansi.Bold}Finger Legend:{Ansi.Reset} {Ansi.BgBlue}   {Ansi.Reset} Pinky | {Ansi.BgYellow}   {Ansi.Reset} Ring | {Ansi.BgRed}   {Ansi.Reset} Middle | {Ansi.BgGreen}   {Ansi.Reset} L_Index | {Ansi.BgCyan}   {Ansi.Reset} R_Index | {Ansi.BgMagenta}   {Ansi.Reset} Thumb | {Ansi.Bold}Unlocked:{Ansi.Reset} Bold text");
		}
		else if (mode == KeyboardMode.KeyType)
		{
			consoleHelper.WriteLine($"{Ansi.Bold}Type Legend:{Ansi.Reset} {Ansi.BgBlue}   {Ansi.Reset} Lower | {Ansi.BgCyan}   {Ansi.Reset} Upper | {Ansi.BgYellow}   {Ansi.Reset} Number | {Ansi.BgRed}   {Ansi.Reset} Punct | {Ansi.BgMagenta}   {Ansi.Reset} Symbol | {Ansi.Bold}Unlocked:{Ansi.Reset} Bold text");
		}
		else
		{
			consoleHelper.WriteLine($"{Ansi.Bold}Status Legend:{Ansi.Reset} {Ansi.BgGreen}   {Ansi.Reset} Unlocked | {Ansi.BgYellow}   {Ansi.Reset} Focus | {Ansi.BgRed}   {Ansi.Reset} Locked");
		}

		consoleHelper.WriteLine();
	}
}
