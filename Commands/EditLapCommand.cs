using TcxEditor.Models;

namespace TcxEditor.Commands;

// Command pattern with embedded Memento for before/after state
public class EditLapCommand : IUndoableCommand
{
    private readonly TcxLap _target;
    private readonly TcxLapMemento _before;
    private readonly TcxLapMemento _after;

    public EditLapCommand(TcxLap target, TcxLapMemento before, TcxLapMemento after)
    {
        _target = target;
        _before = before;
        _after = after;
    }

    public string Description => "Edit lap";
    public void Execute() => _after.RestoreTo(_target);
    public void Undo() => _before.RestoreTo(_target);
}
