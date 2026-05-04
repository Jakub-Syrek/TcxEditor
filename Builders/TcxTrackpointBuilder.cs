using TcxEditor.Models;

namespace TcxEditor.Builders;

// Builder pattern — fluent construction of TcxTrackpoint with sensible defaults
public class TcxTrackpointBuilder
{
    private DateTime _time = DateTime.Now;
    private double? _lat;
    private double? _lon;
    private double? _altitude;
    private double? _distance;
    private int? _heartRate;
    private int? _cadence;
    private double? _speed;

    public TcxTrackpointBuilder WithTime(DateTime value) { _time = value; return this; }
    public TcxTrackpointBuilder WithPosition(double lat, double lon) { _lat = lat; _lon = lon; return this; }
    public TcxTrackpointBuilder WithAltitude(double meters) { _altitude = meters; return this; }
    public TcxTrackpointBuilder WithDistance(double meters) { _distance = meters; return this; }
    public TcxTrackpointBuilder WithHeartRate(int bpm) { _heartRate = bpm; return this; }
    public TcxTrackpointBuilder WithCadence(int rpm) { _cadence = rpm; return this; }
    public TcxTrackpointBuilder WithSpeed(double metersPerSecond) { _speed = metersPerSecond; return this; }

    public TcxTrackpoint Build() => new()
    {
        Time = _time,
        LatitudeDegrees = _lat,
        LongitudeDegrees = _lon,
        AltitudeMeters = _altitude,
        DistanceMeters = _distance,
        HeartRateBpm = _heartRate,
        Cadence = _cadence,
        Speed = _speed
    };
}
