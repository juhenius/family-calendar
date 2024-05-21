namespace FamilyCalendar.Entries;

public static class EntryToEntryDtoMapper
{
  public static EntryDto ToEntryDto(this Entry source)
  {
    return new EntryDto
    {
      Id = source.Id,
      Title = source.Title,
      Date = source.Date,
    };
  }
}
