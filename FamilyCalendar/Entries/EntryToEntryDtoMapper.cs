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
      Member = source.Member,
    };
  }
}
