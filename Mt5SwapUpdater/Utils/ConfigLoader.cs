using System.IO;
using System.Text.Json;
using SwapUpdater.Models;

namespace SwapUpdater.Utils
{
    public static class ConfigLoader
    {
        public static AppConfig Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Файл конфигурации не найден: {path}");

            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<AppConfig>(json);

            if (config == null)
                throw new InvalidDataException("Конфигурация не может быть пустой");

            return config;
        }
    }
}
