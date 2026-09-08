using DSharpPlus.SlashCommands;
using FestivalBot.Armors;
using FestivalBot.Bot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class ArmorCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("armour", "Look up armour by name.")]
        public Task ArmourAsync(
            InteractionContext context,
            [Option("name", "Armour name, such as Emblem Jacket."),
             Autocomplete(typeof(ArmorAutocompleteProvider))] string name) =>
            LookupAsync(context, name, false);

        [SlashCommand("armadura", "Procura uma armadura pelo nome.")]
        public Task ArmaduraAsync(
            InteractionContext context,
            [Option("nome", "Nome da armadura, como Emblem Jacket."),
             Autocomplete(typeof(ArmorAutocompleteProvider))] string name) =>
            LookupAsync(context, name, true);

        private async Task LookupAsync(
            InteractionContext context, string name, bool portuguese)
        {
            ArmorRecord armor = Context.ArmorCatalog.FindExact(name);
            if (armor != null)
            {
                await SlashCommandResponder.RespondAsync(
                    context, ArmorEmbedBuilder.Build(armor, portuguese));
                return;
            }

            IReadOnlyList<ArmorRecord> suggestions =
                Context.ArmorCatalog.Suggest(name);
            if (suggestions.Count == 0)
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Hoo... Não encontrei nenhuma armadura parecida com **{name}**."
                    : $"Hoo... I couldn't find armour resembling **{name}**.");
                return;
            }

            string names = string.Join(", ",
                suggestions.Select(suggestion => $"**{suggestion.Name}**"));
            await SlashCommandResponder.RespondAsync(context, portuguese
                ? $"Hoo... Não encontrei **{name}**. Você quis dizer {names}? Use `/armadura` novamente."
                : $"Hoo... I couldn't find **{name}**. Did you mean {names}? Run `/armour` again.");
        }
    }
}
