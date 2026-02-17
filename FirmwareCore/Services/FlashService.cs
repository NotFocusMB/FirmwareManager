using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FrimwareDatabase.Core.Models;

namespace FrimwareDatabase.Core.Services
{
    /// <summary>
    /// Реализация сервиса для записи прошивок на USB-носители.
    /// </summary>
    public class FlashService : IFlashService
    {
        /// <summary>
        /// Получает список доступных USB-накопителей.
        /// </summary>
        /// <returns>Список букв дисков (например, "D:", "E:")</returns>
        public List<string> GetAvailableUsbDrives()
        {
            return DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                .Select(d => d.Name)
                .ToList();
        }

        /// <summary>
        /// Получает информацию о свободном месте на диске.
        /// </summary>
        /// <param name="driveLetter">Буква диска</param>
        /// <returns>Свободное место в байтах</returns>
        public long GetFreeSpace(string driveLetter)
        {
            var drive = new DriveInfo(driveLetter);
            return drive.AvailableFreeSpace;
        }

        /// <summary>
        /// Проверяет, достаточно ли места на носителе для файла.
        /// </summary>
        /// <param name="driveLetter">Буква диска (например, "D:")</param>
        /// <param name="fileSize">Размер файла в байтах</param>
        /// <returns>true, если места достаточно</returns>
        public bool HasEnoughSpace(string driveLetter, long fileSize)
        {
            try
            {
                var drive = new DriveInfo(driveLetter);
                return drive.AvailableFreeSpace >= fileSize;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Записывает прошивку на USB-носитель.
        /// </summary>
        /// <param name="firmware">Прошивка для записи</param>
        /// <param name="targetDrive">Целевой диск (например, "D:")</param>
        /// <returns>true, если запись успешна</returns>
        public bool WriteFirmware(Firmware firmware, string targetDrive)
        {
            try
            {
                // Проверяем, существует ли исходный файл
                if (!File.Exists(firmware.FilePath))
                {
                    throw new FileNotFoundException($"Файл не найден: {firmware.FilePath}");
                }

                // Проверяем, существует ли целевой диск
                if (!Directory.Exists(targetDrive))
                {
                    throw new DirectoryNotFoundException($"Диск не найден: {targetDrive}");
                }

                // Проверяем свободное место
                var fileInfo = new FileInfo(firmware.FilePath);
                if (!HasEnoughSpace(targetDrive, fileInfo.Length))
                {
                    throw new Exception("Недостаточно свободного места на носителе.");
                }

                // Формируем путь назначения
                string destPath = Path.Combine(targetDrive, firmware.FileName);

                // Копируем файл (true = разрешить перезапись)
                File.Copy(firmware.FilePath, destPath, true);

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}