namespace KeybrAnalyzer.Models;

public sealed class KeyPerformance
{
	public int CodePoint { get; set; }
	public string Key { get; set; } = string.Empty;
	public int AllH { get; set; }
	public int AllM { get; set; }
	public double Latency { get; set; }
	public double AllErr { get; set; }
	public double AllWpm { get; set; }

	// Aggregated periods
	public int L7H { get; set; }
	public int L7M { get; set; }
	public double L7Latency { get; set; }
	public double L7Err { get; set; }
	public double L7Wpm { get; set; }
	public double L7CV { get; set; }
	public int L3H { get; set; }
	public int L3M { get; set; }
	public double L3Err { get; set; }
	public double L3Wpm { get; set; }
	public int L1H { get; set; }
	public int L1M { get; set; }
	public double L1Err { get; set; }
	public double L1Wpm { get; set; }

	// Individual days (D1 = Latest, D2 = D1-1, D3 = D1-2)
	public int D1H { get; set; }
	public int D1M { get; set; }
	public double D1Err { get; set; }
	public double D1Wpm { get; set; }

	public int D2H { get; set; }
	public int D2M { get; set; }
	public double D2Err { get; set; }
	public double D2Wpm { get; set; }

	public int D3H { get; set; }
	public int D3M { get; set; }
	public double D3Err { get; set; }
	public double D3Wpm { get; set; }

	public IReadOnlyList<double> DailyWpm { get; set; } = Array.Empty<double>();

	// Comparison/Improvement
	public double Improvement { get; set; }
	public double ImprovementYesterday { get; set; }

	public double CV { get; set; }
	public double Mastery { get; set; }
	public double L7Impact { get; set; }

	public double P50 { get; set; }
	public double P95 { get; set; }
	public double StallRatio { get; set; }
}
