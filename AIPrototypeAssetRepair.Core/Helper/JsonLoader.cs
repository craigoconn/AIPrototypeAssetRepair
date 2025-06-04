using System.Text.Json;

namespace AIPrototypeAssetRepair.Helper
{
    public static class JsonLoader
    {
        public static List<T> LoadJsonList<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
    }
}
