using TcxEditor.Models;

namespace TcxEditor.Validation;

// Strategy pattern — context that runs all registered validation strategies
public class ValidationStrategyContext
{
    private readonly IReadOnlyList<IValidationStrategy> _strategies;

    public ValidationStrategyContext(params IValidationStrategy[] strategies)
    {
        _strategies = strategies;
    }

    public List<ValidationIssue> ExecuteAll(TcxDatabase db) =>
        _strategies.SelectMany(s => s.Validate(db)).ToList();
}
