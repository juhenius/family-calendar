namespace FamilyCalendar.Entries;

public static class EntryDtoToEntryMapper
{
  public static Entry ToEntry(this EntryDto source)
  {
    return new Entry
    {
      Id = source.Id,
      CalendarId = source.CalendarId,
      Title = source.Title,
      Member = source.Member,
      Date = source.Date,
    };
  }
}
