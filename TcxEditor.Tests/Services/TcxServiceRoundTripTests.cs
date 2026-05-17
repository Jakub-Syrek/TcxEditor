using System.IO;
using NSubstitute;
using NUnit.Framework;
using TcxEditor.Builders;
using TcxEditor.Models;
using TcxEditor.Repositories;
using TcxEditor.Services;

namespace TcxEditor.Tests.Services;

/// <summary>
/// Round-trip tests for <see cref="TcxService"/>: build a database, save it
/// to a temp file, reload it, and confirm the structurally important
/// fields survive serialisation. Also exercises <see cref="ITcxRepository"/>
/// via NSubstitute to demonstrate the seam.
/// </summary>
[TestFixture]
public class TcxServiceRoundTripTests
{
    private string _tempPath = string.Empty;

    /// <summary>Allocates a unique temporary file path for each test.</summary>
    [SetUp]
    public void SetUp()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"tcx-roundtrip-{Guid.NewGuid():N}.tcx");
    }

    /// <summary>Removes the temp file produced by the test, if any.</summary>
    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
    }

    /// <summary>
    /// Verifies that saving and reloading a populated database preserves
    /// activity sport, lap stats and trackpoint coordinates / heart rate.
    /// </summary>
    [Test]
    public void SaveThenLoad_PreservesStructuralFields()
    {
        var startUtc = new DateTime(2026, 5, 17, 7, 30, 0, DateTimeKind.Utc);

        var db = new TcxDatabase();
        var act = new TcxActivity
        {
            Sport = "Biking",
            StartTime = startUtc.ToLocalTime(),
            Notes = "round trip"
        };
        var lap = new TcxLapBuilder()
            .WithStartTime(startUtc.ToLocalTime())
            .WithDuration(1800)
            .WithDistance(15000)
            .WithMaxSpeed(11.1)
            .WithCalories(500)
            .WithAverageHeartRate(135)
            .WithMaxHeartRate(170)
            .Build();
        lap.Trackpoints.Add(new TcxTrackpointBuilder()
            .WithTime(startUtc.ToLocalTime())
            .WithPosition(50.0614, 19.9366)
            .WithAltitude(220.0)
            .WithDistance(0)
            .WithHeartRate(120)
            .Build());
        lap.Trackpoints.Add(new TcxTrackpointBuilder()
            .WithTime(startUtc.AddSeconds(1800).ToLocalTime())
            .WithPosition(50.10, 19.95)
            .WithAltitude(240.0)
            .WithDistance(15000)
            .WithHeartRate(160)
            .Build());
        act.Laps.Add(lap);
        db.Activities.Add(act);

        TcxService.Save(db, _tempPath);
        var reloaded = TcxService.Load(_tempPath);

        Assert.That(reloaded.Activities, Has.Count.EqualTo(1));
        var reloadedAct = reloaded.Activities[0];
        var reloadedLap = reloadedAct.Laps[0];

        Assert.Multiple(() =>
        {
            Assert.That(reloadedAct.Sport, Is.EqualTo("Biking"));
            Assert.That(reloadedAct.Notes, Is.EqualTo("round trip"));
            Assert.That(reloadedLap.TotalTimeSeconds, Is.EqualTo(1800).Within(0.5));
            Assert.That(reloadedLap.DistanceMeters, Is.EqualTo(15000).Within(0.5));
            Assert.That(reloadedLap.MaximumSpeed, Is.EqualTo(11.1).Within(0.01));
            Assert.That(reloadedLap.Calories, Is.EqualTo(500));
            Assert.That(reloadedLap.AverageHeartRate, Is.EqualTo(135));
            Assert.That(reloadedLap.MaximumHeartRate, Is.EqualTo(170));
            Assert.That(reloadedLap.Trackpoints, Has.Count.EqualTo(2));
            Assert.That(reloadedLap.Trackpoints[0].LatitudeDegrees, Is.EqualTo(50.0614).Within(1e-5));
            Assert.That(reloadedLap.Trackpoints[1].HeartRateBpm, Is.EqualTo(160));
        });
    }

    /// <summary>
    /// Verifies the <see cref="ITcxRepository"/> seam by substituting it
    /// with NSubstitute: collaborators that depend on the interface can
    /// be tested in isolation without touching the file system.
    /// </summary>
    [Test]
    public void Repository_CanBeSubstitutedAndReturnsInjectedDatabase()
    {
        var repository = Substitute.For<ITcxRepository>();
        var expected = new TcxDatabase();
        expected.Activities.Add(new TcxActivity { Sport = "Other" });
        repository.Load("any.tcx").Returns(expected);

        var actual = repository.Load("any.tcx");
        repository.Save(actual, "out.tcx");

        Assert.That(actual, Is.SameAs(expected));
        repository.Received(1).Save(expected, "out.tcx");
    }
}
