using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using FestivalBot.Weapons;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class WeaponCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("weapons", "Look up a weapon by name.")]
        public Task WeaponsAsync(
            InteractionContext context,
            [Option("name", "Weapon name, such as Short Sword."),
             Autocomplete(typeof(WeaponAutocompleteProvider))] string name) =>
            LookupAsync(context, name, false);

        [SlashCommand("armas", "Procura uma arma pelo nome.")]
        public Task ArmasAsync(
            InteractionContext context,
            [Option("nome", "Nome da arma, como Short Sword."),
             Autocomplete(typeof(WeaponAutocompleteProvider))] string name) =>
            LookupAsync(context, name, true);

        private async Task LookupAsync(
            InteractionContext context, string name, bool portuguese)
        {
            WeaponRecord weapon = Context.WeaponCatalog.FindExact(name);
            if (weapon != null)
            {
                await SlashCommandResponder.RespondAsync(
                    context, WeaponEmbedBuilder.Build(weapon, portuguese));
                return;
            }

            IReadOnlyList<WeaponRecord> suggestions =
                Context.WeaponCatalog.Suggest(name);
            if (suggestions.Count == 0)
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Hoo... Não encontrei nenhuma arma parecida com **{name}**."
                    : $"Hoo... I couldn't find a weapon resembling **{name}**.");
                return;
            }

            string names = string.Join(", ",
                suggestions.Select(suggestion => $"**{suggestion.Name}**"));
            await SlashCommandResponder.RespondAsync(context, portuguese
                ? $"Hoo... Não encontrei **{name}**. Você quis dizer {names}? Use `/armas` novamente."
                : $"Hoo... I couldn't find **{name}**. Did you mean {names}? Run `/weapons` again.");
        }
    }
}
