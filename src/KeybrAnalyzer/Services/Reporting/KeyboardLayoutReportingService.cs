using KeybrAnalyzer.Helpers;

namespace KeybrAnalyzer.Services.Reporting;

public class KeyboardLayoutReportingService(
	IConsoleHelper consoleHelper,
	IKeyStatusService keyStatusService)
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

		foreach (var (shifted, normal) in KeyboardLayoutData.Layout)
		{
			for (var i = 0; i < shifted.Length; i++)
			{
				consoleHelper.Write(FormatKey(shifted[i], mode));
			}

			consoleHelper.WriteLine();

			for (var i = 0; i < normal.Length; i++)
			{
				consoleHelper.Write(FormatKey(normal[i], mode));
			}

			consoleHelper.WriteLine();
		}

		var spaceKey = FormatKey("                         SPACE                         ", mode);
		consoleHelper.WriteLine($"            {spaceKey}");
		consoleHelper.WriteLine();

		PrintLegend(mode);
	}

	private static bool IsSpecialKey(string key)
	{
		var trimmed = key.Trim();
		return trimmed == "SPACE" || trimmed == "TAB" || trimmed == "CAPS" || trimmed == "ENTER" || trimmed == "BACKSPACE" || trimmed.Contains("SHIFT", StringComparison.Ordinal);
	}

	private static string GetFingerColor(char c)
	{
		return c switch
		{
			_ when "~`1!qQaAzZ".Contains(c, StringComparison.Ordinal) => Ansi.BgBlue,
			_ when "2@wWsSxX".Contains(c, StringComparison.Ordinal) => Ansi.BgYellow,
			_ when "3#eEdDcC".Contains(c, StringComparison.Ordinal) => Ansi.BgRed,
			_ when "4$5%rRtTfFgGvVbB".Contains(c, StringComparison.Ordinal) => Ansi.BgGreen,
			' ' => Ansi.BgMagenta,
			_ when "6^7&yYuUhHjJnNmM".Contains(c, StringComparison.Ordinal) => Ansi.BgCyan,
			_ when "8*iIkK,<".Contains(c, StringComparison.Ordinal) => Ansi.BgRed,
			_ when "9(oOlL.>".Contains(c, StringComparison.Ordinal) => Ansi.BgYellow,
			_ when "0)-_=+pP[{]}\\|;:'\"/?".Contains(c, StringComparison.Ordinal) => Ansi.BgBlue,
			_ => string.Empty
		};
	}

	private static string GetKeyTypeColorBg(char c)
	{
		if (char.IsLetter(c))
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

	private string FormatSingleChar(char c, KeyboardMode mode)
	{
		var status = keyStatusService.GetKeyStatus(c);
		var isFocus = status is KeyStatus.Focus;
		var isUnlocked = status is KeyStatus.Unlocked or KeyStatus.Focus;
		var isLocked = status is KeyStatus.Locked;

		var bg = mode switch
		{
			KeyboardMode.Status => isFocus ? Ansi.BgYellow : (isUnlocked ? Ansi.BgGreen : (isLocked ? Ansi.BgRed : Ansi.BgWhite)),
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

		var style = isUnlocked ? Ansi.Bold : string.Empty;
		return $"{bg}{fg}{style} {c} {Ansi.Reset}";
	}

	private string FormatKey(string key, KeyboardMode mode)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return key;
		}

		var trimmed = key.Trim();
		if (trimmed.Length == 1 && !IsSpecialKey(trimmed))
		{
			return FormatSingleChar(trimmed[0], mode);
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
