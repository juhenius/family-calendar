namespace FamilyCalendar.Entries;

public interface IEntryRepository
{
  Task<Entry?> GetAsync(Guid calendarId, Guid entryId, CancellationToken cancellationToken = default);
  Task<IEnumerable<Entry>> GetAllAsync(Guid calendarId, CancellationToken cancellationToken = default);
  Task<IEnumerable<Entry>> GetByDateRangeAsync(Guid calendarId, DateTime rangeStart, DateTime rangeEnd, CancellationToken cancellationToken = default);
  Task<bool> CreateAsync(Entry entry, CancellationToken cancellationToken = default);
  Task<bool> UpdateAsync(Entry entry, CancellationToken cancellationToken = default);
  Task<bool> DeleteAsync(Guid calendarId, Guid entryId, CancellationToken cancellationToken = default);
}
