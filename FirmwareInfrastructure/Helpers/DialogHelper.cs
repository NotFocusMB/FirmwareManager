using System.Windows.Forms;

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
    }
}