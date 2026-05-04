namespace TcxEditor.Models;

public enum IssueSeverity { Error, Warning }

public class ValidationIssue
{
    public IssueSeverity Severity { get; }
    public string Context { get; }
    public string Message { get; }

    public ValidationIssue(IssueSeverity severity, string context, string message)
    {
        Severity = severity;
        Context = context;
        Message = message;
    }

    public string SeverityLabel => Severity == IssueSeverity.Error ? "Error" : "Warning";
}
