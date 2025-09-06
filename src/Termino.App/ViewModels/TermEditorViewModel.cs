using CommunityToolkit.Mvvm.ComponentModel;
using Termino.Domain.Entities;

namespace Termino.App.ViewModels;

/// VM для окна добавления/редактирования
public partial class TermEditorViewModel : ObservableObject
{
    public TermItem Item { get; }

    public string Title => Item.CreatedAt == default
        ? "Новый термин"
        : "Изменить термин";

    public TermEditorViewModel(TermItem item)
    {
        Item = item;

        // Минимум: сейчас + 70 минут, чтобы точно пройти все проверки
        if (Item.DueAt < System.DateTimeOffset.Now.AddMinutes(70))
            Item.DueAt = System.DateTimeOffset.Now.AddMinutes(70);

        _dueDate = Item.DueAt.LocalDateTime.Date;

        _dueTimeText = Item.DueAt.ToLocalTime().ToString("HH:mm");
    }

    [ObservableProperty] 
    private System.DateTime _dueDate;

    [ObservableProperty] 
    private string _dueTimeText = "12:00";

    partial void OnDueDateChanged(DateTime value) => UpdateDueAt();

    partial void OnDueTimeTextChanged(string value) => UpdateDueAt();

    private void UpdateDueAt()
    {
        if (TimeSpan.TryParse(_dueTimeText, out var t))
        {
            var local = new DateTime(_dueDate.Year, _dueDate.Month, _dueDate.Day,
                                            t.Hours, t.Minutes, 0, DateTimeKind.Local);

            Item.DueAt = new DateTimeOffset(local);
        }
    }
}
