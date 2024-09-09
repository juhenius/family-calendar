using Ical.Net;
using Ical.Net.DataTypes;

namespace FamilyCalendar.Entries;

public static class EntryRecurrenceExtensions
{
  private static readonly int displayDateSearchRangeInYears = 200;

  public static DateTimeOffset ResolveDisplayStartDate(this Entry self)
  {
    return self.Date;
  }

  public static DateTimeOffset ResolveDisplayEndDate(this Entry self)
  {
    if (self.Recurrence.Count == 0)
    {
      return self.Date;
    }

    var calendar = new Calendar();
    calendar.Events.Add(self.ToCalendarEvent());

    var rangeStart = CalDateTime.Now.AddYears(-displayDateSearchRangeInYears);
    var rangeEnd = CalDateTime.Now.AddYears(displayDateSearchRangeInYears);

    var lastOccurence = calendar.GetOccurrences(rangeStart, rangeEnd)
      .MaxBy(o => o.Period.StartTime);

    return lastOccurence == null ? self.Date : lastOccurence.Period.StartTime.ToDateTimeOffset();
  }
}

