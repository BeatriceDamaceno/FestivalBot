using DSharpPlus;
using FestivalBot.Bot;
using FestivalBot.Commands;
using FestivalBot.Creatures;
using FestivalBot.Fun;
using FestivalBot.Services;
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

            Console.WriteLine("Using database: " + databasePath);
            Batteries.Init();
            DatabaseInitializer.EnsureWorkColumns(databasePath);

            CreatureCatalog creatureCatalog = CreatureCatalog.Load(creaturesPath);
            Console.WriteLine(
                $"Loaded {creatureCatalog.Count} creatures from: {creaturesPath}");

            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            RunAsync(databasePath, creatureCatalog, _discord).GetAwaiter().GetResult();
        }

        private static async Task RunAsync(
            string databasePath,
            CreatureCatalog creatureCatalog,
            DiscordClient discord)
        {
            var context = new BotContext(databasePath, creatureCatalog, discord);
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
