using NUnit.Framework;
using TcxEditor.Builders;
using TcxEditor.Commands;
using TcxEditor.Models;

namespace TcxEditor.Tests.Commands;

/// <summary>
/// Tests for <see cref="ShiftTimeCommand"/>.
/// </summary>
[TestFixture]
public class ShiftTimeCommandTests
{
    /// <summary>
    /// Verifies that <see cref="ShiftTimeCommand.Execute"/> shifts the
    /// activity start, every lap start and every trackpoint timestamp by
    /// the same offset, and that <c>Undo</c> exactly reverses the shift.
    /// </summary>
    [Test]
    public void ExecuteThenUndo_ShiftsAllTimestampsAndRestoresThem()
    {
        var start = new DateTime(2026, 5, 17, 8, 0, 0, DateTimeKind.Local);

        var lap = new TcxLapBuilder().WithStartTime(start).WithDuration(60).Build();
        lap.Trackpoints.Add(new TcxTrackpointBuilder().WithTime(start).Build());
        lap.Trackpoints.Add(new TcxTrackpointBuilder().WithTime(start.AddSeconds(30)).Build());

        var activity = new TcxActivity { StartTime = start };
        activity.Laps.Add(lap);

        var offset = TimeSpan.FromHours(2);
        var command = new ShiftTimeCommand(activity, offset);

        command.Execute();

        Assert.Multiple(() =>
        {
            Assert.That(activity.StartTime, Is.EqualTo(start + offset));
            Assert.That(lap.StartTime, Is.EqualTo(start + offset));
            Assert.That(lap.Trackpoints[0].Time, Is.EqualTo(start + offset));
            Assert.That(lap.Trackpoints[1].Time, Is.EqualTo(start.AddSeconds(30) + offset));
        });

        command.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(activity.StartTime, Is.EqualTo(start));
            Assert.That(lap.StartTime, Is.EqualTo(start));
            Assert.That(lap.Trackpoints[0].Time, Is.EqualTo(start));
            Assert.That(lap.Trackpoints[1].Time, Is.EqualTo(start.AddSeconds(30)));
        });
    }

    /// <summary>
    /// Verifies that the human-readable description encodes both the
    /// direction (back/forward) and the absolute hh:mm:ss magnitude.
    /// </summary>
    [TestCase(3661, "forward 01:01:01")]
    [TestCase(-90, "back 00:01:30")]
    public void Description_EncodesDirectionAndMagnitude(int offsetSeconds, string expectedTail)
    {
        var command = new ShiftTimeCommand(new TcxActivity(), TimeSpan.FromSeconds(offsetSeconds));

        Assert.That(command.Description, Does.EndWith(expectedTail));
        Assert.That(command.Description, Does.StartWith("Shift time"));
    }
}
