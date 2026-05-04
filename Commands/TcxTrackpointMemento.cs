using TcxEditor.Models;

namespace TcxEditor.Commands;

// Memento pattern — snapshot of TcxTrackpoint state for undo/redo
public record TcxTrackpointMemento(
    DateTime Time,
    double? LatitudeDegrees,
    double? LongitudeDegrees,
    double? AltitudeMeters,
    double? DistanceMeters,
    int? HeartRateBpm,
    int? Cadence,
    double? Speed)
{
    public static TcxTrackpointMemento Capture(TcxTrackpoint tp) => new(
        tp.Time, tp.LatitudeDegrees, tp.LongitudeDegrees, tp.AltitudeMeters,
        tp.DistanceMeters, tp.HeartRateBpm, tp.Cadence, tp.Speed);

    public void RestoreTo(TcxTrackpoint tp)
    {
        tp.Time = Time;
        tp.LatitudeDegrees = LatitudeDegrees;
        tp.LongitudeDegrees = LongitudeDegrees;
        tp.AltitudeMeters = AltitudeMeters;
        tp.DistanceMeters = DistanceMeters;
        tp.HeartRateBpm = HeartRateBpm;
        tp.Cadence = Cadence;
        tp.Speed = Speed;
    }

    public TcxTrackpoint ToTrackpoint()
    {
        var tp = new TcxTrackpoint();
        RestoreTo(tp);
        return tp;
    }
}
