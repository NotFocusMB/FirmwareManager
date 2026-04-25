#define _CRT_SECURE_NO_WARNINGS
#define WIN32_LEAN_AND_MEAN
#define _WINSOCK_DEPRECATED_NO_WARNINGS

// Сначала включаем Windows-заголовки для Winsock
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <winioctl.h>

// Затем libcurl
#include <curl/curl.h>

// Затем остальные библиотеки
#include <iostream>
#include <string>
#include <vector>
#include <filesystem>
#include <fstream>
#include <thread>
#include <atomic>
#include "httplib.h"
#include "json.hpp"
#include <shlobj.h>
#include <shlwapi.h>
#include <atlbase.h>
#include <atlconv.h>

// Подключаем библиотеки
#pragma comment(lib, "shell32.lib")
#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "libcurl_imp.lib")

using json = nlohmann::json;
namespace fs = std::filesystem;

const int BLOCK_SIZE = 16 * 1024 * 1024; // 16 МБ

// === ПЕРЕМЕННЫЕ ДЛЯ WINDOWS SERVICE ===
SERVICE_STATUS g_ServiceStatus = { 0 };
SERVICE_STATUS_HANDLE g_StatusHandle = NULL;
std::atomic<bool> g_StopService(false);
httplib::Server* g_Server = nullptr;

// Структура для хранения информации о USB-накопителе
struct UsbDrive {
    std::string letter;
    std::string volume_label;
    long long total_bytes;
    long long free_bytes;
};

// Глобальные переменные для callback'а libcurl при чтении с устройства
HANDLE g_hDevice = nullptr;
long long g_totalRead = 0;
long long g_deviceSize = 0;

/*
 * Callback для записи HTTP-ответа в строку
 * Используется libcurl при получении данных от сервера
 */
size_t WriteCallback(void* contents, size_t size, size_t nmemb, void* userp) {
    ((std::string*)userp)->append((char*)contents, size * nmemb);
    return size * nmemb;
}

/*
 * Callback для чтения данных с USB-устройства при загрузке на сервер
 * libcurl вызывает эту функцию для получения данных, которые нужно отправить
 */
size_t ReadCallback(char* buffer, size_t size, size_t nitems, void* userp) {
    if (g_hDevice == INVALID_HANDLE_VALUE) {
        return CURL_READFUNC_ABORT;
    }

    DWORD bytes_to_read = (DWORD)(size * nitems);
    DWORD bytes_read;

    if (!ReadFile(g_hDevice, buffer, bytes_to_read, &bytes_read, nullptr)) {
        return CURL_READFUNC_ABORT;
    }

    g_totalRead += bytes_read;

    if (g_totalRead % (10 * 1024 * 1024) < BLOCK_SIZE) {
        std::cout << "Uploaded: " << g_totalRead / 1024 / 1024 << " / "
            << g_deviceSize / 1024 / 1024 << " MB" << std::endl;
    }

    return bytes_read;
}

/*
 * Callback для записи скачанных данных в файл
 * Используется libcurl при сохранении HTTP-ответа на диск
 */
size_t WriteFileCallback(void* contents, size_t size, size_t nmemb, void* userp) {
    size_t total_size = size * nmemb;
    FILE* f = (FILE*)userp;
    size_t written = fwrite(contents, 1, total_size, f);
    return written;
}

/*
 * Callback для отображения прогресса загрузки
 * Вызывается libcurl периодически во время передачи данных
 */
int ProgressCallback(void* clientp, curl_off_t dltotal, curl_off_t dlnow, curl_off_t ultotal, curl_off_t ulnow) {
    if (dltotal > 0) {
        int percent = (int)((double)dlnow / dltotal * 100);
        double downloaded_mb = dlnow / 1024.0 / 1024.0;
        double total_mb = dltotal / 1024.0 / 1024.0;

        std::cout << "\r[PROGRESS] Downloading: " << downloaded_mb << " / " << total_mb
            << " MB (" << percent << "%)" << std::flush;

        if (percent == 100) {
            std::cout << std::endl;
        }
    }
    return 0;
}

/*
 * Получает список всех USB-накопителей в системе
 * Возвращает вектор структур UsbDrive с информацией о каждом устройстве
 */
std::vector<UsbDrive> getUsbDrives() {
    std::vector<UsbDrive> drives;
    DWORD drives_mask = GetLogicalDrives();

    for (char letter = 'D'; letter <= 'Z'; letter++) {
        if (drives_mask & (1 << (letter - 'A'))) {
            std::string root = std::string(1, letter) + ":\\";
            UINT drive_type = GetDriveTypeA(root.c_str());

            if (drive_type == DRIVE_REMOVABLE) {
                UsbDrive drive;
                drive.letter = std::string(1, letter) + ":";

                char volume_name[MAX_PATH];
                if (GetVolumeInformationA(root.c_str(), volume_name, MAX_PATH, nullptr, nullptr, nullptr, nullptr, 0)) {
                    drive.volume_label = volume_name;
                }
                else {
                    drive.volume_label = "Без метки";
                }

                ULARGE_INTEGER free_bytes, total_bytes;
                if (GetDiskFreeSpaceExA(root.c_str(), &free_bytes, &total_bytes, nullptr)) {
                    drive.free_bytes = free_bytes.QuadPart;
                    drive.total_bytes = total_bytes.QuadPart;
                }
                else {
                    drive.free_bytes = 0;
                    drive.total_bytes = 0;
                }

                drives.push_back(drive);
            }
        }
    }
    return drives;
}

/*
 * Получает полный размер физического устройства в байтах
 * Использует IOCTL для получения информации о накопителе
 */
long long getDriveSize(const std::string& drive_letter) {
    std::string device_path = "\\\\.\\" + drive_letter.substr(0, 2);
    HANDLE hDevice = CreateFileA(device_path.c_str(), GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, 0, nullptr);
    if (hDevice == INVALID_HANDLE_VALUE) return 0;

    GET_LENGTH_INFORMATION length_info;
    DWORD bytes_returned;
    if (DeviceIoControl(hDevice, IOCTL_DISK_GET_LENGTH_INFO, nullptr, 0, &length_info, sizeof(length_info), &bytes_returned, nullptr)) {
        CloseHandle(hDevice);
        return length_info.Length.QuadPart;
    }
    CloseHandle(hDevice);
    return 0;
}

/*
 * Записывает образ из файла на USB-устройство (прямая запись на устройство)
 * Использует низкоуровневый доступ к устройству через CreateFile
 */
bool writeImageToDrive(const std::string& drive_letter, const std::string& file_path) {
    std::cout << "[DEBUG] Starting writeImageToDrive" << std::endl;
    std::cout << "[DEBUG] Drive: " << drive_letter << std::endl;
    std::cout << "[DEBUG] File path: " << file_path << std::endl;

    std::string device_path = "\\\\.\\" + drive_letter.substr(0, 2);
    std::cout << "[DEBUG] Device path: " << device_path << std::endl;

    if (!fs::exists(file_path)) {
        std::cerr << "[ERROR] File not found: " << file_path << std::endl;
        return false;
    }

    std::ifstream in_file(file_path, std::ios::binary);
    if (!in_file.is_open()) {
        std::cerr << "[ERROR] Cannot open file: " << file_path << std::endl;
        return false;
    }
    std::cout << "[DEBUG] Source file opened" << std::endl;

    HANDLE hDevice = CreateFileA(
        device_path.c_str(),
        GENERIC_WRITE,
        FILE_SHARE_WRITE,
        nullptr,
        OPEN_EXISTING,
        0,
        nullptr
    );

    if (hDevice == INVALID_HANDLE_VALUE) {
        std::cerr << "[ERROR] Cannot open device: " << device_path << std::endl;
        std::cerr << "[ERROR] GetLastError: " << GetLastError() << std::endl;
        in_file.close();
        return false;
    }
    std::cout << "[DEBUG] Device opened for writing" << std::endl;

    char buffer[BLOCK_SIZE];
    DWORD bytes_written;
    bool success = true;
    long long total = 0;

    while (in_file.read(buffer, BLOCK_SIZE) || in_file.gcount() > 0) {
        if (g_StopService) {
            std::cout << "[INFO] Service stopping, aborting write" << std::endl;
            success = false;
            break;
        }
        DWORD bytes_to_write = (DWORD)in_file.gcount();
        if (!WriteFile(hDevice, buffer, bytes_to_write, &bytes_written, nullptr) || bytes_written != bytes_to_write) {
            std::cerr << "[ERROR] WriteFile failed at offset " << total << std::endl;
            std::cerr << "[ERROR] GetLastError: " << GetLastError() << std::endl;
            success = false;
            break;
        }
        total += bytes_written;

        if (total % (100 * BLOCK_SIZE) == 0) {
            std::cout << "[DEBUG] Written: " << total / 1024 / 1024 << " MB" << std::endl;
        }
    }

    CloseHandle(hDevice);
    in_file.close();

    if (success) {
        std::cout << "[DEBUG] Successfully wrote " << total / 1024 / 1024 << " MB to drive" << std::endl;
    }
    else {
        std::cerr << "[ERROR] Write failed after " << total / 1024 / 1024 << " MB" << std::endl;
    }

    return success;
}

/*
 * Запись образа на USB-устройство с отображением прогресса
 * Включает размонтирование тома перед записью и показывает процент выполнения
 */
bool writeImageToDriveWithProgress(const std::string& drive_letter, const std::string& file_path) {
    std::string device_path = "\\\\.\\" + drive_letter.substr(0, 2);
    std::cout << "Opening device: " << device_path << std::endl;

    HANDLE hDevice = CreateFileA(
        device_path.c_str(),
        GENERIC_WRITE,
        FILE_SHARE_WRITE,
        nullptr,
        OPEN_EXISTING,
        0,
        nullptr
    );

    if (hDevice == INVALID_HANDLE_VALUE) {
        std::cerr << "Cannot open device. Error: " << GetLastError() << std::endl;
        return false;
    }

    DWORD bytes_returned;
    DeviceIoControl(hDevice, FSCTL_DISMOUNT_VOLUME, nullptr, 0, nullptr, 0, &bytes_returned, nullptr);
    std::cout << "Volume dismounted" << std::endl;

    std::ifstream in_file(file_path, std::ios::binary);
    if (!in_file.is_open()) {
        std::cerr << "Cannot open file: " << file_path << std::endl;
        CloseHandle(hDevice);
        return false;
    }

    in_file.seekg(0, std::ios::end);
    long long file_size = in_file.tellg();
    in_file.seekg(0, std::ios::beg);

    std::cout << "File size: " << file_size / 1024 / 1024 << " MB" << std::endl;

    const int BUFFER_SIZE = 16 * 1024 * 1024;
    char* buffer = new char[BUFFER_SIZE];
    DWORD bytes_written;
    long long total_written = 0;
    int last_percent = -1;

    while (in_file.read(buffer, BUFFER_SIZE) || in_file.gcount() > 0) {
        if (g_StopService) {
            std::cout << "\n[INFO] Stopping write operation" << std::endl;
            break;
        }
        DWORD bytes_to_write = (DWORD)in_file.gcount();
        if (!WriteFile(hDevice, buffer, bytes_to_write, &bytes_written, nullptr) || bytes_written != bytes_to_write) {
            std::cerr << "Write failed at offset " << total_written << ". Error: " << GetLastError() << std::endl;
            break;
        }
        total_written += bytes_written;

        int percent = (int)((double)total_written / file_size * 100);
        if (percent != last_percent && percent % 5 == 0) {
            std::cout << "\rWriting: " << total_written / 1024 / 1024
                << " / " << file_size / 1024 / 1024 << " MB (" << percent << "%)" << std::flush;
            last_percent = percent;
        }
    }

    std::cout << std::endl;

    delete[] buffer;
    in_file.close();
    CloseHandle(hDevice);

    return total_written == file_size;
}

// ==================== HTTP-ОБРАБОТЧИКИ ====================

/*
 * Обработчик GET /status
 * Возвращает статус сервиса в формате JSON
 */
void handleStatus(const httplib::Request& req, httplib::Response& res) {
    json response = { {"status", "ok"}, {"message", "Flash writer service is running"} };
    res.set_content(response.dump(), "application/json");
    std::cout << "GET /status" << std::endl;
}

/*
 * Обработчик GET /usb-drives
 * Возвращает список подключенных USB-накопителей в формате JSON
 */
void handleGetUsbDrives(const httplib::Request& req, httplib::Response& res) {
    auto drives = getUsbDrives();
    json drives_json = json::array();
    for (const auto& drive : drives) {
        drives_json.push_back({
            {"letter", drive.letter},
            {"volume_label", drive.volume_label},
            {"total_mb", drive.total_bytes / 1024 / 1024},
            {"free_mb", drive.free_bytes / 1024 / 1024}
            });
    }
    res.set_content(drives_json.dump(), "application/json");
    std::cout << "GET /usb-drives - " << drives.size() << " drives" << std::endl;
}

/*
 * Обработчик POST /write-from-md5
 * Скачивает образ прошивки с удаленного сервера по MD5-хешу и записывает на указанный USB-накопитель
 * Ожидает JSON с полями: md5, drive, db_server
 */
void handleWriteFromMd5(const httplib::Request& req, httplib::Response& res) {
    try {
        auto json_body = json::parse(req.body);
        std::string md5 = json_body["md5"];
        std::string drive = json_body["drive"];
        std::string db_server = json_body["db_server"];

        std::cout << "\n========== WRITE FROM MD5 ==========" << std::endl;
        std::cout << "MD5: " << md5 << std::endl;
        std::cout << "Target drive: " << drive << std::endl;

        std::string url = db_server + "/image/" + md5 + "/file";
        std::cout << "Download URL: " << url << std::endl;

        char temp_path[MAX_PATH];
        GetTempPathA(MAX_PATH, temp_path);
        std::string temp_file = std::string(temp_path) + "firmware_" + md5 + ".img";
        std::cout << "Temp file: " << temp_file << std::endl;

        CURL* curl = curl_easy_init();
        if (!curl) {
            json error = { {"error", "CURL init failed"} };
            res.status = 500;
            res.set_content(error.dump(), "application/json");
            return;
        }

        FILE* out_file = fopen(temp_file.c_str(), "wb");
        if (!out_file) {
            json error = { {"error", "Cannot create temp file"} };
            res.status = 500;
            res.set_content(error.dump(), "application/json");
            curl_easy_cleanup(curl);
            return;
        }

        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteFileCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, out_file);
        curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1L);
        curl_easy_setopt(curl, CURLOPT_TIMEOUT, 7200L);
        curl_easy_setopt(curl, CURLOPT_NOPROGRESS, 0L);
        curl_easy_setopt(curl, CURLOPT_XFERINFOFUNCTION, ProgressCallback);

        std::cout << "Downloading file from DB server..." << std::endl;

        CURLcode curl_res = curl_easy_perform(curl);
        fclose(out_file);

        if (curl_res != CURLE_OK) {
            std::remove(temp_file.c_str());
            json error = { {"error", "Download failed: " + std::string(curl_easy_strerror(curl_res))} };
            res.status = 500;
            res.set_content(error.dump(), "application/json");
            curl_easy_cleanup(curl);
            return;
        }

        curl_easy_cleanup(curl);

        long long file_size = fs::file_size(temp_file);
        std::cout << "Downloaded " << file_size / 1024 / 1024 << " MB" << std::endl;
        std::cout << "Writing to USB drive " << drive << "..." << std::endl;

        bool success = writeImageToDriveWithProgress(drive, temp_file);
        std::remove(temp_file.c_str());

        if (success) {
            json response = { {"status", "ok"}, {"message", "Firmware written successfully"} };
            res.set_content(response.dump(), "application/json");
            std::cout << "Write completed successfully" << std::endl;
        }
        else {
            json error = { {"error", "Failed to write to drive"} };
            res.status = 500;
            res.set_content(error.dump(), "application/json");
        }

        std::cout << "===================================\n" << std::endl;

    }
    catch (const std::exception& e) {
        json error = { {"error", e.what()} };
        res.status = 400;
        res.set_content(error.dump(), "application/json");
    }
}

// === ФУНКЦИИ WINDOWS SERVICE ===

/*
 * Обновляет статус службы Windows
 * Вызывается для уведомления SCM о текущем состоянии службы
 */
void UpdateServiceStatus(DWORD state, DWORD exitCode = 0, DWORD checkPoint = 0) {
    g_ServiceStatus.dwCurrentState = state;
    g_ServiceStatus.dwWin32ExitCode = exitCode;
    g_ServiceStatus.dwCheckPoint = checkPoint;
    SetServiceStatus(g_StatusHandle, &g_ServiceStatus);
}

/*
 * Останавливает HTTP-сервер
 * Устанавливает флаг остановки и останавливает сервер, если он запущен
 */
void StopServer() {
    g_StopService = true;
    if (g_Server) {
        g_Server->stop();
    }
    std::cout << "Server stopped" << std::endl;
}

/*
 * Обработчик управляющих команд от SCM (Service Control Manager)
 * Обрабатывает команды остановки и опроса состояния службы
 */
VOID WINAPI ServiceCtrlHandler(DWORD CtrlCode) {
    switch (CtrlCode) {
    case SERVICE_CONTROL_STOP:
        UpdateServiceStatus(SERVICE_STOP_PENDING);
        StopServer();
        UpdateServiceStatus(SERVICE_STOPPED);
        break;
    case SERVICE_CONTROL_INTERROGATE:
        SetServiceStatus(g_StatusHandle, &g_ServiceStatus);
        break;
    }
}

/*
 * Запускает flash-сервис как HTTP-сервер
 * Инициализирует сетевые библиотеки, настраивает маршруты и запускает прослушивание
 */
void RunFlashService() {
    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);

    curl_global_init(CURL_GLOBAL_ALL);

    httplib::Server server;
    g_Server = &server;

    server.Get("/status", handleStatus);
    server.Get("/usb-drives", handleGetUsbDrives);
    server.Post("/write-from-md5", handleWriteFromMd5);

    std::cout << "Flash Writer Service started on port 8080" << std::endl;

    server.listen("0.0.0.0", 8080);

    curl_global_cleanup();
    WSACleanup();
    g_Server = nullptr;
}

/*
 * Главная функция службы Windows
 * Регистрирует обработчик управляющих команд и запускает flash-сервис
 */
VOID WINAPI ServiceMain(DWORD argc, LPWSTR* argv) {
    g_StatusHandle = RegisterServiceCtrlHandlerW(L"FlashService", ServiceCtrlHandler);
    if (!g_StatusHandle) return;

    g_ServiceStatus.dwServiceType = SERVICE_WIN32_OWN_PROCESS;
    g_ServiceStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP | SERVICE_ACCEPT_SHUTDOWN;
    UpdateServiceStatus(SERVICE_START_PENDING);

    UpdateServiceStatus(SERVICE_RUNNING);

    RunFlashService();

    UpdateServiceStatus(SERVICE_STOPPED);
}

// === ФУНКЦИИ АВТОЗАГРУЗКИ ===

/*
 * Проверяет, запущена ли программа с правами администратора
 * Возвращает true, если текущий процесс имеет права администратора
 */
bool IsRunningAsAdmin() {
    BOOL is_admin = FALSE;
    PSID admin_group = nullptr;
    SID_IDENTIFIER_AUTHORITY nt_authority = SECURITY_NT_AUTHORITY;
    if (AllocateAndInitializeSid(&nt_authority, 2, SECURITY_BUILTIN_DOMAIN_RID,
        DOMAIN_ALIAS_RID_ADMINS, 0, 0, 0, 0, 0, 0, &admin_group)) {
        CheckTokenMembership(nullptr, admin_group, &is_admin);
        FreeSid(admin_group);
    }
    return is_admin != FALSE;
}

/*
 * Добавляет программу в автозагрузку через реестр Windows (HKCU)
 * Этот метод не требует прав администратора и работает для текущего пользователя
 */
void AddToRegistryStartup() {
    HKEY hKey;
    LONG result = RegOpenKeyExA(HKEY_CURRENT_USER,
        "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
        0, KEY_SET_VALUE, &hKey);

    if (result != ERROR_SUCCESS) {
        std::cerr << "Failed to open registry key: " << result << std::endl;
        return;
    }

    char exePath[MAX_PATH];
    GetModuleFileNameA(NULL, exePath, MAX_PATH);

    std::string command = std::string("\"") + exePath + "\" --background";

    result = RegSetValueExA(hKey, "FlashWriterService", 0, REG_SZ,
        (BYTE*)command.c_str(), (DWORD)(command.length() + 1));

    RegCloseKey(hKey);

    if (result == ERROR_SUCCESS) {
        std::cout << "Added to registry startup: " << command << std::endl;
    }
    else {
        std::cerr << "Failed to set registry value: " << result << std::endl;
    }
}

/*
 * Добавляет программу в планировщик задач Windows
 * Создает задачу с наивысшими правами (HIGHEST), которая запускается при входе в систему
 * Требует прав администратора для создания такой задачи
 */
void AddToTaskScheduler() {
    std::string exePath;
    char buffer[MAX_PATH];
    GetModuleFileNameA(NULL, buffer, MAX_PATH);
    exePath = buffer;

    std::string taskName = "FlashWriterService";
    std::string command = "schtasks /create /tn \"" + taskName +
        "\" /tr \"\\\"" + exePath + "\\\" --background\" /sc ONLOGON " +
        "/rl HIGHEST /f /it";

    std::cout << "Creating scheduled task: " << command << std::endl;

    STARTUPINFOA si = { sizeof(si) };
    PROCESS_INFORMATION pi;

    if (CreateProcessA(NULL, (LPSTR)command.c_str(), NULL, NULL, FALSE,
        CREATE_NO_WINDOW, NULL, NULL, &si, &pi)) {
        WaitForSingleObject(pi.hProcess, INFINITE);

        DWORD exitCode;
        GetExitCodeProcess(pi.hProcess, &exitCode);

        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);

        if (exitCode == 0) {
            std::cout << "Scheduled task created successfully" << std::endl;
        }
        else {
            std::cerr << "Failed to create scheduled task, exit code: " << exitCode << std::endl;
        }
    }
    else {
        std::cerr << "Failed to run schtasks. Error: " << GetLastError() << std::endl;
    }
}

/*
 * Основная функция добавления в автозагрузку
 * Использует два метода: реестр (для всех случаев) и планировщик задач (если есть права admin)
 * При первом запуске с правами администратора программа будет добавлена в автозагрузку
 */
void AddToStartupWithAdmin() {
    // Метод 1: Реестр (HKCU - не требует прав admin, работает всегда)
    AddToRegistryStartup();

    // Метод 2: Планировщик задач (требует права admin, запуск с высочайшими правами)
    if (IsRunningAsAdmin()) {
        AddToTaskScheduler();
    }
    else {
        std::cout << "Not running as admin - skipping Task Scheduler method" << std::endl;
    }
}

/*
 * Удаляет программу из автозагрузки
 * Очищает записи в реестре и удаляет задачу в планировщике
 */
void RemoveFromStartup() {
    // Удаление из реестра
    HKEY hKey;
    LONG result = RegOpenKeyExA(HKEY_CURRENT_USER,
        "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
        0, KEY_SET_VALUE, &hKey);
    if (result == ERROR_SUCCESS) {
        RegDeleteValueA(hKey, "FlashWriterService");
        RegCloseKey(hKey);
        std::cout << "Removed from registry startup" << std::endl;
    }

    // Удаление задачи планировщика
    std::string command = "schtasks /delete /tn \"FlashWriterService\" /f";
    WinExec(command.c_str(), SW_HIDE);
    std::cout << "Removed scheduled task" << std::endl;
}

/*
 * Запускает программу в фоновом режиме
 * Скрывает консольное окно и запускает HTTP-сервер без видимого интерфейса
 */
void RunBackgroundMode() {
    // Скрываем консольное окно
    HWND hWnd = GetConsoleWindow();
    if (hWnd) {
        ShowWindow(hWnd, SW_HIDE);
    }

    // Запускаем HTTP сервер в фоне
    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);
    curl_global_init(CURL_GLOBAL_ALL);

    httplib::Server server;
    g_Server = &server;

    server.Get("/status", handleStatus);
    server.Get("/usb-drives", handleGetUsbDrives);
    server.Post("/write-from-md5", handleWriteFromMd5);

    std::cout << "Flash Writer Service started in background on port 8080" << std::endl;

    server.listen("0.0.0.0", 8080);

    curl_global_cleanup();
    WSACleanup();
    g_Server = nullptr;
}

// === ОСНОВНАЯ ФУНКЦИЯ ===

/*
 * Главная точка входа в программу
 * Поддерживает несколько режимов запуска:
 * - Как служба Windows
 * - В фоновом режиме (--background)
 * - Установка в автозагрузку (--install)
 * - Удаление из автозагрузки (--remove)
 * - Обычный консольный режим
 */
int main(int argc, char* argv[]) {
    // Проверяем аргументы командной строки
    bool background_mode = false;
    for (int i = 1; i < argc; i++) {
        if (strcmp(argv[i], "--background") == 0) {
            background_mode = true;
        }
        else if (strcmp(argv[i], "--install") == 0) {
            // Явная установка в автозагрузку
            if (IsRunningAsAdmin()) {
                AddToStartupWithAdmin();
                std::cout << "Installed to startup successfully" << std::endl;
            }
            else {
                std::cerr << "Need administrator rights to install" << std::endl;
            }
            return 0;
        }
        else if (strcmp(argv[i], "--remove") == 0) {
            // Явное удаление из автозагрузки
            RemoveFromStartup();
            std::cout << "Removed from startup" << std::endl;
            return 0;
        }
    }

    SERVICE_TABLE_ENTRYW ServiceTable[] = {
        { (LPWSTR)L"FlashService", (LPSERVICE_MAIN_FUNCTIONW)ServiceMain },
        { NULL, NULL }
    };

    // Пытаемся запустить как службу Windows
    if (StartServiceCtrlDispatcherW(ServiceTable)) {
        return 0;
    }

    // Если указан флаг background - запускаемся в фоновом режиме
    if (background_mode) {
        RunBackgroundMode();
        return 0;
    }

    // Обычный консольный запуск с отображением информации
    std::cout << "Running as console application..." << std::endl;

    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);
    curl_global_init(CURL_GLOBAL_ALL);

    // Проверяем права администратора
    if (!IsRunningAsAdmin()) {
        std::cerr << "WARNING: Not running as administrator!" << std::endl;
        std::cerr << "USB write operations will fail." << std::endl;
    }
    else {
        std::cout << "Running with administrator privileges." << std::endl;

        // Проверяем, установлен ли уже в автозагрузку
        HKEY hKey;
        bool already_installed = false;
        if (RegOpenKeyExA(HKEY_CURRENT_USER,
            "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
            0, KEY_READ, &hKey) == ERROR_SUCCESS) {
            char value[1024];
            DWORD size = sizeof(value);
            if (RegQueryValueExA(hKey, "FlashWriterService", NULL, NULL,
                (BYTE*)value, &size) == ERROR_SUCCESS) {
                already_installed = true;
            }
            RegCloseKey(hKey);
        }

        // Если еще не в автозагрузке - добавляем
        if (!already_installed) {
            std::cout << "Installing to autorun..." << std::endl;
            AddToStartupWithAdmin();
        }
        else {
            std::cout << "Already installed in autorun" << std::endl;
        }
    }

    // Запускаем HTTP-сервер в консольном режиме
    httplib::Server server;
    g_Server = &server;

    server.Get("/status", handleStatus);
    server.Get("/usb-drives", handleGetUsbDrives);
    server.Post("/write-from-md5", handleWriteFromMd5);

    std::cout << "Server started on port 8080" << std::endl;
    std::cout << "Press Ctrl+C to stop" << std::endl;

    server.listen("0.0.0.0", 8080);

    curl_global_cleanup();
    WSACleanup();

    return 0;
}