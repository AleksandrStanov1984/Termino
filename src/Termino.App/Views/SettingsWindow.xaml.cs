using System.Collections.ObjectModel;
using System.Windows;

namespace Termino.App;

public partial class SettingsWindow : Window
{
    public ObservableCollection<string> Emails { get; } = new();

    public bool Use12h { get; set; }

    public SettingsWindow()
    {
        InitializeComponent();

        var s = App.Settings!.Value;

        foreach (var e in s.Emails)
            Emails.Add(e);

        while (Emails.Count < 3)
            Emails.Add(string.Empty);

        Use12h = s.Use12h;

        DataContext = this;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var s = App.Settings!.Value;

        s.Emails = new(Enumerable.Take(Emails, 3));

        s.Use12h = Use12h;

        App.Settings!.Save();

        DialogResult = true;

        Close();
    }
}

public class InverseBoolConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => value is bool b ? !b : value;

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => value is bool b ? !b : value;
}
