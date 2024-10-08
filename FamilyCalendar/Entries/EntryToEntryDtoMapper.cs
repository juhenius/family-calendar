namespace FamilyCalendar.Entries;

public static class EntryToEntryDtoMapper
{
  public static string ToEntrySk(this Guid source)
  {
    return $"{EntryDto.SkPrefix}{source}";
  }

  public static EntryDto ToEntryDto(this Entry source)
  {
    return new EntryDto
    {
      Pk = source.CalendarId.ToString(),
      Sk = source.Id.ToEntrySk(),
      Id = source.Id,
      CalendarId = source.CalendarId,
      Title = source.Title,
      Date = source.Date.ToUniversalTime(),
      Location = source.Location,
      Participants = source.Participants,
      Recurrence = source.Recurrence,
      Prompt = source.Prompt,
      CreatedAt = source.CreatedAt.ToUniversalTime(),
      TimeZone = source.TimeZone,
      DisplayStartDate = source.ResolveDisplayStartDate(),
      DisplayEndDate = source.ResolveDisplayEndDate(),
    };
  }
}
