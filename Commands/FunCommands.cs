using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class FunCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("askfrost", "Ask FrostBot a question.")]
        public Task AskFrostAsync(
            InteractionContext context,
            [Option("question", "The question to ask FrostBot.")] string question)
        {
            int answer = Random.Shared.Next(1, 23);
            bool isPremium =
                Context.PremiumUsers.Any(Context.PremiumUsers.Contains);
            string response = FrostResponsesEN.GetResponseEN(
                answer, isPremium, context.User.Mention);
            return SlashCommandResponder.RespondAsync(context, response);
        }

        [SlashCommand("pergunta", "Faça uma pergunta ao FrostBot.")]
        public Task PerguntaAsync(
            InteractionContext context,
            [Option("pergunta", "A pergunta para o FrostBot.")] string question)
        {
            int answer = Random.Shared.Next(1, 22);
            bool isPremium =
                Context.PremiumUsers.Any(Context.PremiumUsers.Contains);
            string response = FrostResponsesPT.GetResponsePT(
                answer, isPremium, context.User.Mention);
            return SlashCommandResponder.RespondAsync(context, response);
        }
    }
}
