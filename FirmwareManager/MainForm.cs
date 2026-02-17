using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FrimwareDatabase.Core.Models;
using FrimwareDatabase.Core.Database;
using FrimwareDatabase.Core.Services;
using FrimwareDatabase.Infrastructure.Helpers;
using System.Windows.Forms.VisualStyles;

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
        private string _currentDatabasePath;

        /// <summary>
        /// Конструктор главной формы.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            _xmlDatabase = new XmlDatabase();
            _hashService = new HashService();
            _fileService = new FileService();
            this.Height = 100;
            this.Width = 343;

            // Настройка ListView для отображения в 2 колонки (убрали расширение)
            ConfigureListView();
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
            listViewFirmwares.Columns.Add("Имя файла", 180);  // Ширина увеличена, так как расширение теперь в названии
            listViewFirmwares.Columns.Add("Дата добавления", 120);
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

                    // Отображаем путь в заголовке группы
                    groupBoxDatabase.Text = $"База данных: {Path.GetFileName(filePath)}";

                    // Очищаем список и загружаем содержимое (пустое)
                    LoadFirmwareList();

                    MessageBox.Show(
                        $"База данных успешно создана!",
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

                    // Отображаем путь в заголовке группы
                    groupBoxDatabase.Text = $"База данных: {Path.GetFileName(filePath)}";

                    // Загружаем и отображаем содержимое БД
                    LoadFirmwareList();
                    groupBoxDatabase.Visible = true;
                    this.Height = 415;
                    this.Width = 343;
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

                // Колонка с расширением больше не добавляется
                var item = new ListViewItem(firmware.FileName);
                item.SubItems.Add(firmware.RegistrationDate ?? "—");

                // Сохраняем контрольную сумму в Tag, пригодится для записи
                item.Tag = firmware;

                listViewFirmwares.Items.Add(item);
            }

            // Если список пуст, показываем заглушку (только одна колонка с прочерком для даты)
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
            // Проверяем, выбрана ли БД
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
                // Открываем файл прошивки
                string filePath = DialogHelper.ShowOpenFileDialog(
                    "Выберите файл прошивки",
                    "Все поддерживаемые файлы (*.bin;*.hex)|*.bin;*.hex|BIN файлы (*.bin)|*.bin|HEX файлы (*.hex)|*.hex|Все файлы (*.*)|*.*");

                if (string.IsNullOrEmpty(filePath))
                    return;

                string fileName = Path.GetFileName(filePath);
                string extension = Path.GetExtension(filePath).ToLower();

                // Вычисляем MD5 (по заданию руководителя, для всех файлов)
                string md5Hash = _hashService.CalculateMD5(filePath);

                // Проверяем, нет ли уже такой прошивки в БД
                if (_xmlDatabase.IsChecksumExists(_currentDatabasePath, md5Hash))
                {
                    MessageBox.Show(
                        $"Прошивка с такой контрольной суммой уже есть в базе данных.\n\nФайл: {fileName}\nMD5: {md5Hash}",
                        "Дубликат",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Добавляем в БД
                var firmware = new Firmware
                {
                    FileName = fileName,
                    CheckSum = md5Hash,
                    RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                _xmlDatabase.AddFirmware(_currentDatabasePath, firmware);

                // Обновляем список
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
            // Проверяем, выбрана ли БД
            if (string.IsNullOrEmpty(_currentDatabasePath))
            {
                MessageBox.Show(
                    "Сначала выберите базу данных.",
                    "Нет БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Проверяем, выбран ли элемент в списке
            if (listViewFirmwares.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Выберите файл для удаления из списка.",
                    "Ничего не выбрано",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Получаем выбранный элемент
            var selectedItem = listViewFirmwares.SelectedItems[0];
            string fileName = selectedItem.Text;

            // Проверяем, не заглушка ли это
            if (fileName == "(база данных пуста)")
            {
                MessageBox.Show(
                    "В базе данных нет файлов для удаления.",
                    "Пустая БД",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Спрашиваем подтверждение
            var confirmResult = MessageBox.Show(
                $"Вы уверены, что хотите удалить файл '{fileName}' из базы данных?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
                return;

            try
            {
                // Удаляем из БД
                _xmlDatabase.DeleteFirmware(_currentDatabasePath, fileName);

                // Обновляем список
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

        private void groupBoxDatabase_Enter(object sender, EventArgs e)
        {

        }
    }
}