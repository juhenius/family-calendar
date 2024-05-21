namespace FamilyCalendar.Entries;

public static class EntryDtoToEntryMapper
{
  public static Entry ToEntry(this EntryDto source)
  {
    return new Entry
    {
      Id = source.Id,
      Title = source.Title,
      Date = source.Date,
    };
  }
}
