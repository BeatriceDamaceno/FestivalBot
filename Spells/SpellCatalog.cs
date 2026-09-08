using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FestivalBot.Spells
{
    public sealed class SpellRecord
    {
        private readonly IReadOnlyDictionary<string, string> _values;

        internal SpellRecord(IReadOnlyDictionary<string, string> values)
        {
            _values = values;
        }

        public string Name => Get("name");
        public string Tier => Get("tier");
        public string Categories => Get("categories");
        public string Type => Get("type");
        public string Time => Get("time");
        public string Reach => Get("reach");
        public string Duration => Get("duration");
        public string Effect => Get("effect");

        private string Get(string column) =>
            _values.TryGetValue(column, out string value)
                ? value?.Trim() ?? ""
                : "";
    }

    public sealed class SpellCatalog
    {
        private readonly IReadOnlyList<SpellRecord> _spells;
        private readonly Dictionary<string, SpellRecord> _byName;

        private SpellCatalog(IReadOnlyList<SpellRecord> spells)
        {
            _spells = spells;
            _byName = spells
                .Where(spell => !string.IsNullOrWhiteSpace(spell.Name))
                .GroupBy(spell => Normalize(spell.Name))
                .ToDictionary(group => group.Key, group => group.First());
        }

        public int Count => _spells.Count;

        public static SpellCatalog Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Spell data file was not found.", path);

            List<List<string>> rows =
                ParseCsv(File.ReadAllText(path, Encoding.UTF8));
            if (rows.Count == 0)
                throw new InvalidDataException("Spells.csv is empty.");

            string[] headers = rows[0]
                .Select(value => value.Trim().TrimStart('\uFEFF'))
                .ToArray();
            int nameIndex = Array.FindIndex(headers,
                header => header.Equals("name", StringComparison.OrdinalIgnoreCase));
            if (nameIndex < 0)
                throw new InvalidDataException("Spells.csv must contain a name column.");

            var spells = new List<SpellRecord>();
            foreach (List<string> row in rows.Skip(1))
            {
                if (row.All(string.IsNullOrWhiteSpace))
                    continue;

                var values =
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < headers.Length; index++)
                    values[headers[index]] = index < row.Count ? row[index] : "";

                if (!string.IsNullOrWhiteSpace(values[headers[nameIndex]]))
                    spells.Add(new SpellRecord(values));
            }

            return new SpellCatalog(spells);
        }

        public SpellRecord FindExact(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            _byName.TryGetValue(Normalize(name), out SpellRecord spell);
            return spell;
        }

        public IReadOnlyList<SpellRecord> Suggest(string name, int limit = 3)
        {
            string query = Normalize(name ?? "");
            if (query.Length == 0 || limit <= 0)
                return Array.Empty<SpellRecord>();

            int maximumDistance = Math.Max(2, query.Length / 3);
            return Rank(query)
                .Where(match => match.Category < 3
                    || match.Distance <= maximumDistance)
                .Take(limit)
                .Select(match => match.Spell)
                .ToArray();
        }

        public IReadOnlyList<SpellRecord> Autocomplete(
            string name, int limit = 25)
        {
            string query = Normalize(name ?? "");
            if (limit <= 0)
                return Array.Empty<SpellRecord>();

            if (query.Length == 0)
            {
                return _spells
                    .OrderBy(spell => spell.Name)
                    .Take(limit)
                    .ToArray();
            }

            return Rank(query)
                .Take(limit)
                .Select(match => match.Spell)
                .ToArray();
        }

        private IEnumerable<SpellMatch> Rank(string query) =>
            _spells
                .Where(spell => !string.IsNullOrWhiteSpace(spell.Name))
                .Select(spell =>
                {
                    string candidate = Normalize(spell.Name);
                    int category = candidate.StartsWith(query, StringComparison.Ordinal)
                        ? 0
                        : candidate.Contains(query, StringComparison.Ordinal)
                            ? 1
                            : query.Contains(candidate, StringComparison.Ordinal) ? 2 : 3;
                    return new SpellMatch(
                        spell, category, LevenshteinDistance(query, candidate));
                })
                .OrderBy(match => match.Category)
                .ThenBy(match => match.Distance)
                .ThenBy(match => match.Spell.Name);

        private sealed class SpellMatch
        {
            public SpellMatch(SpellRecord spell, int category, int distance)
            {
                Spell = spell;
                Category = category;
                Distance = distance;
            }

            public SpellRecord Spell { get; }
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
                    "Spells.csv contains an unterminated quoted field.");

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
