using Garth.Enums;

namespace Garth.Helpers;

public class CalEvent
{
    public string Name { get; set; }
    public string Summary { get; set; }
    public string Class { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public EventType Type { get; set; }
}