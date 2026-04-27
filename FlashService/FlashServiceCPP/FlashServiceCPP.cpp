#define _CRT_SECURE_NO_WARNINGS
#define WIN32_LEAN_AND_MEAN
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <winioctl.h>
#include <curl/curl.h>
#include <iostream>
#include <string>
#include <vector>
#include <filesystem>
#include <fstream>
#include <thread>
#include <atomic>
#include "httplib.h"
#include "json.hpp"

#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "libcurl_imp.lib")

using json = nlohmann::json;
namespace fs = std::filesystem;

const int BLOCK_SIZE = 16 * 1024 * 1024;

SERVICE_STATUS g_ServiceStatus = { 0 };
SERVICE_STATUS_HANDLE g_StatusHandle = NULL;
std::atomic<bool> g_StopService(false);
httplib::Server* g_Server = nullptr;

struct UsbDrive {
    std::string letter;
    std::string volume_label;
    long long total_bytes;
    long long free_bytes;
};

// Колбэк для сохранения скачанного файла
size_t WriteFileCallback(void* contents, size_t size, size_t nmemb, void* userp) {
    size_t total_size = size * nmemb;
    FILE* f = (FILE*)userp;
    return fwrite(contents, 1, total_size, f);
}

// Колбэк для отображения прогресса загрузки
int ProgressCallback(void* clientp, curl_off_t dltotal, curl_off_t dlnow, curl_off_t ultotal, curl_off_t ulnow) {
    if (dltotal > 0) {
        int percent = (int)((double)dlnow / dltotal * 100);
        std::cout << "\r[DOWNLOAD] " << dlnow / 1024 / 1024 << " / " << dltotal / 1024 / 1024
            << " MB (" << percent << "%)" << std::flush;
        if (percent == 100) std::cout << std::endl;
    }
    return 0;
}

// Получение списка USB-накопителей
std::vector<UsbDrive> getUsbDrives() {
    std::vector<UsbDrive> drives;
    DWORD drives_mask = GetLogicalDrives();

    for (char letter = 'D'; letter <= 'Z'; letter++) {
        if (drives_mask & (1 << (letter - 'A'))) {
            std::string root = std::string(1, letter) + ":\\";
            if (GetDriveTypeA(root.c_str()) == DRIVE_REMOVABLE) {
                UsbDrive drive;
                drive.letter = std::string(1, letter) + ":";

                char volume_name[MAX_PATH];
                if (GetVolumeInformationA(root.c_str(), volume_name, MAX_PATH, nullptr, nullptr, nullptr, nullptr, 0))
                    drive.volume_label = volume_name;
                else
                    drive.volume_label = "Без метки";

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

// Запись образа на USB-накопитель
bool writeImageToDriveWithProgress(const std::string& drive_letter, const std::string& file_path) {
    std::string device_path = "\\\\.\\" + drive_letter.substr(0, 2);
    std::cout << "[WRITE] Opening device: " << device_path << std::endl;

    HANDLE hDevice = CreateFileA(device_path.c_str(), GENERIC_WRITE, FILE_SHARE_WRITE,
        nullptr, OPEN_EXISTING, 0, nullptr);

    if (hDevice == INVALID_HANDLE_VALUE) {
        std::cerr << "[ERROR] Cannot open device. Error: " << GetLastError() << std::endl;
        return false;
    }

    DWORD bytes_returned;
    DeviceIoControl(hDevice, FSCTL_DISMOUNT_VOLUME, nullptr, 0, nullptr, 0, &bytes_returned, nullptr);
    std::cout << "[WRITE] Volume dismounted" << std::endl;

    std::ifstream in_file(file_path, std::ios::binary);
    if (!in_file.is_open()) {
        std::cerr << "[ERROR] Cannot open file: " << file_path << std::endl;
        CloseHandle(hDevice);
        return false;
    }

    in_file.seekg(0, std::ios::end);
    long long file_size = in_file.tellg();
    in_file.seekg(0, std::ios::beg);

    std::cout << "[WRITE] File size: " << file_size / 1024 / 1024 << " MB" << std::endl;

    char* buffer = new char[BLOCK_SIZE];
    DWORD bytes_written;
    long long total_written = 0;
    int last_percent = -1;

    while (in_file.read(buffer, BLOCK_SIZE) || in_file.gcount() > 0) {
        if (g_StopService) { std::cout << "\n[INFO] Cancelled" << std::endl; break; }

        DWORD bytes_to_write = (DWORD)in_file.gcount();
        if (!WriteFile(hDevice, buffer, bytes_to_write, &bytes_written, nullptr) || bytes_written != bytes_to_write) {
            std::cerr << "\n[ERROR] Write failed at " << total_written / 1024 / 1024
                << " MB. Error: " << GetLastError() << std::endl;
            delete[] buffer;
            in_file.close();
            CloseHandle(hDevice);
            return false;
        }
        total_written += bytes_written;

        int percent = (int)((double)total_written / file_size * 100);
        if (percent != last_percent && percent % 10 == 0) {
            std::cout << "\r[WRITE] " << total_written / 1024 / 1024 << " / "
                << file_size / 1024 / 1024 << " MB (" << percent << "%)" << std::flush;
            last_percent = percent;
        }
    }

    std::cout << "\r[WRITE] " << file_size / 1024 / 1024 << " / "
        << file_size / 1024 / 1024 << " MB (100%)" << std::endl;

    delete[] buffer;
    in_file.close();
    CloseHandle(hDevice);

    return total_written == file_size;
}

// GET /status
void handleStatus(const httplib::Request& req, httplib::Response& res) {
    json response = { {"status", "ok"}, {"message", "Flash writer service is running"} };
    res.set_content(response.dump(), "application/json");
}

// GET /usb-drives
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
}

// POST /write-from-md5
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
            res.status = 500;
            res.set_content("{\"error\":\"CURL init failed\"}", "application/json");
            return;
        }

        FILE* out_file = fopen(temp_file.c_str(), "wb");
        if (!out_file) {
            res.status = 500;
            res.set_content("{\"error\":\"Cannot create temp file\"}", "application/json");
            curl_easy_cleanup(curl);
            return;
        }

        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteFileCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, out_file);
        curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1L);
        curl_easy_setopt(curl, CURLOPT_TIMEOUT, 0L);
        curl_easy_setopt(curl, CURLOPT_NOPROGRESS, 0L);
        curl_easy_setopt(curl, CURLOPT_XFERINFOFUNCTION, ProgressCallback);

        std::cout << "Downloading file from DB server..." << std::endl;

        CURLcode curl_res = curl_easy_perform(curl);
        fclose(out_file);

        if (curl_res != CURLE_OK) {
            std::remove(temp_file.c_str());
            res.status = 500;
            std::string err = "{\"error\":\"Download failed: " + std::string(curl_easy_strerror(curl_res)) + "\"}";
            res.set_content(err, "application/json");
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
            res.set_content("{\"status\":\"ok\",\"message\":\"Firmware written successfully\"}", "application/json");
            std::cout << "Write completed successfully" << std::endl;
        }
        else {
            res.status = 500;
            res.set_content("{\"error\":\"Failed to write to drive\"}", "application/json");
        }

        std::cout << "===================================\n" << std::endl;

    }
    catch (const std::exception& e) {
        res.status = 400;
        res.set_content("{\"error\":\"" + std::string(e.what()) + "\"}", "application/json");
    }
}

// Служба Windows
void StopServer() {
    g_StopService = true;
    if (g_Server) g_Server->stop();
}

VOID WINAPI ServiceCtrlHandler(DWORD CtrlCode) {
    if (CtrlCode == SERVICE_CONTROL_STOP || CtrlCode == SERVICE_CONTROL_SHUTDOWN) {
        g_ServiceStatus.dwCurrentState = SERVICE_STOP_PENDING;
        SetServiceStatus(g_StatusHandle, &g_ServiceStatus);
        StopServer();
        g_ServiceStatus.dwCurrentState = SERVICE_STOPPED;
        SetServiceStatus(g_StatusHandle, &g_ServiceStatus);
    }
}

VOID WINAPI ServiceMain(DWORD argc, LPWSTR* argv) {
    g_StatusHandle = RegisterServiceCtrlHandlerW(L"FlashService", ServiceCtrlHandler);
    if (!g_StatusHandle) return;

    g_ServiceStatus.dwServiceType = SERVICE_WIN32_OWN_PROCESS;
    g_ServiceStatus.dwCurrentState = SERVICE_RUNNING;
    SetServiceStatus(g_StatusHandle, &g_ServiceStatus);

    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);
    curl_global_init(CURL_GLOBAL_ALL);

    httplib::Server server;
    g_Server = &server;

    server.Get("/status", handleStatus);
    server.Get("/usb-drives", handleGetUsbDrives);
    server.Post("/write-from-md5", handleWriteFromMd5);

    server.listen("0.0.0.0", 8080);

    curl_global_cleanup();
    WSACleanup();
}

// Автозагрузка через планировщик задач
void AddToStartup() {
    char exePath[MAX_PATH];
    GetModuleFileNameA(NULL, exePath, MAX_PATH);

    std::string command = "schtasks /create /tn \"FlashWriterService\" /tr \"\\\"" +
        std::string(exePath) + "\\\" --background\" /sc ONLOGON /rl HIGHEST /f /it";

    STARTUPINFOA si = { sizeof(si) };
    PROCESS_INFORMATION pi;
    si.dwFlags = STARTF_USESHOWWINDOW;
    si.wShowWindow = SW_HIDE;

    if (CreateProcessA(NULL, (LPSTR)command.c_str(), NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi)) {
        WaitForSingleObject(pi.hProcess, INFINITE);
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    }
}

// Удаление из автозагрузки
void RemoveFromStartup() {
    WinExec("schtasks /delete /tn \"FlashWriterService\" /f", SW_HIDE);
}

// Фоновый режим
void RunBackgroundMode() {
    HWND hWnd = GetConsoleWindow();
    if (hWnd) ShowWindow(hWnd, SW_HIDE);

    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);
    curl_global_init(CURL_GLOBAL_ALL);

    httplib::Server server;
    g_Server = &server;

    server.Get("/status", handleStatus);
    server.Get("/usb-drives", handleGetUsbDrives);
    server.Post("/write-from-md5", handleWriteFromMd5);

    server.listen("0.0.0.0", 8080);

    curl_global_cleanup();
    WSACleanup();
}

// Главная функция
int main(int argc, char* argv[]) {
    for (int i = 1; i < argc; i++) {
        if (strcmp(argv[i], "--background") == 0) {
            RunBackgroundMode();
            return 0;
        }
        else if (strcmp(argv[i], "--install") == 0) {
            AddToStartup();
            std::cout << "Installed to startup" << std::endl;
            return 0;
        }
        else if (strcmp(argv[i], "--remove") == 0) {
            RemoveFromStartup();
            std::cout << "Removed from startup" << std::endl;
            return 0;
        }
    }

    SERVICE_TABLE_ENTRYW ServiceTable[] = {
        { (LPWSTR)L"FlashService", (LPSERVICE_MAIN_FUNCTIONW)ServiceMain },
        { NULL, NULL }
    };

    if (StartServiceCtrlDispatcherW(ServiceTable)) return 0;

    // Консольный режим
    std::cout << "Flash Writer Service (console mode)" << std::endl;

    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);
    curl_global_init(CURL_GLOBAL_ALL);

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