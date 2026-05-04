using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using TcxEditor.Builders;
using TcxEditor.Commands;
using TcxEditor.Factories;
using TcxEditor.Models;
using TcxEditor.Repositories;
using TcxEditor.Services;
using TcxEditor.Views;

namespace TcxEditor;

public partial class MainWindow : Window
{
    // Repository pattern — load/save abstracted behind interface
    private readonly ITcxRepository _repository = new TcxFileRepository();

    // Command pattern — tracks history for undo/redo
    private readonly CommandHistory _commandHistory = new();

    // Factory pattern — creates dialogs without coupling MainWindow to concrete types
    private readonly TcxDialogFactory _dialogFactory;

    private TcxDatabase _db = new();
    private TcxActivity? _currentActivity;
    private string? _currentFilePath;
    private bool _isDirty;

    public MainWindow()
    {
        InitializeComponent();
        _dialogFactory = new TcxDialogFactory(this);
        _commandHistory.Changed += OnHistoryChanged;
        SetupKeyBindings();
        foreach (var s in SportTypes.All) SportCombo.Items.Add(s);
        NewActivity();
    }

    // ── Command history (Observer pattern via event) ───────────────────────

    private void OnHistoryChanged(object? sender, EventArgs e)
    {
        UndoMenuItem.IsEnabled = _commandHistory.CanUndo;
        UndoMenuItem.Header = _commandHistory.CanUndo
            ? $"_Undo ({_commandHistory.UndoDescription})"
            : "_Undo";

        RedoMenuItem.IsEnabled = _commandHistory.CanRedo;
        RedoMenuItem.Header = _commandHistory.CanRedo
            ? $"_Redo ({_commandHistory.RedoDescription})"
            : "_Redo";

        SetDirty(true);
        RefreshGrids();
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        _commandHistory.Undo();
        SyncActivityToUi();
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        _commandHistory.Redo();
        SyncActivityToUi();
    }

    // ── File operations ────────────────────────────────────────────────────

    private void New_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;
        _currentFilePath = null;
        _db = new TcxDatabase();
        _commandHistory.Clear();
        NewActivity();
        ClearValidation();
        SetDirty(false);
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;

        var dlg = new OpenFileDialog
        {
            Filter = "TCX files (*.tcx)|*.tcx|All files (*.*)|*.*",
            Title = "Open TCX file"
        };
        if (dlg.ShowDialog() != true) return;

        try
        {
            _db = _repository.Load(dlg.FileName);
            _currentFilePath = dlg.FileName;
            _commandHistory.Clear();

            if (_db.Activities.Count > 0)
                LoadActivity(_db.Activities[0]);
            else
                NewActivity();

            SetDirty(false);
            SetStatus($"Opened: {Path.GetFileName(dlg.FileName)}");

            var issues = ValidationService.Validate(_db);
            ShowValidationResults(issues);
            if (issues.Count > 0) ValidationExpander.IsExpanded = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening file:\n{ex.Message}", "Read error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFilePath == null) SaveAs_Click(sender, e);
        else SaveToFile(_currentFilePath);
    }

    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Filter = "TCX files (*.tcx)|*.tcx",
            Title = "Save TCX file",
            DefaultExt = ".tcx",
            FileName = _currentFilePath != null ? Path.GetFileName(_currentFilePath) : "activity.tcx"
        };
        if (dlg.ShowDialog() != true) return;
        SaveToFile(dlg.FileName);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;
        Application.Current.Shutdown();
    }

    // ── Validation (Facade pattern in ValidationService) ───────────────────

    private void Validate_Click(object sender, RoutedEventArgs e)
    {
        ApplyActivityFromUi();
        var issues = ValidationService.Validate(_db);
        ShowValidationResults(issues);
        ValidationExpander.IsExpanded = true;
        if (issues.Count == 0) SetStatus("Validation complete — no issues found.");
    }

    private void ShowValidationResults(List<ValidationIssue> issues)
    {
        ValidationGrid.ItemsSource = issues;
        int errors = issues.Count(i => i.Severity == IssueSeverity.Error);
        int warnings = issues.Count(i => i.Severity == IssueSeverity.Warning);

        if (issues.Count == 0)
        {
            ValidationSummaryText.Text = "Validation: OK";
            ValidationSummaryText.Foreground = System.Windows.Media.Brushes.Green;
        }
        else
        {
            var parts = new List<string>();
            if (errors > 0) parts.Add($"{errors} error(s)");
            if (warnings > 0) parts.Add($"{warnings} warning(s)");
            ValidationSummaryText.Text = "Validation: " + string.Join(", ", parts);
            ValidationSummaryText.Foreground = errors > 0
                ? System.Windows.Media.Brushes.Red
                : System.Windows.Media.Brushes.DarkOrange;
        }
    }

    private void ClearValidation()
    {
        ValidationGrid.ItemsSource = null;
        ValidationSummaryText.Text = "";
    }

    // ── Date / time operations (Command pattern) ───────────────────────────

    private void ChangeDate_Click(object sender, RoutedEventArgs e)
    {
        if (_currentActivity == null) return;
        ApplyActivityFromUi();

        var dlg = _dialogFactory.CreateChangeDateDialog(_currentActivity.StartTime);
        if (dlg.ShowDialog() != true) return;

        var offset = dlg.NewDate - _currentActivity.StartTime.Date;
        if (offset == TimeSpan.Zero) return;

        _commandHistory.Execute(new ShiftTimeCommand(_currentActivity, offset));
        SyncActivityToUi();
        SetStatus($"Changed date by {(int)offset.TotalDays} day(s).");
    }

    private void ShiftTime_Click(object sender, RoutedEventArgs e)
    {
        if (_currentActivity == null) return;
        ApplyActivityFromUi();

        var dlg = _dialogFactory.CreateShiftTimeDialog();
        if (dlg.ShowDialog() != true || dlg.Offset == TimeSpan.Zero) return;

        _commandHistory.Execute(new ShiftTimeCommand(_currentActivity, dlg.Offset));
        SyncActivityToUi();

        var abs = dlg.Offset.Duration();
        SetStatus($"Time shifted by {abs.Hours:D2}:{abs.Minutes:D2}:{abs.Seconds:D2}.");
    }

    // ── Laps (Command + Builder patterns) ─────────────────────────────────

    private void AddLap_Click(object sender, RoutedEventArgs e)
    {
        if (_currentActivity == null) return;

        // Builder pattern — creates a new lap with a sensible default start time
        var lap = new TcxLapBuilder()
            .WithStartTime(_currentActivity.StartTime)
            .Build();

        var dlg = _dialogFactory.CreateLapDialog(lap);
        if (dlg.ShowDialog() != true) return;

        _commandHistory.Execute(new AddLapCommand(_currentActivity, lap));
        LapsGrid.SelectedItem = lap;
    }

    private void EditLap_Click(object sender, RoutedEventArgs e)
    {
        if (LapsGrid.SelectedItem is not TcxLap lap) return;

        var before = TcxLapMemento.Capture(lap);
        var copy = before.ToLap(); // edit on a detached copy

        var dlg = _dialogFactory.CreateLapDialog(copy);
        if (dlg.ShowDialog() != true) return;

        var after = TcxLapMemento.Capture(copy);
        _commandHistory.Execute(new EditLapCommand(lap, before, after));
    }

    private void DeleteLap_Click(object sender, RoutedEventArgs e)
    {
        if (_currentActivity == null || LapsGrid.SelectedItem is not TcxLap lap) return;

        if (MessageBox.Show("Delete selected lap (with its trackpoints)?",
            "Confirm deletion", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        _commandHistory.Execute(new DeleteLapCommand(_currentActivity, lap));
        TrackpointsGrid.ItemsSource = null;
    }

    private void LapsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (LapsGrid.SelectedItem is TcxLap lap)
        {
            TrackpointsGrid.ItemsSource = lap.Trackpoints;
            TrackpointsHeader.Text = $"Trackpoints — lap {_currentActivity?.Laps.IndexOf(lap) + 1}";
        }
        else
        {
            TrackpointsGrid.ItemsSource = null;
            TrackpointsHeader.Text = "Trackpoints";
        }
    }

    // ── Trackpoints (Command + Builder patterns) ───────────────────────────

    private void AddTrackpoint_Click(object sender, RoutedEventArgs e)
    {
        if (LapsGrid.SelectedItem is not TcxLap lap) return;

        var lastTime = lap.Trackpoints.Count > 0
            ? lap.Trackpoints[^1].Time.AddSeconds(1)
            : lap.StartTime;

        // Builder pattern
        var tp = new TcxTrackpointBuilder().WithTime(lastTime).Build();

        var dlg = _dialogFactory.CreateTrackpointDialog(tp);
        if (dlg.ShowDialog() != true) return;

        _commandHistory.Execute(new AddTrackpointCommand(lap, tp));
        TrackpointsGrid.SelectedItem = tp;
    }

    private void EditTrackpoint_Click(object sender, RoutedEventArgs e)
    {
        if (TrackpointsGrid.SelectedItem is not TcxTrackpoint tp) return;

        var before = TcxTrackpointMemento.Capture(tp);
        var copy = before.ToTrackpoint();

        var dlg = _dialogFactory.CreateTrackpointDialog(copy);
        if (dlg.ShowDialog() != true) return;

        var after = TcxTrackpointMemento.Capture(copy);
        _commandHistory.Execute(new EditTrackpointCommand(tp, before, after));
    }

    private void DeleteTrackpoint_Click(object sender, RoutedEventArgs e)
    {
        if (LapsGrid.SelectedItem is not TcxLap lap) return;
        if (TrackpointsGrid.SelectedItem is not TcxTrackpoint tp) return;

        _commandHistory.Execute(new DeleteTrackpointCommand(lap, tp));
    }

    private void AutoTime_Click(object sender, RoutedEventArgs e)
    {
        if (LapsGrid.SelectedItem is not TcxLap lap) return;
        if (lap.Trackpoints.Count < 2) return;

        var totalDist = lap.Trackpoints.LastOrDefault()?.DistanceMeters ?? lap.DistanceMeters;
        if (totalDist <= 0)
        {
            MessageBox.Show("Set distance for trackpoints before using this function.",
                "No data", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _commandHistory.Execute(new AutoTimeCommand(lap));
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private void NewActivity()
    {
        var activity = new TcxActivity { StartTime = DateTime.Now };
        _db.Activities.Clear();
        _db.Activities.Add(activity);
        LoadActivity(activity);
    }

    private void LoadActivity(TcxActivity activity)
    {
        ApplyActivityFromUi();
        _currentActivity = activity;
        SportCombo.SelectedItem = activity.Sport;
        if (SportCombo.SelectedItem == null) SportCombo.SelectedIndex = 0;
        StartDatePicker.SelectedDate = activity.StartTime.Date;
        StartTimePicker.Text = activity.StartTime.ToString("HH:mm:ss");
        LapsGrid.ItemsSource = activity.Laps;
        TrackpointsGrid.ItemsSource = null;
        TrackpointsHeader.Text = "Trackpoints";
    }

    private void SyncActivityToUi()
    {
        if (_currentActivity == null) return;
        StartDatePicker.SelectedDate = _currentActivity.StartTime.Date;
        StartTimePicker.Text = _currentActivity.StartTime.ToString("HH:mm:ss");
        RefreshGrids();
    }

    private void ApplyActivityFromUi()
    {
        if (_currentActivity == null) return;
        _currentActivity.Sport = (string?)SportCombo.SelectedItem ?? "Running";
        if (StartDatePicker.SelectedDate is DateTime date &&
            TimeSpan.TryParse(StartTimePicker.Text, out var time))
            _currentActivity.StartTime = date + time;
    }

    private void RefreshGrids()
    {
        LapsGrid.Items.Refresh();
        TrackpointsGrid.Items.Refresh();
    }

    private void SaveToFile(string path)
    {
        ApplyActivityFromUi();

        var issues = ValidationService.Validate(_db);
        ShowValidationResults(issues);

        int errors = issues.Count(i => i.Severity == IssueSeverity.Error);
        if (errors > 0)
        {
            ValidationExpander.IsExpanded = true;
            MessageBox.Show(
                $"File contains {errors} validation error(s).\nFix errors before saving.",
                "Validation errors — save cancelled", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        int warnings = issues.Count(i => i.Severity == IssueSeverity.Warning);
        if (warnings > 0)
        {
            ValidationExpander.IsExpanded = true;
            if (MessageBox.Show(
                $"File contains {warnings} warning(s). Save anyway?",
                "Validation warnings", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
        }

        try
        {
            _repository.Save(_db, path);
            _currentFilePath = path;
            SetDirty(false);
            SetStatus($"Saved: {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool ConfirmDiscardChanges()
    {
        if (!_isDirty) return true;
        return MessageBox.Show("You have unsaved changes. Continue?",
            "Unsaved changes", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    private void SetDirty(bool dirty)
    {
        _isDirty = dirty;
        var name = _currentFilePath != null ? Path.GetFileName(_currentFilePath) : "New file";
        Title = dirty ? $"TCX Editor — {name} *" : $"TCX Editor — {name}";
    }

    private void SetStatus(string msg) => StatusText.Text = msg;

    private void SetupKeyBindings()
    {
        Bind(Key.N, ModifierKeys.Control, New_Click);
        Bind(Key.O, ModifierKeys.Control, Open_Click);
        Bind(Key.S, ModifierKeys.Control, Save_Click);
        Bind(Key.S, ModifierKeys.Control | ModifierKeys.Shift, SaveAs_Click);
        Bind(Key.Z, ModifierKeys.Control, Undo_Click);
        Bind(Key.Y, ModifierKeys.Control, Redo_Click);
        Bind(Key.F5, ModifierKeys.None, Validate_Click);
    }

    private void Bind(Key key, ModifierKeys mod, RoutedEventHandler handler)
    {
        var cmd = new RoutedCommand();
        CommandBindings.Add(new CommandBinding(cmd, (s, e) => handler(s, e)));
        InputBindings.Add(new KeyBinding(cmd, key, mod));
    }
}
