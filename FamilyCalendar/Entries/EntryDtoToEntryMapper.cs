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
      Date = source.Date,
      Location = source.Location,
      Participants = source.Participants,
    };
  }
}
