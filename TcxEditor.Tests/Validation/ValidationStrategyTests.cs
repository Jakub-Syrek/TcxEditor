using NUnit.Framework;
using TcxEditor.Builders;
using TcxEditor.Models;
using TcxEditor.Services;
using TcxEditor.Validation;

namespace TcxEditor.Tests.Validation;

/// <summary>
/// Tests for the validation strategies and the
/// <see cref="ValidationService"/> facade that composes them.
/// </summary>
[TestFixture]
public class ValidationStrategyTests
{
    /// <summary>
    /// An empty database must produce a single "No activities" error.
    /// </summary>
    [Test]
    public void Validate_EmptyDatabase_ReportsNoActivities()
    {
        var issues = ValidationService.Validate(new TcxDatabase());

        Assert.That(issues, Has.Exactly(1).Matches<ValidationIssue>(
            i => i.Severity == IssueSeverity.Error
                 && i.Message.Contains("No activities")));
    }

    /// <summary>
    /// A lap with zero duration is a hard error; the message must clearly
    /// flag it for the user.
    /// </summary>
    [Test]
    public void Validate_LapWithZeroDuration_IsError()
    {
        var db = BuildSingleLapDatabase(_ => { });

        var issues = ValidationService.Validate(db);

        Assert.That(issues, Has.Some.Matches<ValidationIssue>(
            i => i.Severity == IssueSeverity.Error
                 && i.Message.Contains("Lap time")));
    }

    /// <summary>
    /// Latitude outside the [-90, 90] range must be flagged as an error.
    /// </summary>
    [Test]
    public void Validate_TrackpointWithLatitudeOutOfRange_IsError()
    {
        var db = BuildSingleLapDatabase(lap =>
        {
            lap.TotalTimeSeconds = 60;
            lap.Trackpoints.Add(new TcxTrackpointBuilder()
                .WithTime(DateTime.UtcNow)
                .WithPosition(95.0, 0.0)
                .Build());
        });

        var issues = ValidationService.Validate(db);

        Assert.That(issues, Has.Some.Matches<ValidationIssue>(
            i => i.Severity == IssueSeverity.Error
                 && i.Message.Contains("Latitude")));
    }

    /// <summary>
    /// Trackpoints whose timestamps go backwards must be flagged as an
    /// error by the trackpoint ordering check.
    /// </summary>
    [Test]
    public void Validate_NonChronologicalTrackpoints_IsError()
    {
        var t0 = new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc);
        var db = BuildSingleLapDatabase(lap =>
        {
            lap.TotalTimeSeconds = 60;
            lap.Trackpoints.Add(new TcxTrackpointBuilder().WithTime(t0).Build());
            lap.Trackpoints.Add(new TcxTrackpointBuilder().WithTime(t0.AddSeconds(-5)).Build());
        });

        var issues = ValidationService.Validate(db);

        Assert.That(issues, Has.Some.Matches<ValidationIssue>(
            i => i.Severity == IssueSeverity.Error
                 && i.Message.Contains("earlier than the previous")));
    }

    /// <summary>
    /// Heart-rate outside the 20-300 bpm physiological range must be a
    /// warning, not an error (the file is still importable).
    /// </summary>
    [Test]
    public void Validate_HeartRateOutOfRange_IsWarning()
    {
        var db = BuildSingleLapDatabase(lap =>
        {
            lap.TotalTimeSeconds = 60;
            lap.AverageHeartRate = 400;
        });

        var issues = ValidationService.Validate(db);

        Assert.That(issues, Has.Some.Matches<ValidationIssue>(
            i => i.Severity == IssueSeverity.Warning
                 && i.Message.Contains("heart rate")));
    }

    /// <summary>
    /// A clean activity with a valid lap and a valid trackpoint must
    /// produce no issues at all.
    /// </summary>
    [Test]
    public void Validate_HealthyActivity_HasNoIssues()
    {
        var db = BuildSingleLapDatabase(lap =>
        {
            lap.TotalTimeSeconds = 600;
            lap.DistanceMeters = 1500;
            lap.AverageHeartRate = 140;
            lap.MaximumHeartRate = 170;
            lap.Trackpoints.Add(new TcxTrackpointBuilder()
                .WithTime(new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc))
                .WithPosition(50.0, 19.9)
                .WithAltitude(220)
                .WithDistance(0)
                .WithHeartRate(140)
                .Build());
            lap.Trackpoints.Add(new TcxTrackpointBuilder()
                .WithTime(new DateTime(2026, 5, 17, 9, 5, 0, DateTimeKind.Utc))
                .WithPosition(50.01, 19.91)
                .WithAltitude(225)
                .WithDistance(1500)
                .WithHeartRate(150)
                .Build());
        });

        var issues = ValidationService.Validate(db);

        Assert.That(issues, Is.Empty);
    }

    private static TcxDatabase BuildSingleLapDatabase(Action<TcxLap> configureLap)
    {
        var db = new TcxDatabase();
        var act = new TcxActivity { Sport = "Running" };
        var lap = new TcxLapBuilder().Build();
        configureLap(lap);
        act.Laps.Add(lap);
        db.Activities.Add(act);
        return db;
    }
}
