using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Armors
{
    public sealed class ArmorAutocompleteProvider : IAutocompleteProvider
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
                botContext.ArmorCatalog
                    .Autocomplete(input, 25)
                    .Select(armor =>
                        new DiscordAutoCompleteChoice(armor.Name, armor.Name));
            return Task.FromResult(choices);
        }
    }
}
