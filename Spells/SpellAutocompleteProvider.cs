using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Spells
{
    public sealed class SpellAutocompleteProvider : IAutocompleteProvider
    {
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(
            AutocompleteContext context)
        {
            var botContext =
                context.Services?.GetService(typeof(BotContext)) as BotContext;
            if (botContext == null)
            {
                return Task.FromResult(
                    Enumerable.Empty<DiscordAutoCompleteChoice>());
            }

            string input = Convert.ToString(context.OptionValue) ?? "";
            IEnumerable<DiscordAutoCompleteChoice> choices =
                botContext.SpellCatalog
                    .Autocomplete(input, 25)
                    .Select(spell =>
                        new DiscordAutoCompleteChoice(
                            spell.Name.Length <= 100
                                ? spell.Name
                                : spell.Name.Substring(0, 100),
                            spell.Name));

            return Task.FromResult(choices);
        }
    }
}
