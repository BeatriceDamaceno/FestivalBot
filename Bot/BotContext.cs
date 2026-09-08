using DSharpPlus;
using FestivalBot.Accessories;
using FestivalBot.Armors;
using FestivalBot.Consumables;
using FestivalBot.Creatures;
using FestivalBot.Feats;
using FestivalBot.Spells;
using FestivalBot.Weapons;
using System.Collections.Concurrent;

namespace FestivalBot.Bot
{
    public sealed class BotContext
    {
        public BotContext(
            string databasePath,
            CreatureCatalog creatureCatalog,
            SpellCatalog spellCatalog,
            ArmorCatalog armorCatalog,
            WeaponCatalog weaponCatalog,
            AccessoryCatalog accessoryCatalog,
            ConsumableCatalog consumableCatalog,
            FeatCatalog featCatalog,
            DiscordClient discord)
        {
            DatabasePath = databasePath;
            CreatureCatalog = creatureCatalog;
            SpellCatalog = spellCatalog;
            ArmorCatalog = armorCatalog;
            WeaponCatalog = weaponCatalog;
            AccessoryCatalog = accessoryCatalog;
            ConsumableCatalog = consumableCatalog;
            FeatCatalog = featCatalog;
            Discord = discord;
        }

        public string DatabasePath { get; }
        public CreatureCatalog CreatureCatalog { get; }
        public SpellCatalog SpellCatalog { get; }
        public ArmorCatalog ArmorCatalog { get; }
        public WeaponCatalog WeaponCatalog { get; }
        public AccessoryCatalog AccessoryCatalog { get; }
        public ConsumableCatalog ConsumableCatalog { get; }
        public FeatCatalog FeatCatalog { get; }
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
