using TcxEditor.Models;

namespace TcxEditor.Commands;

// Command pattern with embedded Memento for before/after state
public class EditTrackpointCommand : IUndoableCommand
{
    private readonly TcxTrackpoint _target;
    private readonly TcxTrackpointMemento _before;
    private readonly TcxTrackpointMemento _after;

    public EditTrackpointCommand(TcxTrackpoint target, TcxTrackpointMemento before, TcxTrackpointMemento after)
    {
        _target = target;
        _before = before;
        _after = after;
    }

    public string Description => "Edit trackpoint";
    public void Execute() => _after.RestoreTo(_target);
    public void Undo() => _before.RestoreTo(_target);
}
