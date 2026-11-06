using System.Text.Encodings.Web;
using System.Text.Json;

namespace Gameapi.Controllers;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}

public class JsonWorker<T>
{
    private readonly string FilePath;

    public JsonWorker(string filename)
    {
        FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", filename);
    }


    public List<T> LoadJson()
    {
        if (!System.IO.File.Exists(FilePath)) return new List<T>();
        var json = System.IO.File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<T>>(json, JsonDefaults.ReadOptions) ?? new List<T>();
    }

    public void SaveJson(List<T> data)
    {
        var json = JsonSerializer.Serialize(data, JsonDefaults.WriteOptions);
        System.IO.File.WriteAllText(FilePath, json);
    }
}