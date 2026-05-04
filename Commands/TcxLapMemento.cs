using TcxEditor.Models;

namespace TcxEditor.Commands;

// Memento pattern — snapshot of TcxLap state for undo/redo
public record TcxLapMemento(
    DateTime StartTime,
    double TotalTimeSeconds,
    double DistanceMeters,
    double MaximumSpeed,
    int Calories,
    int? AverageHeartRate,
    int? MaximumHeartRate,
    string Intensity,
    string Notes)
{
    public static TcxLapMemento Capture(TcxLap lap) => new(
        lap.StartTime, lap.TotalTimeSeconds, lap.DistanceMeters,
        lap.MaximumSpeed, lap.Calories, lap.AverageHeartRate,
        lap.MaximumHeartRate, lap.Intensity, lap.Notes);

    public void RestoreTo(TcxLap lap)
    {
        lap.StartTime = StartTime;
        lap.TotalTimeSeconds = TotalTimeSeconds;
        lap.DistanceMeters = DistanceMeters;
        lap.MaximumSpeed = MaximumSpeed;
        lap.Calories = Calories;
        lap.AverageHeartRate = AverageHeartRate;
        lap.MaximumHeartRate = MaximumHeartRate;
        lap.Intensity = Intensity;
        lap.Notes = Notes;
    }

    public TcxLap ToLap()
    {
        var lap = new TcxLap();
        RestoreTo(lap);
        return lap;
    }
}
