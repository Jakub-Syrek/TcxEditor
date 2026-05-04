using System.Windows;
using TcxEditor.Models;

namespace TcxEditor.Views;

public partial class TrackpointDialog : Window
{
    public TcxTrackpoint Trackpoint { get; }

    public TrackpointDialog(TcxTrackpoint tp)
    {
        InitializeComponent();
        Trackpoint = tp;

        Time.Text = tp.Time.ToString("yyyy-MM-dd HH:mm:ss");
        Lat.Text = tp.LatitudeDegrees?.ToString("F7") ?? "";
        Lon.Text = tp.LongitudeDegrees?.ToString("F7") ?? "";
        Altitude.Text = tp.AltitudeMeters?.ToString("F1") ?? "";
        Distance.Text = tp.DistanceMeters?.ToString("F2") ?? "";
        HeartRate.Text = tp.HeartRateBpm?.ToString() ?? "";
        Cadence.Text = tp.Cadence?.ToString() ?? "";
        Speed.Text = tp.Speed?.ToString("F3") ?? "";
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
        if (!DateTime.TryParseExact(Time.Text.Trim(), "yyyy-MM-dd HH:mm:ss",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var time))
        {
            MessageBox.Show("Czas musi być w formacie RRRR-MM-DD HH:mm:ss", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        Trackpoint.Time = time;

        Trackpoint.LatitudeDegrees = ParseNullableDouble(Lat.Text);
        Trackpoint.LongitudeDegrees = ParseNullableDouble(Lon.Text);

        if (Trackpoint.LatitudeDegrees.HasValue &&
            (Trackpoint.LatitudeDegrees < -90 || Trackpoint.LatitudeDegrees > 90))
        {
            MessageBox.Show("Szerokość geograficzna musi być między -90 a 90.", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (Trackpoint.LongitudeDegrees.HasValue &&
            (Trackpoint.LongitudeDegrees < -180 || Trackpoint.LongitudeDegrees > 180))
        {
            MessageBox.Show("Długość geograficzna musi być między -180 a 180.", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        Trackpoint.AltitudeMeters = ParseNullableDouble(Altitude.Text);
        Trackpoint.DistanceMeters = ParseNullableDouble(Distance.Text);
        Trackpoint.HeartRateBpm = ParseNullableInt(HeartRate.Text);
        Trackpoint.Cadence = ParseNullableInt(Cadence.Text);
        Trackpoint.Speed = ParseNullableDouble(Speed.Text);

        return true;
    }

    private static double? ParseNullableDouble(string s) =>
        string.IsNullOrWhiteSpace(s) ? null :
        double.TryParse(s.Trim(), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out var v) ? v : null;

    private static int? ParseNullableInt(string s) =>
        string.IsNullOrWhiteSpace(s) ? null :
        int.TryParse(s.Trim(), out var v) ? v : null;
}
