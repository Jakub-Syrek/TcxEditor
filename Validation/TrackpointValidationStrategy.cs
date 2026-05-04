using TcxEditor.Models;

namespace TcxEditor.Validation;

public class TrackpointValidationStrategy : IValidationStrategy
{
    public IEnumerable<ValidationIssue> Validate(TcxDatabase db)
    {
        for (int ai = 0; ai < db.Activities.Count; ai++)
        {
            var act = db.Activities[ai];
            var actCtx = db.Activities.Count > 1 ? $"Activity {ai + 1}" : "Activity";

            for (int li = 0; li < act.Laps.Count; li++)
            {
                var lap = act.Laps[li];
                var lapCtx = $"{actCtx} / Lap {li + 1}";

                for (int ti = 0; ti < lap.Trackpoints.Count; ti++)
                    foreach (var issue in ValidateTrackpoint(lap.Trackpoints[ti], $"{lapCtx} / Trackpoint {ti + 1}"))
                        yield return issue;

                foreach (var issue in ValidateOrder(lap, lapCtx))
                    yield return issue;
            }
        }
    }

    private static IEnumerable<ValidationIssue> ValidateTrackpoint(TcxTrackpoint tp, string ctx)
    {
        if (tp.LatitudeDegrees.HasValue && (tp.LatitudeDegrees < -90 || tp.LatitudeDegrees > 90))
            yield return new ValidationIssue(IssueSeverity.Error, ctx,
                $"Latitude ({tp.LatitudeDegrees:F6}°) out of range −90 to 90.");

        if (tp.LongitudeDegrees.HasValue && (tp.LongitudeDegrees < -180 || tp.LongitudeDegrees > 180))
            yield return new ValidationIssue(IssueSeverity.Error, ctx,
                $"Longitude ({tp.LongitudeDegrees:F6}°) out of range −180 to 180.");

        if (tp.LatitudeDegrees.HasValue ^ tp.LongitudeDegrees.HasValue)
            yield return new ValidationIssue(IssueSeverity.Error, ctx,
                "Only latitude or only longitude provided — both or neither required.");

        if (tp.AltitudeMeters.HasValue && (tp.AltitudeMeters < -500 || tp.AltitudeMeters > 9000))
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Altitude ({tp.AltitudeMeters:F1} m) outside typical range −500 to 9000 m.");

        if (tp.HeartRateBpm.HasValue && (tp.HeartRateBpm < 20 || tp.HeartRateBpm > 300))
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Heart rate ({tp.HeartRateBpm} bpm) out of range 20–300.");

        if (tp.Cadence.HasValue && (tp.Cadence < 0 || tp.Cadence > 250))
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Cadence ({tp.Cadence} rpm) out of range 0–250.");

        if (tp.Speed.HasValue && tp.Speed < 0)
            yield return new ValidationIssue(IssueSeverity.Error, ctx, "Speed cannot be negative.");

        if (tp.DistanceMeters.HasValue && tp.DistanceMeters < 0)
            yield return new ValidationIssue(IssueSeverity.Error, ctx, "Distance from start cannot be negative.");
    }

    private static IEnumerable<ValidationIssue> ValidateOrder(TcxLap lap, string lapCtx)
    {
        var tps = lap.Trackpoints;
        for (int i = 1; i < tps.Count; i++)
        {
            if (tps[i].Time < tps[i - 1].Time)
                yield return new ValidationIssue(IssueSeverity.Error, $"{lapCtx} / Trackpoint {i + 1}",
                    "Timestamp is earlier than the previous trackpoint.");

            var prev = tps[i - 1].DistanceMeters;
            var curr = tps[i].DistanceMeters;
            if (prev.HasValue && curr.HasValue && curr < prev)
                yield return new ValidationIssue(IssueSeverity.Warning, $"{lapCtx} / Trackpoint {i + 1}",
                    $"Distance ({curr:F1} m) is less than in the previous trackpoint ({prev:F1} m).");
        }
    }
}
