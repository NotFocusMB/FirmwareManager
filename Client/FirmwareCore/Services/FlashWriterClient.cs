using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FirmwareClient.Services
{
    /// <summary>
    /// Клиент для взаимодействия с сервисом записи (C++)
    /// </summary>
    public class FlashWriterClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public FlashWriterClient(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(30);
        }

        /// <summary>
        /// Проверяет, доступен ли сервис записи
        /// </summary>
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/status");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получает список всех подключённых USB-накопителей
        /// </summary>
        public async Task<List<UsbDriveInfo>> GetUsbDrivesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/usb-drives");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<UsbDriveInfo>>(json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения списка USB: {ex.Message}");
            }
        }

        /// <summary>
        /// Записывает прошивку на флешку (по MD5, без скачивания клиентом)
        /// </summary>
        public async Task<bool> WriteToUsbByMd5Async(string md5, string driveLetter, string dbServerUrl)
        {
            try
            {
                var request = new
                {
                    md5 = md5,
                    drive = driveLetter,
                    db_server = dbServerUrl
                };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/write-from-md5", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<WriteResponse>(responseJson);

                return result.status == "ok";
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка записи на USB: {ex.Message}");
            }
        }

        /// <summary>
        /// Копирует образ с флешки в БД
        /// </summary>
        public async Task<bool> CopyFromUsbToDbAsync(string driveLetter, string dbServerUrl, string filename)
        {
            try
            {
                var request = new
                {
                    drive = driveLetter,
                    db_server = dbServerUrl,
                    filename = filename
                };
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/copy-to-db", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<WriteResponse>(responseJson);

                return result.status == "ok";
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка копирования с USB: {ex.Message}");
            }
        }

        // Вспомогательные классы
        public class UsbDriveInfo
        {
            public string letter { get; set; }
            public string volume_label { get; set; }
            public long total_mb { get; set; }
            public long free_mb { get; set; }
        }

        private class WriteResponse
        {
            public string status { get; set; }
            public string message { get; set; }
        }
    }
}