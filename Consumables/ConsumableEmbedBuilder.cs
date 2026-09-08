using DSharpPlus.Entities;

namespace FestivalBot.Consumables
{
    public static class ConsumableEmbedBuilder
    {
        public static DiscordEmbedBuilder Build(
            ConsumableRecord item, bool portuguese)
        {
            var embed = new DiscordEmbedBuilder().WithTitle(item.Name);
            embed.AddField(
                portuguese ? "Efeito" : "Effect",
                string.IsNullOrWhiteSpace(item.Effect) ? "—" : item.Effect);
            embed.AddField(
                portuguese ? "Preço" : "Price",
                string.IsNullOrWhiteSpace(item.Price) ? "—" : item.Price,
                true);
            embed.AddField(
                portuguese ? "Raridade" : "Rarity",
                string.IsNullOrWhiteSpace(item.Rarity) ? "—" : item.Rarity,
                true);
            return embed;
        }
    }
}
