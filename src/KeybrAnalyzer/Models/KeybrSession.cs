using System.Text.Json.Serialization;

namespace KeybrAnalyzer.Models;

public sealed record KeybrSession(
	[property: JsonPropertyName("timeStamp")] DateTime TimeStamp,
	[property: JsonPropertyName("speed")] double Speed,
	[property: JsonPropertyName("errors")] int Errors,
	[property: JsonPropertyName("length")] int Length,
	[property: JsonPropertyName("time")] int Time,
	[property: JsonPropertyName("textType")] string TextType,
	[property: JsonPropertyName("histogram")] IReadOnlyList<HistogramEntry> Histogram);
