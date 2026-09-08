using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace FestivalBot.Services
{
    public sealed class StaminaRefillService
    {
        private readonly string _databasePath;

        public StaminaRefillService(string databasePath)
        {
            _databasePath = databasePath;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                try
                {
                    DateTime now = DateTime.Now;
                    TimeSpan delay = now.Date.AddDays(1) - now;
                    if (delay < TimeSpan.FromSeconds(1))
                        delay = TimeSpan.FromSeconds(1);

                    await Task.Delay(delay);

                    using var connection =
                        new SqliteConnection($"Data Source={_databasePath}");
                    await connection.OpenAsync();
                    var refill = connection.CreateCommand();
                    refill.CommandText = "UPDATE Users SET Stamina = 5;";
                    int updated = await refill.ExecuteNonQueryAsync();
                    Console.WriteLine(
                        $"[{DateTime.Now:O}] Stamina refilled to 5 for {updated} user(s).");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Stamina refill error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }
    }
}
