using TcxEditor.Models;

namespace TcxEditor.Commands;

public class AddLapCommand : IUndoableCommand
{
    private readonly TcxActivity _activity;
    private readonly TcxLap _lap;

    public AddLapCommand(TcxActivity activity, TcxLap lap)
    {
        _activity = activity;
        _lap = lap;
    }

    public string Description => "Add lap";
    public void Execute() => _activity.Laps.Add(_lap);
    public void Undo() => _activity.Laps.Remove(_lap);
}
