using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Termino.App.Models;
using Termino.Data.Repositories;
using Termino.Domain.Entities;
using Termino.Domain.Enums;
using Termino.Localization.Core;
using System.Windows;

namespace Termino.App.ViewModels;
public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<TermItem> Items { get; } = new();

    public ListCollectionView View { get; }

    [ObservableProperty] 
    private TermItem? selectedItem;

    [ObservableProperty] 
    private string? bannerText;

    [ObservableProperty] 
    private bool isBannerVisible;

    public Brush BannerBrush { get; private set; } = Brushes.Transparent;

    public FilterState Filter { get; } = new();

    private Window? _filterWindow;

    private readonly DispatcherTimer _timer = new();

    public MainViewModel()
    {
        View = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);

        ApplyFilter();

        _timer.Interval = TimeSpan.FromSeconds(1);

        _timer.Tick += (_, __) => IsBannerVisible = false; // или твоя логика обновления

        _timer.Start();
    }

    private static DateOnly AsLocalDate(DateTimeOffset dto)
    => DateOnly.FromDateTime(dto.ToLocalTime().DateTime);

    private static DateOnly AsLocalDate(DateTime date)
        => DateOnly.FromDateTime(date);

    public void ApplyFilter()
    {
        if (View is null) return;
        var f = Filter;

        View.Filter = o =>
        {
            if (o is not TermItem t) return true;

            if (!f.ShowClosed && t.Status == TermStatus.Completed)
                return false;

            if (f.Mode == FilterMode.ByDate && f.Date.HasValue)
            {
                var left = AsLocalDate(t.DueAt);
                var right = AsLocalDate(f.Date.Value);
                return left == right;
            }

            if (f.Mode == FilterMode.ByStatus)
            {
                var days = (t.DueAt - DateTimeOffset.Now).TotalDays;
                bool urgent = days <= 7;
                bool soon = days > 7 && days <= 15;
                bool later = days > 15;

                bool any = f.Urgent || f.Soon || f.Later;
                return !any || (f.Urgent && urgent) || (f.Soon && soon) || (f.Later && later);
            }

            return true;
        };

        View.Refresh();
    }


    [RelayCommand]
    private void OpenFilter()
    {
        // не открывать вторую копию
        if (_filterWindow is { IsVisible: true })
        {
            _filterWindow.Activate();

            return;
        }

        // создаём окно и передаём callback, который сразу применяет фильтр
        _filterWindow = new FilterWindow(View, Filter, ApplyFilter)
        {
            Owner = Application.Current.MainWindow,
            Topmost = false
        };

        // немодально, чтобы можно было кликнуть "вне" и закрыть
        _filterWindow.Show();
    }

    private void ShowBanner(string text, Color color)
    {
        BannerText=text; BannerBrush=new SolidColorBrush(color); OnPropertyChanged(nameof(BannerBrush));

        IsBannerVisible=true; _timer.Stop(); _timer.Start();
    }

    [RelayCommand]
    public async Task Refresh()
    {
        using var scope = App.AppHost!.Services.CreateScope();

        var repo=scope.ServiceProvider.GetRequiredService<ITermRepository>();

        var list=await repo.GetAllAsync();

        Items.Clear(); foreach(var t in list) Items.Add(t);

        View.Refresh();
    }

    [RelayCommand] 
    private void SetLangRu()
    {
        Localizer.Instance.Load("Ru"); OnPropertyChanged(string.Empty); 
    }

    [RelayCommand]
    private void SetLangEn()
    {
        Localizer.Instance.Load("En"); OnPropertyChanged(string.Empty); 
    }

    [RelayCommand] 
    private void SetLangDe()
    { 
        Localizer.Instance.Load("De"); OnPropertyChanged(string.Empty);
    }

    [RelayCommand] 
    private void OpenSettings()
    { var w=new SettingsWindow{
        Owner = Application.Current.MainWindow }; w.ShowDialog(); 
    }

    [RelayCommand] 
    private async Task Add()
    {
        var vm = new TermEditorViewModel(new TermItem
        {
            DueAt = DateTimeOffset.Now.AddMinutes(70), Status=TermStatus.Pending
        });

        var w=new TermEditorWindow
        {
            DataContext=vm, Owner = Application.Current.MainWindow
        };

        if(w.ShowDialog()==true)
        {
            using var s = App.AppHost!.Services.CreateScope();

            var repo = s.ServiceProvider.GetRequiredService<ITermRepository>();

            await repo.AddAsync(vm.Item); 

            await Refresh();

            ShowBanner(Localizer.Instance["Banner_Saved"], Colors.SeaGreen);
        }

        //await Refresh();
        ApplyFilter();
    }

    [RelayCommand]
    private async Task EditSelected()
    {
        if (SelectedItem is null)
            return;

        var clone = new TermItem
        {
            Id = SelectedItem.Id,
            Title = SelectedItem.Title,
            Description = SelectedItem.Description,
            DueAt = SelectedItem.DueAt,
            Status = SelectedItem.Status,
            CreatedAt = SelectedItem.CreatedAt,
            UpdatedAt = SelectedItem.UpdatedAt,
            CompletedAt = SelectedItem.CompletedAt,
            LastReminderSentAt = SelectedItem.LastReminderSentAt,
            Reminder24hSentAt = SelectedItem.Reminder24hSentAt,
            Reminder12hSentAt = SelectedItem.Reminder12hSentAt,
            Reminder1hSentAt = SelectedItem.Reminder1hSentAt
        };

        var vm = new TermEditorViewModel(clone);

        var w = new TermEditorWindow
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow
        };

        if (w.ShowDialog() == true)
        {
            using var s = App.AppHost!.Services.CreateScope();

            var repo = s.ServiceProvider.GetRequiredService<ITermRepository>();

            // принудительно вернуть в “открыто”
            vm.Item.Status = TermStatus.Pending;      // если у тебя статус называется иначе (Open/Pending) – подставь своё имя

            vm.Item.CompletedAt = null;              // убрать отметку выполнения

            // сбросить флаги отправленных напоминаний, чтобы по новой дате они ушли заново
            vm.Item.LastReminderSentAt = null;

            vm.Item.Reminder24hSentAt = null;

            vm.Item.Reminder12hSentAt = null;

            vm.Item.Reminder1hSentAt = null;

            vm.Item.UpdatedAt = System.DateTimeOffset.Now;

            await repo.UpdateAsync(vm.Item);

            await Refresh();

            ShowBanner(Localizer.Instance["Banner_Updated"], Colors.SeaGreen);
        }

        //await Refresh();
        ApplyFilter();
    }


    [RelayCommand]
    private async Task CompleteInline(TermItem? term)
    {
        if(term is null) 
            return;

        using var s = Termino.App.App.AppHost!.Services.CreateScope();

        var repo = s.ServiceProvider.GetRequiredService<ITermRepository>();

        await repo.MarkCompletedAsync(term.Id);

        await Refresh();
        ApplyFilter();

        ShowBanner(Localizer.Instance["Banner_Completed"], Colors.SteelBlue);
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ReopenSelected()
    {
        if(SelectedItem is null)
            return;

        var vm = new ReopenViewModel(SelectedItem);

        var w = new ReopenWindow
        { 
            DataContext=vm, Owner=System.Windows.Application.Current.MainWindow
        };

        if(w.ShowDialog() == true)
        {
            using var s=Termino.App.App.AppHost!.Services.CreateScope();

            var repo=s.ServiceProvider.GetRequiredService<ITermRepository>();

            await repo.ReopenAsync(SelectedItem.Id, vm.NewDueAt);

            await Refresh();
            ApplyFilter();

            ShowBanner(Localizer.Instance["Banner_Reopened"], Colors.SeaGreen);
        }
    }

    [RelayCommand]
    private async Task MarkCompleted(Guid id)
    {
        using var scope = App.AppHost!.Services.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<Termino.Data.Repositories.ITermRepository>();

        await repo.MarkCompletedAsync(id);

        await Refresh();
        ApplyFilter();

    }
}
