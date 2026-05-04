using TcxEditor.Models;

namespace TcxEditor.Validation;

public class LapValidationStrategy : IValidationStrategy
{
    public IEnumerable<ValidationIssue> Validate(TcxDatabase db)
    {
        for (int ai = 0; ai < db.Activities.Count; ai++)
        {
            var act = db.Activities[ai];
            var actCtx = db.Activities.Count > 1 ? $"Aktywność {ai + 1}" : "Aktywność";

            for (int li = 0; li < act.Laps.Count; li++)
                foreach (var issue in ValidateLap(act.Laps[li], $"{actCtx} / Okrążenie {li + 1}"))
                    yield return issue;
        }
    }

    private static IEnumerable<ValidationIssue> ValidateLap(TcxLap lap, string ctx)
    {
        if (lap.TotalTimeSeconds <= 0)
            yield return new ValidationIssue(IssueSeverity.Error, ctx, "Czas okrążenia musi być > 0 s.");

        if (lap.DistanceMeters < 0)
            yield return new ValidationIssue(IssueSeverity.Error, ctx, "Dystans nie może być ujemny.");

        if (lap.MaximumSpeed < 0)
            yield return new ValidationIssue(IssueSeverity.Error, ctx, "Maks. prędkość nie może być ujemna.");

        if (lap.Calories < 0)
            yield return new ValidationIssue(IssueSeverity.Warning, ctx, "Kalorie nie mogą być ujemne.");

        if (lap.AverageHeartRate.HasValue && (lap.AverageHeartRate < 20 || lap.AverageHeartRate > 300))
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Średnie tętno ({lap.AverageHeartRate} bpm) poza zakresem 20–300.");

        if (lap.MaximumHeartRate.HasValue && (lap.MaximumHeartRate < 20 || lap.MaximumHeartRate > 300))
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Maks. tętno ({lap.MaximumHeartRate} bpm) poza zakresem 20–300.");

        if (lap.AverageHeartRate.HasValue && lap.MaximumHeartRate.HasValue
            && lap.MaximumHeartRate < lap.AverageHeartRate)
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                "Maks. tętno jest mniejsze niż średnie tętno.");
    }
}
