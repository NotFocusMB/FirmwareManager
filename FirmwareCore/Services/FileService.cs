using System;
using System.IO;
using System.Text;

namespace FrimwareDatabase.Core.Services
{
    /// <summary>
    /// Сервис для работы с файловой системой.
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// Читает содержимое файла в кодировке UTF-8.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>Содержимое файла или сообщение об ошибке.</returns>
        public string ReadFileContent(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка чтения файла: {ex.Message}";
            }
        }
    }
}