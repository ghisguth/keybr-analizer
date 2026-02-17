namespace KeybrAnalyzer.Models;

public sealed record AccuracyStreak(
	double Threshold,
	int Lessons,
	int Characters,
	double TopSpeed,
	double AverageSpeed,
	DateTime StartDate,
	bool IsMax = false,
	bool IsCurrent = false);
