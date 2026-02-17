using System.Text.Json.Serialization;

namespace KeybrAnalyzer.Models;

public sealed record HistogramEntry(
	[property: JsonPropertyName("codePoint")] int CodePoint,
	[property: JsonPropertyName("hitCount")] int HitCount,
	[property: JsonPropertyName("missCount")] int MissCount,
	[property: JsonPropertyName("timeToType")] double TimeToType);
