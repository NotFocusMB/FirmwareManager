using System;
using System.Collections.Generic;
using System.IO;  
using System.Linq;
using System.Xml.Linq;
using FrimwareDatabase.Core.Models;

namespace FrimwareDatabase.Core.Database
{
    /// <summary>
    /// Класс для работы с XML базой данных прошивок.
    /// </summary>
    public class XmlDatabase
    {
        /// <summary>
        /// Создает новую XML базу данных прошивок.
        /// </summary>
        /// <param name="filePath">Путь для сохранения базы данных.</param>
        /// <exception cref="Exception">Возникает при ошибке создания файла.</exception>
        public void CreateDatabase(string filePath)
        {
            var xmlDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("FirmWares"));
            xmlDoc.Save(filePath);
        }

        /// <summary>
        /// Добавляет информацию о прошивке в XML базу данных.
        /// </summary>
        /// <param name="filePath">Путь к файлу базы данных.</param>
        /// <param name="firmware">Данные прошивки.</param>
        /// <exception cref="InvalidOperationException">Возникает, если корневой элемент не найден.</exception>
        public void AddFirmware(string filePath, Firmware firmware)
        {
            XDocument xmlDoc = XDocument.Load(filePath);

            XElement newFirmWare = new XElement("FirmWare",
                new XElement("CheckSum", firmware.CheckSum),
                new XElement("FileName", firmware.FileName),
                new XElement("RegistrationDate", firmware.RegistrationDate));

            XElement rootElement = xmlDoc.Root;
            if (rootElement != null)
            {
                rootElement.Add(newFirmWare);
                xmlDoc.Save(filePath);
            }
            else
            {
                throw new InvalidOperationException("Корневой элемент 'FirmWares' не найден в XML файле.");
            }
        }

        /// <summary>
        /// Проверяет наличие дубликата контрольной суммы в XML базе данных.
        /// </summary>
        /// <param name="filePath">Путь к файлу базы данных.</param>
        /// <param name="checksum">Контрольная сумма для проверки.</param>
        /// <returns>true, если контрольная сумма уже существует; иначе false.</returns>
        public bool IsChecksumExists(string filePath, string checksum)
        {
            XDocument xmlDoc = XDocument.Load(filePath);
            return xmlDoc.Descendants("FirmWare")
                         .Elements("CheckSum")
                         .Any(cs => cs.Value == checksum);
        }

        /// <summary>
        /// Получает список всех прошивок из базы данных.
        /// </summary>
        /// <param name="filePath">Путь к файлу базы данных.</param>
        /// <returns>Список прошивок.</returns>
        public List<Firmware> GetAllFirmwares(string filePath)
        {
            var firmwares = new List<Firmware>();

            if (!File.Exists(filePath))
                return firmwares;

            XDocument xmlDoc = XDocument.Load(filePath);

            foreach (var element in xmlDoc.Descendants("FirmWare"))
            {
                firmwares.Add(new Firmware
                {
                    FileName = element.Element("FileName")?.Value,
                    CheckSum = element.Element("CheckSum")?.Value,
                    RegistrationDate = element.Element("RegistrationDate")?.Value
                });
            }

            return firmwares;
        }

        /// <summary>
        /// Удаляет прошивку из базы данных по имени файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу базы данных.</param>
        /// <param name="fileName">Имя файла прошивки для удаления.</param>
        public void DeleteFirmware(string filePath, string fileName)
        {
            XDocument xmlDoc = XDocument.Load(filePath);

            var elementToRemove = xmlDoc.Descendants("FirmWare")
                .FirstOrDefault(f => f.Element("FileName")?.Value == fileName);

            if (elementToRemove != null)
            {
                elementToRemove.Remove();
                xmlDoc.Save(filePath);
            }
        }
    }
}