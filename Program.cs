using DSharpPlus;
using DSharpPlus.Entities;
using Emzi0767;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLitePCL;

namespace FestivalBot
{
    class Program
    {
        static class Globals
        {
            public static DiscordClient discord;
        }

        static void Main(string[] args)
        {

            SQLitePCL.Batteries.Init();
            string botToken;
            string dbPath = "C:\\Users\\sophie.mendonca\\GrimoireOTH";

            Globals.discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = null,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            MainAsync(dbPath, Globals.discord).GetAwaiter().GetResult();

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            Globals.discord.SendMessageAsync(
                Globals.discord.GetChannelAsync(929162376371118221).Result,
                "G-hoo-d bye! Frostbot is shutting down."
            );
        }

        static async Task MainAsync(string dbPath, DiscordClient discord)
        {
            string[] validENChannels = { "chatting", "memes", "battlefield", "battlefield-2", "moderator-chat", "admin-chat", "patron-lounge", "bot-test", "voice-chat", "frost-reign" };
            string[] validPTChannels = { "conversa", "perguntas" };
            String[] premiumUsers = { "aphotic.hymn", ".castellian", "sanerion", "coffeethehermit", "wyplue" };

            int retCode = 0;

            discord.MessageCreated += async (s, e) =>
            {
                retCode = 0;
                Random rd = new Random();
                string channel = e.Message.Channel.Name;



                // =========================
                // !users command 
                // =========================
                if (e.Message.Content.ToLower().StartsWith("!users"))
                {
                    bool isPremium = premiumUsers.Any(premiumUsers.Contains);

                    if (!isPremium)
                    {
                        await e.Message.RespondAsync("This is a FROSTBOT PLATINUMN (tm) Answer, hee! Staff Only!");
                    }

                    try
                    {
                        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                        {
                            await connection.OpenAsync();

                            var command = connection.CreateCommand();

                            command.CommandText = "SELECT UserName, HP, Persona, KillCount, DeathCount, Faction FROM Users;";

                            var reader = await command.ExecuteReaderAsync();

                            if (!reader.HasRows)
                            {
                                await e.Message.RespondAsync("No users found.");
                                goto Skip;
                            }

                            string result = "";

                            while (await reader.ReadAsync())
                            {
                                result += $"Name: {reader.GetString(0)} | HP: {reader.GetString(1)} {(reader.GetString(2).Length > 0 ? $"| Persona: {reader.GetString(2)}" : "")}  | KillCount: {reader.GetInt32(3)} | DeathCount: {reader.GetInt32(4)} {(reader.GetString(5).Length > 0 ? $"| Faction : {reader.GetString(5)}" : "")} {Environment.NewLine}";
                   
                                if (result.Length > 1800)
                                {
                                    await e.Message.RespondAsync(result);
                                    result = "";
                                }
                            }

                            if (!string.IsNullOrEmpty(result))
                                await e.Message.RespondAsync(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Message.RespondAsync($"Database error: {ex.Message}");
                    }

                    goto Skip;
                }

                // =========================
                // !register user command
                // =========================

                if (e.Message.Content.ToLower().StartsWith("!register"))
                {
                    bool isPremium = premiumUsers.Any(premiumUsers.Contains);

                    if (!isPremium)
                    {
                        await e.Message.RespondAsync("This is a FROSTBOT PLATINUMN (tm) Answer, hee! Staff Only!");
                    }
                    
                    Console.WriteLine("event:", e);

                    try
                    {
                        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                        {
                            await connection.OpenAsync();

                            var command = connection.CreateCommand();

                            command.CommandText = "" +
                            "INSERT INTO Users (UserID, UserName, KillCount, DeathCount, Faction, Persona, HP)" +
                            $@"VALUES ({e.Author.Id}, '{e.Author.Username}', {0}, {0}, '', '', {100})";


                            var reader = await command.ExecuteReaderAsync();


                        }
                    }
                    catch (Exception ex)
                    {
                        await e.Message.RespondAsync($"Database error: {ex.Message}");
                    }


                }

                if ((!validENChannels.Any(channel.Contains) && !validPTChannels.Any(channel.Contains)) || e.Message.Author.IsBot)
                {
                    goto Skip;
                }

                // =========================
                // "hee" response
                // =========================
                if (e.Message.Content.ToLower().Contains("hee"))
                {
                    retCode = 3;
                    int haw = rd.Next(1, 20);

                    if (haw == 19)
                        await e.Message.RespondAsync("HEE-HAW!!");
                    else
                        await e.Message.RespondAsync(FindHeeWord(e.Message.Content));
                }

                // =========================
                // askfrost
                // =========================
                if (e.Message.Content.ToLower().StartsWith("!askfrost"))
                {
                    if (e.Message.Content.ToLower() == "!askfrost")
                    {
                        await e.Message.RespondAsync("You gotta ask something, dummy!");
                    }
                    else
                    {
                        int ans = rd.Next(1, 23);

                        bool isPremium = premiumUsers.Any(premiumUsers.Contains);

                        string response = FrostResponsesEN.GetResponseEN(ans, isPremium, e.Message.Author.Mention);
                        await e.Message.RespondAsync(response);
                    }
                }

                if (e.Message.Content.ToLower().StartsWith("!pergunta"))
                {
                    retCode = 02;
                    if (e.Message.Content.ToLower().Equals("!pergunta"))
                    {
                        await e.Message.RespondAsync("Você precisa perguntar algo seu bobo!");
                    }
                    else
                    {
                        int ans = rd.Next(1, 22);

                        bool isPremium = premiumUsers.Any(premiumUsers.Contains);

                        string response = FrostResponsesPT.GetResponsePT(ans, isPremium, e.Message.Author.Mention);

                        await e.Message.RespondAsync(response);
                    }
    ;
                }

                // =========================
                // help
                // =========================
                if (e.Message.Content.ToLower().StartsWith("!help"))
                {
                    await e.Message.RespondAsync("I'm sorry, but this is a work in progress.");
                }

            Skip:
                Console.WriteLine("Command processed with retCode " + retCode);
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static string FindHeeWord(string fullMessage)
        {
            int heeBefore = fullMessage.ToLower().IndexOf("hee");
            int heeAfter = heeBefore + 2;

            string fullWord = "*hee*";
            bool isLetter = true;

            while (isLetter)
            {
                if (heeBefore - 1 >= 0)
                {
                    heeBefore--;

                    if (fullMessage[heeBefore].IsBasicLetter())
                        fullWord = fullMessage.Substring(heeBefore, 1) + fullWord;
                    else
                        isLetter = false;
                }
                else isLetter = false;
            }

            isLetter = true;

            while (isLetter)
            {
                if (heeAfter + 1 < fullMessage.Length)
                {
                    heeAfter++;

                    if (fullMessage[heeAfter].IsBasicLetter())
                        fullWord += fullMessage.Substring(heeAfter, 1);
                    else
                        isLetter = false;
                }
                else isLetter = false;
            }

            fullWord += ", hoo!";
            return fullWord;
        }
    }
}