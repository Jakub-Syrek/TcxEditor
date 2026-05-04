using TcxEditor.Models;

namespace TcxEditor.Builders;

// Builder pattern — fluent construction of TcxLap with sensible defaults
public class TcxLapBuilder
{
    private DateTime _startTime = DateTime.Now;
    private double _totalTimeSeconds;
    private double _distanceMeters;
    private double _maximumSpeed;
    private int _calories;
    private int? _averageHeartRate;
    private int? _maximumHeartRate;
    private string _intensity = "Active";
    private string _notes = "";

    public TcxLapBuilder WithStartTime(DateTime value) { _startTime = value; return this; }
    public TcxLapBuilder WithDuration(double seconds) { _totalTimeSeconds = seconds; return this; }
    public TcxLapBuilder WithDistance(double meters) { _distanceMeters = meters; return this; }
    public TcxLapBuilder WithMaxSpeed(double metersPerSecond) { _maximumSpeed = metersPerSecond; return this; }
    public TcxLapBuilder WithCalories(int value) { _calories = value; return this; }
    public TcxLapBuilder WithAverageHeartRate(int? bpm) { _averageHeartRate = bpm; return this; }
    public TcxLapBuilder WithMaxHeartRate(int? bpm) { _maximumHeartRate = bpm; return this; }
    public TcxLapBuilder WithIntensity(string value) { _intensity = value; return this; }
    public TcxLapBuilder WithNotes(string value) { _notes = value; return this; }

    public TcxLap Build() => new()
    {
        StartTime = _startTime,
        TotalTimeSeconds = _totalTimeSeconds,
        DistanceMeters = _distanceMeters,
        MaximumSpeed = _maximumSpeed,
        Calories = _calories,
        AverageHeartRate = _averageHeartRate,
        MaximumHeartRate = _maximumHeartRate,
        Intensity = _intensity,
        Notes = _notes
    };
}
