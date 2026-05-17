using NSubstitute;
using NUnit.Framework;
using TcxEditor.Commands;

namespace TcxEditor.Tests.Commands;

/// <summary>
/// Tests for the <see cref="CommandHistory"/> invoker covering execute,
/// undo, redo and the clearing of the redo stack on a new execution.
/// </summary>
[TestFixture]
public class CommandHistoryTests
{
    /// <summary>
    /// Verifies that <see cref="CommandHistory.Execute"/> runs the command
    /// and exposes it via <c>CanUndo</c> / <c>UndoDescription</c>.
    /// </summary>
    [Test]
    public void Execute_RunsCommandAndEnablesUndo()
    {
        var history = new CommandHistory();
        var cmd = Substitute.For<IUndoableCommand>();
        cmd.Description.Returns("do thing");

        history.Execute(cmd);

        cmd.Received(1).Execute();
        Assert.Multiple(() =>
        {
            Assert.That(history.CanUndo, Is.True);
            Assert.That(history.CanRedo, Is.False);
            Assert.That(history.UndoDescription, Is.EqualTo("do thing"));
        });
    }

    /// <summary>
    /// Verifies the full Execute -> Undo -> Redo cycle, including the
    /// stack-state transitions and the second Execute call on Redo.
    /// </summary>
    [Test]
    public void UndoThenRedo_CallsUndoThenExecuteAgain()
    {
        var history = new CommandHistory();
        var cmd = Substitute.For<IUndoableCommand>();
        cmd.Description.Returns("op");

        history.Execute(cmd);
        history.Undo();

        Assert.Multiple(() =>
        {
            Assert.That(history.CanUndo, Is.False);
            Assert.That(history.CanRedo, Is.True);
            Assert.That(history.RedoDescription, Is.EqualTo("op"));
        });

        history.Redo();

        cmd.Received(2).Execute();
        cmd.Received(1).Undo();
        Assert.That(history.CanUndo, Is.True);
        Assert.That(history.CanRedo, Is.False);
    }

    /// <summary>
    /// Verifies that executing a new command after an undo discards the
    /// redo stack (standard linear-history invariant).
    /// </summary>
    [Test]
    public void Execute_AfterUndo_ClearsRedoStack()
    {
        var history = new CommandHistory();
        var first = Substitute.For<IUndoableCommand>();
        var second = Substitute.For<IUndoableCommand>();

        history.Execute(first);
        history.Undo();
        Assert.That(history.CanRedo, Is.True);

        history.Execute(second);

        Assert.That(history.CanRedo, Is.False);
    }

    /// <summary>
    /// Verifies that <see cref="CommandHistory.Undo"/> and <c>Redo</c> are
    /// no-ops when their respective stacks are empty.
    /// </summary>
    [Test]
    public void UndoAndRedo_OnEmptyHistory_AreNoOps()
    {
        var history = new CommandHistory();

        Assert.DoesNotThrow(() => history.Undo());
        Assert.DoesNotThrow(() => history.Redo());
        Assert.Multiple(() =>
        {
            Assert.That(history.CanUndo, Is.False);
            Assert.That(history.CanRedo, Is.False);
            Assert.That(history.UndoDescription, Is.Null);
            Assert.That(history.RedoDescription, Is.Null);
        });
    }
}
