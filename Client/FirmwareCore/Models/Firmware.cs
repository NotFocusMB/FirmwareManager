using System;

namespace FrimwareDatabase.Core.Models
{
    /// <summary>
    /// Модель данных прошивки.
    /// </summary>
    public class Firmware
    {
        /// <summary>
        /// MD5 контрольная сумма (уникальный идентификатор на сервере)
        /// </summary>
        public string Md5 { get; set; }

        /// <summary>
        /// Получает или задает имя файла прошивки.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Получает или задает дату регистрации прошивки в базе данных.
        /// </summary>
        public string RegistrationDate { get; set; }

        /// <summary>
        /// Получает или задает размер файла в байтах.
        /// </summary>
        public long FileSize { get; set; }
    }
}