using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FestivalBot.Consumables
{
    public sealed class ConsumableRecord
    {
        internal ConsumableRecord(string[] row)
        {
            Name = Get(row, 0);
            Effect = Get(row, 1);
            Price = Get(row, 2);
            Rarity = Get(row, 3);
        }

        public string Name { get; }
        public string Effect { get; }
        public string Price { get; }
        public string Rarity { get; }

        private static string Get(string[] row, int index) =>
            index < row.Length ? row[index]?.Trim() ?? "" : "";
    }

    public sealed class ConsumableCatalog
    {
        private readonly IReadOnlyList<ConsumableRecord> _items;
        private readonly Dictionary<string, ConsumableRecord> _byName;

        private ConsumableCatalog(IReadOnlyList<ConsumableRecord> items)
        {
            _items = items;
            _byName = items
                .GroupBy(item => Normalize(item.Name))
                .ToDictionary(group => group.Key, group => group.First());
        }

        public int Count => _items.Count;

        public static ConsumableCatalog Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    "Consumable data file was not found.", path);

            using var parser = new TextFieldParser(path, Encoding.UTF8);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            string[] headers = parser.ReadFields();
            if (headers == null || headers.Length < 3
                || !headers[0].Trim().Equals(
                    "name", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Consumables.csv must begin with name, effect, and price columns.");
            }

            var items = new List<ConsumableRecord>();
            while (!parser.EndOfData)
            {
                var item =
                    new ConsumableRecord(parser.ReadFields() ?? Array.Empty<string>());
                if (!string.IsNullOrWhiteSpace(item.Name) && item.Name != "--")
                    items.Add(item);
            }

            return new ConsumableCatalog(items);
        }

        public ConsumableRecord FindExact(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            _byName.TryGetValue(Normalize(name), out ConsumableRecord item);
            return item;
        }

        public IReadOnlyList<ConsumableRecord> Suggest(
            string name, int limit = 3)
        {
            string query = Normalize(name ?? "");
            if (query.Length == 0 || limit <= 0)
                return Array.Empty<ConsumableRecord>();

            int maximumDistance = Math.Max(2, query.Length / 3);
            return Rank(query)
                .Where(match => match.Category < 3
                    || match.Distance <= maximumDistance)
                .Take(limit)
                .Select(match => match.Item)
                .ToArray();
        }

        public IReadOnlyList<ConsumableRecord> Autocomplete(
            string name, int limit = 25)
        {
            string query = Normalize(name ?? "");
            if (limit <= 0)
                return Array.Empty<ConsumableRecord>();
            if (query.Length == 0)
                return _items.OrderBy(item => item.Name).Take(limit).ToArray();

            return Rank(query)
                .Take(limit)
                .Select(match => match.Item)
                .ToArray();
        }

        private IEnumerable<ConsumableMatch> Rank(string query) =>
            _items
                .Select(item =>
                {
                    string candidate = Normalize(item.Name);
                    int category = candidate.StartsWith(query, StringComparison.Ordinal)
                        ? 0
                        : candidate.Contains(query, StringComparison.Ordinal)
                            ? 1
                            : query.Contains(candidate, StringComparison.Ordinal) ? 2 : 3;
                    return new ConsumableMatch(
                        item, category, LevenshteinDistance(query, candidate));
                })
                .OrderBy(match => match.Category)
                .ThenBy(match => match.Distance)
                .ThenBy(match => match.Item.Name);

        private sealed class ConsumableMatch
        {
            public ConsumableMatch(
                ConsumableRecord item, int category, int distance)
            {
                Item = item;
                Category = category;
                Distance = distance;
            }

            public ConsumableRecord Item { get; }
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
