using System.Windows;

namespace Termino.App;

public partial class ReopenWindow : Window
{
    public ReopenWindow()
    {
        InitializeComponent();
    }

    private void Ok_Click(object s, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ReopenViewModel vm)
        {
            var min = DateTimeOffset.Now.AddHours(1);
            if (vm.NewDueAt < min)
            {
                MessageBox.Show("Нельзя указать время раньше чем через 1 час.",
                                "Валидация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        DialogResult = true;

        Close();
    }
}
