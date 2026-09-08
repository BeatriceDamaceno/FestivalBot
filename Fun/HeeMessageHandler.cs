using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FestivalBot.Bot;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Fun
{
    public sealed class HeeMessageHandler
    {
        private readonly BotContext _context;

        public HeeMessageHandler(BotContext context)
        {
            _context = context;
        }

        public async Task HandleAsync(MessageCreateEventArgs e)
        {
            string channel = e.Message.Channel.Name;
            bool validChannel =
                _context.ValidEnglishChannels.Any(channel.Contains)
                || _context.ValidPortugueseChannels.Any(channel.Contains);
            if (!validChannel || e.Message.Author.IsBot
                || !e.Message.Content.ToLower().Contains("hee"))
                return;

            if (System.Random.Shared.Next(1, 11) == 1)
            {
                string memePath = MemePicker.GetRandomPath();
                if (memePath != null)
                {
                    await using var stream = File.OpenRead(memePath);
                    await e.Message.RespondAsync(
                        new DiscordMessageBuilder()
                            .WithFile(Path.GetFileName(memePath), stream));
                    return;
                }
            }

            await e.Message.RespondAsync(
                System.Random.Shared.Next(1, 20) == 19
                    ? "HEE-HAW!!"
                    : HeeResponder.BuildResponse(e.Message.Content));
        }
    }
}
