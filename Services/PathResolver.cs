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
            string outputPath =
                Path.Combine(AppContext.BaseDirectory, "Creatures.csv");
            if (File.Exists(outputPath))
                return outputPath;

            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "Creatures.csv");
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "Creatures.csv");
        }
    }
}
