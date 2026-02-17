namespace KeybrAnalyzer.Services.Reporting;

internal static class KeyboardLayoutData
{
	public const string EmptyLabel = "           ";
	public const string BackLabel = " [ BACKSPACE ] ";
	public const string EnterLabel = " [  ENTER  ] ";
	public const string RShiftLabel = " [ R_SHIFT ] ";
	public const string Off1 = "       ";

	public static readonly (string[] Shifted, string[] Normal)[] Layout =
	[
		([Off1, " ~", " !", " @", " #", " $", " %", " ^", " &", " *", " (", " )", " _", " +", BackLabel],
		 [Off1, " `", " 1", " 2", " 3", " 4", " 5", " 6", " 7", " 8", " 9", " 0", " -", " =", " "]),

		([" [  TAB  ] ", " Q", " W", " E", " R", " T", " Y", " U", " I", " O", " P", " {", " }", " |"],
		 [EmptyLabel, " q", " w", " e", " r", " t", " y", " u", " i", " o", " p", " [", " ]", " \\"]),

		([" [  CAPS  ] ", " A", " S", " D", " F", " G", " H", " J", " K", " L", " :", " \"", EnterLabel],
		 [EmptyLabel + " ", " a", " s", " d", " f", " g", " h", " j", " k", " l", " ;", " '", " "]),

		([" [ L_SHIFT ] ", " Z", " X", " C", " V", " B", " N", " M", " <", " >", " ?", RShiftLabel],
		 [EmptyLabel + "  ", " z", " x", " c", " v", " b", " n", " m", " ,", " .", " /", " "])
	];
}
