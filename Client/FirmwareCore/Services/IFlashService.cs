using System.Collections.Generic;
using FrimwareDatabase.Core.Models;

namespace FrimwareDatabase.Core.Services
{
    /// <summary>
    /// Интерфейс сервиса для записи прошивок на USB-носители.
    /// </summary>
    public interface IFlashService
    {
        /// <summary>
        /// Получает список доступных USB-накопителей.
        /// </summary>
        /// <returns>Список букв дисков (например, "D:", "E:")</returns>
        List<string> GetAvailableUsbDrives();

        /// <summary>
        /// Проверяет, достаточно ли места на носителе для файла.
        /// </summary>
        /// <param name="driveLetter">Буква диска (например, "D:")</param>
        /// <param name="fileSize">Размер файла в байтах</param>
        /// <returns>true, если места достаточно</returns>
        bool HasEnoughSpace(string driveLetter, long fileSize);

        /// <summary>
        /// Записывает прошивку на USB-носитель.
        /// </summary>
        /// <param name="firmware">Прошивка для записи</param>
        /// <param name="targetDrive">Целевой диск (например, "D:")</param>
        /// <returns>true, если запись успешна</returns>
        bool WriteFirmware(Firmware firmware, string targetDrive);

        /// <summary>
        /// Получает информацию о свободном месте на диске.
        /// </summary>
        /// <param name="driveLetter">Буква диска</param>
        /// <returns>Свободное место в байтах</returns>
        long GetFreeSpace(string driveLetter);
    }
}