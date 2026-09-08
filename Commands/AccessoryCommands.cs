using DSharpPlus.SlashCommands;
using FestivalBot.Accessories;
using FestivalBot.Bot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Commands
{
    public sealed class AccessoryCommands : ApplicationCommandModule
    {
        public BotContext Context { private get; set; }

        [SlashCommand("accessories", "Look up an accessory by name.")]
        public Task AccessoriesAsync(
            InteractionContext context,
            [Option("name", "Accessory name, such as Bracelet of Power."),
             Autocomplete(typeof(AccessoryAutocompleteProvider))] string name) =>
            LookupAsync(context, name, false);

        [SlashCommand("acessorios", "Procura um acessório pelo nome.")]
        public Task AcessoriosAsync(
            InteractionContext context,
            [Option("nome", "Nome do acessório, como Bracelet of Power."),
             Autocomplete(typeof(AccessoryAutocompleteProvider))] string name) =>
            LookupAsync(context, name, true);

        private async Task LookupAsync(
            InteractionContext context, string name, bool portuguese)
        {
            AccessoryRecord accessory = Context.AccessoryCatalog.FindExact(name);
            if (accessory != null)
            {
                await SlashCommandResponder.RespondAsync(
                    context, AccessoryEmbedBuilder.Build(accessory, portuguese));
                return;
            }

            IReadOnlyList<AccessoryRecord> suggestions =
                Context.AccessoryCatalog.Suggest(name);
            if (suggestions.Count == 0)
            {
                await SlashCommandResponder.RespondAsync(context, portuguese
                    ? $"Hoo... Não encontrei nenhum acessório parecido com **{name}**."
                    : $"Hoo... I couldn't find an accessory resembling **{name}**.");
                return;
            }

            string names = string.Join(", ",
                suggestions.Select(suggestion => $"**{suggestion.Name}**"));
            await SlashCommandResponder.RespondAsync(context, portuguese
                ? $"Hoo... Não encontrei **{name}**. Você quis dizer {names}? Use `/acessorios` novamente."
                : $"Hoo... I couldn't find **{name}**. Did you mean {names}? Run `/accessories` again.");
        }
    }
}
