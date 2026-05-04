using TcxEditor.Models;
using TcxEditor.Validation;

namespace TcxEditor.Services;

// Facade pattern — single entry point that wires up all IValidationStrategy instances
public static class ValidationService
{
    private static readonly ValidationStrategyContext _context = new(
        new ActivityValidationStrategy(),
        new LapValidationStrategy(),
        new TrackpointValidationStrategy());

    public static List<ValidationIssue> Validate(TcxDatabase db) =>
        _context.ExecuteAll(db);
}
