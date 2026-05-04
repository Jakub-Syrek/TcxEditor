using System.Windows;
using System.Windows.Controls;

namespace TcxEditor.Views;

public partial class ShiftTimeDialog : Window
{
    public TimeSpan Offset { get; private set; }

    public ShiftTimeDialog()
    {
        InitializeComponent();
        Hours.TextChanged += UpdatePreview;
        Minutes.TextChanged += UpdatePreview;
        Seconds.TextChanged += UpdatePreview;
        UpdatePreview(null, null);
    }

    private void UpdatePreview(object? sender, TextChangedEventArgs? e)
    {
        if (!TryGetOffset(out var offset))
        {
            PreviewText.Text = "Podaj liczby całkowite (mogą być ujemne).";
            return;
        }

        if (offset == TimeSpan.Zero)
        {
            PreviewText.Text = "Brak przesunięcia.";
            return;
        }

        var sign = offset < TimeSpan.Zero ? "cofnięcie o" : "przesunięcie do przodu o";
        var abs = offset.Duration();
        var parts = new List<string>();
        if (abs.Hours != 0) parts.Add($"{abs.Hours} godz.");
        if (abs.Minutes != 0) parts.Add($"{abs.Minutes} min.");
        if (abs.Seconds != 0) parts.Add($"{abs.Seconds} sek.");
        PreviewText.Text = parts.Count > 0 ? $"Wynik: {sign} {string.Join(" ", parts)}" : "Brak przesunięcia.";
    }

    private bool TryGetOffset(out TimeSpan offset)
    {
        offset = TimeSpan.Zero;
        if (!int.TryParse(Hours.Text?.Trim(), out var h)) return false;
        if (!int.TryParse(Minutes.Text?.Trim(), out var m)) return false;
        if (!int.TryParse(Seconds.Text?.Trim(), out var s)) return false;
        offset = TimeSpan.FromSeconds(h * 3600 + m * 60 + s);
        return true;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetOffset(out var offset))
        {
            MessageBox.Show("Podaj liczby całkowite w polach Godziny, Minuty i Sekundy.", "Błąd",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Offset = offset;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
