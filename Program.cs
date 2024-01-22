using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Emzi0767;
using Microsoft.Data.SqlClient;
using Microsoft.Office.Interop.Excel;

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
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            string botToken = "";

            if (File.Exists("C:\\bot_info.txt"))
            {

                using (StreamReader reader = new StreamReader("C:\\bot_info.txt"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        switch (i)
                        {
                            case 0: builder.DataSource = reader.ReadLine(); break;
                            case 1: builder.UserID = reader.ReadLine(); break;
                            case 2: builder.Password = reader.ReadLine(); break;
                            case 3: builder.InitialCatalog = reader.ReadLine(); break;
                            case 4: botToken = reader.ReadLine(); break;
                        }
                    }
                }
            }
            else
            {
                throw new FileNotFoundException("bot_info file not found!");
            }
            
            Globals.discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = botToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            MainAsync(builder, Globals.discord).GetAwaiter().GetResult();

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            Globals.discord.SendMessageAsync(Globals.discord.GetChannelAsync(929162376371118221).Result, "G-hoo-d bye! Frostbot is shutting down.");
        }

        static async Task MainAsync(SqlConnectionStringBuilder builder, DiscordClient discord)
        {
            String[] validChannels = {"chatting", "memes", "battlefield", "battlefield-2", "moderator-chat", "admin-chat", "patron-lounge", "bot-test", "voice-chat"};
            String[] premiumUsers = { "starlight.bea", ".castellian", "lost.crow", "wynastra", "silverfoxy", "alaendin", "amaterasu6x", "evoro", "xenia_", "rdlm", "moonsnake21", "chazghost"};

            int retCode = 0; 
            
            //00 - No bot interaction
            //01 - askfrost 
            //02 - 'hee' found
            //10 - user registered 
            //11 - snowball collected
            //12 - snowball thrown 
            //99 - admin commands (killfrost, list) 

            String snowballs = "";
            bool hasSnow;
            String last_pickup = "";
            TimeSpan diff;

            await discord.SendMessageAsync(discord.GetChannelAsync(929162376371118221).Result, "Hee-hello! Frostbot is online.");

            discord.MessageCreated += async (s, e) =>
            {
                retCode = 0; 
                Random rd = new Random();
                String channel = e.Message.Channel.Name;
                
                if (e.Message.Content.ToLower().StartsWith("!list"))
                {
                    retCode = 99;

                    String mem_list;
                    mem_list = "";

                    List<string> users = (List<string>)(await e.Guild.GetAllMembersAsync().ConfigureAwait(false)). Select(member => member.DisplayName).ToList();

                    users.Sort();

                    foreach (String user in users)
                    {
                        mem_list = mem_list + " | " + user.Replace("?", ""); //Environment.NewLine
                    }

                    Console.WriteLine(mem_list);

                    await e.Message.RespondAsync("Hoo!?");

                    goto Skip;
                }

                if (!validChannels.Any(channel.Contains) || e.Message.Author.IsBot)
                {
                    goto Skip;
                } 

                if (e.Message.Content.ToLower().StartsWith("!register") || e.Message.Content.ToLower().StartsWith("!collect") || e.Message.Content.ToLower().StartsWith("!throw"))
                {
                    if (channel != "battlefield" && channel != "battlefield-2")
                    {
                        await e.Message.RespondAsync("Thank hoo-you for your enthusiasm, but this isn't the #battlefield...");
                        goto NonBattle;
                    }
                }

                if (e.Message.Content.ToLower().StartsWith("!register"))
                {
                    retCode = 10;
                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {
                        String sql = "SELECT * FROM SnowBallEvent WHERE name = '" + e.Message.Author.Mention + "'";
                        connection.Open();
                        SqlCommand command = new SqlCommand(sql, connection);
                        SqlDataReader reader = command.ExecuteReader();

                        if (!reader.HasRows)
                        {
                            using (SqlConnection conn2 = new SqlConnection(builder.ConnectionString))
                            {
                                sql = "INSERT INTO SnowBallEvent (ID, name, team, snowballs, hits) VALUES ((SELECT COALESCE(MAX(ID),0)+1 FROM SnowBallEvent),  '" + e.Message.Author.Mention + "','',0,0)";
                                conn2.Open();
                                command = new SqlCommand(sql, conn2);
                                reader = command.ExecuteReader();
                                conn2.Close();
                            }

                            await e.Message.RespondAsync("Hee-ho! " + e.Message.Author.Mention.ToString() + " has been hee-registered!");
                        }
                        else
                        {
                            await e.Message.RespondAsync("Hoo! Nice to hee-see you, " + e.Message.Author.Mention.ToString() + "! You are already hee-registered.");
                        }

                        connection.Close();
                    }
                }

                if (e.Message.Content.ToLower().StartsWith("!collect"))
                {
                    retCode = 11; 
                    String target = e.Message.Content.ToString();
                    target = target.Remove(0, 7);

                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {
                        String sql = "SELECT * FROM SnowBallEvent WHERE name = '" + e.Message.Author.Mention + "'";
                        connection.Open();
                        SqlCommand command = new SqlCommand(sql, connection);
                        SqlDataReader reader = command.ExecuteReader();

                        if (!reader.HasRows)
                        {
                            await e.Message.RespondAsync("Hoo... You are not registered, use !register first!");
                        }
                        else
                        {
                            using (SqlConnection conn2 = new SqlConnection(builder.ConnectionString))
                            {
                                sql = "UPDATE SnowBallEvent SET snowballs = snowballs+1, last_pickup = GETDATE() WHERE name = '" + e.Message.Author.Mention.ToString() + "'";
                                conn2.Open();
                                command = new SqlCommand(sql, conn2);
                                reader = command.ExecuteReader();
                                conn2.Close();
                            }

                            using (SqlConnection conn3 = new SqlConnection(builder.ConnectionString))
                            {
                                sql = "Select snowballs from SnowBallEvent WHERE name = '" + e.Message.Author.Mention.ToString() + "'";
                                conn3.Open();
                                command = new SqlCommand(sql, conn3);
                                reader = command.ExecuteReader();
                                if (reader.Read())
                                    snowballs = String.Format("{0}", reader["snowballs"]);
                                conn3.Close();
                            }

                            await e.Message.RespondAsync("Hoo! You picked up a snowball, " + e.Message.Author.Mention.ToString() + "! You have " + snowballs + " snowball(s)!");
                        }

                        connection.Close();
                    }
                }

                if (e.Message.Content.ToLower().StartsWith("!throw"))
                {
                    retCode = 12; 
                    String target = e.Message.Content.ToString();
                    target = target.Remove(0, 9);
                    target = "<@!" + target;
                    if (e.Message.Author.Mention.Equals(target))
                    {
                        await e.Message.RespondAsync("Hee?! You can't hit yourself, silly!");
                        goto NonBattle;
                    }

                    using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                    {
                        String sql = "SELECT * FROM SnowBallEvent WHERE name = '" + e.Message.Author.Mention + "'";
                        connection.Open();
                        SqlCommand command = new SqlCommand(sql, connection);
                        SqlDataReader reader = command.ExecuteReader();

                        if (!reader.HasRows)
                        {
                            await e.Message.RespondAsync("Hoo... You are not registered, use !register first!");
                        }
                        else
                        {
                            using (SqlConnection conn3 = new SqlConnection(builder.ConnectionString))
                            {
                                sql = "Select snowballs from SnowBallEvent WHERE name = '" + e.Message.Author.Mention.ToString() + "'";
                                conn3.Open();
                                command = new SqlCommand(sql, conn3);
                                reader = command.ExecuteReader();
                                if (reader.Read())
                                    snowballs = String.Format("{0}", reader["snowballs"]);
                                conn3.Close();
                            }

                            using (SqlConnection conn3 = new SqlConnection(builder.ConnectionString))
                            {
                                sql = "Select snowballs from SnowBallEvent WHERE name = '" + target + "'";
                                conn3.Open();
                                command = new SqlCommand(sql, conn3);
                                reader = command.ExecuteReader();
                                if (!reader.Read())
                                {
                                    await e.Message.RespondAsync("Hoo, your target is not registered!");
                                    goto NonBattle;
                                } 
                                else
                                {
                                    if (String.Format("{0}", reader["snowballs"]) == "0")
                                        hasSnow = false;
                                    else
                                        hasSnow = true;
                                }
                                conn3.Close();
                            }

                            if (snowballs.Equals("0"))
                            {
                                await e.Message.RespondAsync("You're out of snowballs, ho... You need to !collect some!");
                            }
                            else
                            {
                                int hit = rd.Next(1, 100);
                                hit = hit + Convert.ToInt32((Convert.ToInt32(snowballs) * 0.5));
                                if (hit < 30)
                                {
                                    await e.Message.RespondAsync("Sorry, hee-you missed! Collect more and try again!");

                                    using (SqlConnection conn2 = new SqlConnection(builder.ConnectionString))
                                    {
                                        sql = "UPDATE SnowBallEvent SET snowballs = 0 WHERE name = '" + e.Message.Author.Mention.ToString() + "'";
                                        conn2.Open();
                                        command = new SqlCommand(sql, conn2);
                                        reader = command.ExecuteReader();
                                        conn2.Close();
                                    }
                                }
                                else
                                {
                                    if (hasSnow)
                                    {
                                        using (SqlConnection conn3 = new SqlConnection(builder.ConnectionString))
                                        {
                                            sql = "Select last_pickup from SnowBallEvent WHERE name = '" + e.Message.Author.Mention.ToString() + "'";
                                            conn3.Open();
                                            command = new SqlCommand(sql, conn3);
                                            reader = command.ExecuteReader();
                                            if (reader.Read())
                                                last_pickup = String.Format("{0}", reader["last_pickup"]);
                                            conn3.Close();
                                        }

                                        diff = DateTime.Parse(last_pickup) - DateTime.Now;
                                        if (diff.TotalSeconds < 5) {
                                            await e.Message.RespondAsync("Hee, let me roll them up first! Try again, hoo.");
                                            goto NonBattle;
                                        }

                                        using (SqlConnection conn2 = new SqlConnection(builder.ConnectionString))
                                        {
                                            sql = "UPDATE SnowBallEvent SET snowballs = CEILING(snowballs/2) WHERE name = '" + target + "'";
                                            conn2.Open();
                                            command = new SqlCommand(sql, conn2);
                                            reader = command.ExecuteReader();
                                            conn2.Close();
                                        }

                                        using (SqlConnection conn2 = new SqlConnection(builder.ConnectionString))
                                        {
                                            sql = "UPDATE SnowBallEvent SET snowballs = 0 WHERE name = '" + e.Message.Author.Mention.ToString() + "'";
                                            conn2.Open();
                                            command = new SqlCommand(sql, conn2);
                                            reader = command.ExecuteReader();
                                            conn2.Close();
                                        }

                                        using (SqlConnection conn2 = new SqlConnection(builder.ConnectionString))
                                        {
                                            sql = "UPDATE SnowBallEvent SET hits = hits+" + snowballs + " WHERE name = '" + e.Message.Author.Mention.ToString() + "'";
                                            conn2.Open();
                                            command = new SqlCommand(sql, conn2);
                                            reader = command.ExecuteReader();
                                            conn2.Close();
                                        }

                                        await e.Message.RespondAsync("Hee-youch! You hit them!");
                                    } else
                                    {
                                        using (SqlConnection conn2 = new SqlConnection(builder.ConnectionString))
                                        {
                                            sql = "UPDATE SnowBallEvent SET snowballs = CEILING(snowballs/2) WHERE name = '" + e.Message.Author.Mention.ToString() + "'";
                                            conn2.Open();
                                            command = new SqlCommand(sql, conn2);
                                            reader = command.ExecuteReader();
                                            conn2.Close();
                                        }

                                        await e.Message.RespondAsync("Hoo, they don't have any snowballs, no points!");
                                    }
                                }
                            }
                        }

                        connection.Close();
                    }
                }

            NonBattle:
                if (e.Message.Content.ToLower().Contains("hee"))
                {
                    retCode = 02;
                    int haw = rd.Next(1, 20);
                    if (haw == 19)
                    {
                            await e.Message.RespondAsync("HEE-HAW!!");
                    } else 
                    {
                            await e.Message.RespondAsync(FindHeeWord(e.Message.Content));
                    }
                }
               
                if (e.Message.Content.ToLower().StartsWith("!askfrost"))
                {
                    retCode = 01; 
                    if (e.Message.Content.ToLower().Equals("!askfrost"))
                    {
                        await e.Message.RespondAsync("You gotta ask something, dummy!");
                    }
                    else
                    {
                        int ans = rd.Next(1, 21);

                        switch (ans)
                        {
                            case 1:
                                await e.Message.RespondAsync(RandomCaps("Well, if it isn't the dumbass with too many friends!"));
                                break;
                            case 2:
                                await e.Message.RespondAsync("Hoo! Don't count on hee-it!");
                                break;
                            case 3:
                                await e.Message.RespondAsync("King Hoo-Frost said 'No-ho!'...");
                                break;
                            case 4:
                                await e.Message.RespondAsync("[Jack Frost looks away, disgusted]");
                                break;
                            case 5:
                                await e.Message.RespondAsync("Hee-no!");
                                break;
                            case 6:
                                await e.Message.RespondAsync("Hee... It's so foggy... Ask again, ho!");
                                break;
                            case 7:
                                await e.Message.RespondAsync("[They seem asleep... Perhaps try again later.]");
                                break;
                            case 8:
                                await e.Message.RespondAsync("HEE! I'm not telling!");
                                break;
                            case 9:
                                await e.Message.RespondAsync("Give me some Macca first, ho!");
                                break;
                            case 10:
                                await e.Message.RespondAsync("Try again, hee.");
                                break;
                            case 11:
                                await e.Message.RespondAsync("That's certain, I guarant-hee it!");
                                break;
                            case 12:
                                await e.Message.RespondAsync("Hee! Decidedly so!");
                                break;
                            case 13:
                                await e.Message.RespondAsync("No doubt about it, hoo!");
                                break;
                            case 14:
                                await e.Message.RespondAsync("Yes, for sur-hee");
                                break;
                            case 15:
                                await e.Message.RespondAsync("Probabl-hee.");
                                break;
                            case 16:
                                await e.Message.RespondAsync("PSYCHO RAGE");
                                break;
                            case 17:
                                await e.Message.RespondAsync("Yes. Hee.");
                                break;
                            case 18:
                                await e.Message.RespondAsync("Loo-hoo-king good, hee!");
                                break;
                            case 19:
                                await e.Message.RespondAsync("As I see-hee it, yes.");
                                break;
                            case 20:
                                await e.Message.RespondAsync("Most likely, ho!");
                                break;
                            case 21:
                                if (premiumUsers.Any(e.Author.Username.Contains))
                                {
                                    await e.Message.RespondAsync("*What-hee-ver you say, boss!*");
                                }
                                else
                                {
                                    await e.Message.RespondAsync("This is a FROSTBOT GOLD (tm) Answer, hee! Patrons Only!");
                                }
                                break;
                            default:
                                await e.Message.RespondAsync("Hee?! Something's wrong. [Festival Frost encountered an error]");
                                break;
                        }
                    }
                }

                if (e.Message.Content.ToLower().StartsWith("!killfrost"))
                {
                    retCode = 99; 
                    if (e.Message.Author.Username == "starlightbea")
                    {
                        await discord.SendMessageAsync(discord.GetChannelAsync(929162376371118221).Result, "G-hoo-d bye! Frostbot is shutting down.");
                        Environment.Exit(0);
                    } else
                    {
                        await e.Message.RespondAsync(RandomCaps("\"!killfrost\" nice try dumbass!"));
                    }
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

            Boolean isLetter = true; 

            while (isLetter)
            {
                if (heeBefore - 1 >= 0)
                {
                    heeBefore--;

                    if (fullMessage[heeBefore].IsBasicLetter())
                    {
                        fullWord = fullMessage.Substring(heeBefore, 1) + fullWord;
                    }
                    else isLetter = false;
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
                    {
                        fullWord += fullMessage.Substring(heeAfter, 1);
                    }
                    else isLetter = false;
                }
                else isLetter = false;
            }

            fullWord += ", hoo!";
            return fullWord; 
        }

        private static string RandomCaps(string entranceMessage)
        {
            string exitMessage = "";
            Random rd = new Random();
            int ans; 

            foreach (char c in entranceMessage)
            {
                if (c.IsBasicLetter())
                {
                    ans = rd.Next(0, 2);
                    if (ans == 0)
                        exitMessage += c.ToString().ToLower();
                    else
                        exitMessage += c.ToString().ToUpper();
                } else
                    exitMessage += c;
            }

            return exitMessage; 
        }

    }
}