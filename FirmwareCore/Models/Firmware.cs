using System;

namespace FrimwareDatabase.Core.Models
{
    /// <summary>
    /// Модель данных прошивки.
    /// </summary>
    public class Firmware
    {
        /// <summary>
        /// Получает или задает контрольную сумму прошивки (MD5 или CRC32).
        /// </summary>
        public string CheckSum { get; set; }

        /// <summary>
        /// Получает или задает имя файла прошивки.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Получает или задает дату регистрации прошивки в базе данных.
        /// </summary>
        public string RegistrationDate { get; set; }
    }
}