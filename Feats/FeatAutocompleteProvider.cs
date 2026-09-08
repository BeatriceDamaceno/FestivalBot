using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FestivalBot.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FestivalBot.Feats
{
    public sealed class FeatAutocompleteProvider : IAutocompleteProvider
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
                botContext.FeatCatalog
                    .Autocomplete(input, 25)
                    .Select(feat =>
                        new DiscordAutoCompleteChoice(feat.Name, feat.Name));
            return Task.FromResult(choices);
        }
    }
}
