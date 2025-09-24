using CommunityToolkit.Mvvm.ComponentModel;
using Termino.Domain.Entities;

namespace Termino.App.ViewModels;

/// VM для окна «Обновить» (ре-открыть)
public partial class ReopenViewModel : ObservableObject
{
    public DateTime DueDate
    {
        get => _dueDate;
        set 
        { 
            SetProperty( ref _dueDate, value );
            Update(); 
        }
    }

    public string DueTimeText
    {
        get => _dueTimeText;
        set 
        {
            SetProperty( ref _dueTimeText, value );
            Update();
        }
    }

    public DateTimeOffset NewDueAt { get; private set; }

    private DateTime _dueDate;

    private string _dueTimeText = "12:00";

    public ReopenViewModel( TermItem _ )
    {
        var baseTime = DateTimeOffset.Now.AddMinutes( 70 );

        _dueDate = baseTime.LocalDateTime.Date;

        _dueTimeText = baseTime.ToLocalTime().ToString( "HH:mm" );

        Update();
    }

    private void Update()
    {
        if (TimeSpan.TryParse( _dueTimeText, out var t ))
        {
            var local = new DateTime(
                _dueDate.Year,
                _dueDate.Month,
                _dueDate.Day, 
                t.Hours, 
                t.Minutes,
                0,
                DateTimeKind.Local
                );

            NewDueAt = new DateTimeOffset( local );
        }
    }
}
