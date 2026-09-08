using System;
using System.IO;

namespace FestivalBot.Services
{
    public static class PathResolver
    {
        public static string ResolveDatabasePath()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "GrimoireOTH");
                string project = Path.Combine(directory.FullName, "FestivalBot.csproj");
                if (File.Exists(candidate) && File.Exists(project))
                    return candidate;

                directory = directory.Parent;
            }

            directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "GrimoireOTH");
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "GrimoireOTH");
        }

        public static string ResolveCreaturesPath()
        {
            return ResolveDataFile("Creatures.csv");
        }

        public static string ResolveSpellsPath()
        {
            return ResolveDataFile("Spells.csv");
        }

        public static string ResolveArmorsPath()
        {
            return ResolveDataFile("Armors.csv");
        }

        public static string ResolveWeaponsPath()
        {
            return ResolveDataFile("Weapons.csv");
        }

        public static string ResolveAccessoriesPath()
        {
            return ResolveDataFile("Accessories.csv");
        }

        public static string ResolveConsumablesPath()
        {
            return ResolveDataFile("Consumables.csv");
        }

        public static string ResolveFeatsPath()
        {
            return ResolveDataFile("Feats.csv");
        }

        private static string ResolveDataFile(string fileName)
        {
            string outputPath =
                Path.Combine(AppContext.BaseDirectory, fileName);
            if (File.Exists(outputPath))
                return outputPath;

            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, fileName);
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }
    }
}
