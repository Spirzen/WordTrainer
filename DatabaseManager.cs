using System;
using System.Data.SQLite;

namespace WordTrainer
{
    public class DatabaseManager
    {
        private readonly SQLiteConnection _connection;

        public DatabaseManager(string dbPath)
        {
            _connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            _connection.Open();
            InitializeDatabase();
            SeedIfEmpty();
        }

        private void InitializeDatabase()
        {
            try
            {
                const string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Words (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        eng TEXT NOT NULL,
                        rus TEXT NOT NULL,
                        difficulty INTEGER NOT NULL
                    );";
                using var command = new SQLiteCommand(createTableQuery, _connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка инициализации базы данных", ex);
            }
        }

        private void SeedIfEmpty()
        {
            using var countCmd = new SQLiteCommand("SELECT COUNT(*) FROM Words", _connection);
            if (Convert.ToInt32(countCmd.ExecuteScalar()) > 0)
                return;

            (string eng, string rus, int difficulty)[] starterWords =
            {
                ("hello", "привет", 1),
                ("book", "книга", 1),
                ("water", "вода", 1),
                ("friend", "друг", 1),
                ("house", "дом", 1),
                ("school", "школа", 1),
                ("beautiful", "красивый", 2),
                ("important", "важный", 2),
                ("remember", "помнить", 2),
                ("decision", "решение", 2),
                ("experience", "опыт", 2),
                ("environment", "окружающая среда", 2),
                ("accomplish", "достигать", 3),
                ("inevitable", "неизбежный", 3),
                ("ambiguous", "двусмысленный", 3),
                ("persuade", "убеждать", 3),
                ("reluctant", "неохотный", 3),
                ("sophisticated", "утончённый; изощрённый", 3),
            };

            const string insertQuery = "INSERT INTO Words (eng, rus, difficulty) VALUES (@eng, @rus, @difficulty);";
            foreach (var (eng, rus, difficulty) in starterWords)
            {
                using var insert = new SQLiteCommand(insertQuery, _connection);
                insert.Parameters.AddWithValue("@eng", eng);
                insert.Parameters.AddWithValue("@rus", rus);
                insert.Parameters.AddWithValue("@difficulty", difficulty);
                insert.ExecuteNonQuery();
            }
        }

        public int GetWordCount(int difficulty)
        {
            using var command = new SQLiteCommand(
                "SELECT COUNT(*) FROM Words WHERE difficulty = @difficulty;",
                _connection);
            command.Parameters.AddWithValue("@difficulty", difficulty);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public (string word, string translation) GetRandomWord(int difficulty, bool isEngRusMode)
        {
            string query = isEngRusMode
                ? "SELECT eng, rus FROM Words WHERE difficulty = @difficulty ORDER BY RANDOM() LIMIT 1;"
                : "SELECT rus, eng FROM Words WHERE difficulty = @difficulty ORDER BY RANDOM() LIMIT 1;";

            using var command = new SQLiteCommand(query, _connection);
            command.Parameters.AddWithValue("@difficulty", difficulty);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return (reader.GetString(0), reader.GetString(1));
            }

            throw new Exception(Constants.NoWordsMessage);
        }

        public void CloseConnection()
        {
            _connection.Close();
        }
    }
}
