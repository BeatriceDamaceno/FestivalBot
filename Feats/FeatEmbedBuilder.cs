using DSharpPlus.Entities;

namespace FestivalBot.Feats
{
    public static class FeatEmbedBuilder
    {
        public static DiscordEmbedBuilder Build(FeatRecord feat, bool portuguese)
        {
            var embed = new DiscordEmbedBuilder().WithTitle(feat.Name);
            embed.AddField(
                portuguese ? "Efeito" : "Effect",
                string.IsNullOrWhiteSpace(feat.Effect) ? "—" : feat.Effect);
            embed.AddField(
                portuguese ? "Requisitos" : "Requirements",
                string.IsNullOrWhiteSpace(feat.Requirements)
                    ? "—"
                    : feat.Requirements);
            embed.AddField(
                portuguese ? "Informações adicionais" : "Additional",
                string.IsNullOrWhiteSpace(feat.Additional)
                    ? "—"
                    : feat.Additional);
            return embed;
        }
    }
}
