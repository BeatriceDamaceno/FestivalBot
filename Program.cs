using DSharpPlus;
using FestivalBot.Accessories;
using FestivalBot.Armors;
using FestivalBot.Bot;
using FestivalBot.Commands;
using FestivalBot.Consumables;
using FestivalBot.Creatures;
using FestivalBot.Feats;
using FestivalBot.Fun;
using FestivalBot.Services;
using FestivalBot.Spells;
using FestivalBot.Weapons;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using SQLitePCL;
using System;
using System.Threading.Tasks;

namespace FestivalBot
{
    internal static class Program
    {
        private static DiscordClient _discord;

        private static void Main(string[] args)
        {
            string token = "";
            string databasePath = PathResolver.ResolveDatabasePath();
            string creaturesPath = PathResolver.ResolveCreaturesPath();
            string spellsPath = PathResolver.ResolveSpellsPath();
            string armorsPath = PathResolver.ResolveArmorsPath();
            string weaponsPath = PathResolver.ResolveWeaponsPath();
            string accessoriesPath = PathResolver.ResolveAccessoriesPath();
            string consumablesPath = PathResolver.ResolveConsumablesPath();
            string featsPath = PathResolver.ResolveFeatsPath();

            Console.WriteLine("Using database: " + databasePath);
            Batteries.Init();
            DatabaseInitializer.EnsureWorkColumns(databasePath);

            CreatureCatalog creatureCatalog = CreatureCatalog.Load(creaturesPath);
            Console.WriteLine(
                $"Loaded {creatureCatalog.Count} creatures from: {creaturesPath}");
            SpellCatalog spellCatalog = SpellCatalog.Load(spellsPath);
            Console.WriteLine(
                $"Loaded {spellCatalog.Count} spells from: {spellsPath}");
            ArmorCatalog armorCatalog = ArmorCatalog.Load(armorsPath);
            Console.WriteLine(
                $"Loaded {armorCatalog.Count} armors from: {armorsPath}");
            WeaponCatalog weaponCatalog = WeaponCatalog.Load(weaponsPath);
            Console.WriteLine(
                $"Loaded {weaponCatalog.Count} weapons from: {weaponsPath}");
            AccessoryCatalog accessoryCatalog =
                AccessoryCatalog.Load(accessoriesPath);
            Console.WriteLine(
                $"Loaded {accessoryCatalog.Count} accessories from: {accessoriesPath}");
            ConsumableCatalog consumableCatalog =
                ConsumableCatalog.Load(consumablesPath);
            Console.WriteLine(
                $"Loaded {consumableCatalog.Count} consumables from: {consumablesPath}");
            FeatCatalog featCatalog = FeatCatalog.Load(featsPath);
            Console.WriteLine(
                $"Loaded {featCatalog.Count} feats from: {featsPath}");

            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            RunAsync(
                    databasePath, creatureCatalog, spellCatalog, armorCatalog,
                    weaponCatalog, accessoryCatalog, consumableCatalog,
                    featCatalog, _discord)
                .GetAwaiter().GetResult();
        }

        private static async Task RunAsync(
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
            var context = new BotContext(
                databasePath, creatureCatalog, spellCatalog, armorCatalog,
                weaponCatalog, accessoryCatalog, consumableCatalog,
                featCatalog, discord);
            var services = new ServiceCollection()
                .AddSingleton(context)
                .BuildServiceProvider();
            var slash = discord.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            });
            slash.SlashCommandErrored += async (_, e) =>
            {
                Console.WriteLine($"Slash command error: {e.Exception}");
                try
                {
                    await SlashCommandResponder.RespondAsync(
                        e.Context,
                        "FrostBot encountered an error while running this command: " +
                        $"`{e.Exception.GetBaseException().Message}`");
                }
                catch (Exception responseError)
                {
                    Console.WriteLine(
                        $"Could not send slash command error response: {responseError}");
                }
            };
            slash.AutocompleteErrored += (_, e) =>
            {
                Console.WriteLine($"Autocomplete error: {e.Exception}");
                return Task.CompletedTask;
            };
            slash.RegisterCommands<UserCommands>();
            slash.RegisterCommands<CreatureCommands>();
            slash.RegisterCommands<SpellCommands>();
            slash.RegisterCommands<ArmorCommands>();
            slash.RegisterCommands<WeaponCommands>();
            slash.RegisterCommands<AccessoryCommands>();
            slash.RegisterCommands<ConsumableCommands>();
            slash.RegisterCommands<FeatCommands>();
            slash.RegisterCommands<WorkCommands>();
            slash.RegisterCommands<DiceCommands>();
            slash.RegisterCommands<FactionCommands>();
            slash.RegisterCommands<FunCommands>();
            slash.RegisterCommands<HelpCommands>();

            var commandChannel = await discord.GetChannelAsync(929162376371118221);
            ulong primaryGuildId = commandChannel.GuildId
                ?? throw new InvalidOperationException(
                    "The slash-command registration channel is not in a guild.");
            slash.RegisterCommands<UserCommands>(primaryGuildId);
            slash.RegisterCommands<CreatureCommands>(primaryGuildId);
            slash.RegisterCommands<SpellCommands>(primaryGuildId);
            slash.RegisterCommands<ArmorCommands>(primaryGuildId);
            slash.RegisterCommands<WeaponCommands>(primaryGuildId);
            slash.RegisterCommands<AccessoryCommands>(primaryGuildId);
            slash.RegisterCommands<ConsumableCommands>(primaryGuildId);
            slash.RegisterCommands<FeatCommands>(primaryGuildId);
            slash.RegisterCommands<WorkCommands>(primaryGuildId);
            slash.RegisterCommands<DiceCommands>(primaryGuildId);
            slash.RegisterCommands<FactionCommands>(primaryGuildId);
            slash.RegisterCommands<FunCommands>(primaryGuildId);
            slash.RegisterCommands<HelpCommands>(primaryGuildId);

            var staminaRefill = new StaminaRefillService(databasePath);
            _ = Task.Run(staminaRefill.RunAsync);

            var heeHandler = new HeeMessageHandler(context);
            discord.MessageCreated += async (_, e) => await heeHandler.HandleAsync(e);

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            if (_discord == null)
                return;

            _discord.SendMessageAsync(
                _discord.GetChannelAsync(929162376371118221).Result,
                "G-hoo-d bye! Frostbot is shutting down.");
        }
    }
}
