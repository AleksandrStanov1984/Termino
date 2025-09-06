using System.Net.NetworkInformation;
using System.Text.Json;
using System.IO;

namespace Termino.App.Services;

public class Settings{

    public List<string> Emails {get;set;} = new();

    public bool Use12h {get;set;}
}

public class SettingsService{

    private readonly string _path;

    public Settings Value {get; private set;} = new();

    public SettingsService(string path)
    { 
        _path=path; Load();
    }

    public void Load(){

        try{ 
            if(File.Exists(_path))
                Value = JsonSerializer.Deserialize<Settings>(File.ReadAllText(_path)) ?? new(); 
        }
        catch
        {
            Value = new Settings();
        }
    }

    public void Save()
    {
        Value.Emails = Value.Emails.Where(e=>!string.IsNullOrWhiteSpace(e)).Select(e=>e.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Take(3).ToList();

        var dir=Path.GetDirectoryName(_path)!;

        if(!Directory.Exists(dir)) 
            Directory.CreateDirectory(dir);

        var direc = Path.GetDirectoryName(_path)!;

        if (!Directory.Exists(direc))
            Directory.CreateDirectory(direc);

        File.WriteAllText(
            _path, JsonSerializer.Serialize(
                Value, new JsonSerializerOptions{WriteIndented=true}
                )
            );
    }
}