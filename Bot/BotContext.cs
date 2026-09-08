using DSharpPlus;
using FestivalBot.Creatures;
using System.Collections.Concurrent;

namespace FestivalBot.Bot
{
    public sealed class BotContext
    {
        public BotContext(string databasePath, CreatureCatalog creatureCatalog, DiscordClient discord)
        {
            DatabasePath = databasePath;
            CreatureCatalog = creatureCatalog;
            Discord = discord;
        }

        public string DatabasePath { get; }
        public CreatureCatalog CreatureCatalog { get; }
        public DiscordClient Discord { get; }

        public ConcurrentDictionary<ulong, byte> ActiveDiceGames { get; } = new();

        public string[] ValidEnglishChannels { get; } =
        {
            "chatting", "memes", "battlefield", "battlefield-2", "moderator-chat",
            "admin-chat", "patron-lounge", "bot-test", "voice-chat", "frost-reign"
        };

        public string[] ValidPortugueseChannels { get; } = { "conversa", "perguntas" };

        public string[] PremiumUsers { get; } =
        {
            "aphotic.hymn", ".castellian", "sanerion", "coffeethehermit", "wyplue"
        };
    }
}
