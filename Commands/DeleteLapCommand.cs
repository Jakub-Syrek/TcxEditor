using TcxEditor.Models;

namespace TcxEditor.Commands;

public class DeleteLapCommand : IUndoableCommand
{
    private readonly TcxActivity _activity;
    private readonly TcxLap _lap;
    private int _index;

    public DeleteLapCommand(TcxActivity activity, TcxLap lap)
    {
        _activity = activity;
        _lap = lap;
    }

    public string Description => "Delete lap";

    public void Execute()
    {
        _index = _activity.Laps.IndexOf(_lap);
        _activity.Laps.Remove(_lap);
    }

    public void Undo() => _activity.Laps.Insert(_index, _lap);
}
