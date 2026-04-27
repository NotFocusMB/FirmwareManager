using FirmwareClient.Services;
using FirmwareManager;
using FrimwareDatabase.Core.Models;
using FrimwareDatabase.Core.Services;
using FrimwareDatabase.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrimwareDatabase.UI.Forms
{
    /// <summary>
    /// Главная форма приложения для управления базой данных прошивок.
    /// </summary>
    public partial class MainForm : Form
    {
        private DatabaseClient _databaseClient;
        private FlashWriterClient _flashWriterClient;
        private AppConfig _config;
        private List<Firmware> _currentFirmwares = new List<Firmware>();

        // Для панели параметров
        private int _normalHeight = 415;
        private int _normalWidth = 400;
        private int _expandedHeight = 500;
        private System.Timers.Timer _statusTimer;

        public MainForm()
        {
            InitializeComponent();

            _config = ConfigService.LoadConfig();
            MessageBox.Show($"Connecting to: {_config.DatabaseServerUrl}");
            _databaseClient = new DatabaseClient(_config.DatabaseServerUrl);
            _flashWriterClient = new FlashWriterClient(_config.FlashServiceUrl);

            // Начальные размеры
            this.Height = _normalHeight;
            this.Width = _normalWidth;

            groupBoxDatabase.Visible = true;

            UpdateDatabaseGroupBoxTitle();

            ConfigureListView();
            StartStatusTimer();

            // Загрузка списка прошивок 
            _ = LoadFirmwareListAsync();
        }

        /// <summary>
        /// Обновляет заголовок groupBoxDatabase с информацией о БД
        /// </summary>
        private void UpdateDatabaseGroupBoxTitle()
        {
            string serverUrl = _config.DatabaseServerUrl;
            string serverInfo = serverUrl.Replace("http://", "").Replace("https://", "");
            groupBoxDatabase.Text = $"База данных прошивок [{serverInfo}]";
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
            listViewFirmwares.Columns.Add("Имя файла", 115);
            listViewFirmwares.Columns.Add("Размер (МБ)", 100);
            listViewFirmwares.Columns.Add("Дата добавления", 130);
        }

        /// <summary>
        /// Запускает таймер для отслеживания состояния серверов.
        /// </summary>
        private void StartStatusTimer()
        {
            _statusTimer = new System.Timers.Timer(3000); // Каждые 3 секунды
            _statusTimer.Elapsed += (s, e) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    _ = UpdateServersStatus();
                });
            };
            _statusTimer.Start();
        }

        /// <summary>
        /// Обновляет отображение статуса обоих серверов.
        /// </summary>
        private async Task UpdateServersStatus()
        {
            if (labelDatabaseStatus == null) return;

            // Проверка статуса БД-сервера
            bool dbAvailable = await _databaseClient.IsServerAvailableAsync();

            if (dbAvailable)
            {
                labelDatabaseStatus.Text = "ПОДКЛЮЧЁН";
                labelDatabaseStatus.ForeColor = Color.Green;
            }
            else
            {
                labelDatabaseStatus.Text = "НЕ ПОДКЛЮЧЁН";
                labelDatabaseStatus.ForeColor = Color.Red;
            }

            // Проверка статуса сервиса записи
            bool flashServiceAvailable = await CheckFlashServiceStatusAsync();

            if (flashServiceAvailable)
            {
                labelFlashServiceStatus.Text = "ПОДКЛЮЧЁН";
                labelFlashServiceStatus.ForeColor = Color.Green;
            }
            else
            {
                labelFlashServiceStatus.Text = "НЕ ПОДКЛЮЧЁН";
                labelFlashServiceStatus.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Проверяет доступность сервиса записи
        /// </summary>
        private async Task<bool> CheckFlashServiceStatusAsync()
        {
            return await _flashWriterClient.IsAvailableAsync();
        }

        /// <summary>
        /// Загружает список прошивок из удалённой БД в ListView.
        /// </summary>
        private async Task LoadFirmwareListAsync()
        {
            try
            {
                // Проверка доступности сервера
                if (!await _databaseClient.IsServerAvailableAsync())
                {
                    listViewFirmwares.Items.Clear();
                    var item = new ListViewItem("⚠️ СЕРВЕР БД НЕ ДОСТУПЕН");
                    item.SubItems.Add("—");
                    item.SubItems.Add("—");
                    listViewFirmwares.Items.Add(item);
                    return;
                }

                _currentFirmwares = await _databaseClient.GetAllFirmwaresAsync();
                listViewFirmwares.Items.Clear();

                foreach (var firmware in _currentFirmwares)
                {
                    var item = new ListViewItem(firmware.FileName);

                    // Перевод размера в МБ
                    string sizeMB = firmware.FileSize > 0
                        ? $"{firmware.FileSize / 1024.0 / 1024.0:F2}"
                        : "—";
                    item.SubItems.Add(sizeMB);
                    item.SubItems.Add(firmware.RegistrationDate ?? "—");
                    item.Tag = firmware;
                    listViewFirmwares.Items.Add(item);
                }

                if (listViewFirmwares.Items.Count == 0)
                {
                    var item = new ListViewItem("(база данных пуста)");
                    item.SubItems.Add("—");
                    item.SubItems.Add("—");
                    listViewFirmwares.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Добавить файл"
        /// </summary>
        private async void buttonAddFile_Click(object sender, EventArgs e)
        {
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

                if (!await _databaseClient.IsServerAvailableAsync())
                {
                    MessageBox.Show("БД-сервер недоступен.", "Сервер не отвечает",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                buttonAddFile.Enabled = false;
                buttonAddFile.Text = "Добавление...";

                // Отображение формы прогресса
                using (var progressForm = new ProgressForm("Добавление прошивки"))
                {
                    var progress = new Progress<long>(bytesSent =>
                    {
                        // Можно обновлять текст по необходимости
                    });

                    var cts = new CancellationTokenSource();
                    progressForm.CancelRequested += (s, ev) => cts.Cancel();

                    progressForm.Show();

                    try
                    {
                        var newFirmware = await _databaseClient.AddFirmwareWithProgressAsync(filePath, progress, cts.Token);
                        progressForm.Close();

                        await LoadFirmwareListAsync();

                        MessageBox.Show($"Файл успешно добавлен в базу данных!\n\nИмя: {newFirmware.FileName}\nMD5: {newFirmware.Md5}",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        progressForm.Close();
                        MessageBox.Show($"Ошибка при добавлении файла: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        buttonAddFile.Enabled = true;
                        buttonAddFile.Text = "Добавить файл";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Добавить образ носителя" (создание образа с флешки).
        /// НЕ ИСПОЛЬЗУЕТСЯ
        /// </summary>
        private async void buttonAddImageFromUSB_Click(object sender, EventArgs e)
        {
            // Проверка доступности БД-сервера
            if (!await _databaseClient.IsServerAvailableAsync())
            {
                MessageBox.Show("БД-сервер недоступен.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка доступности сервиса записи
            if (!await CheckFlashServiceStatusAsync())
            {
                MessageBox.Show(
                    "Сервис записи не подключён. Убедитесь, что сервис запущен на порту 8080.",
                    "Сервис недоступен",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Получение списка USB-накопителей
            List<FlashWriterClient.UsbDriveInfo> usbDrives;
            try
            {
                usbDrives = await _flashWriterClient.GetUsbDrivesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения списка USB: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (usbDrives.Count == 0)
            {
                MessageBox.Show("Не найдено подключенных USB-накопителей.", "Нет носителя",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Выбор диска - источника
            string sourceDrive;
            if (usbDrives.Count == 1)
            {
                sourceDrive = usbDrives[0].letter;
            }
            else
            {
                var driveNames = usbDrives.Select(d => $"{d.letter} ({d.volume_label}) - {d.total_mb} МБ всего").ToArray();
                int choice = DialogHelper.ShowChoiceDialog("Выберите USB-накопитель (источник)", driveNames);
                if (choice == -1) return;
                sourceDrive = usbDrives[choice].letter;
            }

            // Запрос имени файла для сохранения
            string filename = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите имя файла для сохранения образа:",
                "Имя файла",
                $"usb_image_{DateTime.Now:yyyyMMdd_HHmmss}.img",
                -1, -1);

            if (string.IsNullOrEmpty(filename))
                return;

            // Добавление расширения, если его нет
            if (!filename.Contains('.'))
                filename += ".img";

            var confirmCopy = MessageBox.Show(
                $"Скопировать образ с диска {sourceDrive} в базу данных под именем '{filename}'?",
                "Подтверждение копирования",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmCopy != DialogResult.Yes)
                return;

            try
            {
                //buttonAddImageFromUSB.Enabled = false;
                //buttonAddImageFromUSB.Text = "Копирование...";

                bool success = await _flashWriterClient.CopyFromUsbToDbAsync(sourceDrive, _config.DatabaseServerUrl, filename);

                if (success)
                {
                    MessageBox.Show($"Образ с диска {sourceDrive} успешно скопирован в базу данных.",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Обновление списка прошивок
                    await LoadFirmwareListAsync();
                }
                else
                {
                    MessageBox.Show("Ошибка при копировании образа.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //buttonAddImageFromUSB.Enabled = true;
                //buttonAddImageFromUSB.Text = "Добавить образ носителя";
            }
        }

        /// <summary>
        /// Обработчик кнопки "Удалить выбранный".
        /// </summary>
        private async void buttonDeleteSelected_Click(object sender, EventArgs e)
        {
            if (listViewFirmwares.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите файл для удаления из списка.", "Ничего не выбрано",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = listViewFirmwares.SelectedItems[0];
            var firmware = selectedItem.Tag as Firmware;

            if (firmware == null || string.IsNullOrEmpty(firmware.Md5))
            {
                MessageBox.Show("Невозможно удалить: неверные данные прошивки.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Вы уверены, что хотите удалить файл '{firmware.FileName}' из базы данных?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            try
            {
                buttonDeleteSelected.Enabled = false;

                if (!await _databaseClient.IsServerAvailableAsync())
                {
                    MessageBox.Show("БД-сервер недоступен.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool deleted = await _databaseClient.DeleteFirmwareAsync(firmware.Md5);

                if (deleted)
                {
                    await LoadFirmwareListAsync();
                    MessageBox.Show($"Файл '{firmware.FileName}' успешно удалён из базы данных.",
                        "Удалено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось удалить файл.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении файла: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonDeleteSelected.Enabled = true;
            }
        }

        /// <summary>
        /// Обработчик кнопки "Записать на носитель".
        /// </summary>
        private async void buttonWriteToUSB_Click(object sender, EventArgs e)
        {
            // Проверка доступности БД-сервера
            if (!await _databaseClient.IsServerAvailableAsync())
            {
                MessageBox.Show(
                    "БД-сервер недоступен. Убедитесь, что сервер запущен.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Проверка доступности сервиса записи
            if (!await CheckFlashServiceStatusAsync())
            {
                MessageBox.Show(
                    "Сервис записи не подключён. Убедитесь, что сервис запущен на порту 8080.",
                    "Сервис недоступен",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (listViewFirmwares.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите файл для записи из списка.", "Ничего не выбрано",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = listViewFirmwares.SelectedItems[0];
            var firmware = selectedItem.Tag as Firmware;

            if (firmware == null || string.IsNullOrEmpty(firmware.Md5))
            {
                MessageBox.Show("Ошибка получения данных прошивки.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Получение списка USB-накопителей
            List<FlashWriterClient.UsbDriveInfo> usbDrives;
            try
            {
                usbDrives = await _flashWriterClient.GetUsbDrivesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения списка USB: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (usbDrives.Count == 0)
            {
                MessageBox.Show("Не найдено подключенных USB-накопителей.", "Нет носителя",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Выбор целевого диска
            string targetDrive;
            if (usbDrives.Count == 1)
            {
                targetDrive = usbDrives[0].letter;
            }
            else
            {
                var driveNames = usbDrives.Select(d => $"{d.letter} ({d.volume_label}) - {d.free_mb} МБ свободно").ToArray();
                int choice = DialogHelper.ShowChoiceDialog("Выберите USB-накопитель", driveNames);
                if (choice == -1) return;
                targetDrive = usbDrives[choice].letter;
            }

            // Предупреждение о форматировании
            var confirmWrite = MessageBox.Show(
                $"ВНИМАНИЕ! Запись на диск {targetDrive} УНИЧТОЖИТ все данные на флешке.\n\n" +
                $"Вы действительно хотите записать файл '{firmware.FileName}'?\n\n" +
                "Все данные на флешке будут безвозвратно удалены!",
                "Подтверждение записи",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmWrite != DialogResult.Yes)
                return;

            try
            {
                buttonWriteToUSB.Enabled = false;
                buttonWriteToUSB.Text = "Запись...";

                // Отображение полоски прогресса
                using (var progressForm = new ProgressForm("Запись на USB"))
                {
                    var progress = new Progress<long>(bytesWritten =>
                    {
                        // Обновление текста прогресса
                        if (bytesWritten > 0)
                        {
                            double mb = bytesWritten / 1024.0 / 1024.0;
                            progressForm.UpdateProgress(mb, "MB");
                        }
                    });

                    var cts = new CancellationTokenSource();
                    progressForm.CancelRequested += (s, ev) => cts.Cancel();

                    progressForm.Show();

                    try
                    {
                        // Отправление сервису записи MD5, буквы диска и адреса БД-сервера
                        bool success = await _flashWriterClient.WriteToUsbByMd5WithProgressAsync(
                            firmware.Md5,
                            targetDrive,
                            _config.DatabaseServerUrl,
                            progress,
                            cts.Token);

                        progressForm.Close();

                        if (success)
                        {
                            MessageBox.Show($"Файл '{firmware.FileName}' успешно записан на {targetDrive}",
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при записи на USB.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        progressForm.Close();
                        MessageBox.Show("Операция записи была отменена пользователем.", "Отмена",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        progressForm.Close();
                        MessageBox.Show($"Ошибка при записи файла: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            finally
            {
                buttonWriteToUSB.Enabled = true;
                buttonWriteToUSB.Text = "Записать на носитель";
            }
        }

        /// <summary>
        /// Обработчик кнопки параметров (разворачивание/сворачивание).
        /// </summary>
        private void buttonOptions_Click(object sender, EventArgs e)
        {
            if (this.Height == _normalHeight)
            {
                this.Height = _expandedHeight;
                groupBoxOptions.Visible = true;
                _ = UpdateServersStatus();
            }
            else
            {
                this.Height = _normalHeight;
                groupBoxOptions.Visible = false;
            }
        }

        /// <summary>
        /// Обработчик закрытия формы (останавливаем таймер).
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _statusTimer?.Stop();
            _statusTimer?.Dispose();
            base.OnFormClosing(e);
        }

        private void labelFlashServiceStatus_Click(object sender, EventArgs e)
        {

        }

        private void buttonToggleServer_Click(object sender, EventArgs e)
        {

        }
    }
}