using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace FestivalBot.Bot
{
    public static class SlashCommandResponder
    {
        public static Task RespondAsync(InteractionContext context, string content) =>
            context.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(content));

        public static Task RespondAsync(
            InteractionContext context, DiscordEmbedBuilder embed) =>
            context.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed));

        public static Task FollowUpAsync(InteractionContext context, string content) =>
            context.FollowUpAsync(
                new DiscordFollowupMessageBuilder().WithContent(content));
    }
}
