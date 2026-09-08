using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;

namespace FestivalBot.Creatures
{
    public static class CreatureEmbedBuilder
    {
        public static DiscordEmbedBuilder Build(CreatureRecord creature, bool portuguese)
        {
            string subtitle = string.Join(" • ", new[]
            {
                string.IsNullOrWhiteSpace(creature.Level)
                    ? null
                    : (portuguese ? $"Nível {creature.Level}" : $"Level {creature.Level}"),
                creature.Arcana,
                creature.Type
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

            var embed = new DiscordEmbedBuilder()
                .WithTitle(Truncate(creature.Name, 256));

            if (!string.IsNullOrWhiteSpace(subtitle))
                embed.WithDescription(Truncate(subtitle, 4096));

            string stats =
                $"STR {ValueOrDash(creature.Strength)} | MAG {ValueOrDash(creature.Magic)} | " +
                $"TEC {ValueOrDash(creature.Technique)} | AGI {ValueOrDash(creature.Agility)} | " +
                $"VIT {ValueOrDash(creature.Vitality)} | LCK {ValueOrDash(creature.Luck)}";
            embed.AddField(portuguese ? "Atributos" : "Stats", stats);

            var overview = new List<string>();
            AddLine(overview, portuguese ? "Tipos" : "Types", creature.Types);
            AddLine(overview, portuguese ? "Disposição" : "Disposition", creature.Disposition);
            AddLine(overview, portuguese ? "Recompensa" : "Reward", creature.Reward);
            AddLine(overview, portuguese ? "Bônus" : "Stat bonuses", creature.StatBonuses);
            AddLine(overview, portuguese ? "Página" : "Page", creature.Page);
            if (overview.Count > 0)
                embed.AddField(portuguese ? "Informações" : "Information",
                    Truncate(string.Join("\n", overview), 1024));

            var affinities = new List<string>();
            AddLine(affinities, "Drain", creature.Drain);
            AddLine(affinities, "Reflect", creature.Reflect);
            AddLine(affinities, "Null", creature.Null);
            AddLine(affinities, "Resist", creature.Resist);
            AddLine(affinities, "Evade", string.Join(", ", new[]
            {
                creature.Evade1, creature.Evade2, creature.Evade3
            }.Where(value => !string.IsNullOrWhiteSpace(value))));
            AddLine(affinities, "Weak", creature.Weak);
            if (affinities.Count > 0)
                embed.AddField(portuguese ? "Afinidades" : "Affinities",
                    Truncate(string.Join("\n", affinities), 1024));

            AddField(embed, portuguese ? "Habilidade natural" : "Natural skill",
                creature.NaturalSkill, 750);
            AddField(embed, portuguese ? "Habilidades ativas" : "Active abilities",
                creature.ActiveAbilities, 1000);
            AddField(embed, portuguese ? "Habilidades passivas" : "Passive abilities",
                creature.PassiveAbilities, 1000);

            return embed;
        }

        private static void AddLine(List<string> lines, string label, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                lines.Add($"**{label}:** {value.Trim()}");
        }

        private static void AddField(
            DiscordEmbedBuilder embed, string title, string value, int maximumLength)
        {
            if (!string.IsNullOrWhiteSpace(value))
                embed.AddField(title, Truncate(value.Trim(), maximumLength));
        }

        private static string ValueOrDash(string value) =>
            string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

        private static string Truncate(string value, int maximumLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maximumLength)
                return value;

            return value.Substring(0, maximumLength - 1).TrimEnd() + "…";
        }
    }
}
