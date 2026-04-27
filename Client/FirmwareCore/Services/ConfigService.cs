using System;
using System.IO;
using Newtonsoft.Json;

namespace FrimwareDatabase.Core.Services
{
    /// <summary>
    /// Конфигурация клиента
    /// </summary>
    public class AppConfig
    {
        public string DatabaseServerUrl { get; set; } = "http://192.168.50.230:8081"; 
        public string FlashServiceUrl { get; set; } = "http://localhost:8080"; 
    }

    /// <summary>
    /// Сервис для работы с конфигурационным файлом
    /// </summary>
    public static class ConfigService
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        /// <summary>
        /// Загружает конфигурацию из файла
        /// </summary>
        public static AppConfig LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                // Создание конфига по умолчанию
                var defaultConfig = new AppConfig();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            try
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch
            {
                return new AppConfig();
            }
        }

        /// <summary>
        /// Сохраняет конфигурацию в файл
        /// </summary>
        public static void SaveConfig(AppConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }
    }
}