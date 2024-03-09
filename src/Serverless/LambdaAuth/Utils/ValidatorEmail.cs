using System.Text.RegularExpressions;

namespace TechLanchesLambda.Utils;

public static class ValidatorEmail
{
    public static bool Validar(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
