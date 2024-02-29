using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;

namespace TechLanchesLambda.Service
{
    public interface ICognitoService
    {
        Task<string> SignIn(string cpf);
        Task<bool> SignUp(string cpf);
    }
}