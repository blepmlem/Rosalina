namespace Rosalina.Extensions;

internal static class StringExtensions
{
    public static string ToPascalCase(this string original)
    {
        string newString = string.Empty;
        bool makeNextCharacterUpper = false;
        for (int index = 0; index < original.Length; index++)
        {
            char c = original[index];
            if(index == 0)
                newString += $"{char.ToUpper(c)}";
            else if (makeNextCharacterUpper)
            {
                newString += $"{char.ToUpper(c)}";
                makeNextCharacterUpper = false;
            }
            else if (char.IsUpper(c))
                newString += $" {c}";
            else if (char.IsLower(c) || char.IsNumber(c))
                newString += c;
            else if (char.IsNumber(c))
                newString += $"{c}";
            else
            {
                makeNextCharacterUpper = true;   
                newString += ' ';
            }
        }

        return newString.TrimStart().Replace(" ", "");
    }
}