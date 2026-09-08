using Emzi0767;

namespace FestivalBot.Fun
{
    public static class HeeResponder
    {
        public static string BuildResponse(string fullMessage)
        {
            int heeBefore = fullMessage.ToLower().IndexOf("hee");
            int heeAfter = heeBefore + 2;
            string fullWord = "*hee*";
            bool isLetter = true;

            while (isLetter)
            {
                if (heeBefore - 1 >= 0)
                {
                    heeBefore--;
                    if (fullMessage[heeBefore].IsBasicLetter())
                        fullWord = fullMessage.Substring(heeBefore, 1) + fullWord;
                    else
                        isLetter = false;
                }
                else
                {
                    isLetter = false;
                }
            }

            isLetter = true;
            while (isLetter)
            {
                if (heeAfter + 1 < fullMessage.Length)
                {
                    heeAfter++;
                    if (fullMessage[heeAfter].IsBasicLetter())
                        fullWord += fullMessage.Substring(heeAfter, 1);
                    else
                        isLetter = false;
                }
                else
                {
                    isLetter = false;
                }
            }

            return fullWord + ", hoo!";
        }
    }
}
