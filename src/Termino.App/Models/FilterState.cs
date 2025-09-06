using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Termino.App.Models
{
    public enum FilterMode 
    { 
        None,
        ByDate,
        ByStatus
    }

    public partial class FilterState : ObservableObject
    {
        [ObservableProperty] 
        private FilterMode mode = FilterMode.None;

        [ObservableProperty]
        private DateTime? date;

        // срочность
        [ObservableProperty] 
        private bool urgent;  // ≤ 7 дней

        [ObservableProperty] 
        private bool soon;    // 8–15 дней

        [ObservableProperty] 
        private bool later;   // > 15 дней

        // показывать закрытые
        [ObservableProperty] private bool showClosed;
    }
}
