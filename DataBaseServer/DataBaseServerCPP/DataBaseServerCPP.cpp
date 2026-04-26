#define _CRT_SECURE_NO_WARNINGS

#include <iostream>
#include <string>
#include <vector>
#include <filesystem>
#include <fstream>
#include <ctime>
#include <sstream>
#include <iomanip>
#include <algorithm>
#include <cctype>
#include <cstring>
#include <chrono>
#include <memory>

#define CPPHTTPLIB_PAYLOAD_MAX_LENGTH (16LL * 1024 * 1024 * 1024) // 16 GB

#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>

#include "httplib.h"
#include "json.hpp"
#include "sqlite3.h"

#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "advapi32.lib")

using json = nlohmann::json;
namespace fs = std::filesystem;

// Глобальный указатель на базу данных
sqlite3* g_db = nullptr;


// ==================== ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ ====================

// Инициализация Winsock
bool InitializeWinsock() {
    WSADATA wsaData;
    int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (result != 0) {
        std::cerr << "WSAStartup failed: " << result << std::endl;
        return false;
    }
    return true;
}

// Вычисление MD5-хеша
std::string calculateMD5(const std::string& content) {
    HCRYPTPROV hProv = 0;
    HCRYPTHASH hHash = 0;
    BYTE rgbHash[16];
    DWORD cbHash = 16;

    if (!CryptAcquireContextW(&hProv, NULL, NULL, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT)) {
        return "";
    }

    if (!CryptCreateHash(hProv, CALG_MD5, 0, 0, &hHash)) {
        CryptReleaseContext(hProv, 0);
        return "";
    }

    if (!CryptHashData(hHash, (const BYTE*)content.c_str(), (DWORD)content.size(), 0)) {
        CryptDestroyHash(hHash);
        CryptReleaseContext(hProv, 0);
        return "";
    }

    if (!CryptGetHashParam(hHash, HP_HASHVAL, rgbHash, &cbHash, 0)) {
        CryptDestroyHash(hHash);
        CryptReleaseContext(hProv, 0);
        return "";
    }

    CryptDestroyHash(hHash);
    CryptReleaseContext(hProv, 0);

    std::stringstream ss;
    for (DWORD i = 0; i < cbHash; i++) {
        ss << std::hex << std::setw(2) << std::setfill('0') << (int)rgbHash[i];
    }
    return ss.str();
}

// Получение текущих даты и времени
std::string getCurrentDateTime() {
    time_t now = time(nullptr);
    struct tm timeinfo;
    localtime_s(&timeinfo, &now);
    char buf[20];
    strftime(buf, sizeof(buf), "%Y-%m-%d %H:%M:%S", &timeinfo);
    return std::string(buf);
}

// Инициализация базы данных
sqlite3* initDatabase() {
    sqlite3* db;
    int rc = sqlite3_open("firmwares.db", &db);
    if (rc) {
        std::cerr << "DB opening error: " << sqlite3_errmsg(db) << std::endl;
        return nullptr;
    }

    sqlite3_exec(db, "PRAGMA journal_mode=WAL;", nullptr, nullptr, nullptr);

    const char* createTableSQL = R"(
        CREATE TABLE IF NOT EXISTS firmwares (
            md5 TEXT PRIMARY KEY,
            filename TEXT NOT NULL,
            file_path TEXT NOT NULL,
            file_size INTEGER NOT NULL,
            date_added TEXT NOT NULL
        );
    )";

    char* errMsg = nullptr;
    rc = sqlite3_exec(db, createTableSQL, nullptr, nullptr, &errMsg);
    if (rc != SQLITE_OK) {
        std::cerr << "Table creating error: " << errMsg << std::endl;
        sqlite3_free(errMsg);
    }
    else {
        std::cout << "Table firmwares is ready (PRIMARY KEY = md5)" << std::endl;
    }

    return db;
}

// Добавление записи о прошивке
bool addFirmwareToDB(const std::string& md5, const std::string& filename,
    const std::string& file_path, long long file_size) {
    std::string date_added = getCurrentDateTime();
    std::string sql = "INSERT OR REPLACE INTO firmwares (md5, filename, file_path, file_size, date_added) VALUES (?, ?, ?, ?, ?);";

    sqlite3_stmt* stmt;
    sqlite3_prepare_v2(g_db, sql.c_str(), -1, &stmt, nullptr);
    sqlite3_bind_text(stmt, 1, md5.c_str(), -1, SQLITE_STATIC);
    sqlite3_bind_text(stmt, 2, filename.c_str(), -1, SQLITE_STATIC);
    sqlite3_bind_text(stmt, 3, file_path.c_str(), -1, SQLITE_STATIC);
    sqlite3_bind_int64(stmt, 4, file_size);
    sqlite3_bind_text(stmt, 5, date_added.c_str(), -1, SQLITE_STATIC);

    int rc = sqlite3_step(stmt);
    sqlite3_finalize(stmt);

    return rc == SQLITE_DONE;
}

// Получение списка всех прошивок
json getAllFirmwares() {
    json firmwares = json::array();
    const char* sql = "SELECT md5, filename, file_size, date_added FROM firmwares;";

    sqlite3_stmt* stmt;
    sqlite3_prepare_v2(g_db, sql, -1, &stmt, nullptr);

    while (sqlite3_step(stmt) == SQLITE_ROW) {
        const char* md5 = (const char*)sqlite3_column_text(stmt, 0);
        const char* filename = (const char*)sqlite3_column_text(stmt, 1);
        long long file_size = sqlite3_column_int64(stmt, 2);
        const char* date_added = (const char*)sqlite3_column_text(stmt, 3);

        firmwares.push_back({
            {"md5", md5},
            {"filename", filename},
            {"file_size", file_size},
            {"date_added", date_added}
            });
    }

    sqlite3_finalize(stmt);
    return firmwares;
}

// Получение пути к файлу по MD5
std::string getFilePathByMD5(const std::string& md5) {
    std::string sql = "SELECT file_path FROM firmwares WHERE md5 = ?;";
    sqlite3_stmt* stmt;
    sqlite3_prepare_v2(g_db, sql.c_str(), -1, &stmt, nullptr);
    sqlite3_bind_text(stmt, 1, md5.c_str(), -1, SQLITE_STATIC);

    std::string file_path;
    if (sqlite3_step(stmt) == SQLITE_ROW) {
        file_path = (const char*)sqlite3_column_text(stmt, 0);
    }

    sqlite3_finalize(stmt);
    return file_path;
}

// Получение имени файла по MD5
std::string getFilenameByMD5(const std::string& md5) {
    std::string sql = "SELECT filename FROM firmwares WHERE md5 = ?;";
    sqlite3_stmt* stmt;
    sqlite3_prepare_v2(g_db, sql.c_str(), -1, &stmt, nullptr);
    sqlite3_bind_text(stmt, 1, md5.c_str(), -1, SQLITE_STATIC);

    std::string filename;
    if (sqlite3_step(stmt) == SQLITE_ROW) {
        filename = (const char*)sqlite3_column_text(stmt, 0);
    }

    sqlite3_finalize(stmt);
    return filename;
}

// Удаление прошивки по MD5
bool deleteFirmwareByMD5(const std::string& md5) {
    std::string sql = "DELETE FROM firmwares WHERE md5 = ?;";
    sqlite3_stmt* stmt;
    sqlite3_prepare_v2(g_db, sql.c_str(), -1, &stmt, nullptr);
    sqlite3_bind_text(stmt, 1, md5.c_str(), -1, SQLITE_STATIC);

    int rc = sqlite3_step(stmt);
    sqlite3_finalize(stmt);
    return rc == SQLITE_DONE;
}

// Проверка существования MD5
bool isMD5Exists(const std::string& md5) {
    std::string sql = "SELECT 1 FROM firmwares WHERE md5 = ?;";
    sqlite3_stmt* stmt;
    sqlite3_prepare_v2(g_db, sql.c_str(), -1, &stmt, nullptr);
    sqlite3_bind_text(stmt, 1, md5.c_str(), -1, SQLITE_STATIC);

    bool exists = (sqlite3_step(stmt) == SQLITE_ROW);
    sqlite3_finalize(stmt);
    return exists;
}


// ==================== ОБРАБОТЧИКИ HTTP-ЗАПРОСОВ ====================


// Обработка GET /status
void handleStatus(const httplib::Request& req, httplib::Response& res) {
    try {
        json response = {
            {"status", "ok"},
            {"message", "Database server is running"}
        };
        res.set_content(response.dump(), "application/json");
    }
    catch (const std::exception& e) {
        std::cerr << "Error in /status: " << e.what() << std::endl;
        res.status = 500;
    }
}

// Обработка GET /images
void handleGetImages(const httplib::Request& req, httplib::Response& res) {
    try {
        json firmwares = getAllFirmwares();
        res.set_content(firmwares.dump(), "application/json");
        std::cout << "GET /images - returned " << firmwares.size() << " firmwares" << std::endl;
    }
    catch (const std::exception& e) {
        std::cerr << "Error in /images: " << e.what() << std::endl;
        res.status = 500;
    }
}

// Обработка POST /image
void handlePostImage(const httplib::Request& req, httplib::Response& res) {
    try {
        std::cout << "[DEBUG] ========== POST /image ==========" << std::endl;
        std::cout << "[DEBUG] Body size: " << req.body.size() / 1024 / 1024 << " MB" << std::endl;

        if (req.body.empty()) {
            json error = { {"error", "No data received"} };
            res.status = 400;
            res.set_content(error.dump(), "application/json");
            return;
        }

        std::string md5 = calculateMD5(req.body);
        if (md5.empty()) {
            json error = { {"error", "Failed to calculate MD5"} };
            res.status = 500;
            res.set_content(error.dump(), "application/json");
            return;
        }

        if (isMD5Exists(md5)) {
            json response = {
                {"status", "duplicate"},
                {"md5", md5},
                {"message", "Firmware with this MD5 already exists"}
            };
            res.set_content(response.dump(), "application/json");
            std::cout << "POST /image - duplicate, MD5=" << md5 << std::endl;
            return;
        }

        std::string filename = "firmware_" + md5.substr(0, 8) + ".bin";
        if (req.has_header("X-Filename")) {
            filename = req.get_header_value("X-Filename");
        }

        std::string file_path = "storage/" + md5;
        std::ofstream out_file(file_path, std::ios::binary);
        if (!out_file.is_open()) {
            json error = { {"error", "Failed to save file"} };
            res.status = 500;
            res.set_content(error.dump(), "application/json");
            return;
        }

        out_file.write(req.body.c_str(), req.body.size());
        out_file.close();

        bool success = addFirmwareToDB(md5, filename, file_path, req.body.size());

        if (success) {
            json response = {
                {"status", "ok"},
                {"md5", md5},
                {"filename", filename},
                {"size", req.body.size()},
                {"message", "Firmware added successfully"}
            };
            res.set_content(response.dump(), "application/json");
            std::cout << "POST /image - added firmware MD5=" << md5 << std::endl;
        }
        else {
            json error = { {"error", "Failed to add firmware to database"} };
            res.status = 500;
            res.set_content(error.dump(), "application/json");
        }
    }
    catch (const std::exception& e) {
        std::cerr << "Error in POST /image: " << e.what() << std::endl;
        json error = { {"error", e.what()} };
        res.status = 500;
        res.set_content(error.dump(), "application/json");
    }
}

// Обработка GET /image/{md5}/info
void handleGetImageInfo(const httplib::Request& req, httplib::Response& res) {
    try {
        std::string md5 = req.matches[1];

        if (!isMD5Exists(md5)) {
            json error = { {"error", "Firmware not found"} };
            res.status = 404;
            res.set_content(error.dump(), "application/json");
            return;
        }

        std::string filename = getFilenameByMD5(md5);
        std::string file_path = getFilePathByMD5(md5);

        long long file_size = 0;
        if (fs::exists(file_path)) {
            file_size = fs::file_size(file_path);
        }

        json response = {
            {"md5", md5},
            {"filename", filename},
            {"file_size", file_size}
        };
        res.set_content(response.dump(), "application/json");
        std::cout << "GET /image/" << md5 << "/info - OK" << std::endl;
    }
    catch (const std::exception& e) {
        std::cerr << "Error in GET /image/info: " << e.what() << std::endl;
        res.status = 500;
    }
}

// Обработка GET /image/{md5}/file - потоковая передача файла
void handleGetFile(const httplib::Request& req, httplib::Response& res) {
    try {
        std::string md5 = req.matches[1];
        std::cout << "[DEBUG] GET /image/" << md5 << "/file - started" << std::endl;

        std::string file_path = getFilePathByMD5(md5);
        if (file_path.empty() || !fs::exists(file_path)) {
            std::cout << "[DEBUG] File not found: " << file_path << std::endl;
            res.status = 404;
            return;
        }

        // Получаем размер файла
        long long file_size = fs::file_size(file_path);
        std::cout << "[DEBUG] File size: " << file_size / 1024 / 1024 << " MB" << std::endl;

        // Открываем файл
        auto file_ptr = std::make_shared<std::ifstream>(file_path, std::ios::binary);
        if (!file_ptr->is_open()) {
            std::cout << "[DEBUG] Cannot open file" << std::endl;
            res.status = 500;
            return;
        }

        // Устанавливаем заголовки
        res.set_header("Content-Type", "application/octet-stream");
        res.set_header("Content-Length", std::to_string(file_size));

        // Потоковая передача чанками
        auto start_time = std::chrono::steady_clock::now();

        res.set_chunked_content_provider(
            "application/octet-stream",
            [file_ptr, file_size, start_time](size_t offset, httplib::DataSink& sink) -> bool {
                const size_t BUFFER_SIZE = 1024 * 1024; // 1 МБ
                static char buffer[BUFFER_SIZE];

                if (offset >= (size_t)file_size) {
                    sink.done();
                    return false;
                }

                file_ptr->seekg(offset);
                size_t remaining = (size_t)(file_size - offset);
                size_t to_read = (remaining < BUFFER_SIZE) ? remaining : BUFFER_SIZE;
                file_ptr->read(buffer, to_read);

                if (file_ptr->gcount() > 0) {
                    sink.write(buffer, (size_t)file_ptr->gcount());

                    // Прогресс каждые 100 МБ
                    if (offset % (100 * 1024 * 1024) < BUFFER_SIZE) {
                        auto now = std::chrono::steady_clock::now();
                        auto elapsed = std::chrono::duration_cast<std::chrono::seconds>(now - start_time).count();
                        if (elapsed > 0) {
                            double speed = offset / 1024.0 / 1024.0 / elapsed;
                            std::cout << "\r[PROGRESS] Sent: " << offset / 1024 / 1024
                                << " / " << file_size / 1024 / 1024 << " MB"
                                << " (" << speed << " MB/s)" << std::flush;
                        }
                    }
                    return true;
                }

                sink.done();
                return false;
            }
        );

        std::cout << "\n[DEBUG] Streaming completed" << std::endl;

    }
    catch (const std::exception& e) {
        std::cerr << "[ERROR] Error in GET /image/file: " << e.what() << std::endl;
        res.status = 500;
    }
}

// Обработка DELETE /image/{md5}
void handleDeleteImage(const httplib::Request& req, httplib::Response& res) {
    try {
        std::string md5 = req.matches[1];

        std::string file_path = getFilePathByMD5(md5);
        if (!file_path.empty() && fs::exists(file_path)) {
            fs::remove(file_path);
        }

        bool deleted = deleteFirmwareByMD5(md5);

        if (deleted) {
            json response = { {"message", "Firmware deleted successfully"} };
            res.set_content(response.dump(), "application/json");
            std::cout << "DELETE /image/" << md5 << " - deleted" << std::endl;
        }
        else {
            json error = { {"error", "Firmware not found"} };
            res.status = 404;
            res.set_content(error.dump(), "application/json");
        }
    }
    catch (const std::exception& e) {
        std::cerr << "Error in DELETE: " << e.what() << std::endl;
        res.status = 500;
    }
}


// ==================== ФУНКЦИИ АВТОЗАГРУЗКИ ====================


// Добавление в реестр
void AddToRegistryStartup() {
    HKEY hKey;
    LONG result = RegOpenKeyExA(HKEY_CURRENT_USER,
        "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
        0, KEY_SET_VALUE, &hKey);

    if (result != ERROR_SUCCESS) {
        return;
    }

    char exePath[MAX_PATH];
    GetModuleFileNameA(NULL, exePath, MAX_PATH);

    std::string command = std::string("\"") + exePath + "\" --background";

    RegSetValueExA(hKey, "FirmwareDBServer", 0, REG_SZ,
        (BYTE*)command.c_str(), (DWORD)(command.length() + 1));
    RegCloseKey(hKey);
}

// Удаление из автозагрузки
void RemoveFromStartup() {
    HKEY hKey;
    if (RegOpenKeyExA(HKEY_CURRENT_USER,
        "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
        0, KEY_SET_VALUE, &hKey) == ERROR_SUCCESS) {
        RegDeleteValueA(hKey, "FirmwareDBServer");
        RegCloseKey(hKey);
    }
}

// Запуск в фоновом режиме
void RunBackgroundMode() {
    HWND hWnd = GetConsoleWindow();
    if (hWnd) {
        ShowWindow(hWnd, SW_HIDE);
    }

    if (!InitializeWinsock()) return;

    if (!fs::exists("storage")) {
        fs::create_directory("storage");
    }

    g_db = initDatabase();
    if (!g_db) {
        WSACleanup();
        return;
    }

    httplib::Server server;
    server.set_payload_max_length(16LL * 1024 * 1024 * 1024);

    server.Get("/status", handleStatus);
    server.Get("/images", handleGetImages);
    server.Post("/image", handlePostImage);
    server.Get(R"(/image/([a-fA-F0-9]{32})/info)", handleGetImageInfo);
    server.Get(R"(/image/([a-fA-F0-9]{32})/file)", handleGetFile);
    server.Delete(R"(/image/([a-fA-F0-9]{32}))", handleDeleteImage);

    server.listen("0.0.0.0", 8081);

    sqlite3_close(g_db);
    WSACleanup();
}

// Запуск в консольном режиме
void RunConsoleMode() {
    if (!InitializeWinsock()) return;

    if (!fs::exists("storage")) {
        fs::create_directory("storage");
        std::cout << "Created storage folder" << std::endl;
    }

    g_db = initDatabase();
    if (!g_db) {
        WSACleanup();
        return;
    }

    httplib::Server server;
    server.set_payload_max_length(16LL * 1024 * 1024 * 1024);

    server.Get("/status", handleStatus);
    server.Get("/images", handleGetImages);
    server.Post("/image", handlePostImage);
    server.Get(R"(/image/([a-fA-F0-9]{32})/info)", handleGetImageInfo);
    server.Get(R"(/image/([a-fA-F0-9]{32})/file)", handleGetFile);
    server.Delete(R"(/image/([a-fA-F0-9]{32}))", handleDeleteImage);

    std::cout << "Server started on port 8081" << std::endl;
    std::cout << "Address: http://localhost:8081" << std::endl;
    std::cout << "Press Ctrl+C to stop" << std::endl;

    server.listen("0.0.0.0", 8081);

    sqlite3_close(g_db);
    WSACleanup();
}


// ==================== ОСНОВНАЯ ФУНКЦИЯ ====================


int main(int argc, char* argv[]) {
    try {
        // Установка рабочей директории
        char exePath[MAX_PATH];
        GetModuleFileNameA(NULL, exePath, MAX_PATH);
        std::string exeDir = fs::path(exePath).parent_path().string();
        SetCurrentDirectoryA(exeDir.c_str());

        // Обработка аргументов
        bool background_mode = false;
        for (int i = 1; i < argc; i++) {
            if (strcmp(argv[i], "--background") == 0) {
                background_mode = true;
            }
            else if (strcmp(argv[i], "--install") == 0) {
                AddToRegistryStartup();
                return 0;
            }
            else if (strcmp(argv[i], "--remove") == 0) {
                RemoveFromStartup();
                return 0;
            }
        }

        if (background_mode) {
            RunBackgroundMode();
            return 0;
        }

        // Первый запуск - установка в автозагрузку
        HKEY hKey;
        bool already_installed = false;
        if (RegOpenKeyExA(HKEY_CURRENT_USER,
            "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
            0, KEY_READ, &hKey) == ERROR_SUCCESS) {
            char value[1024];
            DWORD size = sizeof(value);
            if (RegQueryValueExA(hKey, "FirmwareDBServer", NULL, NULL,
                (BYTE*)value, &size) == ERROR_SUCCESS) {
                already_installed = true;
            }
            RegCloseKey(hKey);
        }

        if (!already_installed) {
            AddToRegistryStartup();
        }

        RunConsoleMode();
    }
    catch (const std::exception& e) {
        std::cerr << "Fatal error: " << e.what() << std::endl;
        return 1;
    }

    return 0;
}