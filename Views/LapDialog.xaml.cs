using System.Windows;
using TcxEditor.Models;

namespace TcxEditor.Views;

public partial class LapDialog : Window
{
    public TcxLap Lap { get; }

    public LapDialog(TcxLap lap)
    {
        InitializeComponent();
        Lap = lap;

        foreach (var v in IntensityTypes.All) Intensity.Items.Add(v);

        StartDate.SelectedDate = lap.StartTime.Date;
        StartTime.Text = lap.StartTime.ToString("HH:mm:ss");
        TotalTime.Text = lap.TotalTimeSeconds.ToString("F1");
        Distance.Text = lap.DistanceMeters.ToString("F2");
        MaxSpeed.Text = lap.MaximumSpeed.ToString("F3");
        Calories.Text = lap.Calories.ToString();
        AvgHr.Text = lap.AverageHeartRate?.ToString() ?? "";
        MaxHr.Text = lap.MaximumHeartRate?.ToString() ?? "";
        Intensity.SelectedItem = lap.Intensity;
        Notes.Text = lap.Notes;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (!TryApply())
            return;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private bool TryApply()
    {
        if (StartDate.SelectedDate is not DateTime date)
        {
            MessageBox.Show("Podaj prawidłową datę.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!TimeSpan.TryParseExact(StartTime.Text.Trim(), @"hh\:mm\:ss", null, out var time) &&
            !TimeSpan.TryParseExact(StartTime.Text.Trim(), @"h\:mm\:ss", null, out time))
        {
            MessageBox.Show("Godzina musi być w formacie HH:mm:ss.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        Lap.StartTime = date + time;

        if (!double.TryParse(TotalTime.Text, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out var totalSec) || totalSec < 0)
        {
            MessageBox.Show("Czas całkowity musi być liczbą >= 0.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        Lap.TotalTimeSeconds = totalSec;

        if (!double.TryParse(Distance.Text, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out var dist) || dist < 0)
        {
            MessageBox.Show("Dystans musi być liczbą >= 0.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        Lap.DistanceMeters = dist;

        if (double.TryParse(MaxSpeed.Text, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out var spd))
            Lap.MaximumSpeed = spd;

        if (int.TryParse(Calories.Text, out var cal)) Lap.Calories = cal;

        Lap.AverageHeartRate = int.TryParse(AvgHr.Text, out var avgHr) ? avgHr : null;
        Lap.MaximumHeartRate = int.TryParse(MaxHr.Text, out var maxHr) ? maxHr : null;
        Lap.Intensity = (string?)Intensity.SelectedItem ?? "Active";
        Lap.Notes = Notes.Text;

        return true;
    }
}
