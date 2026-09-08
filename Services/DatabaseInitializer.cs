using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace FestivalBot.Services
{
    public static class DatabaseInitializer
    {
        public static void EnsureWorkColumns(string databasePath)
        {
            using var connection =
                new SqliteConnection($"Data Source={databasePath}");
            connection.Open();

            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info(Users);";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    columns.Add(Convert.ToString(reader.GetValue(1)));
            }

            if (!columns.Contains("Stamina"))
            {
                using var alter = connection.CreateCommand();
                alter.CommandText =
                    "ALTER TABLE Users ADD COLUMN Stamina INTEGER NOT NULL DEFAULT 5;";
                alter.ExecuteNonQuery();
            }

            if (!columns.Contains("LastWorked"))
            {
                using var alter = connection.CreateCommand();
                alter.CommandText =
                    "ALTER TABLE Users ADD COLUMN LastWorked TEXT NULL;";
                alter.ExecuteNonQuery();
            }
        }
    }
}
