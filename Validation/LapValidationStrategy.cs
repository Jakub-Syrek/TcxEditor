using TcxEditor.Models;

namespace TcxEditor.Validation;

public class LapValidationStrategy : IValidationStrategy
{
    public IEnumerable<ValidationIssue> Validate(TcxDatabase db)
    {
        for (int ai = 0; ai < db.Activities.Count; ai++)
        {
            var act = db.Activities[ai];
            var actCtx = db.Activities.Count > 1 ? $"Activity {ai + 1}" : "Activity";

            for (int li = 0; li < act.Laps.Count; li++)
                foreach (var issue in ValidateLap(act.Laps[li], $"{actCtx} / Lap {li + 1}"))
                    yield return issue;
        }
    }

    private static IEnumerable<ValidationIssue> ValidateLap(TcxLap lap, string ctx)
    {
        if (lap.TotalTimeSeconds <= 0)
            yield return new ValidationIssue(IssueSeverity.Error, ctx, "Lap time must be > 0 s.");

        if (lap.DistanceMeters < 0)
            yield return new ValidationIssue(IssueSeverity.Error, ctx, "Distance cannot be negative.");

        if (lap.MaximumSpeed < 0)
            yield return new ValidationIssue(IssueSeverity.Error, ctx, "Max speed cannot be negative.");

        if (lap.Calories < 0)
            yield return new ValidationIssue(IssueSeverity.Warning, ctx, "Calories cannot be negative.");

        if (lap.AverageHeartRate.HasValue && (lap.AverageHeartRate < 20 || lap.AverageHeartRate > 300))
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Average heart rate ({lap.AverageHeartRate} bpm) out of range 20–300.");

        if (lap.MaximumHeartRate.HasValue && (lap.MaximumHeartRate < 20 || lap.MaximumHeartRate > 300))
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                $"Max heart rate ({lap.MaximumHeartRate} bpm) out of range 20–300.");

        if (lap.AverageHeartRate.HasValue && lap.MaximumHeartRate.HasValue
            && lap.MaximumHeartRate < lap.AverageHeartRate)
            yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                "Max heart rate is less than average heart rate.");
    }
}
