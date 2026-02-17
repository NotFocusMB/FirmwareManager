using System;

namespace FirmwareInfrastructure.Logging
{
    /// <summary>
    /// Класс для логирования операций.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Записывает сообщение в лог.
        /// </summary>
        /// <param name="message">Сообщение для записи.</param>
        public static void Write(string message)
        {
            // Заглушка для будущей реализации
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}