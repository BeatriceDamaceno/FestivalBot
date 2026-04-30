using System.Collections.Generic;

public static class FrostResponsesEN
{
    private static readonly Dictionary<int, string> Responses = new()
    {
        {1, "Well, if it isn't the dumbass with too many friends!"},
        {2, "Hoo! Don't count on hee-it!"},
        {3, "King Hoo-Frost said 'No-ho!'..."},
        {4, "[Jack Frost looks away, disgusted]"},
        {5, "Hee-no!"},
        {6, "Hee... It's so foggy... Ask again, ho!" },
        {7, "[They seem asleep... Perhaps try again later.]" },
        {8, "HEE! I'm not telling!" },
        {9, "Give me some Macca first, ho!" },
        {10, "Try again, hee." },
        {11, "That's certain, I guarant-hee it!" },
        {12, "Hee! Decidedly so!" },
        {13, "No doubt about it, hoo!" },
        {14, "Yes, for sur-hee" },
        {15, "Probabl-hee." },
        {16, "PSYCHO RAGE" },
        {17,  "Yes. Hee." },
        {18, "Loo-hoo-king good, hee!" },
        {19, "As I see-hee it, yes." },
        {20, "Most likely, hoo!"}
    };

    public static string GetResponseEN(int ans, bool isPremium)
    {
        if (ans == 21)
        {
            return isPremium
                ? "*What-hee-ver you say, boss!* (This was a FROSTBOT GOLD (tm) Answer!)"
                : "This is a FROSTBOT GOLD (tm) Answer, hee! Patrons Only!";
        }

        return Responses.TryGetValue(ans, out var response)
            ? response
            : "Hee?! Something's wrong. [Festival Frost encountered an error]";
    }
}