using System.Windows;

namespace TcxEditor.Views;

public partial class ChangeDateDialog : Window
{
    public DateTime NewDate { get; private set; }

    public ChangeDateDialog(DateTime currentDate)
    {
        InitializeComponent();
        CurrentDateText.Text = currentDate.ToString("yyyy-MM-dd");
        NewDatePicker.SelectedDate = currentDate.Date;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (NewDatePicker.SelectedDate is not DateTime d)
        {
            MessageBox.Show("Please select a valid date.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        NewDate = d.Date;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
