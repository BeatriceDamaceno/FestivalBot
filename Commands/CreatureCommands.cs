using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using FestivalBot.Creatures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class CreatureCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("creatures", "Look up a creature by name.")]
        public Task CreaturesAsync(
            InteractionContext context,
            [Option("name", "Creature name, such as Pixie."),
             Autocomplete(typeof(CreatureAutocompleteProvider))] string name) =>
            LookupAsync(context, name, false);

        [SlashCommand("criaturas", "Procura uma criatura pelo nome.")]
        public Task CriaturasAsync(
            InteractionContext context,
            [Option("nome", "Nome da criatura, como Pixie."),
             Autocomplete(typeof(CreatureAutocompleteProvider))] string name) =>
            LookupAsync(context, name, true);

        private async Task LookupAsync(
            InteractionContext context, string name, bool portuguese)
        {
            CreatureRecord creature = Context.CreatureCatalog.FindExact(name);
            if (creature != null)
            {
                await SlashCommandResponder.RespondAsync(
                    context, CreatureEmbedBuilder.Build(creature, portuguese));
                return;
            }

            IReadOnlyList<CreatureRecord> suggestions =
                Context.CreatureCatalog.Suggest(name);
            if (suggestions.Count == 0)
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Hoo... Não encontrei nenhuma criatura parecida com **{name}**."
                    : $"Hoo... I couldn't find a creature resembling **{name}**.");
                return;
            }

            string suggestionNames = string.Join(", ",
                suggestions.Select(suggestion => $"**{suggestion.Name}**"));
            await SlashCommandResponder.RespondAsync(context, portuguese
                ? $"Hoo... Não encontrei **{name}**. Você quis dizer {suggestionNames}? Use `/criaturas` novamente."
                : $"Hoo... I couldn't find **{name}**. Did you mean {suggestionNames}? Run `/creatures` again.");
        }
    }
}
