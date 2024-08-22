namespace FamilyCalendar.Entries;

public interface IEntryRepository
{
  Task<Entry?> GetAsync(Guid calendarId, Guid entryId, CancellationToken cancellationToken = default);
  Task<IEnumerable<Entry>> GetAllAsync(Guid calendarId, CancellationToken cancellationToken = default);
  Task<IEnumerable<Entry>> GetByDateRangeAsync(Guid calendarId, DateTimeOffset rangeStart, DateTimeOffset rangeEnd, CancellationToken cancellationToken = default);
  Task CreateAsync(Entry entry, CancellationToken cancellationToken = default);
  Task UpdateAsync(Entry entry, CancellationToken cancellationToken = default);
  Task DeleteAsync(Guid calendarId, Guid entryId, CancellationToken cancellationToken = default);
}
