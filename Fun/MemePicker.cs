using System;
using System.IO;
using System.Linq;

namespace FestivalBot.Fun
{
    public static class MemePicker
    {
        public static string GetRandomPath()
        {
            string memesDirectory = Path.Combine(AppContext.BaseDirectory, "memes");
            if (!Directory.Exists(memesDirectory))
                memesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "memes");

            if (!Directory.Exists(memesDirectory))
                return null;

            string[] extensions =
                { ".gif", ".png", ".jpg", ".jpeg", ".jfif", ".webp" };
            string[] files = Directory.GetFiles(memesDirectory)
                .Where(file =>
                    extensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .ToArray();

            return files.Length == 0 ? null : files[new Random().Next(files.Length)];
        }
    }
}
