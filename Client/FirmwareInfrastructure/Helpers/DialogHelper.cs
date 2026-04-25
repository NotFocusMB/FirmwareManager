using System;
using System.Windows.Forms;
using System.Drawing;

namespace FrimwareDatabase.Infrastructure.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с диалогами открытия/сохранения файлов.
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Показывает диалог открытия файла и возвращает выбранный путь.
        /// </summary>
        /// <param name="title">Заголовок диалога.</param>
        /// <param name="filter">Фильтр файлов.</param>
        /// <returns>Путь к выбранному файлу или null, если отменено.</returns>
        public static string ShowOpenFileDialog(string title, string filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filter;
            openFileDialog.Title = title;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// Показывает диалог сохранения файла и возвращает выбранный путь.
        /// </summary>
        /// <param name="title">Заголовок диалога.</param>
        /// <param name="filter">Фильтр файлов.</param>
        /// <param name="defaultExt">Расширение по умолчанию.</param>
        /// <returns>Путь для сохранения файла или null, если отменено.</returns>
        public static string ShowSaveFileDialog(string title, string filter, string defaultExt)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = filter;
            saveFileDialog.DefaultExt = defaultExt;
            saveFileDialog.AddExtension = true;
            saveFileDialog.Title = title;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// Показывает простой диалог выбора из списка.
        /// </summary>
        /// <param name="prompt">Заголовок.</param>
        /// <param name="options">Варианты выбора.</param>
        /// <returns>Индекс выбранного элемента или -1, если отмена.</returns>
        public static int ShowChoiceDialog(string prompt, string[] options)
        {
            using (var form = new Form())
            using (var listBox = new ListBox())
            using (var buttonOk = new Button())
            using (var buttonCancel = new Button())
            {
                form.Text = prompt;
                form.Size = new Size(400, 300);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                listBox.Dock = DockStyle.Top;
                listBox.Height = 200;
                listBox.Items.AddRange(options);

                buttonOk.Text = "OK";
                buttonOk.DialogResult = DialogResult.OK;
                buttonOk.Location = new Point(200, 220);

                buttonCancel.Text = "Отмена";
                buttonCancel.DialogResult = DialogResult.Cancel;
                buttonCancel.Location = new Point(280, 220);

                form.Controls.Add(listBox);
                form.Controls.Add(buttonOk);
                form.Controls.Add(buttonCancel);

                if (form.ShowDialog() == DialogResult.OK && listBox.SelectedIndex >= 0)
                {
                    return listBox.SelectedIndex;
                }
                return -1;
            }
        }
    }
}