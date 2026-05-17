using NUnit.Framework;
using TcxEditor.Builders;
using TcxEditor.Commands;
using TcxEditor.Models;

namespace TcxEditor.Tests.Commands;

/// <summary>
/// Tests for <see cref="AutoTimeCommand"/>.
/// </summary>
[TestFixture]
public class AutoTimeCommandTests
{
    /// <summary>
    /// Verifies that <see cref="AutoTimeCommand.Execute"/> distributes
    /// trackpoint timestamps proportionally to distance, and that
    /// <c>Undo</c> restores the original timestamps from the memento.
    /// </summary>
    [Test]
    public void Execute_DistributesTimesProportionallyToDistance()
    {
        var start = new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Local);
        var lap = new TcxLapBuilder()
            .WithStartTime(start)
            .WithDuration(100)
            .WithDistance(1000)
            .Build();

        var originalTime = new DateTime(2000, 1, 1);
        lap.Trackpoints.Add(new TcxTrackpointBuilder().WithTime(originalTime).WithDistance(0).Build());
        lap.Trackpoints.Add(new TcxTrackpointBuilder().WithTime(originalTime).WithDistance(250).Build());
        lap.Trackpoints.Add(new TcxTrackpointBuilder().WithTime(originalTime).WithDistance(1000).Build());

        var command = new AutoTimeCommand(lap);
        command.Execute();

        Assert.Multiple(() =>
        {
            Assert.That(lap.Trackpoints[0].Time, Is.EqualTo(start));
            Assert.That(lap.Trackpoints[1].Time, Is.EqualTo(start.AddSeconds(25)));
            Assert.That(lap.Trackpoints[2].Time, Is.EqualTo(start.AddSeconds(100)));
        });

        command.Undo();

        Assert.That(lap.Trackpoints.All(tp => tp.Time == originalTime), Is.True,
            "Undo must restore every trackpoint timestamp from the memento.");
    }

    /// <summary>
    /// Verifies that the command exits cleanly when total distance is
    /// non-positive (no division-by-zero, no mutation).
    /// </summary>
    [Test]
    public void Execute_WithZeroTotalDistance_LeavesTrackpointsUnchanged()
    {
        var lap = new TcxLapBuilder().WithStartTime(DateTime.Now).WithDuration(100).Build();
        var originalTime = new DateTime(2020, 6, 1);
        lap.Trackpoints.Add(new TcxTrackpointBuilder().WithTime(originalTime).WithDistance(0).Build());
        lap.Trackpoints.Add(new TcxTrackpointBuilder().WithTime(originalTime).WithDistance(0).Build());

        var command = new AutoTimeCommand(lap);

        Assert.DoesNotThrow(() => command.Execute());
        Assert.That(lap.Trackpoints.All(tp => tp.Time == originalTime), Is.True);
    }
}
