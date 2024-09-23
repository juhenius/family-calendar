using System.Globalization;

namespace FamilyCalendar.Common;

public static class DateTimeExtensions
{
  public static string ToIsoString(this DateTime self)
  {
    return self.ToString("o", CultureInfo.InvariantCulture);
  }
}
