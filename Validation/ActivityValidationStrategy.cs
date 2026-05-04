using TcxEditor.Models;

namespace TcxEditor.Validation;

public class ActivityValidationStrategy : IValidationStrategy
{
    public IEnumerable<ValidationIssue> Validate(TcxDatabase db)
    {
        if (db.Activities.Count == 0)
        {
            yield return new ValidationIssue(IssueSeverity.Error, "File", "No activities found in file.");
            yield break;
        }

        for (int i = 0; i < db.Activities.Count; i++)
        {
            var act = db.Activities[i];
            var ctx = db.Activities.Count > 1 ? $"Activity {i + 1}" : "Activity";

            if (!SportTypes.All.Contains(act.Sport))
                yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                    $"Unknown sport type: '{act.Sport}'. Garmin may reject the file.");

            if (act.Laps.Count == 0)
                yield return new ValidationIssue(IssueSeverity.Error, ctx, "Activity contains no laps.");
        }
    }
}
