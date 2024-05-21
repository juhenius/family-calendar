namespace FamilyCalendar.Entries;

public interface IEntryRepository
{
  Task<EntryDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
  Task<IEnumerable<EntryDto>> GetAllAsync(CancellationToken cancellationToken = default);
  Task<bool> CreateAsync(EntryDto entry, CancellationToken cancellationToken = default);
  Task<bool> UpdateAsync(EntryDto entry, CancellationToken cancellationToken = default);
  Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
