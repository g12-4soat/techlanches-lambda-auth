using System.Security.Cryptography;
using System.Text;

namespace TechLanchesLambda.Utils;

public static class EncryptionHelper
{
    public static string EncryptPassword(string userName)
    {
        using MD5 md5Hash = MD5.Create();
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(userName));
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
}
