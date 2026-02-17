namespace KeybrAnalyzer.Services;

public static class KeyMetricCalculator
{
	public static double CalculateMastery(double latency, double errorRate, double cv, double csharpWeight)
	{
		return (latency > 0) ? 1000.0 / (latency * (1 + (errorRate / 100.0)) * (1 + cv) * csharpWeight) : 0;
	}

	public static double GetCSharpWeight(int codePoint)
	{
		return (char)codePoint switch
		{
			'{' or '}' or '(' or ')' or '[' or ']' => 2.0,
			';' or '.' or ',' or '_' or '\"' or '\'' => 1.5,
			'<' or '>' or '=' or '+' or '-' or '*' or '/' or '&' or '|' or '!' => 1.3,
			_ => 1.0
		};
	}
}
