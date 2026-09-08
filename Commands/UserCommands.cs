using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class UserCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("users", "List registered FrostBot users (staff).")]
        public async Task UsersAsync(InteractionContext context)
        {
            bool isPremium = Context.PremiumUsers.Any(Context.PremiumUsers.Contains);
            if (!isPremium)
            {
                await SlashCommandResponder.RespondAsync(context,
                    "This is a FROSTBOT PLATINUMN (tm) Answer, hee! Staff Only!");
                return;
            }

            try
            {
                using var connection =
                    new SqliteConnection($"Data Source={Context.DatabasePath}");
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT UserName, HP, Persona, KillCount, DeathCount, Faction FROM Users;";
                using var reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    await SlashCommandResponder.RespondAsync(context, "No users found.");
                    return;
                }

                string result = "";
                while (await reader.ReadAsync())
                {
                    result +=
                        $"Name: {reader.GetString(0)} | HP: {reader.GetString(1)} " +
                        $"{(reader.GetString(2).Length > 0 ? $"| Persona: {reader.GetString(2)}" : "")}  " +
                        $"| KillCount: {reader.GetInt32(3)} | DeathCount: {reader.GetInt32(4)} " +
                        $"{(reader.GetString(5).Length > 0 ? $"| Faction : {reader.GetString(5)}" : "")} " +
                        $"{Environment.NewLine}";
                }

                await SlashCommandResponder.RespondAsync(
                    context, result.Length <= 1900 ? result : result.Substring(0, 1900));
            }
            catch (Exception ex)
            {
                await SlashCommandResponder.RespondAsync(
                    context, $"Database error: {ex.Message}");
            }
        }

        [SlashCommand("register", "Register your Discord user with FrostBot.")]
        public async Task RegisterAsync(InteractionContext context)
        {
            try
            {
                using var connection =
                    new SqliteConnection($"Data Source={Context.DatabasePath}");
                await connection.OpenAsync();
                var exists = connection.CreateCommand();
                exists.CommandText = "SELECT 1 FROM Users WHERE UserID = $userId LIMIT 1;";
                exists.Parameters.AddWithValue("$userId", (long)context.User.Id);
                object registered = await exists.ExecuteScalarAsync();

                if (registered != null && registered != DBNull.Value)
                {
                    await SlashCommandResponder.RespondAsync(context,
                        $"Hoo! Nice to hee-see you, {context.User.Username}! You are already registered.");
                    return;
                }

                var command = connection.CreateCommand();
                command.CommandText =
                    "INSERT INTO Users (UserID, UserName, KillCount, DeathCount, Faction, Persona, HP, FactionID, FrostCoins, Stamina, LastWorked) " +
                    "VALUES ($userId, $userName, 0, 0, '', '', 100, NULL, 100, 5, NULL);";
                command.Parameters.AddWithValue("$userId", (long)context.User.Id);
                command.Parameters.AddWithValue("$userName", context.User.Username);
                await command.ExecuteNonQueryAsync();

                await SlashCommandResponder.RespondAsync(context,
                    $"Hee-ho! {context.User.Username} has been registered with **100 FrostCoins**!");
            }
            catch (Exception ex)
            {
                await SlashCommandResponder.RespondAsync(
                    context, $"Database error: {ex.Message}");
            }
        }

        [SlashCommand("wallet", "Show your FrostCoins.")]
        public Task WalletAsync(InteractionContext context) =>
            ShowWalletAsync(context, false);

        [SlashCommand("carteira", "Mostra suas FrostCoins.")]
        public Task CarteiraAsync(InteractionContext context) =>
            ShowWalletAsync(context, true);

        private async Task ShowWalletAsync(InteractionContext context, bool portuguese)
        {
            try
            {
                using var connection =
                    new SqliteConnection($"Data Source={Context.DatabasePath}");
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT COALESCE(FrostCoins, 0) FROM Users WHERE UserID = $userId LIMIT 1;";
                command.Parameters.AddWithValue("$userId", (long)context.User.Id);
                object result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    await SlashCommandResponder.RespondAsync(context, portuguese
                        ? "Hoo, você precisa usar /register antes de ver sua carteira."
                        : "Hoo, you need to /register before checking your wallet.");
                    return;
                }

                long frostCoins = Convert.ToInt64(result);
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Hee! {context.User.Username}, você tem **{frostCoins}** FrostCoins."
                    : $"Hee! {context.User.Username}, you have **{frostCoins}** FrostCoins.");
            }
            catch (Exception ex)
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Erro no banco de dados: {ex.Message}"
                    : $"Database error: {ex.Message}");
            }
        }
    }
}
