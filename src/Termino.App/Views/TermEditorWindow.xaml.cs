using System.Windows;

namespace Termino.App;

public partial class TermEditorWindow : Window
{
    public TermEditorWindow()
    {
        InitializeComponent();
    }

    private void Save_Click(object s, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.TermEditorViewModel vm)
        {
            var min = DateTimeOffset.Now.AddHours(1);
            if (vm.Item.DueAt < min)
            {
                MessageBox.Show("Минимально допустимое время — не раньше, чем через 1 час от текущего.",
                                "Валидация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        DialogResult = true;

        Close();
    }
}
