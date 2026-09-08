using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FestivalBot.Weapons
{
    public sealed class WeaponRecord
    {
        internal WeaponRecord(IReadOnlyDictionary<string, string> values)
        {
            Name = Get(values, "name");
            Damage = Get(values, "damage");
            Reach = Get(values, "reach");
            Extra = Get(values, "extra");
            Type = Get(values, "type");
            Price = Get(values, "price");
        }

        public string Name { get; }
        public string Damage { get; }
        public string Reach { get; }
        public string Extra { get; }
        public string Type { get; }
        public string Price { get; }

        private static string Get(
            IReadOnlyDictionary<string, string> values, string column) =>
            values.TryGetValue(column, out string value)
                ? value?.Trim() ?? ""
                : "";
    }

    public sealed class WeaponCatalog
    {
        private readonly IReadOnlyList<WeaponRecord> _weapons;
        private readonly Dictionary<string, WeaponRecord> _byName;

        private WeaponCatalog(IReadOnlyList<WeaponRecord> weapons)
        {
            _weapons = weapons;
            _byName = weapons
                .GroupBy(weapon => Normalize(weapon.Name))
                .ToDictionary(group => group.Key, group => group.First());
        }

        public int Count => _weapons.Count;

        public static WeaponCatalog Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Weapon data file was not found.", path);

            using var parser = new TextFieldParser(path, Encoding.UTF8);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            string[] headers = parser.ReadFields()
                ?.Select(header => header.Trim().TrimStart('\uFEFF'))
                .ToArray();
            if (headers == null
                || !headers.Contains("name", StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Weapons.csv must contain a name column.");
            }

            var weapons = new List<WeaponRecord>();
            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields() ?? Array.Empty<string>();
                var values =
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < headers.Length; index++)
                    values[headers[index]] = index < row.Length ? row[index] : "";

                var weapon = new WeaponRecord(values);
                if (!string.IsNullOrWhiteSpace(weapon.Name)
                    && weapon.Name != "--")
                    weapons.Add(weapon);
            }

            return new WeaponCatalog(weapons);
        }

        public WeaponRecord FindExact(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            _byName.TryGetValue(Normalize(name), out WeaponRecord weapon);
            return weapon;
        }

        public IReadOnlyList<WeaponRecord> Suggest(string name, int limit = 3)
        {
            string query = Normalize(name ?? "");
            if (query.Length == 0 || limit <= 0)
                return Array.Empty<WeaponRecord>();

            int maximumDistance = Math.Max(2, query.Length / 3);
            return Rank(query)
                .Where(match => match.Category < 3
                    || match.Distance <= maximumDistance)
                .Take(limit)
                .Select(match => match.Weapon)
                .ToArray();
        }

        public IReadOnlyList<WeaponRecord> Autocomplete(
            string name, int limit = 25)
        {
            string query = Normalize(name ?? "");
            if (limit <= 0)
                return Array.Empty<WeaponRecord>();
            if (query.Length == 0)
                return _weapons.OrderBy(weapon => weapon.Name).Take(limit).ToArray();

            return Rank(query)
                .Take(limit)
                .Select(match => match.Weapon)
                .ToArray();
        }

        private IEnumerable<WeaponMatch> Rank(string query) =>
            _weapons
                .Select(weapon =>
                {
                    string candidate = Normalize(weapon.Name);
                    int category = candidate.StartsWith(query, StringComparison.Ordinal)
                        ? 0
                        : candidate.Contains(query, StringComparison.Ordinal)
                            ? 1
                            : query.Contains(candidate, StringComparison.Ordinal) ? 2 : 3;
                    return new WeaponMatch(
                        weapon, category, LevenshteinDistance(query, candidate));
                })
                .OrderBy(match => match.Category)
                .ThenBy(match => match.Distance)
                .ThenBy(match => match.Weapon.Name);

        private sealed class WeaponMatch
        {
            public WeaponMatch(WeaponRecord weapon, int category, int distance)
            {
                Weapon = weapon;
                Category = category;
                Distance = distance;
            }

            public WeaponRecord Weapon { get; }
            public int Category { get; }
            public int Distance { get; }
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
