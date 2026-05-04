using TcxEditor.Models;

namespace TcxEditor.Validation;

public interface IValidationStrategy
{
    IEnumerable<ValidationIssue> Validate(TcxDatabase db);
}
