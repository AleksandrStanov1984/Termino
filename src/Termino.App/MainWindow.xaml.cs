using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Termino.App.ViewModels;
using Termino.Domain.Entities;
using System.Threading.Tasks;

namespace Termino.App;

public partial class MainWindow:Window
{
    public MainWindow()
    { 
        InitializeComponent();

        Loaded += async (_, __) =>
        {
            if ( DataContext is MainViewModel vm )
            {
                await vm.Refresh();

                vm.ApplyFilter();
            }
        };

        // Time
        void SetNow() => NowText.Text = DateTime.Now.ToString( "HH:mm" );

        SetNow();

        var timer = new DispatcherTimer 
        { 
            Interval = TimeSpan.FromSeconds(30)
        };

        timer.Tick += (_, __) => SetNow();

        timer.Start();
    }

    private void DataGridRow_OnClick( object sender, MouseButtonEventArgs e )
    {
        if ( FindParent<DataGridColumnHeader>( e.OriginalSource as DependencyObject ) != null)
            return;

        if ( FindParent<Button>( e.OriginalSource as DependencyObject ) != null)
            return;

        if ( DataContext is not MainViewModel vm ) 
            return;

        if ( sender is not DataGridRow row || row.Item is not TermItem item )
            return;

        vm.SelectedItem = item;

        if ( vm.EditSelectedCommand.CanExecute( null ) )
        {
            vm.EditSelectedCommand.Execute( null );

            e.Handled = true;
        }
    }

    private static T? FindParent<T>( DependencyObject? child ) where T : DependencyObject
    {
        while ( child != null )
        {
            if ( child is T t )
                return t;

            child = VisualTreeHelper.GetParent( child );
        }

        return null;
    }
}