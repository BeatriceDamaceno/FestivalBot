using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FestivalBot.Armors
{
    public sealed class ArmorRecord
    {
        internal ArmorRecord(string name, string damageReduction, string bonus)
        {
            Name = name.Trim();
            DamageReduction = damageReduction.Trim();
            Bonus = bonus.Trim();
        }

        public string Name { get; }
        public string DamageReduction { get; }
        public string Bonus { get; }
    }

    public sealed class ArmorCatalog
    {
        private readonly IReadOnlyList<ArmorRecord> _armors;
        private readonly Dictionary<string, ArmorRecord> _byName;

        private ArmorCatalog(IReadOnlyList<ArmorRecord> armors)
        {
            _armors = armors;
            _byName = armors
                .GroupBy(armor => Normalize(armor.Name))
                .ToDictionary(group => group.Key, group => group.First());
        }

        public int Count => _armors.Count;

        public static ArmorCatalog Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Armor data file was not found.", path);

            List<List<string>> rows =
                ParseCsv(File.ReadAllText(path, Encoding.UTF8));
            if (rows.Count == 0)
                throw new InvalidDataException("Armors.csv is empty.");

            string[] headers = rows[0]
                .Select(value => value.Trim().TrimStart('\uFEFF'))
                .ToArray();
            int nameIndex = FindHeader(headers, "name");
            int reductionIndex = FindHeader(headers, "damage reduction");
            int bonusIndex = FindHeader(headers, "bonus");
            if (nameIndex < 0 || reductionIndex < 0 || bonusIndex < 0)
            {
                throw new InvalidDataException(
                    "Armors.csv must contain name, damage reduction, and bonus columns.");
            }

            var armors = new List<ArmorRecord>();
            foreach (List<string> row in rows.Skip(1))
            {
                string name = GetValue(row, nameIndex);
                if (string.IsNullOrWhiteSpace(name) || name.Trim() == "--")
                    continue;

                armors.Add(new ArmorRecord(
                    name,
                    GetValue(row, reductionIndex),
                    GetValue(row, bonusIndex)));
            }

            return new ArmorCatalog(armors);
        }

        public ArmorRecord FindExact(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            _byName.TryGetValue(Normalize(name), out ArmorRecord armor);
            return armor;
        }

        public IReadOnlyList<ArmorRecord> Suggest(string name, int limit = 3)
        {
            string query = Normalize(name ?? "");
            if (query.Length == 0 || limit <= 0)
                return Array.Empty<ArmorRecord>();

            int maximumDistance = Math.Max(2, query.Length / 3);
            return Rank(query)
                .Where(match => match.Category < 3
                    || match.Distance <= maximumDistance)
                .Take(limit)
                .Select(match => match.Armor)
                .ToArray();
        }

        public IReadOnlyList<ArmorRecord> Autocomplete(
            string name, int limit = 25)
        {
            string query = Normalize(name ?? "");
            if (limit <= 0)
                return Array.Empty<ArmorRecord>();

            if (query.Length == 0)
                return _armors.OrderBy(armor => armor.Name).Take(limit).ToArray();

            return Rank(query)
                .Take(limit)
                .Select(match => match.Armor)
                .ToArray();
        }

        private IEnumerable<ArmorMatch> Rank(string query) =>
            _armors
                .Select(armor =>
                {
                    string candidate = Normalize(armor.Name);
                    int category = candidate.StartsWith(query, StringComparison.Ordinal)
                        ? 0
                        : candidate.Contains(query, StringComparison.Ordinal)
                            ? 1
                            : query.Contains(candidate, StringComparison.Ordinal) ? 2 : 3;
                    return new ArmorMatch(
                        armor, category, LevenshteinDistance(query, candidate));
                })
                .OrderBy(match => match.Category)
                .ThenBy(match => match.Distance)
                .ThenBy(match => match.Armor.Name);

        private static int FindHeader(string[] headers, string name) =>
            Array.FindIndex(headers,
                header => header.Equals(name, StringComparison.OrdinalIgnoreCase));

        private static string GetValue(List<string> row, int index) =>
            index < row.Count ? row[index] : "";

        private sealed class ArmorMatch
        {
            public ArmorMatch(ArmorRecord armor, int category, int distance)
            {
                Armor = armor;
                Category = category;
                Distance = distance;
            }

            public ArmorRecord Armor { get; }
            public int Category { get; }
            public int Distance { get; }
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
                            inQuotes = false;
                    }
                    else
                        field.Append(current);
                    continue;
                }

                if (current == '"')
                    inQuotes = true;
                else if (current == ',')
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if (current == '\r' || current == '\n')
                {
                    if (current == '\r' && index + 1 < text.Length
                        && text[index + 1] == '\n')
                        index++;
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = new List<string>();
                }
                else
                    field.Append(current);
            }

            if (inQuotes)
                throw new InvalidDataException(
                    "Armors.csv contains an unterminated quoted field.");

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
                if (CharUnicodeInfo.GetUnicodeCategory(character)
                    == UnicodeCategory.NonSpacingMark)
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
                    int cost =
                        left[leftIndex - 1] == right[rightIndex - 1] ? 0 : 1;
                    current[rightIndex] = Math.Min(
                        Math.Min(current[rightIndex - 1] + 1,
                            previous[rightIndex] + 1),
                        previous[rightIndex - 1] + cost);
                }

                (previous, current) = (current, previous);
            }

            return previous[right.Length];
        }
    }
}
