using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using FestivalBot.Consumables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class ConsumableCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("consumables", "Look up a consumable by name.")]
        public Task ConsumablesAsync(
            InteractionContext context,
            [Option("name", "Consumable name, such as Sticky Bandage."),
             Autocomplete(typeof(ConsumableAutocompleteProvider))] string name) =>
            LookupAsync(context, name, false);

        [SlashCommand("consumiveis", "Procura um consumível pelo nome.")]
        public Task ConsumiveisAsync(
            InteractionContext context,
            [Option("nome", "Nome do consumível, como Sticky Bandage."),
             Autocomplete(typeof(ConsumableAutocompleteProvider))] string name) =>
            LookupAsync(context, name, true);

        private async Task LookupAsync(
            InteractionContext context, string name, bool portuguese)
        {
            ConsumableRecord item = Context.ConsumableCatalog.FindExact(name);
            if (item != null)
            {
                await SlashCommandResponder.RespondAsync(
                    context, ConsumableEmbedBuilder.Build(item, portuguese));
                return;
            }

            IReadOnlyList<ConsumableRecord> suggestions =
                Context.ConsumableCatalog.Suggest(name);
            if (suggestions.Count == 0)
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Hoo... Não encontrei nenhum consumível parecido com **{name}**."
                    : $"Hoo... I couldn't find a consumable resembling **{name}**.");
                return;
            }

            string names = string.Join(", ",
                suggestions.Select(suggestion => $"**{suggestion.Name}**"));
            await SlashCommandResponder.RespondAsync(context, portuguese
                ? $"Hoo... Não encontrei **{name}**. Você quis dizer {names}? Use `/consumiveis` novamente."
                : $"Hoo... I couldn't find **{name}**. Did you mean {names}? Run `/consumables` again.");
        }
    }
}
