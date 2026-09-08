using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FestivalBot.Accessories
{
    public sealed class AccessoryRecord
    {
        internal AccessoryRecord(IReadOnlyDictionary<string, string> values)
        {
            Name = Get(values, "name");
            Effect = Get(values, "effect");
            Price = Get(values, "price");
            Rarity = Get(values, "Rarity");
        }

        public string Name { get; }
        public string Effect { get; }
        public string Price { get; }
        public string Rarity { get; }

        private static string Get(
            IReadOnlyDictionary<string, string> values, string column) =>
            values.TryGetValue(column, out string value)
                ? value?.Trim() ?? ""
                : "";
    }

    public sealed class AccessoryCatalog
    {
        private readonly IReadOnlyList<AccessoryRecord> _accessories;
        private readonly Dictionary<string, AccessoryRecord> _byName;

        private AccessoryCatalog(IReadOnlyList<AccessoryRecord> accessories)
        {
            _accessories = accessories;
            _byName = accessories
                .GroupBy(accessory => Normalize(accessory.Name))
                .ToDictionary(group => group.Key, group => group.First());
        }

        public int Count => _accessories.Count;

        public static AccessoryCatalog Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    "Accessory data file was not found.", path);

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
                    "Accessories.csv must contain a name column.");
            }

            var accessories = new List<AccessoryRecord>();
            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields() ?? Array.Empty<string>();
                var values =
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int index = 0; index < headers.Length; index++)
                    values[headers[index]] = index < row.Length ? row[index] : "";

                var accessory = new AccessoryRecord(values);
                if (!string.IsNullOrWhiteSpace(accessory.Name)
                    && accessory.Name != "--")
                    accessories.Add(accessory);
            }

            return new AccessoryCatalog(accessories);
        }

        public AccessoryRecord FindExact(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            _byName.TryGetValue(Normalize(name), out AccessoryRecord accessory);
            return accessory;
        }

        public IReadOnlyList<AccessoryRecord> Suggest(
            string name, int limit = 3)
        {
            string query = Normalize(name ?? "");
            if (query.Length == 0 || limit <= 0)
                return Array.Empty<AccessoryRecord>();

            int maximumDistance = Math.Max(2, query.Length / 3);
            return Rank(query)
                .Where(match => match.Category < 3
                    || match.Distance <= maximumDistance)
                .Take(limit)
                .Select(match => match.Accessory)
                .ToArray();
        }

        public IReadOnlyList<AccessoryRecord> Autocomplete(
            string name, int limit = 25)
        {
            string query = Normalize(name ?? "");
            if (limit <= 0)
                return Array.Empty<AccessoryRecord>();
            if (query.Length == 0)
            {
                return _accessories
                    .OrderBy(accessory => accessory.Name)
                    .Take(limit)
                    .ToArray();
            }

            return Rank(query)
                .Take(limit)
                .Select(match => match.Accessory)
                .ToArray();
        }

        private IEnumerable<AccessoryMatch> Rank(string query) =>
            _accessories
                .Select(accessory =>
                {
                    string candidate = Normalize(accessory.Name);
                    int category = candidate.StartsWith(query, StringComparison.Ordinal)
                        ? 0
                        : candidate.Contains(query, StringComparison.Ordinal)
                            ? 1
                            : query.Contains(candidate, StringComparison.Ordinal) ? 2 : 3;
                    return new AccessoryMatch(
                        accessory, category,
                        LevenshteinDistance(query, candidate));
                })
                .OrderBy(match => match.Category)
                .ThenBy(match => match.Distance)
                .ThenBy(match => match.Accessory.Name);

        private sealed class AccessoryMatch
        {
            public AccessoryMatch(
                AccessoryRecord accessory, int category, int distance)
            {
                Accessory = accessory;
                Category = category;
                Distance = distance;
            }

            public AccessoryRecord Accessory { get; }
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
