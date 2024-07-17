

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

  [JsonPropertyName("date")]
  public DateTimeOffset Date { get; init; }

  [JsonPropertyName("location")]
  public string? Location { get; init; } = default;

  [JsonPropertyName("participants")]
  public List<string> Participants { get; init; } = default!;

  [JsonPropertyName("prompt")]
  public required string Prompt { get; init; }

  [JsonPropertyName("createdAt")]
  public required DateTimeOffset CreatedAt { get; init; }

  [JsonPropertyName("updatedAt")]
  public DateTimeOffset UpdatedAt { get; set; }
}
