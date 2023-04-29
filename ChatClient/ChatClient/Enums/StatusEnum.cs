using System.Windows.Media;

namespace ChatClient.Enums;

public enum State
{
    Offline = 0,
    Online = 1,
    Busy = 2,
    Invisible = 3,
}
public static class StatusEnumHandler
{
    public static SolidColorBrush GetStatusColor(State status) => status switch
    {
        (State)0 => new SolidColorBrush(Colors.Gray),
        (State)1 => new SolidColorBrush(Colors.Green),
        (State)2 => new SolidColorBrush(Colors.IndianRed),
        (State)3 => new SolidColorBrush(Colors.Gray),
        _ => new SolidColorBrush(Colors.Gray),
    };
}
