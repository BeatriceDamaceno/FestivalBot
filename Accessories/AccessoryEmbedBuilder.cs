using DSharpPlus.Entities;

namespace FestivalBot.Accessories
{
    public static class AccessoryEmbedBuilder
    {
        public static DiscordEmbedBuilder Build(
            AccessoryRecord accessory, bool portuguese)
        {
            var embed = new DiscordEmbedBuilder().WithTitle(accessory.Name);
            embed.AddField(
                portuguese ? "Efeito" : "Effect",
                string.IsNullOrWhiteSpace(accessory.Effect)
                    ? "—"
                    : accessory.Effect);
            embed.AddField(
                portuguese ? "Preço" : "Price",
                string.IsNullOrWhiteSpace(accessory.Price)
                    ? "—"
                    : accessory.Price,
                true);
            embed.AddField(
                portuguese ? "Raridade" : "Rarity",
                string.IsNullOrWhiteSpace(accessory.Rarity)
                    ? "—"
                    : accessory.Rarity,
                true);
            return embed;
        }
    }
}
