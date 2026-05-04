using TcxEditor.Models;

namespace TcxEditor.Services;

public static class ValidationService
{
    public static List<ValidationIssue> Validate(TcxDatabase db)
    {
        var issues = new List<ValidationIssue>();

        if (db.Activities.Count == 0)
        {
            issues.Add(new ValidationIssue(IssueSeverity.Error, "Plik", "Brak aktywności w pliku."));
            return issues;
        }

        for (int i = 0; i < db.Activities.Count; i++)
        {
            var ctx = db.Activities.Count > 1 ? $"Aktywność {i + 1}" : "Aktywność";
            ValidateActivity(db.Activities[i], ctx, issues);
        }

        return issues;
    }

    private static void ValidateActivity(TcxActivity act, string ctx, List<ValidationIssue> issues)
    {
        if (!SportTypes.All.Contains(act.Sport))
            issues.Add(new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Nieznany typ sportu: '{act.Sport}'. Garmin może odrzucić plik."));

        if (act.Laps.Count == 0)
            issues.Add(new ValidationIssue(IssueSeverity.Error, ctx, "Aktywność nie zawiera żadnych okrążeń."));

        for (int i = 0; i < act.Laps.Count; i++)
            ValidateLap(act.Laps[i], $"{ctx} / Okrążenie {i + 1}", issues);
    }

    private static void ValidateLap(TcxLap lap, string ctx, List<ValidationIssue> issues)
    {
        if (lap.TotalTimeSeconds <= 0)
            issues.Add(new ValidationIssue(IssueSeverity.Error, ctx, "Czas okrążenia musi być > 0 s."));

        if (lap.DistanceMeters < 0)
            issues.Add(new ValidationIssue(IssueSeverity.Error, ctx, "Dystans nie może być ujemny."));

        if (lap.MaximumSpeed < 0)
            issues.Add(new ValidationIssue(IssueSeverity.Error, ctx, "Maks. prędkość nie może być ujemna."));

        if (lap.Calories < 0)
            issues.Add(new ValidationIssue(IssueSeverity.Warning, ctx, "Kalorie nie mogą być ujemne."));

        if (lap.AverageHeartRate.HasValue && (lap.AverageHeartRate < 20 || lap.AverageHeartRate > 300))
            issues.Add(new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Średnie tętno ({lap.AverageHeartRate} bpm) poza zakresem 20–300."));

        if (lap.MaximumHeartRate.HasValue && (lap.MaximumHeartRate < 20 || lap.MaximumHeartRate > 300))
            issues.Add(new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Maks. tętno ({lap.MaximumHeartRate} bpm) poza zakresem 20–300."));

        if (lap.AverageHeartRate.HasValue && lap.MaximumHeartRate.HasValue
            && lap.MaximumHeartRate < lap.AverageHeartRate)
            issues.Add(new ValidationIssue(IssueSeverity.Warning, ctx,
                "Maks. tętno jest mniejsze niż średnie tętno."));

        ValidateTrackpoints(lap, ctx, issues);
    }

    private static void ValidateTrackpoints(TcxLap lap, string ctx, List<ValidationIssue> issues)
    {
        var tps = lap.Trackpoints;
        if (tps.Count == 0) return;

        for (int i = 0; i < tps.Count; i++)
        {
            var tpCtx = $"{ctx} / Punkt {i + 1}";
            ValidateTrackpoint(tps[i], tpCtx, issues);
        }

        for (int i = 1; i < tps.Count; i++)
        {
            if (tps[i].Time < tps[i - 1].Time)
                issues.Add(new ValidationIssue(IssueSeverity.Error, $"{ctx} / Punkt {i + 1}",
                    "Czas jest wcześniejszy niż w poprzednim punkcie (punkty muszą być w kolejności chronologicznej)."));

            var prevDist = tps[i - 1].DistanceMeters;
            var currDist = tps[i].DistanceMeters;
            if (prevDist.HasValue && currDist.HasValue && currDist < prevDist)
                issues.Add(new ValidationIssue(IssueSeverity.Warning, $"{ctx} / Punkt {i + 1}",
                    $"Dystans ({currDist:F1} m) jest mniejszy niż w poprzednim punkcie ({prevDist:F1} m)."));
        }
    }

    private static void ValidateTrackpoint(TcxTrackpoint tp, string ctx, List<ValidationIssue> issues)
    {
        if (tp.LatitudeDegrees.HasValue && (tp.LatitudeDegrees < -90 || tp.LatitudeDegrees > 90))
            issues.Add(new ValidationIssue(IssueSeverity.Error, ctx,
                $"Szerokość geograficzna ({tp.LatitudeDegrees:F6}°) poza zakresem −90 do 90."));

        if (tp.LongitudeDegrees.HasValue && (tp.LongitudeDegrees < -180 || tp.LongitudeDegrees > 180))
            issues.Add(new ValidationIssue(IssueSeverity.Error, ctx,
                $"Długość geograficzna ({tp.LongitudeDegrees:F6}°) poza zakresem −180 do 180."));

        if (tp.LatitudeDegrees.HasValue ^ tp.LongitudeDegrees.HasValue)
            issues.Add(new ValidationIssue(IssueSeverity.Error, ctx,
                "Podana tylko szerokość lub tylko długość geograficzna — wymagane obie lub żadna."));

        if (tp.AltitudeMeters.HasValue && (tp.AltitudeMeters < -500 || tp.AltitudeMeters > 9000))
            issues.Add(new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Wysokość ({tp.AltitudeMeters:F1} m) poza typowym zakresem −500 do 9000 m."));

        if (tp.HeartRateBpm.HasValue && (tp.HeartRateBpm < 20 || tp.HeartRateBpm > 300))
            issues.Add(new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Tętno ({tp.HeartRateBpm} bpm) poza zakresem 20–300."));

        if (tp.Cadence.HasValue && (tp.Cadence < 0 || tp.Cadence > 250))
            issues.Add(new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Kadencja ({tp.Cadence} rpm) poza zakresem 0–250."));

        if (tp.Speed.HasValue && tp.Speed < 0)
            issues.Add(new ValidationIssue(IssueSeverity.Error, ctx, "Prędkość nie może być ujemna."));

        if (tp.DistanceMeters.HasValue && tp.DistanceMeters < 0)
            issues.Add(new ValidationIssue(IssueSeverity.Error, ctx, "Dystans od startu nie może być ujemny."));
    }
}
