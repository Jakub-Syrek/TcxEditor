using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using TcxEditor.Models;
using TcxEditor.Services;
using TcxEditor.Views;

namespace TcxEditor;

public partial class MainWindow : Window
{
    private TcxDatabase _db = new();
    private TcxActivity? _currentActivity;
    private string? _currentFilePath;
    private bool _isDirty;

    public MainWindow()
    {
        InitializeComponent();
        SetupKeyBindings();
        foreach (var s in SportTypes.All) SportCombo.Items.Add(s);
        NewActivity();
    }

    // ── File operations ────────────────────────────────────────────────────

    private void New_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;
        _currentFilePath = null;
        _db = new TcxDatabase();
        NewActivity();
        ClearValidation();
        SetDirty(false);
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;

        var dlg = new OpenFileDialog
        {
            Filter = "Pliki TCX (*.tcx)|*.tcx|Wszystkie pliki (*.*)|*.*",
            Title = "Otwórz plik TCX"
        };
        if (dlg.ShowDialog() != true) return;

        try
        {
            _db = TcxService.Load(dlg.FileName);
            _currentFilePath = dlg.FileName;

            if (_db.Activities.Count > 0)
                LoadActivity(_db.Activities[0]);
            else
                NewActivity();

            SetDirty(false);
            SetStatus($"Otwarto: {Path.GetFileName(dlg.FileName)}");

            var issues = ValidationService.Validate(_db);
            ShowValidationResults(issues);

            if (issues.Count > 0)
                ValidationExpander.IsExpanded = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd podczas otwierania pliku:\n{ex.Message}", "Błąd odczytu",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_currentFilePath == null)
            SaveAs_Click(sender, e);
        else
            SaveToFile(_currentFilePath);
    }

    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Pliki TCX (*.tcx)|*.tcx",
            Title = "Zapisz plik TCX",
            DefaultExt = ".tcx",
            FileName = _currentFilePath != null
                ? Path.GetFileName(_currentFilePath)
                : "aktywnosc.tcx"
        };
        if (dlg.ShowDialog() != true) return;
        SaveToFile(dlg.FileName);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;
        Application.Current.Shutdown();
    }

    // ── Validation ─────────────────────────────────────────────────────────

    private void Validate_Click(object sender, RoutedEventArgs e)
    {
        ApplyActivityFromUi();
        var issues = ValidationService.Validate(_db);
        ShowValidationResults(issues);
        ValidationExpander.IsExpanded = true;

        if (issues.Count == 0)
            SetStatus("Walidacja zakończona — brak problemów.");
    }

    private void ShowValidationResults(List<ValidationIssue> issues)
    {
        ValidationGrid.ItemsSource = issues;

        int errors = issues.Count(i => i.Severity == IssueSeverity.Error);
        int warnings = issues.Count(i => i.Severity == IssueSeverity.Warning);

        if (issues.Count == 0)
        {
            ValidationSummaryText.Text = "Walidacja: OK";
            ValidationSummaryText.Foreground = System.Windows.Media.Brushes.Green;
        }
        else
        {
            var parts = new List<string>();
            if (errors > 0) parts.Add($"{errors} błąd(-ów)");
            if (warnings > 0) parts.Add($"{warnings} ostrzeżenie(-ń)");
            ValidationSummaryText.Text = "Walidacja: " + string.Join(", ", parts);
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

    // ── Date / time operations ─────────────────────────────────────────────

    private void ChangeDate_Click(object sender, RoutedEventArgs e)
    {
        if (_currentActivity == null) return;
        ApplyActivityFromUi();

        var dlg = new ChangeDateDialog(_currentActivity.StartTime) { Owner = this };
        if (dlg.ShowDialog() != true) return;

        var offset = dlg.NewDate - _currentActivity.StartTime.Date;
        if (offset == TimeSpan.Zero) return;

        ShiftAllTimestamps(offset);
        SetStatus($"Zmieniono datę o {(int)offset.TotalDays} dzień(-ni).");
    }

    private void ShiftTime_Click(object sender, RoutedEventArgs e)
    {
        if (_currentActivity == null) return;
        ApplyActivityFromUi();

        var dlg = new ShiftTimeDialog { Owner = this };
        if (dlg.ShowDialog() != true) return;
        if (dlg.Offset == TimeSpan.Zero) return;

        ShiftAllTimestamps(dlg.Offset);

        var abs = dlg.Offset.Duration();
        var sign = dlg.Offset < TimeSpan.Zero ? "cofnięto" : "przesunięto";
        SetStatus($"Czas {sign} o {abs.Hours:D2}:{abs.Minutes:D2}:{abs.Seconds:D2}.");
    }

    private void ShiftAllTimestamps(TimeSpan offset)
    {
        _currentActivity!.StartTime += offset;

        foreach (var lap in _currentActivity.Laps)
        {
            lap.StartTime += offset;
            foreach (var tp in lap.Trackpoints)
                tp.Time += offset;
        }

        StartDatePicker.SelectedDate = _currentActivity.StartTime.Date;
        StartTimePicker.Text = _currentActivity.StartTime.ToString("HH:mm:ss");
        LapsGrid.Items.Refresh();
        TrackpointsGrid.Items.Refresh();
        SetDirty(true);
    }

    // ── Laps ───────────────────────────────────────────────────────────────

    private void AddLap_Click(object sender, RoutedEventArgs e)
    {
        if (_currentActivity == null) return;

        var lap = new TcxLap { StartTime = _currentActivity.StartTime };
        var dlg = new LapDialog(lap) { Owner = this };
        if (dlg.ShowDialog() != true) return;

        _currentActivity.Laps.Add(lap);
        LapsGrid.SelectedItem = lap;
        SetDirty(true);
    }

    private void EditLap_Click(object sender, RoutedEventArgs e)
    {
        if (LapsGrid.SelectedItem is not TcxLap lap) return;

        var copy = CloneLap(lap);
        var dlg = new LapDialog(copy) { Owner = this };
        if (dlg.ShowDialog() != true) return;

        CopyLapFields(copy, lap);
        SetDirty(true);
        LapsGrid.Items.Refresh();
    }

    private void DeleteLap_Click(object sender, RoutedEventArgs e)
    {
        if (_currentActivity == null) return;
        if (LapsGrid.SelectedItem is not TcxLap lap) return;

        if (MessageBox.Show("Usunąć zaznaczone okrążenie (wraz z punktami trasy)?",
            "Potwierdź usunięcie", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        _currentActivity.Laps.Remove(lap);
        TrackpointsGrid.ItemsSource = null;
        SetDirty(true);
    }

    private void LapsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (LapsGrid.SelectedItem is TcxLap lap)
        {
            TrackpointsGrid.ItemsSource = lap.Trackpoints;
            TrackpointsHeader.Text = $"Punkty trasy — okrążenie {_currentActivity?.Laps.IndexOf(lap) + 1}";
        }
        else
        {
            TrackpointsGrid.ItemsSource = null;
            TrackpointsHeader.Text = "Punkty trasy";
        }
    }

    // ── Trackpoints ────────────────────────────────────────────────────────

    private void AddTrackpoint_Click(object sender, RoutedEventArgs e)
    {
        if (LapsGrid.SelectedItem is not TcxLap lap) return;

        var lastTime = lap.Trackpoints.Count > 0
            ? lap.Trackpoints[^1].Time.AddSeconds(1)
            : lap.StartTime;

        var tp = new TcxTrackpoint { Time = lastTime };
        var dlg = new TrackpointDialog(tp) { Owner = this };
        if (dlg.ShowDialog() != true) return;

        lap.Trackpoints.Add(tp);
        TrackpointsGrid.SelectedItem = tp;
        SetDirty(true);
    }

    private void EditTrackpoint_Click(object sender, RoutedEventArgs e)
    {
        if (TrackpointsGrid.SelectedItem is not TcxTrackpoint tp) return;

        var copy = CloneTrackpoint(tp);
        var dlg = new TrackpointDialog(copy) { Owner = this };
        if (dlg.ShowDialog() != true) return;

        CopyTrackpointFields(copy, tp);
        SetDirty(true);
        TrackpointsGrid.Items.Refresh();
    }

    private void DeleteTrackpoint_Click(object sender, RoutedEventArgs e)
    {
        if (LapsGrid.SelectedItem is not TcxLap lap) return;
        if (TrackpointsGrid.SelectedItem is not TcxTrackpoint tp) return;

        lap.Trackpoints.Remove(tp);
        SetDirty(true);
    }

    private void AutoTime_Click(object sender, RoutedEventArgs e)
    {
        if (LapsGrid.SelectedItem is not TcxLap lap) return;
        if (lap.Trackpoints.Count < 2) return;

        var points = lap.Trackpoints;
        var totalDist = points.LastOrDefault()?.DistanceMeters ?? lap.DistanceMeters;

        if (totalDist <= 0)
        {
            MessageBox.Show("Ustaw dystans (DistanceMeters) dla punktów trasy przed użyciem tej funkcji.",
                "Brak danych", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        for (int i = 0; i < points.Count; i++)
        {
            var dist = points[i].DistanceMeters ?? 0;
            points[i].Time = lap.StartTime.AddSeconds((dist / totalDist) * lap.TotalTimeSeconds);
        }

        TrackpointsGrid.Items.Refresh();
        SetDirty(true);
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
        TrackpointsHeader.Text = "Punkty trasy";
    }

    private void ApplyActivityFromUi()
    {
        if (_currentActivity == null) return;
        _currentActivity.Sport = (string?)SportCombo.SelectedItem ?? "Running";
        if (StartDatePicker.SelectedDate is DateTime date &&
            TimeSpan.TryParse(StartTimePicker.Text, out var time))
            _currentActivity.StartTime = date + time;
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
                $"Plik zawiera {errors} błąd(-ów) walidacji.\n" +
                "Popraw błędy (czerwone wpisy w panelu Walidacja) przed zapisem.\n\n" +
                "Ostrzeżenia (żółte) nie blokują zapisu.",
                "Błędy walidacji — zapis anulowany",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        int warnings = issues.Count(i => i.Severity == IssueSeverity.Warning);
        if (warnings > 0)
        {
            ValidationExpander.IsExpanded = true;
            var result = MessageBox.Show(
                $"Plik zawiera {warnings} ostrzeżenie(-ń) walidacji.\n" +
                "Garmin może zignorować dane z nieprawidłowymi wartościami.\n\n" +
                "Czy mimo to zapisać plik?",
                "Ostrzeżenia walidacji",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
        }

        try
        {
            TcxService.Save(_db, path);
            _currentFilePath = path;
            SetDirty(false);
            SetStatus($"Zapisano: {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd podczas zapisywania:\n{ex.Message}", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool ConfirmDiscardChanges()
    {
        if (!_isDirty) return true;
        return MessageBox.Show("Masz niezapisane zmiany. Czy chcesz kontynuować bez zapisywania?",
            "Niezapisane zmiany", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    private void SetDirty(bool dirty)
    {
        _isDirty = dirty;
        var name = _currentFilePath != null ? Path.GetFileName(_currentFilePath) : "Nowy plik";
        Title = dirty ? $"Edytor TCX — {name} *" : $"Edytor TCX — {name}";
    }

    private void SetStatus(string msg) => StatusText.Text = msg;

    private void SetupKeyBindings()
    {
        var newCmd = new RoutedCommand();
        CommandBindings.Add(new CommandBinding(newCmd, New_Click));
        InputBindings.Add(new KeyBinding(newCmd, Key.N, ModifierKeys.Control));

        var openCmd = new RoutedCommand();
        CommandBindings.Add(new CommandBinding(openCmd, Open_Click));
        InputBindings.Add(new KeyBinding(openCmd, Key.O, ModifierKeys.Control));

        var saveCmd = new RoutedCommand();
        CommandBindings.Add(new CommandBinding(saveCmd, Save_Click));
        InputBindings.Add(new KeyBinding(saveCmd, Key.S, ModifierKeys.Control));

        var saveAsCmd = new RoutedCommand();
        CommandBindings.Add(new CommandBinding(saveAsCmd, SaveAs_Click));
        InputBindings.Add(new KeyBinding(saveAsCmd, Key.S, ModifierKeys.Control | ModifierKeys.Shift));

        var validateCmd = new RoutedCommand();
        CommandBindings.Add(new CommandBinding(validateCmd, Validate_Click));
        InputBindings.Add(new KeyBinding(validateCmd, Key.F5, ModifierKeys.None));
    }

    // ── Clone helpers ──────────────────────────────────────────────────────

    private static TcxLap CloneLap(TcxLap src) => new()
    {
        StartTime = src.StartTime,
        TotalTimeSeconds = src.TotalTimeSeconds,
        DistanceMeters = src.DistanceMeters,
        MaximumSpeed = src.MaximumSpeed,
        Calories = src.Calories,
        AverageHeartRate = src.AverageHeartRate,
        MaximumHeartRate = src.MaximumHeartRate,
        Intensity = src.Intensity,
        Notes = src.Notes
    };

    private static void CopyLapFields(TcxLap src, TcxLap dst)
    {
        dst.StartTime = src.StartTime;
        dst.TotalTimeSeconds = src.TotalTimeSeconds;
        dst.DistanceMeters = src.DistanceMeters;
        dst.MaximumSpeed = src.MaximumSpeed;
        dst.Calories = src.Calories;
        dst.AverageHeartRate = src.AverageHeartRate;
        dst.MaximumHeartRate = src.MaximumHeartRate;
        dst.Intensity = src.Intensity;
        dst.Notes = src.Notes;
    }

    private static TcxTrackpoint CloneTrackpoint(TcxTrackpoint src) => new()
    {
        Time = src.Time,
        LatitudeDegrees = src.LatitudeDegrees,
        LongitudeDegrees = src.LongitudeDegrees,
        AltitudeMeters = src.AltitudeMeters,
        DistanceMeters = src.DistanceMeters,
        HeartRateBpm = src.HeartRateBpm,
        Cadence = src.Cadence,
        Speed = src.Speed
    };

    private static void CopyTrackpointFields(TcxTrackpoint src, TcxTrackpoint dst)
    {
        dst.Time = src.Time;
        dst.LatitudeDegrees = src.LatitudeDegrees;
        dst.LongitudeDegrees = src.LongitudeDegrees;
        dst.AltitudeMeters = src.AltitudeMeters;
        dst.DistanceMeters = src.DistanceMeters;
        dst.HeartRateBpm = src.HeartRateBpm;
        dst.Cadence = src.Cadence;
        dst.Speed = src.Speed;
    }
}
