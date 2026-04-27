using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirmwareManager
{
    /// <summary>
    /// Форма для отображения прогресса выполнения длительных операций 
    /// (загрузка файлов, запись на USB и т.д.)
    /// </summary>
    public partial class ProgressForm : Form
    {
        /// <summary>
        /// Событие, возникающее при нажатии пользователем кнопки "Отмена"
        /// </summary>
        public event EventHandler CancelRequested;
        private bool isIndeterminate;

        public ProgressForm()
        {

        }

        /// <summary>
        /// Конструктор формы прогресса с указанием заголовка окна
        /// </summary>
        /// <param name="title">Заголовок окна формы</param>
        public ProgressForm(string title)
        {
            this.Text = title;
            InitializeComponent();
        }

        /// <summary>
        /// Устанавливает текст состояния операции (например, "Загрузка файла...")
        /// </summary>
        /// <param name="text">Текст состояния для отображения</param>
        public void SetProgressText(string text)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => label.Text = text));
            }
            else
            {
                label.Text = text;
            }
        }

        /// <summary>
        /// Обновляет прогресс-бар и отображает текущее значение с единицей измерения
        /// </summary>
        /// <param name="value">Текущее значение прогресса (например, количество переданных мегабайт)</param>
        /// <param name="unit">Единица измерения (например, "MB" для мегабайт)</param>
        public void UpdateProgress(double value, string unit)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateProgress(value, unit)));
                return;
            }

            if (!isIndeterminate && progressBar.Style != ProgressBarStyle.Blocks)
            {
                progressBar.Style = ProgressBarStyle.Blocks;
            }
        }

        /// <summary>
        /// Обновляет текстовый статус операции с дополнительной информацией
        /// </summary>
        /// <param name="status">Текст статуса для отображения</param>
        public void UpdateStatus(string status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateStatus(status)));
                return;
            }
        }

        /// <summary>
        /// Скрывает кнопку отмены операции
        /// </summary>
        public void HideCancelButton()
        {
            cancelButton.Visible = false;
        }

        /// <summary>
        /// Обработчик нажатия кнопки отмены
        /// Вызывает событие CancelRequested для уведомления вызывающего кода о необходимости прервать операцию
        /// </summary>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {

        }
    }
}
