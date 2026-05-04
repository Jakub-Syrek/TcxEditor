using TcxEditor.Models;

namespace TcxEditor.Commands;

public class DeleteTrackpointCommand : IUndoableCommand
{
    private readonly TcxLap _lap;
    private readonly TcxTrackpoint _trackpoint;
    private int _index;

    public DeleteTrackpointCommand(TcxLap lap, TcxTrackpoint trackpoint)
    {
        _lap = lap;
        _trackpoint = trackpoint;
    }

    public string Description => "Delete trackpoint";

    public void Execute()
    {
        _index = _lap.Trackpoints.IndexOf(_trackpoint);
        _lap.Trackpoints.Remove(_trackpoint);
    }

    public void Undo() => _lap.Trackpoints.Insert(_index, _trackpoint);
}
