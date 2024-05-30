

using System.Text.Json.Serialization;

namespace FamilyCalendar.Entries;

public class EntryDto
{
  public const string SkPrefix = "ENTRY#";

  [JsonPropertyName("pk")]
  public string Pk { get; init; } = default!;

  [JsonPropertyName("sk")]
  public string Sk { get; init; } = default!;

  [JsonPropertyName("id")]
  public Guid Id { get; init; } = default!;

  [JsonPropertyName("calendarId")]
  public Guid CalendarId { get; init; } = default!;

  [JsonPropertyName("title")]
  public string Title { get; init; } = default!;

  [JsonPropertyName("member")]
  public string Member { get; init; } = default!;

  [JsonPropertyName("date")]
  public DateTime Date { get; init; }

  [JsonPropertyName("updatedAt")]
  public DateTime UpdatedAt { get; set; }
}
