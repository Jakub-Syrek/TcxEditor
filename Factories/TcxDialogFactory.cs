using System.Windows;
using TcxEditor.Models;
using TcxEditor.Views;

namespace TcxEditor.Factories;

// Factory pattern — centralises creation of all editor dialogs
public class TcxDialogFactory
{
    private readonly Window _owner;

    public TcxDialogFactory(Window owner) => _owner = owner;

    public LapDialog CreateLapDialog(TcxLap lap) =>
        new(lap) { Owner = _owner };

    public TrackpointDialog CreateTrackpointDialog(TcxTrackpoint trackpoint) =>
        new(trackpoint) { Owner = _owner };

    public ChangeDateDialog CreateChangeDateDialog(DateTime currentDate) =>
        new(currentDate) { Owner = _owner };

    public ShiftTimeDialog CreateShiftTimeDialog() =>
        new() { Owner = _owner };
}
