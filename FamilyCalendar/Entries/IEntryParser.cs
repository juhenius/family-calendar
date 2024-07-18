namespace FamilyCalendar.Entries;

public interface IEntryParser
{
  Task<Entry> ParseFromString(string input, Guid calendarId, Guid entryId, DateTimeOffset now, CancellationToken cancellationToken = default);
  Task<Entry> ParseFromString(string input, Guid calendarId, Guid entryId, CancellationToken cancellationToken = default);
}
