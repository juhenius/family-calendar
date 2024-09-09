using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace FamilyCalendar.Entries;

public static class CalendarEventExtensions
{
  public static CalendarEvent ToCalendarEvent(this Entry self)
  {
    var result = new CalendarEvent()
    {
      Start = self.Date.ToIDateTime(),
      End = self.Date.Add(TimeSpan.FromHours(1)).ToIDateTime(),
      RecurrenceRules = self.Recurrence.Select(rule => new RecurrencePattern(rule)).ToList(),
    };

    return result;
  }
}
