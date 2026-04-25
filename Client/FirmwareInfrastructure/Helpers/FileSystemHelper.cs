using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FirmwareInfrastructure.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с файловой системой и USB-накопителями.
    /// </summary>
    public static class FileSystemHelper
    {
        /// <summary>
        /// Получает список всех доступных USB-накопителей.
        /// </summary>
        /// <returns>Список объектов DriveInfo для съёмных дисков.</returns>
        public static List<DriveInfo> GetUsbDrives()
        {
            return DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                .ToList();
        }

        /// <summary>
        /// Проверяет, является ли диск USB-накопителем.
        /// </summary>
        /// <param name="driveLetter">Буква диска (например, "D:")</param>
        /// <returns>true, если диск съёмный и готов к работе.</returns>
        public static bool IsUsbDrive(string driveLetter)
        {
            try
            {
                var drive = new DriveInfo(driveLetter);
                return drive.DriveType == DriveType.Removable && drive.IsReady;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получает удобочитаемое название диска (например, "D: (Моя флешка)").
        /// </summary>
        /// <param name="drive">Объект диска.</param>
        /// <returns>Строка с буквой диска и меткой тома.</returns>
        public static string GetDriveDisplayName(DriveInfo drive)
        {
            string volumeLabel = string.IsNullOrEmpty(drive.VolumeLabel)
                ? "Без метки"
                : drive.VolumeLabel;

            long freeSpaceMB = drive.AvailableFreeSpace / 1024 / 1024;
            long totalSpaceMB = drive.TotalSize / 1024 / 1024;

            return $"{drive.Name} ({volumeLabel}) - {freeSpaceMB} МБ свободно из {totalSpaceMB} МБ";
        }
    }
}