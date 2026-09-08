using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using FestivalBot.Feats;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class FeatCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("feats", "Look up a feat by name.")]
        public Task FeatsAsync(
            InteractionContext context,
            [Option("name", "Feat name, such as Open Mind."),
             Autocomplete(typeof(FeatAutocompleteProvider))] string name) =>
            LookupAsync(context, name, false);

        [SlashCommand("feitos", "Procura um feito pelo nome.")]
        public Task FeitosAsync(
            InteractionContext context,
            [Option("nome", "Nome do feito, como Open Mind."),
             Autocomplete(typeof(FeatAutocompleteProvider))] string name) =>
            LookupAsync(context, name, true);

        private async Task LookupAsync(
            InteractionContext context, string name, bool portuguese)
        {
            FeatRecord feat = Context.FeatCatalog.FindExact(name);
            if (feat != null)
            {
                await SlashCommandResponder.RespondAsync(
                    context, FeatEmbedBuilder.Build(feat, portuguese));
                return;
            }

            IReadOnlyList<FeatRecord> suggestions = Context.FeatCatalog.Suggest(name);
            if (suggestions.Count == 0)
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Hoo... Não encontrei nenhum feito parecido com **{name}**."
                    : $"Hoo... I couldn't find a feat resembling **{name}**.");
                return;
            }

            string names = string.Join(", ",
                suggestions.Select(suggestion => $"**{suggestion.Name}**"));
            await SlashCommandResponder.RespondAsync(context, portuguese
                ? $"Hoo... Não encontrei **{name}**. Você quis dizer {names}? Use `/feitos` novamente."
                : $"Hoo... I couldn't find **{name}**. Did you mean {names}? Run `/feats` again.");
        }
    }
}
