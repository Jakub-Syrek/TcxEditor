using TcxEditor.Models;

namespace TcxEditor.Validation;

public class TrackpointValidationStrategy : IValidationStrategy
{
    public IEnumerable<ValidationIssue> Validate(TcxDatabase db)
    {
        for (int ai = 0; ai < db.Activities.Count; ai++)
        {
            var act = db.Activities[ai];
            var actCtx = db.Activities.Count > 1 ? $"Aktywność {ai + 1}" : "Aktywność";

            for (int li = 0; li < act.Laps.Count; li++)
            {
                var lap = act.Laps[li];
                var lapCtx = $"{actCtx} / Okrążenie {li + 1}";

                for (int ti = 0; ti < lap.Trackpoints.Count; ti++)
                    foreach (var issue in ValidateTrackpoint(lap.Trackpoints[ti], $"{lapCtx} / Punkt {ti + 1}"))
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
                $"Szerokość geograficzna ({tp.LatitudeDegrees:F6}°) poza zakresem −90 do 90.");

        if (tp.LongitudeDegrees.HasValue && (tp.LongitudeDegrees < -180 || tp.LongitudeDegrees > 180))
            yield return new ValidationIssue(IssueSeverity.Error, ctx,
                $"Długość geograficzna ({tp.LongitudeDegrees:F6}°) poza zakresem −180 do 180.");

        if (tp.LatitudeDegrees.HasValue ^ tp.LongitudeDegrees.HasValue)
            yield return new ValidationIssue(IssueSeverity.Error, ctx,
                "Podana tylko szerokość lub tylko długość — wymagane obie lub żadna.");

        if (tp.AltitudeMeters.HasValue && (tp.AltitudeMeters < -500 || tp.AltitudeMeters > 9000))
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Wysokość ({tp.AltitudeMeters:F1} m) poza typowym zakresem −500 do 9000 m.");

        if (tp.HeartRateBpm.HasValue && (tp.HeartRateBpm < 20 || tp.HeartRateBpm > 300))
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Tętno ({tp.HeartRateBpm} bpm) poza zakresem 20–300.");

        if (tp.Cadence.HasValue && (tp.Cadence < 0 || tp.Cadence > 250))
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Kadencja ({tp.Cadence} rpm) poza zakresem 0–250.");

        if (tp.Speed.HasValue && tp.Speed < 0)
            yield return new ValidationIssue(IssueSeverity.Error, ctx, "Prędkość nie może być ujemna.");

        if (tp.DistanceMeters.HasValue && tp.DistanceMeters < 0)
            yield return new ValidationIssue(IssueSeverity.Error, ctx, "Dystans od startu nie może być ujemny.");
    }

    private static IEnumerable<ValidationIssue> ValidateOrder(TcxLap lap, string lapCtx)
    {
        var tps = lap.Trackpoints;
        for (int i = 1; i < tps.Count; i++)
        {
            if (tps[i].Time < tps[i - 1].Time)
                yield return new ValidationIssue(IssueSeverity.Error, $"{lapCtx} / Punkt {i + 1}",
                    "Czas jest wcześniejszy niż w poprzednim punkcie.");

            var prev = tps[i - 1].DistanceMeters;
            var curr = tps[i].DistanceMeters;
            if (prev.HasValue && curr.HasValue && curr < prev)
                yield return new ValidationIssue(IssueSeverity.Warning, $"{lapCtx} / Punkt {i + 1}",
                    $"Dystans ({curr:F1} m) jest mniejszy niż w poprzednim punkcie ({prev:F1} m).");
        }
    }
}
