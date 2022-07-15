using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DSharpPlus;
using Excel = Microsoft.Office.Interop.Excel;       //Microsoft Excel 14 object in references-> COM tab

namespace FestivalBot
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
            
        }

        static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "OTk3NTYzMDMyMjkwOTgzOTc2.GobAsb.UNXVICZvc-rv7KVh_K6hMP0pheiTLHQUjswi6s",
                TokenType = TokenType.Bot
            });
            int snowballs = 0;

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Message.Content.ToLower().StartsWith("hee"))
                {
                    if (!e.Message.Author.IsBot)
                        await e.Message.RespondAsync("ho!");
                }
                    
            };

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Message.Content.ToLower().StartsWith("/collect"))
                {
                    snowballs++;
                    await e.Message.RespondAsync("Hee-ho! Picked up a Snowball! " + e.Message.Author.Mention + " now has " + snowballs + " snowball(s)!");
                }
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}