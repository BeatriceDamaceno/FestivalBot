using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class DiceCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("d", "Roll dice using notation such as 2d20 or d20.")]
        public Task RollDieAsync(
            InteractionContext context,
            [Option("roll", "Dice notation, such as 2d20 or d20.")] string notation)
        {
            if (!TryParseDiceNotation(notation, out int multiplier, out int sides))
            {
                return SlashCommandResponder.RespondAsync(
                    context,
                    "Use dice notation like `/d roll:2d20` or `/d roll:d20`. " +
                    "The multiplier must be 1–100 and the die must have at least 2 sides.");
            }

            var rolls = new List<long>(multiplier);
            for (int index = 0; index < multiplier; index++)
                rolls.Add(Random.Shared.NextInt64(1, (long)sides + 1));

            long total = rolls.Sum();
            string normalizedNotation = $"{multiplier}d{sides}";
            string rollList = string.Join(", ", rolls);
            return SlashCommandResponder.RespondAsync(context,
                $"{context.User.Username} rolled **{normalizedNotation}**: " +
                $"{rollList}\nTotal: **{total}**");
        }

        private static bool TryParseDiceNotation(
            string notation, out int multiplier, out int sides)
        {
            multiplier = 0;
            sides = 0;
            if (string.IsNullOrWhiteSpace(notation))
                return false;

            string value = notation.Trim().ToLowerInvariant().Replace(" ", "");
            int separator = value.IndexOf('d');
            if (separator < 0 || separator != value.LastIndexOf('d'))
                return false;

            string multiplierText = value.Substring(0, separator);
            string sidesText = value.Substring(separator + 1);
            multiplier = string.IsNullOrEmpty(multiplierText)
                ? 1
                : (int.TryParse(multiplierText, out int parsedMultiplier)
                    ? parsedMultiplier
                    : 0);

            return multiplier >= 1
                && multiplier <= 100
                && int.TryParse(sidesText, out sides)
                && sides >= 2;
        }

        [SlashCommand("risk", "Roll two dice and subtract the first result from the second.")]
        public Task RiskAsync(
            InteractionContext context,
            [Option("number", "Maximum value for both dice (at least 1).")] long number)
        {
            if (number < 1 || number >= long.MaxValue)
            {
                return SlashCommandResponder.RespondAsync(
                    context, "The number must be at least **1**.");
            }

            long firstRoll = Random.Shared.NextInt64(1, number + 1);
            long secondRoll = Random.Shared.NextInt64(1, number + 1);
            long result = firstRoll - secondRoll;

            return SlashCommandResponder.RespondAsync(context,
                $"{context.User.Username} used **/risk {number}**:\n" +
                $"First die: **{firstRoll}**\n" +
                $"Second die: **{secondRoll}**\n" +
                $"Result ({firstRoll} - {secondRoll}): **{result}**");
        }

        [SlashCommand("dice-help", "Show the English dice game rules.")]
        public Task DiceHelpAsync(InteractionContext context) =>
            SlashCommandResponder.RespondAsync(context, BuildEnglishHelp());

        [SlashCommand("dados-ajuda", "Mostra as regras do jogo de dados.")]
        public Task DadosAjudaAsync(InteractionContext context) =>
            SlashCommandResponder.RespondAsync(context, BuildPortugueseHelp());

        [SlashCommand("dice", "Bet FrostCoins in the alternating dice game.")]
        public Task DiceAsync(
            InteractionContext context,
            [Option("bet", "FrostCoins to bet (greater than 1).")] long bet) =>
            HandleGameAsync(context, bet, false);

        [SlashCommand("dados", "Aposte FrostCoins no jogo de dados.")]
        public Task DadosAsync(
            InteractionContext context,
            [Option("aposta", "FrostCoins para apostar (maior que 1).")] long bet) =>
            HandleGameAsync(context, bet, true);

        private async Task HandleGameAsync(
            InteractionContext context, long betAmount, bool portuguese)
        {
            if (betAmount <= 1)
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? "Hee, a aposta deve ser um número maior que 1. Use `/dados-ajuda` para ver as regras."
                    : "Hee, the bet must be greater than 1. Use `/dice-help` for the rules.");
                return;
            }

            if (!Context.ActiveDiceGames.TryAdd(context.User.Id, 0))
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? "Hoo, você já tem um jogo de dados em andamento!"
                    : "Hoo, you already have a dice game running!");
                return;
            }

            bool responded = false;
            try
            {
                long frostCoins;
                using (var connection =
                       new SqliteConnection($"Data Source={Context.DatabasePath}"))
                {
                    await connection.OpenAsync();
                    var findUser = connection.CreateCommand();
                    findUser.CommandText =
                        "SELECT COALESCE(FrostCoins, 0) FROM Users WHERE UserID = $userId LIMIT 1;";
                    findUser.Parameters.AddWithValue("$userId", (long)context.User.Id);
                    object result = await findUser.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                    {
                        await SlashCommandResponder.RespondAsync(context, portuguese
                            ? "Hoo, você precisa usar /register antes de jogar /dados."
                            : "Hoo, you need to /register before playing /dice.");
                        return;
                    }

                    frostCoins = Convert.ToInt64(result);
                    if (frostCoins < betAmount)
                    {
                        await SlashCommandResponder.RespondAsync(context, portuguese
                            ? $"Hoo, você não tem FrostCoins suficientes! Você tem **{frostCoins}**, mas tentou apostar **{betAmount}**."
                            : $"Hoo, you don't have enough FrostCoins! You have **{frostCoins}**, but tried to bet **{betAmount}**.");
                        return;
                    }
                }

                string userName = context.User.Username;
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Começando uma rolagem de dados com {userName} no valor de {betAmount}"
                    : $"Starting a dice roll with {userName} with the amount of {betAmount}");
                responded = true;

                int currentMax =
                    betAmount > int.MaxValue ? int.MaxValue : (int)betAmount;
                bool frostTurn = true;
                bool userWon;

                while (true)
                {
                    await Task.Delay(3000);
                    int roll = (int)Random.Shared.NextInt64(1, (long)currentMax + 1);
                    await SlashCommandResponder.FollowUpAsync(context,
                        frostTurn
                            ? (portuguese
                                ? $"FrostBot rola {roll}"
                                : $"FrostBot rolls {roll}")
                            : (portuguese
                                ? $"{userName} rola {roll}"
                                : $"{userName} rolls {roll}"));

                    if (roll == 1)
                    {
                        userWon = frostTurn;
                        break;
                    }

                    currentMax = roll;
                    frostTurn = !frostTurn;
                }

                await Task.Delay(3000);
                using var updateConnection =
                    new SqliteConnection($"Data Source={Context.DatabasePath}");
                await updateConnection.OpenAsync();
                var update = updateConnection.CreateCommand();
                update.CommandText = userWon
                    ? "UPDATE Users SET FrostCoins = COALESCE(FrostCoins, 0) + $amount WHERE UserID = $userId;"
                    : "UPDATE Users SET FrostCoins = COALESCE(FrostCoins, 0) - $amount WHERE UserID = $userId;";
                update.Parameters.AddWithValue(
                    "$amount", userWon ? betAmount * 2 : betAmount);
                update.Parameters.AddWithValue("$userId", (long)context.User.Id);
                await update.ExecuteNonQueryAsync();

                await SlashCommandResponder.FollowUpAsync(context,
                    userWon
                        ? (portuguese ? $"{userName} ganhou!" : $"{userName} won!")
                        : (portuguese ? "FrostBot ganhou!" : "FrostBot won!"));
            }
            catch (Exception ex)
            {
                string message = portuguese
                    ? $"Erro no banco de dados: {ex.Message}"
                    : $"Database error: {ex.Message}";
                if (responded)
                    await SlashCommandResponder.FollowUpAsync(context, message);
                else
                    await SlashCommandResponder.RespondAsync(context, message);
            }
            finally
            {
                Context.ActiveDiceGames.TryRemove(context.User.Id, out _);
            }
        }

        private static string BuildEnglishHelp() =>
            "**/dice rules**\n" +
            "• Use `/register` first and have enough FrostCoins.\n" +
            "• The bet must be greater than 1.\n" +
            "• FrostBot and you alternate rolls; whoever rolls 1 loses.\n" +
            "• If FrostBot loses, you gain double the bet. If you lose, the bet is removed.";

        private static string BuildPortugueseHelp() =>
            "**Regras do /dados**\n" +
            "• Use `/register` primeiro e tenha FrostCoins suficientes.\n" +
            "• A aposta deve ser maior que 1.\n" +
            "• FrostBot e você alternam rolagens; quem tirar 1 perde.\n" +
            "• Se FrostBot perder, você ganha o dobro. Se você perder, a aposta é removida.";
    }
}
