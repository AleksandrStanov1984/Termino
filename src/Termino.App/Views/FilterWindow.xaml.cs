using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Termino.App.Models;

namespace Termino.App
{
    public partial class FilterWindow : Window
    {
        private readonly ListCollectionView _view;

        private readonly FilterState _state;

        private readonly Action _apply;

        private bool _autoCloseEnabled = true;

        public FilterWindow( ListCollectionView view, FilterState state, Action apply )
        {
            InitializeComponent();

            _view = view;

            _state = state;

            _apply = apply;

            DataContext = _state;

            // подписываемся в явный метод
            Deactivated += FilterWindow_Deactivated;

            // восстановим панели по текущему состоянию
            TogglePanels();

            // при штатном закрытии (крестик) — отписываемся, чтобы не дернуть Close ещё раз
            Closing += (_, __) =>
            {
                _autoCloseEnabled = false;

                Deactivated -= FilterWindow_Deactivated;
            };
        }

        private void FilterWindow_Deactivated(object? sender, EventArgs e)
        {
            if ( !_autoCloseEnabled )
                return;

            // Чтобы не вызвать Close во время Deactivated/Closing и не ловить InvalidOperationException,
            // переносим вызов на следующий тик диспетчера.
            _autoCloseEnabled = false; // чтобы не войти сюда повторно

            Dispatcher.BeginInvoke( new Action(() =>
            {
                try
                {
                    Deactivated -= FilterWindow_Deactivated; // на всякий случай

                    Close();
                }
                catch {
                    /* игнор: окно уже может быть в процессе закрытия */ 
                }
            }),

            DispatcherPriority.Background);
        }

        // Любое изменение чекбоксов — сразу применяем фильтр
        private void AnyChanged( object sender, RoutedEventArgs e ) => _apply();

        private void DatePicker_SelectedDateChanged( object sender, SelectionChangedEventArgs e )
        {
            // Если выбрали дату — включаем режим "По дате"
            _state.Mode = FilterMode.ByDate;

            TogglePanels();

            _apply();
        }

        private void ByDate_Checked( object sender, RoutedEventArgs e )
        {
            _state.Mode = FilterMode.ByDate;

            TogglePanels();

            _apply();
        }

        private void ByDate_Unchecked( object sender, RoutedEventArgs e )
        {
            if ( _state.Mode == FilterMode.ByDate )
                _state.Mode = FilterMode.None;

            TogglePanels();

            _apply();
        }

        private void ByStatus_Checked( object sender, RoutedEventArgs e )
        {
            _state.Mode = FilterMode.ByStatus;

            TogglePanels();

            _apply();
        }

        private void ByStatus_Unchecked( object sender, RoutedEventArgs e )
        {
            if ( _state.Mode == FilterMode.ByStatus ) 
                _state.Mode = FilterMode.None;

            TogglePanels();

            _apply();
        }

        private void TogglePanels()
        {
            DatePanel.Visibility = _state.Mode == FilterMode.ByDate ? Visibility.Visible : Visibility.Collapsed;
           
            StatusPanel.Visibility = _state.Mode == FilterMode.ByStatus ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    // конвертер сравнения enum-значения для биндинга чекбоксов к FilterState.Mode
    public class EnumEqualsConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
            => value?.ToString() == parameter?.ToString();

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
            => ( value is bool b && b && parameter is string s &&
                Enum.TryParse( typeof( FilterMode ), s, out var res ))
               ? res! : Binding.DoNothing;
    }
}
