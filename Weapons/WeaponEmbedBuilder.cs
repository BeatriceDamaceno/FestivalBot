using DSharpPlus.Entities;
using System.Collections.Generic;

namespace FestivalBot.Weapons
{
    public static class WeaponEmbedBuilder
    {
        public static DiscordEmbedBuilder Build(
            WeaponRecord weapon, bool portuguese)
        {
            var embed = new DiscordEmbedBuilder().WithTitle(weapon.Name);
            var details = new List<string>();
            AddLine(details, portuguese ? "Dano" : "Damage", weapon.Damage);
            AddLine(details, portuguese ? "Alcance" : "Reach", weapon.Reach);
            AddLine(details, portuguese ? "Tipo" : "Type", weapon.Type);
            AddLine(details, portuguese ? "Preço" : "Price", weapon.Price);
            if (details.Count > 0)
                embed.WithDescription(string.Join("\n", details));

            if (!string.IsNullOrWhiteSpace(weapon.Extra))
            {
                embed.AddField(
                    portuguese ? "Extra" : "Extra",
                    weapon.Extra.Length <= 1024
                        ? weapon.Extra
                        : weapon.Extra.Substring(0, 1023) + "…");
            }

            return embed;
        }

        private static void AddLine(List<string> lines, string label, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                lines.Add($"**{label}:** {value.Trim()}");
        }
    }
}
