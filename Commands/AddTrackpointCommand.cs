using TcxEditor.Models;

namespace TcxEditor.Commands;

public class AddTrackpointCommand : IUndoableCommand
{
    private readonly TcxLap _lap;
    private readonly TcxTrackpoint _trackpoint;

    public AddTrackpointCommand(TcxLap lap, TcxTrackpoint trackpoint)
    {
        _lap = lap;
        _trackpoint = trackpoint;
    }

    public string Description => "Add trackpoint";
    public void Execute() => _lap.Trackpoints.Add(_trackpoint);
    public void Undo() => _lap.Trackpoints.Remove(_trackpoint);
}
