using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Windows.Forms;

namespace FirmwareInfrastructure.Services
{
    /// <summary>
    /// Класс для управления Windows-службой сервера прошивок.
    /// </summary>
    public static class ServerControlService
    {
        private const string ServiceName = "FirmwareFlashServer";

        /// <summary>
        /// Проверяет, установлена ли служба.
        /// </summary>
        public static bool IsServiceInstalled()
        {
            try
            {
                using (var sc = new ServiceController(ServiceName))
                {
                    var status = sc.Status;
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, запущен ли сервер.
        /// </summary>
        public static bool IsServerRunning()
        {
            try
            {
                using (var sc = new ServiceController(ServiceName))
                {
                    return sc.Status == ServiceControllerStatus.Running;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Запускает сервер.
        /// </summary>
        public static bool StartServer()
        {
            try
            {
                using (var sc = new ServiceController(ServiceName))
                {
                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        /// <summary>
        /// Останавливает сервер.
        /// </summary>
        public static bool StopServer()
        {
            try
            {
                using (var sc = new ServiceController(ServiceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка остановки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        /// <summary>
        /// АВТОМАТИЧЕСКАЯ УСТАНОВКА СЕРВЕРА.
        /// Ищет сервер в разных местах, копирует если нужно, устанавливает.
        /// </summary>
        public static bool InstallServer()
        {
            try
            {
                // Шаг 1: Находим где лежит сервер
                string serverSourcePath = FindServerExe();

                if (string.IsNullOrEmpty(serverSourcePath))
                {
                    MessageBox.Show(
                        "Не найден файл сервера (FirmwareServer.exe).\n\n" +
                        "Убедитесь, что проект FirmwareServer собран.",
                        "Файл не найден",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }

                // Шаг 2: Определяем куда копировать сервер
                string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "FirmwareServer");
                string serverTargetPath = Path.Combine(installDir, "FirmwareServer.exe");

                // Шаг 3: Создаём папку, если её нет
                Directory.CreateDirectory(installDir);

                // Шаг 4: Копируем файл сервера (если нужно)
                if (!File.Exists(serverTargetPath) || File.GetLastWriteTime(serverTargetPath) != File.GetLastWriteTime(serverSourcePath))
                {
                    File.Copy(serverSourcePath, serverTargetPath, true);
                }

                // Шаг 5: Запускаем installutil от администратора
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"\"{FindInstallUtil()}\" \"{serverTargetPath}\"\"",
                    Verb = "runas", // Запуск от администратора
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = Process.Start(psi);
                process?.WaitForExit();

                if (process?.ExitCode == 0)
                {
                    MessageBox.Show(
                        "Сервер успешно установлен!\n\n" +
                        "Он будет автоматически запускаться при старте Windows.",
                        "Успех",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    MessageBox.Show(
                        "Ошибка при установке сервера.\n\n" +
                        "Попробуйте установить вручную от имени администратора.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка установки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Удаление сервера.
        /// </summary>
        public static bool UninstallServer()
        {
            try
            {
                string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "FirmwareServer");
                string serverPath = Path.Combine(installDir, "FirmwareServer.exe");

                if (!File.Exists(serverPath))
                {
                    MessageBox.Show("Сервер не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"\"{FindInstallUtil()}\" /u \"{serverPath}\"\"",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = Process.Start(psi);
                process?.WaitForExit();

                if (process?.ExitCode == 0)
                {
                    // Удаляем папку после деинсталляции
                    try { Directory.Delete(installDir, true); } catch { }

                    MessageBox.Show("Сервер успешно удалён.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Ищет файл сервера в разных местах.
        /// </summary>
        private static string FindServerExe()
        {
            string[] possiblePaths = new[]
            {
                // Рядом с клиентом (если скопировали)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FirmwareServer.exe"),
                
                // В папке Debug сервера (при разработке)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\FirmwareServer\bin\Debug\FirmwareServer.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\FirmwareServer\bin\Release\FirmwareServer.exe"),
                
                // Абсолютный путь (надеюсь, вы знаете где проект)
                @"C:\Users\lavor\source\repos\FirmwareManager\FirmwareServer\bin\Debug\FirmwareServer.exe"
            };

            foreach (string path in possiblePaths)
            {
                string fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }

        /// <summary>
        /// Находит installutil.exe в системе.
        /// </summary>
        private static string FindInstallUtil()
        {
            // Обычные пути для .NET Framework
            string[] possiblePaths = new[]
            {
                @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe",
                @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe",
                @"C:\Windows\Microsoft.NET\Framework\v2.0.50727\installutil.exe",
                @"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\installutil.exe"
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Если не нашли, надеемся что он в PATH
            return "installutil.exe";
        }

        /// <summary>
        /// Получает статус сервера в виде текста.
        /// </summary>
        public static string GetServerStatusText()
        {
            if (!IsServiceInstalled())
            {
                return "НЕ УСТАНОВЛЕН";
            }

            try
            {
                using (var sc = new ServiceController(ServiceName))
                {
                    switch (sc.Status)
                    {
                        case ServiceControllerStatus.Running:
                            return "РАБОТАЕТ";
                        case ServiceControllerStatus.Stopped:
                            return "ОСТАНОВЛЕН";
                        case ServiceControllerStatus.StartPending:
                            return "ЗАПУСКАЕТСЯ...";
                        case ServiceControllerStatus.StopPending:
                            return "ОСТАНАВЛИВАЕТСЯ...";
                        default:
                            return sc.Status.ToString();
                    }
                }
            }
            catch
            {
                return "НЕДОСТУПЕН";
            }
        }

        /// <summary>
        /// Получает цвет для индикатора статуса.
        /// </summary>
        public static System.Drawing.Color GetServerStatusColor()
        {
            string status = GetServerStatusText();

            switch (status)
            {
                case "РАБОТАЕТ":
                    return System.Drawing.Color.Green;
                case "ОСТАНОВЛЕН":
                case "НЕ УСТАНОВЛЕН":
                    return System.Drawing.Color.Red;
                case "ЗАПУСКАЕТСЯ...":
                case "ОСТАНАВЛИВАЕТСЯ...":
                    return System.Drawing.Color.Orange;
                default:
                    return System.Drawing.Color.Gray;
            }
        }
    }
}