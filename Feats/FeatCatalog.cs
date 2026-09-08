using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FestivalBot.Feats
{
    public sealed class FeatRecord
    {
        internal FeatRecord(string[] row)
        {
            Name = Get(row, 0);
            Effect = Get(row, 1);
            Requirements = Get(row, 2);
            Additional = Get(row, 3);
        }

        public string Name { get; }
        public string Effect { get; }
        public string Requirements { get; }
        public string Additional { get; }

        private static string Get(string[] row, int index) =>
            index < row.Length ? row[index]?.Trim() ?? "" : "";
    }

    public sealed class FeatCatalog
    {
        private readonly IReadOnlyList<FeatRecord> _feats;
        private readonly Dictionary<string, FeatRecord> _byName;

        private FeatCatalog(IReadOnlyList<FeatRecord> feats)
        {
            _feats = feats;
            _byName = feats
                .GroupBy(feat => Normalize(feat.Name))
                .ToDictionary(group => group.Key, group => group.First());
        }

        public int Count => _feats.Count;

        public static FeatCatalog Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Feat data file was not found.", path);

            using var parser = new TextFieldParser(path, Encoding.UTF8);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            string[] headers = parser.ReadFields();
            if (headers == null || headers.Length < 4
                || !headers[0].Trim().Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Feats.csv must begin with name, effect, Requirements, and Additional columns.");
            }

            var feats = new List<FeatRecord>();
            while (!parser.EndOfData)
            {
                var feat = new FeatRecord(parser.ReadFields() ?? Array.Empty<string>());
                if (!string.IsNullOrWhiteSpace(feat.Name) && feat.Name != "--")
                    feats.Add(feat);
            }

            return new FeatCatalog(feats);
        }

        public FeatRecord FindExact(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            _byName.TryGetValue(Normalize(name), out FeatRecord feat);
            return feat;
        }

        public IReadOnlyList<FeatRecord> Suggest(string name, int limit = 3)
        {
            string query = Normalize(name ?? "");
            if (query.Length == 0 || limit <= 0)
                return Array.Empty<FeatRecord>();

            int maximumDistance = Math.Max(2, query.Length / 3);
            return Rank(query)
                .Where(match => match.Category < 3
                    || match.Distance <= maximumDistance)
                .Take(limit)
                .Select(match => match.Feat)
                .ToArray();
        }

        public IReadOnlyList<FeatRecord> Autocomplete(string name, int limit = 25)
        {
            string query = Normalize(name ?? "");
            if (limit <= 0)
                return Array.Empty<FeatRecord>();
            if (query.Length == 0)
                return _feats.OrderBy(feat => feat.Name).Take(limit).ToArray();

            return Rank(query).Take(limit).Select(match => match.Feat).ToArray();
        }

        private IEnumerable<FeatMatch> Rank(string query) =>
            _feats
                .Select(feat =>
                {
                    string candidate = Normalize(feat.Name);
                    int category = candidate.StartsWith(query, StringComparison.Ordinal)
                        ? 0
                        : candidate.Contains(query, StringComparison.Ordinal)
                            ? 1
                            : query.Contains(candidate, StringComparison.Ordinal) ? 2 : 3;
                    return new FeatMatch(
                        feat, category, LevenshteinDistance(query, candidate));
                })
                .OrderBy(match => match.Category)
                .ThenBy(match => match.Distance)
                .ThenBy(match => match.Feat.Name);

        private sealed class FeatMatch
        {
            public FeatMatch(FeatRecord feat, int category, int distance)
            {
                Feat = feat;
                Category = category;
                Distance = distance;
            }

            public FeatRecord Feat { get; }
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
                    int cost = left[leftIndex - 1] == right[rightIndex - 1] ? 0 : 1;
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
