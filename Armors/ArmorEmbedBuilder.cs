using DSharpPlus.Entities;

namespace FestivalBot.Armors
{
    public static class ArmorEmbedBuilder
    {
        public static DiscordEmbedBuilder Build(ArmorRecord armor, bool portuguese)
        {
            var embed = new DiscordEmbedBuilder().WithTitle(armor.Name);
            embed.AddField(
                portuguese ? "Redução de dano" : "Damage reduction",
                string.IsNullOrWhiteSpace(armor.DamageReduction)
                    ? "—"
                    : armor.DamageReduction);
            embed.AddField(
                portuguese ? "Bônus" : "Bonus",
                string.IsNullOrWhiteSpace(armor.Bonus) ? "—" : armor.Bonus);
            return embed;
        }
    }
}
