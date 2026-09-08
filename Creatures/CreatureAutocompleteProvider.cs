using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Creatures
{
    public sealed class CreatureAutocompleteProvider : IAutocompleteProvider
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
                botContext.CreatureCatalog
                    .Autocomplete(input, 25)
                    .Select(creature =>
                        new DiscordAutoCompleteChoice(
                            creature.Name.Length <= 100
                                ? creature.Name
                                : creature.Name.Substring(0, 100),
                            creature.Name));

            return Task.FromResult(choices);
        }
    }
}
