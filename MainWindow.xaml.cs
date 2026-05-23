using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WordTrainer
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseManager _databaseManager;
        private string? _currentWord;
        private string? _correctTranslation;
        private bool _hasActiveWord;

        private int _correctCount;
        private int _wrongCount;
        private int _streak;

        public MainWindow()
        {
            InitializeComponent();

            string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "words.db");
            _databaseManager = new DatabaseManager(dbPath);

            var difficulties = new List<DifficultyLevel>
            {
                new() { Name = "Начальный", Value = 1 },
                new() { Name = "Средний", Value = 2 },
                new() { Name = "Продвинутый", Value = 3 }
            };
            DifficultyComboBox.ItemsSource = difficulties;
            DifficultyComboBox.SelectedIndex = 0;

            UpdateWordCountLabel();
            UpdateStatsDisplay();
        }

        private void DifficultyComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateWordCountLabel();
        }

        private void UpdateWordCountLabel()
        {
            if (DifficultyComboBox.SelectedItem is not DifficultyLevel level)
            {
                WordCountTextBlock.Text = string.Empty;
                return;
            }

            int count = _databaseManager.GetWordCount(level.Value);
            WordCountTextBlock.Text = $"{count} слов";
        }

        private void RandomWordButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRandomWord();
        }

        private void LoadRandomWord()
        {
            if (DifficultyComboBox.SelectedItem is not DifficultyLevel selectedLevel)
            {
                MessageBox.Show("Выберите уровень сложности.", "WordTrainer", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isEngRusMode = ModeEngRus.IsChecked == true;

            try
            {
                (_currentWord, _correctTranslation) = _databaseManager.GetRandomWord(selectedLevel.Value, isEngRusMode);
                _hasActiveWord = true;

                WordTextBlock.Text = _currentWord;
                ResultTextBlock.Text = Constants.WaitingMessage;
                ResultTextBlock.Foreground = (Brush)FindResource("TextMutedBrush");
                TranslationTextBox.Clear();
                TranslationTextBox.IsEnabled = true;
                TranslationTextBox.Focus();
                TranslationTextBlock.Text = string.Empty;
                CheckButton.IsEnabled = true;
                HintButton.IsEnabled = true;
                ShowTranslationButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _hasActiveWord = false;
                MessageBox.Show(ex.Message, "WordTrainer", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowTranslationButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_hasActiveWord || _correctTranslation is null)
            {
                MessageBox.Show(Constants.NoActiveWordMessage, "WordTrainer", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TranslationTextBlock.Text = _correctTranslation;
            _streak = 0;
            UpdateStatsDisplay();
        }

        private void HintButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_hasActiveWord || string.IsNullOrEmpty(_correctTranslation))
            {
                MessageBox.Show(Constants.NoActiveWordMessage, "WordTrainer", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string firstVariant = _correctTranslation.Split(',')[0].Trim();
            string hint = firstVariant.Length <= 1
                ? firstVariant
                : firstVariant[0] + new string('•', firstVariant.Length - 1);

            ResultTextBlock.Text = Constants.HintPrefix + hint;
            ResultTextBlock.Foreground = (Brush)FindResource("AccentBrush");
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            ValidateTranslation();
        }

        private void TranslationTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValidateTranslation();
                e.Handled = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                LoadRandomWord();
                e.Handled = true;
            }
        }

        private void ValidateTranslation()
        {
            if (!_hasActiveWord || _correctTranslation is null)
            {
                MessageBox.Show(Constants.NoActiveWordMessage, "WordTrainer", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var userTranslation = TranslationTextBox.Text.Trim();
            if (string.IsNullOrEmpty(userTranslation))
            {
                MessageBox.Show(Constants.EnterTranslationMessage, "WordTrainer", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isCorrect = TranslationChecker.IsCorrect(userTranslation, _correctTranslation);

            if (isCorrect)
            {
                _correctCount++;
                _streak++;
                ResultTextBlock.Text = Constants.CorrectAnswer;
                ResultTextBlock.Foreground = (Brush)FindResource("SuccessBrush");
                ScheduleNextWord();
            }
            else
            {
                _wrongCount++;
                _streak = 0;
                ResultTextBlock.Text = $"{Constants.IncorrectAnswer}. Ответ: {_correctTranslation}";
                ResultTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
            }

            UpdateStatsDisplay();
        }

        private void ScheduleNextWord()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(900) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                if (_hasActiveWord)
                    LoadRandomWord();
            };
            timer.Start();
        }

        private void UpdateStatsDisplay()
        {
            CorrectCountText.Text = _correctCount.ToString();
            WrongCountText.Text = _wrongCount.ToString();
            StreakText.Text = _streak.ToString();

            int total = _correctCount + _wrongCount;
            AccuracyText.Text = total == 0
                ? "—"
                : $"{(int)Math.Round(100.0 * _correctCount / total)}%";
        }

        protected override void OnClosed(EventArgs e)
        {
            _databaseManager.CloseConnection();
            base.OnClosed(e);
        }
    }
}
