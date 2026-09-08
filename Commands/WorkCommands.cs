using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using FestivalBot.Work;
using Microsoft.Data.Sqlite;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class WorkCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("work-help", "Explain how FrostBot jobs work.")]
        public Task WorkHelpAsync(InteractionContext context) =>
            SlashCommandResponder.RespondAsync(context, BuildHelp(false));

        [SlashCommand("trabalhar-ajuda", "Explica como os trabalhos funcionam.")]
        public Task TrabalharAjudaAsync(InteractionContext context) =>
            SlashCommandResponder.RespondAsync(context, BuildHelp(true));

        [SlashCommand("work-options", "List all available workplaces.")]
        public Task WorkOptionsAsync(InteractionContext context) =>
            ShowOptionsAsync(context, false);

        [SlashCommand("trabalhar-opcao", "Lista todos os locais de trabalho.")]
        public Task TrabalharOpcaoAsync(InteractionContext context) =>
            ShowOptionsAsync(context, true);

        [SlashCommand("work", "Work at a workplace that is open now.")]
        public Task WorkAsync(InteractionContext context) =>
            HandleWorkAsync(context, false);

        [SlashCommand("trabalhar", "Trabalha em um local aberto agora.")]
        public Task TrabalharAsync(InteractionContext context) =>
            HandleWorkAsync(context, true);

        private static Task ShowOptionsAsync(
            InteractionContext context, bool portuguese)
        {
            string header = portuguese
                ? "**Locais de trabalho disponíveis**\n"
                : "**Available workplaces**\n";
            string table = "```\n" + Workplaces.AsTable() + "\n```";
            string note = portuguese
                ? "\nSó entram no sorteio de `/trabalhar` os locais abertos no **dia** e **horário** atuais."
                : "\nOnly workplaces open for the current **day** and **time** can be rolled by `/work`.";
            return SlashCommandResponder.RespondAsync(
                context, header + table + note);
        }

        private async Task HandleWorkAsync(
            InteractionContext context, bool portuguese)
        {
            try
            {
                using var connection =
                    new SqliteConnection($"Data Source={Context.DatabasePath}");
                await connection.OpenAsync();

                int stamina;
                string lastWorkedRaw;
                long frostCoins;
                var findUser = connection.CreateCommand();
                findUser.CommandText =
                    "SELECT COALESCE(Stamina, 0), LastWorked, COALESCE(FrostCoins, 0) " +
                    "FROM Users WHERE UserID = $userId LIMIT 1;";
                findUser.Parameters.AddWithValue("$userId", (long)context.User.Id);

                using (var reader = await findUser.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        await SlashCommandResponder.RespondAsync(context, portuguese
                            ? "Hoo, você precisa usar /register antes de trabalhar."
                            : "Hoo, you need to /register before working.");
                        return;
                    }

                    stamina = Convert.ToInt32(reader.GetValue(0));
                    lastWorkedRaw =
                        reader.IsDBNull(1) ? null : Convert.ToString(reader.GetValue(1));
                    frostCoins = Convert.ToInt64(reader.GetValue(2));
                }

                if (stamina <= 0)
                {
                    await SlashCommandResponder.RespondAsync(context, portuguese
                        ? "Hoo... Sua stamina está em **0**. Ela recarrega à meia-noite (00:00)."
                        : "Hoo... Your stamina is **0**. It refills at midnight (00:00).");
                    return;
                }

                DateTime now = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(lastWorkedRaw)
                    && DateTime.TryParse(lastWorkedRaw, null,
                        DateTimeStyles.RoundtripKind, out DateTime lastWorked))
                {
                    TimeSpan since = now - lastWorked.ToLocalTime();
                    if (since < TimeSpan.FromHours(2))
                    {
                        TimeSpan remaining = TimeSpan.FromHours(2) - since;
                        string wait =
                            $"{(int)remaining.TotalHours}h {remaining.Minutes:D2}m";
                        await SlashCommandResponder.RespondAsync(context, portuguese
                            ? $"Hoo, você precisa esperar **{wait}** antes de trabalhar de novo."
                            : $"Hoo, you need to wait **{wait}** before working again.");
                        return;
                    }
                }

                var available = Workplaces.AvailableNow(now);
                if (available.Count == 0)
                {
                    await SlashCommandResponder.RespondAsync(context, portuguese
                        ? "Hoo... Não há trabalhos abertos neste horário/dia. Tente mais tarde!"
                        : "Hoo... No workplaces are open at this day/time. Try again later!");
                    return;
                }

                var job = available[Random.Shared.Next(available.Count)];
                int pay = job.BasePay;
                int newStamina = stamina - 1;
                long newCoins = frostCoins + pay;
                var update = connection.CreateCommand();
                update.CommandText =
                    "UPDATE Users SET Stamina = $stamina, LastWorked = $lastWorked, FrostCoins = $coins " +
                    "WHERE UserID = $userId;";
                update.Parameters.AddWithValue("$stamina", newStamina);
                update.Parameters.AddWithValue("$lastWorked", now.ToString("o"));
                update.Parameters.AddWithValue("$coins", newCoins);
                update.Parameters.AddWithValue("$userId", (long)context.User.Id);
                await update.ExecuteNonQueryAsync();

                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Hee-ho! {context.User.Username} trabalhou em **{job.Name}** ({job.Location}) e ganhou **{pay}** FrostCoins!\n" +
                      $"Stamina restante: **{newStamina}/5** | Carteira: **{newCoins}**"
                    : $"Hee-ho! {context.User.Username} worked at **{job.Name}** ({job.Location}) and earned **{pay}** FrostCoins!\n" +
                      $"Stamina left: **{newStamina}/5** | Wallet: **{newCoins}**");
            }
            catch (Exception ex)
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Erro no banco de dados: {ex.Message}"
                    : $"Database error: {ex.Message}");
            }
        }

        private static string BuildHelp(bool portuguese) =>
            portuguese
                ? "**/trabalhar — regras**\n" +
                  "• Pré-requisito: você precisa usar `/register` antes.\n" +
                  "• Cada trabalho gasta **1 stamina** e tem cooldown de **2 horas**.\n" +
                  "• A stamina recarrega para **5** à meia-noite (**00:00**).\n" +
                  "• Daytime: 06:00–17:59 | Evening: 18:00–21:59 | Night: 22:00–05:59\n" +
                  "• `/trabalhar-opcao` lista todos os locais."
                : "**/work — rules**\n" +
                  "• Prerequisite: use `/register` first.\n" +
                  "• Each job costs **1 stamina** and has a **2-hour** cooldown.\n" +
                  "• Stamina refills to **5** at midnight (**00:00**).\n" +
                  "• Daytime: 06:00–17:59 | Evening: 18:00–21:59 | Night: 22:00–05:59\n" +
                  "• `/work-options` lists every workplace.";
    }
}
