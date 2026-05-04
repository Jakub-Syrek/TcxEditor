using TcxEditor.Models;

namespace TcxEditor.Commands;

public class AutoTimeCommand : IUndoableCommand
{
    private readonly TcxLap _lap;
    private readonly List<TcxTrackpointMemento> _before;

    public AutoTimeCommand(TcxLap lap)
    {
        _lap = lap;
        _before = lap.Trackpoints.Select(TcxTrackpointMemento.Capture).ToList();
    }

    public string Description => "Auto-distribute trackpoint times";

    public void Execute()
    {
        var points = _lap.Trackpoints;
        var totalDist = points.LastOrDefault()?.DistanceMeters ?? _lap.DistanceMeters;
        if (totalDist <= 0) return;

        for (int i = 0; i < points.Count; i++)
            points[i].Time = _lap.StartTime.AddSeconds(
                ((points[i].DistanceMeters ?? 0) / totalDist) * _lap.TotalTimeSeconds);
    }

    public void Undo()
    {
        for (int i = 0; i < _before.Count && i < _lap.Trackpoints.Count; i++)
            _before[i].RestoreTo(_lap.Trackpoints[i]);
    }
}
