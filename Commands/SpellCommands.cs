using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using FestivalBot.Spells;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class SpellCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("spells", "Look up a spell by name.")]
        public Task SpellsAsync(
            InteractionContext context,
            [Option("name", "Spell name, such as Skull Cracker."),
             Autocomplete(typeof(SpellAutocompleteProvider))] string name) =>
            LookupAsync(context, name, false);

        [SlashCommand("feiticos", "Procura um feitiço pelo nome.")]
        public Task FeiticosAsync(
            InteractionContext context,
            [Option("nome", "Nome do feitiço, como Skull Cracker."),
             Autocomplete(typeof(SpellAutocompleteProvider))] string name) =>
            LookupAsync(context, name, true);

        private async Task LookupAsync(
            InteractionContext context, string name, bool portuguese)
        {
            SpellRecord spell = Context.SpellCatalog.FindExact(name);
            if (spell != null)
            {
                await SlashCommandResponder.RespondAsync(
                    context, SpellEmbedBuilder.Build(spell, portuguese));
                return;
            }

            IReadOnlyList<SpellRecord> suggestions =
                Context.SpellCatalog.Suggest(name);
            if (suggestions.Count == 0)
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Hoo... Não encontrei nenhum feitiço parecido com **{name}**."
                    : $"Hoo... I couldn't find a spell resembling **{name}**.");
                return;
            }

            string names = string.Join(", ",
                suggestions.Select(suggestion => $"**{suggestion.Name}**"));
            await SlashCommandResponder.RespondAsync(context, portuguese
                ? $"Hoo... Não encontrei **{name}**. Você quis dizer {names}? Use `/feiticos` novamente."
                : $"Hoo... I couldn't find **{name}**. Did you mean {names}? Run `/spells` again.");
        }
    }
}
