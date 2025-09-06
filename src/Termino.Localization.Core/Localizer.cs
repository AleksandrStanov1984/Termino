using System.Globalization;
using System.Text.Json;
namespace Termino.Localization.Core;
public class Localizer{
    static readonly Lazy<Localizer> _i=new(()=>new Localizer());
    public static Localizer Instance=>_i.Value;
    readonly Dictionary<string,string> _d=new(StringComparer.OrdinalIgnoreCase);
    public string CurrentCulture{get;private set;}=CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    public event EventHandler? LanguageChanged;
    public void Load(string c){
        var baseDir=AppContext.BaseDirectory;
        var path=Path.Combine(baseDir,"Resources",c+".json");
        if(!File.Exists(path)){ c="en"; path=Path.Combine(baseDir,"Resources","en.json"); }
        _d.Clear();
        try{
            var text=File.ReadAllText(path);
            var dict=JsonSerializer.Deserialize<Dictionary<string,string>>(text) ?? new();
            foreach(var kv in dict) _d[kv.Key]=kv.Value;
        }catch{}
        CurrentCulture=c;
        LanguageChanged?.Invoke(this,EventArgs.Empty);
    }
    public string this[string key]=> _d.TryGetValue(key, out var v) ? v : key;
}