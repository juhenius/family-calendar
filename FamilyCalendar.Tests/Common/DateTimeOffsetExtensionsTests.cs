using FamilyCalendar.Common;

namespace FamilyCalendar.Tests.Common;

public class DateTimeOffsetExtensionsTests
{
  [Fact]
  public void InTimeZone()
  {
    var time = new DateTimeOffset(2023, 5, 20, 14, 5, 0, TimeSpan.Zero);
    var timeZone = "Europe/Helsinki";

    var result = time.InTimeZone(timeZone);

    Assert.Equal(2023, result.Year);
    Assert.Equal(17, result.Hour);
    Assert.Equal(3, result.Offset.Hours);
  }
}