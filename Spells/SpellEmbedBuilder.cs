using DSharpPlus.Entities;
using System.Collections.Generic;

namespace FestivalBot.Spells
{
    public static class SpellEmbedBuilder
    {
        public static DiscordEmbedBuilder Build(SpellRecord spell, bool portuguese)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle(Truncate(spell.Name, 256));

            var summary = new List<string>();
            AddLine(summary, "Tier", spell.Tier);
            AddLine(summary, portuguese ? "Tipo" : "Type", spell.Type);
            AddLine(summary, portuguese ? "Categorias" : "Categories",
                spell.Categories);
            if (summary.Count > 0)
                embed.WithDescription(Truncate(string.Join("\n", summary), 4096));

            var casting = new List<string>();
            AddLine(casting, portuguese ? "Tempo" : "Time", spell.Time);
            AddLine(casting, portuguese ? "Alcance" : "Reach", spell.Reach);
            AddLine(casting, portuguese ? "Duração" : "Duration", spell.Duration);
            if (casting.Count > 0)
                embed.AddField(portuguese ? "Conjuração" : "Casting",
                    Truncate(string.Join("\n", casting), 1024));

            if (!string.IsNullOrWhiteSpace(spell.Effect))
            {
                embed.AddField(portuguese ? "Efeito" : "Effect",
                    Truncate(spell.Effect, 1024));
            }

            return embed;
        }

        private static void AddLine(List<string> lines, string label, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                lines.Add($"**{label}:** {value.Trim()}");
        }

        private static string Truncate(string value, int maximumLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maximumLength)
                return value;

            return value.Substring(0, maximumLength - 1).TrimEnd() + "…";
        }
    }
}
