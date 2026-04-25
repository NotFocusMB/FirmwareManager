using FrimwareDatabase.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FrimwareDatabase.Core.Services
{
    /// <summary>
    /// HTTP-клиент для работы с удалённым БД-сервером (C++ + SQLite)
    /// </summary>
    public class DatabaseClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public DatabaseClient(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        /// <summary>
        /// Проверяет, доступен ли сервер БД
        /// </summary>
        public async Task<bool> IsServerAvailableAsync()
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
        /// Получает список всех прошивок с сервера
        /// </summary>
        public async Task<List<Firmware>> GetAllFirmwaresAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/images");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var serverFirmwares = JsonConvert.DeserializeObject<List<ServerFirmware>>(json);

                var firmwares = new List<Firmware>();
                foreach (var sf in serverFirmwares)
                {
                    firmwares.Add(new Firmware
                    {
                        Md5 = sf.md5,
                        FileName = sf.filename,
                        FileSize = sf.file_size,
                        RegistrationDate = sf.date_added
                    });
                }
                return firmwares;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения списка прошивок: {ex.Message}");
            }
        }

        /// <summary>
        /// Добавляет новую прошивку на сервер
        /// </summary>
        public async Task<Firmware> AddFirmwareAsync(string localFilePath)
        {
            try
            {
                var fileName = Path.GetFileName(localFilePath);

                // НЕ читаем весь файл в память!
                using (var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                using (var content = new StreamContent(fileStream))
                {
                    content.Headers.Add("X-Filename", fileName);

                    var response = await _httpClient.PostAsync($"{_baseUrl}/image", content);
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<AddFirmwareResponse>(responseJson);

                    if (result.status == "duplicate")
                    {
                        throw new Exception($"Файл с таким MD5 уже существует в базе данных.\n\nMD5: {result.md5}");
                    }

                    if (!response.IsSuccessStatusCode || result.status != "ok")
                    {
                        throw new Exception(result?.message ?? "Неизвестная ошибка");
                    }

                    return new Firmware
                    {
                        Md5 = result.md5,
                        FileName = result.filename,
                        FileSize = result.size,
                        RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления прошивки: {ex.Message}");
            }
        }

        /// <summary>
        /// Добавляет новую прошивку на сервер с прогрессом через HttpClientHandler
        /// </summary>
        public async Task<Firmware> AddFirmwareWithProgressAsync(string localFilePath, IProgress<long> progress, CancellationToken cancellationToken)
        {
            try
            {
                var fileName = Path.GetFileName(localFilePath);
                var fileInfo = new FileInfo(localFilePath);
                var totalSize = fileInfo.Length;

                // Используем HttpClientHandler с прогрессом
                using (var handler = new HttpClientHandler())
                using (var clientWithProgress = new HttpClient(handler))
                {
                    clientWithProgress.Timeout = TimeSpan.FromMinutes(30);

                    using (var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                    {
                        var content = new StreamContent(fileStream);
                        content.Headers.Add("X-Filename", fileName);

                        // Создаём запрос
                        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/image");
                        request.Content = content;
                        request.Headers.ExpectContinue = false;

                        // Отправляем с прогрессом через IProgress
                        var response = await clientWithProgress.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                        // Простой способ: прогресс не отображается в HttpClient по умолчанию
                        // Поэтому показываем только начало и конец
                        progress?.Report(0);

                        var responseJson = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<AddFirmwareResponse>(responseJson);

                        progress?.Report(totalSize);

                        if (result.status == "duplicate")
                        {
                            throw new Exception($"Файл с таким MD5 уже существует в базе данных.\n\nMD5: {result.md5}");
                        }

                        if (!response.IsSuccessStatusCode || result.status != "ok")
                        {
                            throw new Exception(result?.message ?? "Неизвестная ошибка");
                        }

                        return new Firmware
                        {
                            Md5 = result.md5,
                            FileName = result.filename,
                            FileSize = result.size,
                            RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw new Exception("Операция добавления прошивки была отменена");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления прошивки: {ex.Message}");
            }
        }

        /// <summary>
        /// Скачивает файл прошивки по MD5 во временную папку
        /// </summary>
        public async Task<string> DownloadFirmwareFileAsync(string md5, string filename)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/image/{md5}/file");
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var tempPath = Path.GetTempPath();
                    var tempFile = Path.Combine(tempPath, filename);

                    using (var fileStream = File.Create(tempFile))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    return tempFile;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка скачивания прошивки MD5={md5}: {ex.Message}");
            }
        }

        /// <summary>
        /// Удаляет прошивку с сервера по MD5
        /// </summary>
        public async Task<bool> DeleteFirmwareAsync(string md5)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/image/{md5}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка удаления прошивки MD5={md5}: {ex.Message}");
            }
        }



        // Вспомогательные классы для десериализации JSON
        private class ServerFirmware
        {
            public string md5 { get; set; }
            public string filename { get; set; }
            public long file_size { get; set; }
            public string date_added { get; set; }
        }

        private class AddFirmwareResponse
        {
            public string status { get; set; }
            public string md5 { get; set; }
            public string filename { get; set; }
            public long size { get; set; }
            public string message { get; set; }
        }

        private class ErrorResponse
        {
            public string error { get; set; }
            public string message { get; set; }
        }
    }
}