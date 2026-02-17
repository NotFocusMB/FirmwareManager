using System;
using System.IO;
using System.Security.Cryptography;
using Force.Crc32;

namespace FrimwareDatabase.Core.Services
{
    /// <summary>
    /// Сервис для вычисления хеш-сумм файлов.
    /// </summary>
    public class HashService
    {
        /// <summary>
        /// Вычисляет MD5 хеш файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>MD5 хеш в виде шестнадцатеричной строки.</returns>
        /// <exception cref="Exception">Возникает при ошибке чтения файла или вычисления хеша.</exception>
        public string CalculateMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// Вычисляет CRC32 контрольную сумму файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>CRC32 сумма в виде шестнадцатеричной строки с префиксом "0x".</returns>
        public string CalculateCRC32Checksum(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                return CalculateCRC32Checksum(fileStream);
            }
        }

        /// <summary>
        /// Вычисляет CRC32 контрольную сумму из потока.
        /// </summary>
        /// <param name="stream">Поток данных.</param>
        /// <returns>CRC32 сумма в виде шестнадцатеричной строки.</returns>
        private string CalculateCRC32Checksum(Stream stream)
        {
            using (var crc32 = new Crc32Algorithm())
            {
                byte[] hashBytes = crc32.ComputeHash(stream);
                return ByteArrayToHexString(hashBytes);
            }
        }

        /// <summary>
        /// Преобразует массив байтов в шестнадцатеричную строку.
        /// </summary>
        /// <param name="bytes">Массив байтов.</param>
        /// <returns>Шестнадцатеричная строка без разделителей.</returns>
        private string ByteArrayToHexString(byte[] bytes)
        {
            string hex = BitConverter.ToString(bytes);
            return hex.Replace("-", "");
        }
    }
}