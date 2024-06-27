namespace FamilyCalendar.Entries;

public interface IEntryParser
{
  Task<Entry> ParseFromString(string input, Guid calendarId, CancellationToken cancellationToken = default);
}
