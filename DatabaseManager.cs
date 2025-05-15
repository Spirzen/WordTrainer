using System;
using System.Data.SQLite;

namespace WordTrainer
{
    /// <summary>
    /// Менеджер по работе с базой данных
    /// </summary>
    public class DatabaseManager
    {
        /// <summary>
        /// Соединение с базой данных
        /// </summary>
        private SQLiteConnection _connection;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="dbPath"></param>
        public DatabaseManager(string dbPath)
        {
            _connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            _connection.Open();
            InitializeDatabase();
        }
        /// <summary>
        /// Инициализация базы данных
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void InitializeDatabase()
        {
            try
            {
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Words (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        eng TEXT NOT NULL,
                        rus TEXT NOT NULL,
                        difficulty INTEGER NOT NULL
                    );";
                using (var command = new SQLiteCommand(createTableQuery, _connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка инициализации базы данных", ex);
            }
        }
        /// <summary>
        /// Получение случайного слова
        /// </summary>
        /// <param name="difficulty">Уровень сложности</param>
        /// <param name="isEngRusMode">Режим работы - английский или русский</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (string word, string translation) GetRandomWord(int difficulty, bool isEngRusMode)
        {
            string query;
            if (isEngRusMode)
            {
                query = $"SELECT eng, rus FROM Words WHERE difficulty = {difficulty} ORDER BY RANDOM() LIMIT 1;";
            }
            else
            {
                query = $"SELECT rus, eng FROM Words WHERE difficulty = {difficulty} ORDER BY RANDOM() LIMIT 1;";
            }

            using (var command = new SQLiteCommand(query, _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return (reader[0].ToString(), reader[1].ToString());
                    }
                }
            }

            throw new Exception(Constants.NoWordsMessage);
        }
        /// <summary>
        /// Закрытие соединения с базой данных
        /// </summary>
        public void CloseConnection()
        {
            _connection?.Close();
        }
    }
}