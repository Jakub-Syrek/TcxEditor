using TcxEditor.Models;

namespace TcxEditor.Commands;

public class ShiftTimeCommand : IUndoableCommand
{
    private readonly TcxActivity _activity;
    private readonly TimeSpan _offset;

    public ShiftTimeCommand(TcxActivity activity, TimeSpan offset)
    {
        _activity = activity;
        _offset = offset;
        Description = BuildDescription(offset);
    }

    public string Description { get; }

    public void Execute() => ApplyShift(_offset);
    public void Undo() => ApplyShift(-_offset);

    private void ApplyShift(TimeSpan offset)
    {
        _activity.StartTime += offset;
        foreach (var lap in _activity.Laps)
        {
            lap.StartTime += offset;
            foreach (var tp in lap.Trackpoints)
                tp.Time += offset;
        }
    }

    private static string BuildDescription(TimeSpan offset)
    {
        var abs = offset.Duration();
        var direction = offset < TimeSpan.Zero ? "back" : "forward";
        return $"Shift time {direction} {abs.Hours:D2}:{abs.Minutes:D2}:{abs.Seconds:D2}";
    }
}
