using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FestivalBot.Creatures
{
    public sealed class CreatureRecord
    {
        private readonly IReadOnlyDictionary<string, string> _values;

        internal CreatureRecord(IReadOnlyDictionary<string, string> values)
        {
            _values = values;
        }

        public string Name => Get("Persona");
        public string Level => Get("Lv.");
        public string Arcana => Get("Arcana");
        public string Types => Get("Types");
        public string Type => Get("Type");
        public string Drain => Get("Drain");
        public string Reflect => Get("Reflect");
        public string Null => Get("Null");
        public string Resist => Get("Resist");
        public string Evade1 => Get("Evade 1");
        public string Evade2 => Get("Evade 2");
        public string Evade3 => Get("Evade 3");
        public string Weak => Get("Weak");
        public string StatBonuses => Get("Stat Bonuses");
        public string NaturalSkill => Get("Natural Skill");
        public string ActiveAbilities => Get("Active Abilities");
        public string PassiveAbilities => Get("Passive Abilities");
        public string Disposition => Get("Disposition");
        public string Reward => Get("Reward");
        public string Page => Get("Pg.");
        public string Strength => Get("STR");
        public string Magic => Get("MAG");
        public string Technique => Get("TEC");
        public string Agility => Get("AGI");
        public string Vitality => Get("VIT");
        public string Luck => Get("LCK");

        public string Get(string column) =>
            _values.TryGetValue(column, out string value) ? value?.Trim() ?? "" : "";
    }

    public sealed class CreatureCatalog
    {
        private readonly IReadOnlyList<CreatureRecord> _creatures;
        private readonly Dictionary<string, CreatureRecord> _byName;

        private CreatureCatalog(IReadOnlyList<CreatureRecord> creatures)
        {
            _creatures = creatures;
            _byName = creatures
                .Where(creature => !string.IsNullOrWhiteSpace(creature.Name))
                .GroupBy(creature => Normalize(creature.Name))
                .ToDictionary(group => group.Key, group => group.First());
        }

        public int Count => _creatures.Count;

        public static CreatureCatalog Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Creature data file was not found.", path);

            List<List<string>> rows = ParseCsv(File.ReadAllText(path, Encoding.UTF8));
            if (rows.Count == 0)
                throw new InvalidDataException("Creatures.csv is empty.");

            string[] headers = rows[0].Select(value => value.Trim().TrimStart('\uFEFF')).ToArray();
            int nameIndex = Array.FindIndex(headers,
                header => header.Equals("Persona", StringComparison.OrdinalIgnoreCase));
            if (nameIndex < 0)
                throw new InvalidDataException("Creatures.csv must contain a Persona column.");

            var creatures = new List<CreatureRecord>();
            foreach (List<string> row in rows.Skip(1))
            {
                if (row.All(string.IsNullOrWhiteSpace))
                    continue;

                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < headers.Length; index++)
                    values[headers[index]] = index < row.Count ? row[index] : "";

                if (!string.IsNullOrWhiteSpace(values[headers[nameIndex]]))
                    creatures.Add(new CreatureRecord(values));
            }

            return new CreatureCatalog(creatures);
        }

        public CreatureRecord FindExact(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            _byName.TryGetValue(Normalize(name), out CreatureRecord creature);
            return creature;
        }

        public IReadOnlyList<CreatureRecord> Suggest(string name, int limit = 3)
        {
            string query = Normalize(name);
            if (query.Length == 0 || limit <= 0)
                return Array.Empty<CreatureRecord>();

            int maximumDistance = Math.Max(2, query.Length / 3);
            return _creatures
                .Where(creature => !string.IsNullOrWhiteSpace(creature.Name))
                .Select(creature =>
                {
                    string candidate = Normalize(creature.Name);
                    int category = candidate.StartsWith(query, StringComparison.Ordinal) ? 0
                        : candidate.Contains(query, StringComparison.Ordinal) ? 1
                        : query.Contains(candidate, StringComparison.Ordinal) ? 2
                        : 3;
                    int distance = LevenshteinDistance(query, candidate);
                    return new { Creature = creature, Category = category, Distance = distance };
                })
                .Where(match => match.Category < 3 || match.Distance <= maximumDistance)
                .OrderBy(match => match.Category)
                .ThenBy(match => match.Distance)
                .ThenBy(match => match.Creature.Name.Length)
                .Take(limit)
                .Select(match => match.Creature)
                .ToArray();
        }

        public IReadOnlyList<CreatureRecord> Autocomplete(
            string name, int limit = 25)
        {
            string query = Normalize(name ?? "");
            if (limit <= 0)
                return Array.Empty<CreatureRecord>();

            if (query.Length == 0)
            {
                return _creatures
                    .Where(creature => !string.IsNullOrWhiteSpace(creature.Name))
                    .OrderBy(creature => creature.Name)
                    .Take(limit)
                    .ToArray();
            }

            return _creatures
                .Where(creature => !string.IsNullOrWhiteSpace(creature.Name))
                .Select(creature =>
                {
                    string candidate = Normalize(creature.Name);
                    int category = candidate.StartsWith(query, StringComparison.Ordinal)
                        ? 0
                        : candidate.Contains(query, StringComparison.Ordinal) ? 1 : 2;
                    int distance = LevenshteinDistance(query, candidate);
                    return new { Creature = creature, Category = category, Distance = distance };
                })
                .OrderBy(match => match.Category)
                .ThenBy(match => match.Distance)
                .ThenBy(match => match.Creature.Name)
                .Take(limit)
                .Select(match => match.Creature)
                .ToArray();
        }

        private static List<List<string>> ParseCsv(string text)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;

            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                if (inQuotes)
                {
                    if (current == '"')
                    {
                        if (index + 1 < text.Length && text[index + 1] == '"')
                        {
                            field.Append('"');
                            index++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(current);
                    }

                    continue;
                }

                if (current == '"')
                {
                    inQuotes = true;
                }
                else if (current == ',')
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if (current == '\r' || current == '\n')
                {
                    if (current == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                        index++;

                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = new List<string>();
                }
                else
                {
                    field.Append(current);
                }
            }

            if (inQuotes)
                throw new InvalidDataException("Creatures.csv contains an unterminated quoted field.");

            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                rows.Add(row);
            }

            return rows;
        }

        private static string Normalize(string value)
        {
            string decomposed = value.Trim().Normalize(NormalizationForm.FormD);
            var result = new StringBuilder(decomposed.Length);
            bool previousWasSpace = false;

            foreach (char character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(character))
                {
                    result.Append(char.ToLowerInvariant(character));
                    previousWasSpace = false;
                }
                else if (!previousWasSpace && result.Length > 0)
                {
                    result.Append(' ');
                    previousWasSpace = true;
                }
            }

            return result.ToString().Trim();
        }

        private static int LevenshteinDistance(string left, string right)
        {
            int[] previous = Enumerable.Range(0, right.Length + 1).ToArray();
            int[] current = new int[right.Length + 1];

            for (int leftIndex = 1; leftIndex <= left.Length; leftIndex++)
            {
                current[0] = leftIndex;
                for (int rightIndex = 1; rightIndex <= right.Length; rightIndex++)
                {
                    int substitutionCost = left[leftIndex - 1] == right[rightIndex - 1] ? 0 : 1;
                    current[rightIndex] = Math.Min(
                        Math.Min(current[rightIndex - 1] + 1, previous[rightIndex] + 1),
                        previous[rightIndex - 1] + substitutionCost);
                }

                (previous, current) = (current, previous);
            }

            return previous[right.Length];
        }
    }
}
