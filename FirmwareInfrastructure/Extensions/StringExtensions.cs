namespace FirmwareInfrastructure.Extensions
{
    /// <summary>
    /// Методы расширения для строк.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Проверяет, является ли строка пустой или null.
        /// </summary>
        /// <param name="value">Проверяемая строка.</param>
        /// <returns>true, если строка null или пустая.</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}