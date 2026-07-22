using System;
using System.Collections.Generic;

public static class FrostResponsesPT
{
    private static readonly Dictionary<int, string> Responses = new()
    {
        {1, "Ora, ora, se não é o idiota com amigos demais!"},
        {2, "Não conte com isso!"},
        {3, "O Rei Frost disse 'Não!'..."},
        {4, "[Jack Frost desvia o olhar, com nojo]"},
        {5, "Não!"},
        {6, "... Está tão nebuloso... Pergunte de novo! Hee hoo." },
        {7, "[Ele parece estar dormindo... Talvez tente novamente mais tarde.]" },
        {8, "Não vou te contar! Hee hoo!" },
        {9, "Me dê um pouco de Macca primeiro! Hoo!" },
        {10, "Tente hoo novamente." },
        {11, "Isso é certo, eu hee garanto!" },
        {12, "Com hoo certeza!" },
        {13, "Sem hee dúvida alguma!" },
        {14, "Sim, com certeza. Hee." },
        {15, "Provavelmente. Hoo" },
        {16, "FÚRIA PSICÓTICA! HEEE!" },
        {17,  "Sim. Hee." },
        {18, "Parece bom!. Hoo" },
        {19, "Pelo que vejo, sim. Hee." },
        {20, "Muito provável! HEE!"},
        {21, "Eu sei onde você mora {author}. Hee."},
        {22, "Hee. Eu faço as perguntas aqui {author}. Hoo."}
    };

    public static string GetResponsePT(int ans, bool isPremium, string author)
    {
        if (ans == 21)
        {
            return isPremium
                ? "*O que você disser, chefe!* (Esta foi uma resposta FROSTBOT GOLD (tm)!)"
                : "Esta é uma resposta FROSTBOT GOLD (tm)! Apenas para apoiadores!";
        }
        Console.WriteLine(author);
        return Responses.TryGetValue(ans, out var response)
            ? response.Replace("{author}", author)
            : "?! Algo está errado. [Festival Frost encontrou um erro]";
    }
}