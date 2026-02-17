using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FrimwareDatabase.Core.Models;
using FrimwareDatabase.Core.Database;
using FrimwareDatabase.Core.Services;
using FrimwareDatabase.Infrastructure.Helpers;
using FirmwareInfrastructure.Helpers;
using FirmwareInfrastructure.Services;

namespace FrimwareDatabase.UI.Forms
{
    /// <summary>
    /// Главная форма приложения для управления базой данных прошивок.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly XmlDatabase _xmlDatabase;
        private readonly HashService _hashService;
        private readonly FileService _fileService;
        private readonly IFlashService _flashService;
        private string _currentDatabasePath;

        // Для панели сервера
        private int _normalHeight = 415;
        private int _expandedHeight = 520;
        private System.Timers.Timer _serverStatusTimer;

        /// <summary>
        /// Конструктор главной формы.
        /// </summary>
        /// <summary>
        /// Конструктор главной формы.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            _xmlDatabase = new XmlDatabase();
            _hashService = new HashService();
            _fileService = new FileService();
            _flashService = new FlashService();

            // Начальные размеры
            this.Height = 100;

            // Явно скрываем панели при запуске
            groupBoxDatabase.Visible = false;
            groupBoxOptions.Visible = false;

            ConfigureListView();
            StartStatusTimer();
        }

        /// <summary>
        /// Настройка колонок ListView.
        /// </summary>
        private void ConfigureListView()
        {
            listViewFirmwares.View = View.Details;
            listViewFirmwares.FullRowSelect = true;
            listViewFirmwares.GridLines = true;
            listViewFirmwares.Columns.Clear();
            listViewFirmwares.Columns.Add("Имя файла", 175); // Ширина 175 вместо 200
            listViewFirmwares.Columns.Add("Дата добавления", 120);
        }

        /// <summary>
        /// Запускает таймер для отслеживания состояния сервера.
        /// </summary>
        private void StartStatusTimer()
        {
            _serverStatusTimer = new System.Timers.Timer(2000); // Каждые 2 секунды
            _serverStatusTimer.Elapsed += (s, e) =>
            {
                // Обновляем UI в потоке формы
                this.Invoke((MethodInvoker)delegate
                {
                    UpdateServerStatus();
                });
            };
            _serverStatusTimer.Start();
        }

        /// <summary>
        /// Обновляет отображение статуса сервера.
        /// </summary>
        private void UpdateServerStatus()
        {
            if (labelServerStatus == null) return;

            string status = ServerControlService.GetServerStatusText();

            if (status == "РАБОТАЕТ")
            {
                labelServerStatus.Text = "ЗАПУЩЕН";
                labelServerStatus.ForeColor = Color.Green;
                buttonToggleServer.Text = "Остановить сервер";
            }
            else if (status == "ОСТАНОВЛЕН")
            {
                labelServerStatus.Text = "ОТКЛЮЧЁН";
                labelServerStatus.ForeColor = Color.Red;
                buttonToggleServer.Text = "Запустить сервер";
            }
            else if (status == "НЕ УСТАНОВЛЕН")
            {
                labelServerStatus.Text = "НЕ УСТАНОВЛЕН";
                labelServerStatus.ForeColor = Color.Gray;
                buttonToggleServer.Text = "Установить сервер";
            }
            else
            {
                labelServerStatus.Text = status;
                labelServerStatus.ForeColor = Color.Orange;
                buttonToggleServer.Text = "Обновить";
            }
        }

        /// <summary>
        /// Обработчик кнопки "Создать БД".
        /// </summary>
        private void buttonCreateDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath = DialogHelper.ShowSaveFileDialog(
                    "Создать новую базу данных",
                    "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                    "xml");

                if (!string.IsNullOrEmpty(filePath))
                {
                    _xmlDatabase.CreateDatabase(filePath);
                    _currentDatabasePath = filePath;

                    groupBoxDatabase.Text = $"База данных: {Path.GetFileName(filePath)}";
                    LoadFirmwareList();

                    // Создаём папки сервера, если их нет
                    Directory.CreateDirectory(@"C:\ProgramData\FirmwareServer\Tasks\");
                    Directory.CreateDirectory(@"C:\ProgramData\FirmwareServer\Results\");
                    Directory.CreateDirectory(@"C:\ProgramData\FirmwareServer\Logs\");

                    // Показываем форму после создания БД
                    groupBoxDatabase.Visible = true;
                    this.Height = _normalHeight;

                    MessageBox.Show(
                        "База данных успешно создана!",
                        "Успех",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при создании базы данных: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Выбрать БД".
        /// </summary>
        private void buttonSelectDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath = DialogHelper.ShowOpenFileDialog(
                    "Выберите файл базы данных",
                    "XML files (*.xml)|*.xml|All files (*.*)|*.*");

                if (!string.IsNullOrEmpty(filePath))
                {
                    _currentDatabasePath = filePath;

                    groupBoxDatabase.Text = $"База данных: {Path.GetFileName(filePath)}";
                    LoadFirmwareList();

                    groupBoxDatabase.Visible = true;
                    this.Height = _normalHeight;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при открытии базы данных: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Загружает список прошивок из текущей БД в ListView.
        /// </summary>
        private void LoadFirmwareList()
        {
            if (string.IsNullOrEmpty(_currentDatabasePath) || !File.Exists(_currentDatabasePath))
            {
                listViewFirmwares.Items.Clear();
                return;
            }

            var firmwares = _xmlDatabase.GetAllFirmwares(_currentDatabasePath);
            listViewFirmwares.Items.Clear();

            foreach (var firmware in firmwares)
            {
                if (string.IsNullOrEmpty(firmware.FileName))
                    continue;

                var item = new ListViewItem(firmware.FileName);
                item.SubItems.Add(firmware.RegistrationDate ?? "—");
                item.Tag = firmware;

                listViewFirmwares.Items.Add(item);
            }

            if (listViewFirmwares.Items.Count == 0)
            {
                var item = new ListViewItem("(база данных пуста)");
                item.SubItems.Add("—");
                listViewFirmwares.Items.Add(item);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Добавить файл в БД".
        /// </summary>
        private void buttonAddFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentDatabasePath))
            {
                MessageBox.Show(
                    "Сначала выберите или создайте базу данных.",
                    "Нет БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string filePath = DialogHelper.ShowOpenFileDialog(
                    "Выберите файл прошивки",
                    "Все поддерживаемые файлы (*.bin;*.hex;*.iso;*.img)|*.bin;*.hex;*.iso;*.img|" +
                    "BIN файлы (*.bin)|*.bin|HEX файлы (*.hex)|*.hex|" +
                    "ISO образы (*.iso)|*.iso|IMG образы (*.img)|*.img|" +
                    "Все файлы (*.*)|*.*");

                if (string.IsNullOrEmpty(filePath))
                    return;

                string fileName = Path.GetFileName(filePath);
                string md5Hash = _hashService.CalculateMD5(filePath);

                if (_xmlDatabase.IsChecksumExists(_currentDatabasePath, md5Hash))
                {
                    MessageBox.Show(
                        $"Прошивка с такой контрольной суммой уже есть в базе данных.\n\nФайл: {fileName}\nMD5: {md5Hash}",
                        "Дубликат",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var firmware = new Firmware
                {
                    FileName = fileName,
                    FilePath = filePath,
                    CheckSum = md5Hash,
                    RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                _xmlDatabase.AddFirmware(_currentDatabasePath, firmware);
                LoadFirmwareList();

                MessageBox.Show(
                    $"Файл успешно добавлен в базу данных!\n\nИмя: {fileName}\nMD5: {md5Hash}",
                    "Успех",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при добавлении файла: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Удалить выбранный".
        /// </summary>
        private void buttonDeleteSelected_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentDatabasePath))
            {
                MessageBox.Show(
                    "Сначала выберите базу данных.",
                    "Нет БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (listViewFirmwares.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите файл для удаления из списка.",
                    "Ничего не выбрано",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = listViewFirmwares.SelectedItems[0];
            string fileName = selectedItem.Text;

            if (fileName == "(база данных пуста)")
            {
                MessageBox.Show(
                    "В базе данных нет файлов для удаления.",
                    "Пустая БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Вы уверены, что хотите удалить файл '{fileName}' из базы данных?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            try
            {
                _xmlDatabase.DeleteFirmware(_currentDatabasePath, fileName);
                LoadFirmwareList();

                MessageBox.Show(
                    $"Файл '{fileName}' успешно удалён из базы данных.",
                    "Удалено",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при удалении файла: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Записать на носитель".
        /// </summary>
        private void buttonWriteToUSB_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentDatabasePath))
            {
                MessageBox.Show(
                    "Сначала выберите базу данных.",
                    "Нет БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (listViewFirmwares.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите файл для записи из списка.",
                    "Ничего не выбрано",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Проверяем состояние сервера
            string serverStatus = ServerControlService.GetServerStatusText();

            if (serverStatus != "РАБОТАЕТ")
            {
                var result = MessageBox.Show(
                    $"Сервер {serverStatus.ToLower()}. Для записи на USB необходим работающий сервер.\n\n" +
                    "Открыть панель управления сервером?",
                    "Сервер не работает",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    // Разворачиваем форму, если она свёрнута
                    if (this.Height == _normalHeight)
                    {
                        this.Height = _expandedHeight;
                        groupBoxOptions.Visible = true;
                        buttonOptions.Text = "▲ Параметры сервера";
                    }
                }
                return;
            }

            var selectedItem = listViewFirmwares.SelectedItems[0];
            string fileName = selectedItem.Text;

            if (fileName == "(база данных пуста)")
            {
                MessageBox.Show(
                    "В базе данных нет файлов для записи.",
                    "Пустая БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var firmware = selectedItem.Tag as Firmware;
            if (firmware == null)
            {
                MessageBox.Show(
                    "Ошибка получения данных прошивки.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Получаем список доступных USB-накопителей
            var usbDrives = _flashService.GetAvailableUsbDrives();

            if (usbDrives.Count == 0)
            {
                MessageBox.Show(
                    "Не найдено подключенных USB-накопителей.\n\n" +
                    "Подключите флешку и нажмите 'Записать' снова.",
                    "Нет носителя",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string targetDrive;

                if (usbDrives.Count == 1)
                {
                    targetDrive = usbDrives[0];
                }
                else
                {
                    var driveInfoList = usbDrives.Select(d =>
                        FileSystemHelper.GetDriveDisplayName(new DriveInfo(d))).ToArray();

                    int choice = DialogHelper.ShowChoiceDialog("Выберите USB-накопитель", driveInfoList);
                    if (choice == -1) return;
                    targetDrive = usbDrives[choice];
                }

                // Проверяем свободное место
                var fileInfo = new FileInfo(firmware.FilePath);
                if (!_flashService.HasEnoughSpace(targetDrive, fileInfo.Length))
                {
                    long freeSpace = _flashService.GetFreeSpace(targetDrive) / 1024 / 1024;
                    long needSpace = fileInfo.Length / 1024 / 1024;

                    MessageBox.Show(
                        $"На носителе недостаточно места.\n\n" +
                        $"Нужно: {needSpace} МБ\n" +
                        $"Свободно: {freeSpace} МБ",
                        "Недостаточно места",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Спрашиваем подтверждение
                var confirmWrite = MessageBox.Show(
                    $"Записать файл '{firmware.FileName}' на диск {targetDrive}?",
                    "Подтверждение записи",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmWrite != DialogResult.Yes)
                    return;

                // Выполняем запись
                bool success = _flashService.WriteFirmware(firmware, targetDrive);

                if (success)
                {
                    MessageBox.Show(
                        $"Файл {firmware.FileName} успешно записан на {targetDrive}",
                        "Успех",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при записи файла: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки параметров (разворачивание/сворачивание).
        /// </summary>
        private void buttonOptions_Click(object sender, EventArgs e)
        {
            if (this.Height == _normalHeight)
            {
                // Разворачиваем
                this.Height = _expandedHeight;
                groupBoxOptions.Visible = true;
                UpdateServerStatus(); // Обновляем статус при открытии
            }
            else
            {
                // Сворачиваем
                this.Height = _normalHeight;
                groupBoxOptions.Visible = false;
            }
        }

        /// <summary>
        /// Обработчик кнопки запуска/остановки сервера.
        /// </summary>
        /// <summary>
        /// Обработчик кнопки запуска/остановки/установки сервера.
        /// </summary>
        private void buttonToggleServer_Click(object sender, EventArgs e)
        {
            string status = ServerControlService.GetServerStatusText();

            try
            {
                if (status == "РАБОТАЕТ")
                {
                    var result = MessageBox.Show(
                        "Остановить сервер? Это может прервать текущие операции записи.",
                        "Подтверждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        ServerControlService.StopServer();
                    }
                }
                else if (status == "ОСТАНОВЛЕН")
                {
                    var result = MessageBox.Show(
                        "Запустить сервер? Он будет работать в фоновом режиме.",
                        "Подтверждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        ServerControlService.StartServer();
                    }
                }
                else if (status == "НЕ УСТАНОВЛЕН")
                {
                    var result = MessageBox.Show(
                        "Сервер не установлен. Хотите установить его сейчас?\n\n" +
                        "Для установки потребуются права администратора.",
                        "Установка сервера",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Отключаем кнопку на время установки
                        buttonToggleServer.Enabled = false;
                        buttonToggleServer.Text = "УСТАНОВКА...";

                        // Асинхронно устанавливаем, чтобы форма не зависла
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            bool success = ServerControlService.InstallServer();

                            // Возвращаемся в UI поток
                            this.Invoke((MethodInvoker)delegate
                            {
                                buttonToggleServer.Enabled = true;
                                if (success)
                                {
                                    // Пробуем запустить после установки
                                    ServerControlService.StartServer();
                                }
                                UpdateServerStatus();
                            });
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonToggleServer.Enabled = true;
            }

            UpdateServerStatus();
        }

        private void buttonUninstallServer_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить сервер?\n\n" +
                "После удаления запись на USB станет невозможна.",
                "Удаление сервера",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                ServerControlService.UninstallServer();
                UpdateServerStatus();
            }
        }

        /// <summary>
        /// Обработчик для открытия папки заданий.
        /// </summary>
        private void linkTasks_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = @"C:\ProgramData\FirmwareServer\Tasks\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        /// <summary>
        /// Обработчик для открытия папки логов.
        /// </summary>
        private void linkLogs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = @"C:\ProgramData\FirmwareServer\Logs\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        /// <summary>
        /// Обработчик закрытия формы (останавливаем таймер).
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _serverStatusTimer?.Stop();
            _serverStatusTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}