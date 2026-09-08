using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class FactionCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("createfaction", "Create a faction (premium users only).")]
        public async Task CreateAsync(
            InteractionContext context,
            [Option("name", "Faction name.")] string name,
            [Option("description", "Optional faction description.")] string description = null,
            [Option("banner", "Optional banner image URL.")] string banner = null)
        {
            bool isPremium =
                Context.PremiumUsers.Any(context.User.Username.Contains);
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
                    "INSERT INTO Factions (Name, CreationDate, Banner, Description, boss) " +
                    "VALUES ($name, $creationDate, $banner, $description, $boss);";
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue(
                    "$creationDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("$banner", (object)banner ?? DBNull.Value);
                command.Parameters.AddWithValue(
                    "$description", (object)description ?? DBNull.Value);
                command.Parameters.AddWithValue("$boss", context.User.Username);
                await command.ExecuteNonQueryAsync();

                await SlashCommandResponder.RespondAsync(context,
                    $"Hee-ho! Faction **{name}** has been created! Boss: **{context.User.Username}**");
            }
            catch (Exception ex)
            {
                await SlashCommandResponder.RespondAsync(
                    context, $"Database error: {ex.Message}");
            }
        }

        [SlashCommand("invitetofaction", "Invite a registered Discord user to your faction.")]
        public async Task InviteAsync(
            InteractionContext context,
            [Option("user", "Discord user to invite.")] DiscordUser invitedUser)
        {
            try
            {
                using var connection =
                    new SqliteConnection($"Data Source={Context.DatabasePath}");
                await connection.OpenAsync();

                var findFaction = connection.CreateCommand();
                findFaction.CommandText =
                    "SELECT ID, Name FROM Factions WHERE boss = $boss ORDER BY ID LIMIT 1;";
                findFaction.Parameters.AddWithValue("$boss", context.User.Username);

                long factionId;
                string factionName;
                using (var reader = await findFaction.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        await SlashCommandResponder.RespondAsync(
                            context, "Hee, looks like you dont own a faction");
                        return;
                    }

                    factionId = reader.GetInt64(0);
                    factionName =
                        reader.IsDBNull(1) ? "Unknown" : reader.GetString(1);
                }

                var findUser = connection.CreateCommand();
                findUser.CommandText =
                    "SELECT UserName FROM Users WHERE UserID = $userId LIMIT 1;";
                findUser.Parameters.AddWithValue("$userId", (long)invitedUser.Id);
                object registeredUser = await findUser.ExecuteScalarAsync();
                if (registeredUser == null || registeredUser == DBNull.Value)
                {
                    await SlashCommandResponder.RespondAsync(
                        context, "Hoo, that user needs to /register first!");
                    return;
                }

                var alreadyMember = connection.CreateCommand();
                alreadyMember.CommandText =
                    "SELECT 1 FROM FactionMembers WHERE FactionID = $factionId AND UserID = $userId LIMIT 1;";
                alreadyMember.Parameters.AddWithValue("$factionId", factionId);
                alreadyMember.Parameters.AddWithValue("$userId", (long)invitedUser.Id);
                if (await alreadyMember.ExecuteScalarAsync() != null)
                {
                    await SlashCommandResponder.RespondAsync(
                        context, "Hoo, that user is already in your faction!");
                    return;
                }

                var invite = connection.CreateCommand();
                invite.CommandText =
                    "INSERT INTO FactionMembers (FactionID, UserID, JoinedDate) " +
                    "VALUES ($factionId, $userId, $joinedDate);";
                invite.Parameters.AddWithValue("$factionId", factionId);
                invite.Parameters.AddWithValue("$userId", (long)invitedUser.Id);
                invite.Parameters.AddWithValue(
                    "$joinedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                await invite.ExecuteNonQueryAsync();

                var update = connection.CreateCommand();
                update.CommandText =
                    "UPDATE Users SET Faction = $factionName, FactionID = $factionId " +
                    "WHERE UserID = $userId;";
                update.Parameters.AddWithValue("$factionName", factionName);
                update.Parameters.AddWithValue("$factionId", factionId);
                update.Parameters.AddWithValue("$userId", (long)invitedUser.Id);
                await update.ExecuteNonQueryAsync();

                await SlashCommandResponder.RespondAsync(context,
                    $"Hee-ho! **{registeredUser}** was invited to **{factionName}**!");
            }
            catch (Exception ex)
            {
                await SlashCommandResponder.RespondAsync(
                    context, $"Database error: {ex.Message}");
            }
        }

        [SlashCommand("factions", "List all FrostBot factions.")]
        public async Task ListAsync(InteractionContext context)
        {
            try
            {
                using var connection =
                    new SqliteConnection($"Data Source={Context.DatabasePath}");
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT ID, Name, CreationDate, boss FROM Factions ORDER BY ID;";
                using var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                {
                    await SlashCommandResponder.RespondAsync(
                        context, "No factions found.");
                    return;
                }

                string result = "```\nID | Name | Created | Boss\n";
                result += "---+------+---------+-----\n";
                while (await reader.ReadAsync())
                {
                    string line =
                        $"{reader.GetValue(0)} | {reader.GetValue(1)} | " +
                        $"{reader.GetValue(2)} | {reader.GetValue(3)}\n";
                    if (result.Length + line.Length + 3 > 1950)
                        break;
                    result += line;
                }

                await SlashCommandResponder.RespondAsync(context, result + "```");
            }
            catch (Exception ex)
            {
                await SlashCommandResponder.RespondAsync(
                    context, $"Database error: {ex.Message}");
            }
        }

        [SlashCommand("showfaction", "Show details for a faction.")]
        public async Task ShowAsync(
            InteractionContext context,
            [Option("id", "Faction ID.")] long factionId)
        {
            try
            {
                using var connection =
                    new SqliteConnection($"Data Source={Context.DatabasePath}");
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT Name, Banner, Description, boss, CreationDate " +
                    "FROM Factions WHERE ID = $id;";
                command.Parameters.AddWithValue("$id", factionId);

                string name;
                string banner;
                string description;
                string boss;
                string creationDate;
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        await SlashCommandResponder.RespondAsync(context,
                            $"Hoo... No faction found with ID {factionId}.");
                        return;
                    }

                    name = reader.IsDBNull(0) ? "Unknown" : reader.GetString(0);
                    banner = reader.IsDBNull(1) ? null : reader.GetString(1);
                    description = reader.IsDBNull(2) ? null : reader.GetString(2);
                    boss = reader.IsDBNull(3) ? "Unknown" : reader.GetString(3);
                    creationDate =
                        reader.IsDBNull(4) ? "Unknown" : reader.GetString(4);
                }

                var membersCommand = connection.CreateCommand();
                membersCommand.CommandText =
                    "SELECT u.UserName FROM FactionMembers fm " +
                    "INNER JOIN Users u ON u.UserID = fm.UserID " +
                    "WHERE fm.FactionID = $id ORDER BY u.UserName;";
                membersCommand.Parameters.AddWithValue("$id", factionId);
                var members = new List<string>();
                using (var reader = await membersCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0))
                            members.Add(reader.GetString(0));
                    }
                }

                string membersText = members.Count == 0
                    ? "(none)"
                    : string.Join("\n", members.Select(member => $"- {member}"));
                string embedDescription =
                    (string.IsNullOrWhiteSpace(description)
                        ? ""
                        : description + "\n\n") +
                    $"**Members:**\n{membersText}";
                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Here's the faction {name}:")
                    .WithDescription(embedDescription)
                    .WithFooter($"Created by: {boss} {creationDate}");
                if (!string.IsNullOrWhiteSpace(banner))
                    embed.WithImageUrl(banner);

                await SlashCommandResponder.RespondAsync(context, embed);
            }
            catch (Exception ex)
            {
                await SlashCommandResponder.RespondAsync(
                    context, $"Database error: {ex.Message}");
            }
        }
    }
}
