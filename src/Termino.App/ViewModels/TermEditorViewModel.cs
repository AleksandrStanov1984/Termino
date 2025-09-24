using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Termino.Domain.Entities;
using Termino.Domain.Enums;

namespace Termino.App.ViewModels;

// VM редактора/добавления термина
public partial class TermEditorViewModel : ObservableValidator
{
    private readonly bool _isEdit;

    public TermEditorViewModel( TermItem? item = null )
    {
        _isEdit = item != null;

        if ( item == null )
        {
            // значения по умолчанию: + 1ч 10м
            var min = DateTime.Now.AddMinutes( 70 );

            DueDate = min.Date;

            DueTime = min.TimeOfDay;

            Status = TermStatus.Pending;
        }
        else
        {
            Id = item.Id;

            Title = item.Title;

            Description = item.Description;

            DueDate = item.DueAt.LocalDateTime.Date;

            DueTime = item.DueAt.LocalDateTime.TimeOfDay;

            Status = item.Status;
        }

        // один раз валидируем стартовые значения (чтобы сразу подсветка/кнопка)
        ValidateAllProperties();

        SaveCommand.NotifyCanExecuteChanged();
    }

    public Guid Id { get; private set; }

    [ObservableProperty]
    [Required(ErrorMessage = "Введите название")]
    [StringLength(50, MinimumLength = 5,
    ErrorMessage = "Название должно быть от 5 до 50 символов")]
    private string? title;

    [ObservableProperty]
    [Required(ErrorMessage = "Введите описание")]
    [StringLength(200, MinimumLength = 10,
    ErrorMessage = "Описание должно быть от 10 до 500 символов")]
    private string? description;

    [ObservableProperty]
    [Required(ErrorMessage = "Выберите дату")]
    private DateTime? dueDate;

    [ObservableProperty]
    [Required(ErrorMessage = "Выберите время")]
    private TimeSpan? dueTime;

    [ObservableProperty]
    private TermStatus status = TermStatus.Pending;

    // Автовалидация и обновление доступности кнопки Сохранить
    partial void OnTitleChanged( string? value )
    {
        ValidateProperty( value, nameof( Title ));

        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnDescriptionChanged( string? value )
    {
        ValidateProperty( value, nameof( Description ));

        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnDueDateChanged( DateTime? value )
    {
        ValidateProperty( value, nameof( DueDate ));

        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnDueTimeChanged( TimeSpan? value )
    {
        ValidateProperty( value, nameof( DueTime ));

        SaveCommand.NotifyCanExecuteChanged();
    }

    private DateTimeOffset BuildDueAtLocal()
    {
        // Дата и время — обязательны, проверены выше
        var local = DateTime.SpecifyKind( DueDate!.Value.Date + DueTime!.Value, DateTimeKind.Local );

        return new DateTimeOffset( local );
    }

    private bool ValidateBusinessRules(out string? error)
    {
        // 1) дата/время уже проверены атрибутами Required
        // 2) минимум через 1ч10м
        var min = DateTimeOffset.Now.AddMinutes( 70 );

        var due = BuildDueAtLocal();

        if ( due < min )
        {
            error = $"Минимальное время: не раньше {min:dd.MM.yyyy HH:mm} ";

            return false;
        }

        error = null;

        return true;
    }

    private string CollectAllErrors()
    {
        var sb = new StringBuilder();

        foreach ( var kv in GetErrors().GroupBy( e => e.MemberNames.FirstOrDefault() ?? string.Empty ))
        {
            foreach ( var err in kv )
                sb.AppendLine( err.ErrorMessage );
        }

        return sb.ToString().Trim();
    }

    public TermItem ToEntity()
    {
        return new TermItem
        {
            Id = Id == Guid.Empty ? Guid.NewGuid() : Id,
            Title = Title!.Trim(),
            Description = Description!.Trim(),
            DueAt = BuildDueAtLocal(),
            Status = Status,
        };
    }

    // --- Команды ---

    [RelayCommand(CanExecute = nameof( CanSave ))]
    private void Save()
    {
        // подрезаем пробелы, чтобы "пять пробелов" не считались валидными
        Title = Title?.Trim();

        Description = Description?.Trim();

        // Проверяем DataAnnotations
        ValidateAllProperties();

        // И бизнес-правила
        string? businessError = null;

        var isBusinessOk = ValidateBusinessRules( out businessError );

        if (HasErrors || !isBusinessOk)
        {
            var text = !string.IsNullOrWhiteSpace( businessError )
                ? businessError!
                : CollectAllErrors();

            MessageBox.Show( text, "Вы заполнили не все поля !",
                MessageBoxButton.OK, MessageBoxImage.Warning );

            return;
        }

        // Всё ок — закрываем окно с DialogResult=true
        CloseWithResult( true );
    }

    private bool CanSave() =>
        !HasErrors &&
        !string.IsNullOrWhiteSpace( Title ) &&
        !string.IsNullOrWhiteSpace( Description ) &&
        DueDate is not null &&
        DueTime is not null;

    [RelayCommand]
    private void Cancel() => CloseWithResult( false );

    // аккуратное закрытие через владельца окна
    private void CloseWithResult( bool result )
    {
        var win = Application.Current?.Windows.OfType<Window>()
                      .FirstOrDefault( w => ReferenceEquals( w.DataContext, this ));

        if ( win is null ) 
            return;

        win.DialogResult = result;

        win.Close();
    }
}
