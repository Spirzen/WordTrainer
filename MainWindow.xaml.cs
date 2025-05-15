using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WordTrainer
{
    /// <summary>
    /// Основное окно
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseManager _databaseManager;
        private string _currentWord;
        private string _correctTranslation;

        public MainWindow()
        {
            InitializeComponent();

            string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "words.db");
            _databaseManager = new DatabaseManager(dbPath);

            var difficulties = new List<DifficultyLevel>
            {
                new DifficultyLevel { Name = "Низкий", Value = 1 },
                new DifficultyLevel { Name = "Средний", Value = 2 },
                new DifficultyLevel { Name = "Высокий", Value = 3 }
            };
            DifficultyComboBox.ItemsSource = difficulties;
            DifficultyComboBox.SelectedIndex = 0;
        }
        /// <summary>
        /// Обработчик кнопки "Показать перевод"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowTranslationButton_Click(object sender, RoutedEventArgs e)
        {
            TranslationTextBlock.Text = _correctTranslation;
        }
        /// <summary>
        /// Обработчик кнопки "Случайное слово"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RandomWordButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedLevel = DifficultyComboBox.SelectedItem as DifficultyLevel;
            if (selectedLevel == null)
            {
                MessageBox.Show("Выберите уровень сложности.");
                return;
            }

            int selectedDifficulty = selectedLevel.Value;
            bool isEngRusMode = ModeEngRus.IsChecked == true;

            try
            {
                (_currentWord, _correctTranslation) = _databaseManager.GetRandomWord(selectedDifficulty, isEngRusMode);

                WordTextBlock.Text = _currentWord;
                ResultTextBlock.Text = "Ожидаем";
                ResultTextBlock.Foreground = Brushes.Black;
                TranslationTextBox.Clear();
                TranslationTextBlock.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Обработчик кнопки "Проверить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            ValidateTranslation();
        }
        /// <summary>
        /// Текст перевода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TranslationTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ValidateTranslation();
            }
        }
        /// <summary>
        /// Проверка перевода
        /// </summary>
        private void ValidateTranslation()
        {
            var userTranslation = TranslationTextBox.Text.Trim();

            if (string.IsNullOrEmpty(userTranslation))
            {
                MessageBox.Show(Constants.EnterTranslationMessage);
                return;
            }

            bool isCorrect = userTranslation.Equals(_correctTranslation, StringComparison.OrdinalIgnoreCase);
            ResultTextBlock.Text = isCorrect ? Constants.CorrectAnswer : Constants.IncorrectAnswer;
            ResultTextBlock.Foreground = isCorrect ? Brushes.Green : Brushes.Red;
        }
        /// <summary>
        /// Закрытие соединения
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            _databaseManager.CloseConnection();
            base.OnClosed(e);
        }
    }
}