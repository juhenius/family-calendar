

using System.Text.Json.Serialization;

namespace FamilyCalendar.Entries;

public class EntryDto
{
  [JsonPropertyName("pk")]
  public string Pk => Id.ToString();
  [JsonPropertyName("sk")]
  public string Sk => Id.ToString();
  [JsonPropertyName("id")]
  public Guid Id { get; init; } = default!;
  [JsonPropertyName("title")]
  public string Title { get; init; } = default!;
  [JsonPropertyName("date")]
  public DateTime Date { get; init; }
  [JsonPropertyName("updatedAt")]
  public DateTime UpdatedAt { get; set; }
}
