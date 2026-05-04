using TcxEditor.Models;

namespace TcxEditor.Validation;

public class ActivityValidationStrategy : IValidationStrategy
{
    public IEnumerable<ValidationIssue> Validate(TcxDatabase db)
    {
        if (db.Activities.Count == 0)
        {
            yield return new ValidationIssue(IssueSeverity.Error, "Plik", "Brak aktywności w pliku.");
            yield break;
        }

        for (int i = 0; i < db.Activities.Count; i++)
        {
            var act = db.Activities[i];
            var ctx = db.Activities.Count > 1 ? $"Aktywność {i + 1}" : "Aktywność";

            if (!SportTypes.All.Contains(act.Sport))
                yield return new ValidationIssue(IssueSeverity.Warning, ctx,
                    $"Nieznany typ sportu: '{act.Sport}'. Garmin może odrzucić plik.");

            if (act.Laps.Count == 0)
                yield return new ValidationIssue(IssueSeverity.Error, ctx,
                    "Aktywność nie zawiera żadnych okrążeń.");
        }
    }
}
