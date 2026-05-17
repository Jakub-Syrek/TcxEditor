using NUnit.Framework;
using TcxEditor.Commands;
using TcxEditor.Models;

namespace TcxEditor.Tests.Commands;

/// <summary>
/// Tests for the <see cref="TcxLapMemento"/> and
/// <see cref="TcxTrackpointMemento"/> snapshot records.
/// </summary>
[TestFixture]
public class TcxMementoTests
{
    /// <summary>
    /// Verifies that mutating a lap after capturing a memento and then
    /// restoring from it returns every snapshotted field to its original
    /// value.
    /// </summary>
    [Test]
    public void LapMemento_RestoreTo_RevertsMutations()
    {
        var lap = new TcxLap
        {
            StartTime = new DateTime(2026, 5, 17, 10, 0, 0),
            TotalTimeSeconds = 600,
            DistanceMeters = 2500,
            MaximumSpeed = 5.0,
            Calories = 200,
            AverageHeartRate = 140,
            MaximumHeartRate = 170,
            Intensity = "Active",
            Notes = "original"
        };
        var memento = TcxLapMemento.Capture(lap);

        lap.TotalTimeSeconds = 1;
        lap.DistanceMeters = 1;
        lap.Calories = 0;
        lap.Notes = "mutated";

        memento.RestoreTo(lap);

        Assert.Multiple(() =>
        {
            Assert.That(lap.TotalTimeSeconds, Is.EqualTo(600));
            Assert.That(lap.DistanceMeters, Is.EqualTo(2500));
            Assert.That(lap.Calories, Is.EqualTo(200));
            Assert.That(lap.Notes, Is.EqualTo("original"));
        });
    }

    /// <summary>
    /// Verifies that <see cref="TcxTrackpointMemento.ToTrackpoint"/>
    /// produces a fresh trackpoint that is value-equal to the captured
    /// state across every nullable field.
    /// </summary>
    [Test]
    public void TrackpointMemento_ToTrackpoint_ReproducesAllFields()
    {
        var tp = new TcxTrackpoint
        {
            Time = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc),
            LatitudeDegrees = 50.0,
            LongitudeDegrees = 19.9,
            AltitudeMeters = 220,
            DistanceMeters = 100,
            HeartRateBpm = 140,
            Cadence = 85,
            Speed = 3.2
        };
        var memento = TcxTrackpointMemento.Capture(tp);

        var copy = memento.ToTrackpoint();

        Assert.Multiple(() =>
        {
            Assert.That(copy.Time, Is.EqualTo(tp.Time));
            Assert.That(copy.LatitudeDegrees, Is.EqualTo(tp.LatitudeDegrees));
            Assert.That(copy.LongitudeDegrees, Is.EqualTo(tp.LongitudeDegrees));
            Assert.That(copy.AltitudeMeters, Is.EqualTo(tp.AltitudeMeters));
            Assert.That(copy.DistanceMeters, Is.EqualTo(tp.DistanceMeters));
            Assert.That(copy.HeartRateBpm, Is.EqualTo(tp.HeartRateBpm));
            Assert.That(copy.Cadence, Is.EqualTo(tp.Cadence));
            Assert.That(copy.Speed, Is.EqualTo(tp.Speed));
        });
    }
}
