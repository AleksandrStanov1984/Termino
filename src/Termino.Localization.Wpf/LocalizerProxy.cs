using System.ComponentModel;
using Termino.Localization.Core;

namespace Termino.Localization.Wpf;

public class LocalizerProxy:INotifyPropertyChanged
{
    static readonly Lazy<LocalizerProxy> _i = new(() => new LocalizerProxy());

    public static LocalizerProxy Instance => _i.Value;

    private LocalizerProxy()
    { 
        Localizer.Instance.LanguageChanged += (_,__) => PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("Item[]"));
    }

    public string this[string key] => Localizer.Instance[key];

    public event PropertyChangedEventHandler? PropertyChanged;
}