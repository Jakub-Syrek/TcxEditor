using NUnit.Framework;
using TcxEditor.Builders;

namespace TcxEditor.Tests.Builders;

/// <summary>
/// Tests for <see cref="TcxTrackpointBuilder"/>.
/// </summary>
[TestFixture]
public class TcxTrackpointBuilderTests
{
    /// <summary>
    /// Verifies that <see cref="TcxTrackpointBuilder.WithPosition"/> stores
    /// both coordinates atomically rather than overwriting one of them.
    /// </summary>
    [Test]
    public void Build_WithPosition_StoresBothCoordinates()
    {
        var time = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc);

        var tp = new TcxTrackpointBuilder()
            .WithTime(time)
            .WithPosition(50.0614, 19.9366)
            .WithAltitude(220.5)
            .WithDistance(1234.56)
            .WithHeartRate(140)
            .WithCadence(85)
            .WithSpeed(3.5)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(tp.Time, Is.EqualTo(time));
            Assert.That(tp.LatitudeDegrees, Is.EqualTo(50.0614));
            Assert.That(tp.LongitudeDegrees, Is.EqualTo(19.9366));
            Assert.That(tp.AltitudeMeters, Is.EqualTo(220.5));
            Assert.That(tp.DistanceMeters, Is.EqualTo(1234.56));
            Assert.That(tp.HeartRateBpm, Is.EqualTo(140));
            Assert.That(tp.Cadence, Is.EqualTo(85));
            Assert.That(tp.Speed, Is.EqualTo(3.5));
        });
    }

    /// <summary>
    /// Verifies that optional fields remain null when the builder is used
    /// without them, mirroring TCX schema "absent element" semantics.
    /// </summary>
    [Test]
    public void Build_WithoutOptionalFields_LeavesThemNull()
    {
        var tp = new TcxTrackpointBuilder().Build();

        Assert.Multiple(() =>
        {
            Assert.That(tp.LatitudeDegrees, Is.Null);
            Assert.That(tp.LongitudeDegrees, Is.Null);
            Assert.That(tp.AltitudeMeters, Is.Null);
            Assert.That(tp.DistanceMeters, Is.Null);
            Assert.That(tp.HeartRateBpm, Is.Null);
            Assert.That(tp.Cadence, Is.Null);
            Assert.That(tp.Speed, Is.Null);
        });
    }
}
