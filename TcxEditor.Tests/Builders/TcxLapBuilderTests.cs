using NUnit.Framework;
using TcxEditor.Builders;

namespace TcxEditor.Tests.Builders;

/// <summary>
/// Tests for <see cref="TcxLapBuilder"/> covering fluent chaining, defaults
/// and propagation of every configured field to the built lap.
/// </summary>
[TestFixture]
public class TcxLapBuilderTests
{
    /// <summary>
    /// Verifies that an unconfigured builder produces a lap with sensible
    /// schema-safe defaults (Intensity = "Active", empty notes).
    /// </summary>
    [Test]
    public void Build_WithoutConfiguration_ReturnsSchemaSafeDefaults()
    {
        var lap = new TcxLapBuilder().Build();

        Assert.Multiple(() =>
        {
            Assert.That(lap.Intensity, Is.EqualTo("Active"));
            Assert.That(lap.Notes, Is.EqualTo(string.Empty));
            Assert.That(lap.TotalTimeSeconds, Is.EqualTo(0));
            Assert.That(lap.DistanceMeters, Is.EqualTo(0));
            Assert.That(lap.AverageHeartRate, Is.Null);
            Assert.That(lap.MaximumHeartRate, Is.Null);
        });
    }

    /// <summary>
    /// Verifies that every fluent <c>With*</c> method propagates its value
    /// onto the built lap and that the chain returns the same builder.
    /// </summary>
    [Test]
    public void Build_AfterFluentChain_PropagatesAllFields()
    {
        var start = new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Local);

        var lap = new TcxLapBuilder()
            .WithStartTime(start)
            .WithDuration(1800)
            .WithDistance(5000)
            .WithMaxSpeed(4.2)
            .WithCalories(420)
            .WithAverageHeartRate(150)
            .WithMaxHeartRate(180)
            .WithIntensity("Resting")
            .WithNotes("warm-up")
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(lap.StartTime, Is.EqualTo(start));
            Assert.That(lap.TotalTimeSeconds, Is.EqualTo(1800));
            Assert.That(lap.DistanceMeters, Is.EqualTo(5000));
            Assert.That(lap.MaximumSpeed, Is.EqualTo(4.2));
            Assert.That(lap.Calories, Is.EqualTo(420));
            Assert.That(lap.AverageHeartRate, Is.EqualTo(150));
            Assert.That(lap.MaximumHeartRate, Is.EqualTo(180));
            Assert.That(lap.Intensity, Is.EqualTo("Resting"));
            Assert.That(lap.Notes, Is.EqualTo("warm-up"));
            // Derived properties must reflect the source fields.
            Assert.That(lap.DistanceKm, Is.EqualTo(5.0));
            Assert.That(lap.Duration, Is.EqualTo("00:30:00"));
        });
    }
}
