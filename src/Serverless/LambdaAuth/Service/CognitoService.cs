using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.Extensions.Options;
using TechLanchesLambda.Utils;
using static TechLanchesLambda.Service.CognitoService;

namespace TechLanchesLambda.Service;

public interface ICognitoService
{
    Task<Resultado> SignUp(User user);
    Task<Resultado<TokenResult>> SignIn(string userName);
}

public class User
{
    public string Cpf { get; set; }
    public string Email { get; set; }
    public string Nome { get; set; }

    public bool Validar()
    {
        if (string.IsNullOrEmpty(Cpf) || string.IsNullOrWhiteSpace(Cpf))
            return false;
        if (string.IsNullOrEmpty(Email) || string.IsNullOrWhiteSpace(Email))
            return false;
        if (string.IsNullOrEmpty(Nome) || string.IsNullOrWhiteSpace(Nome))
            return false;
        return true;
    }
}

public class CognitoService : ICognitoService
{
    private readonly AWS.Options.AWSOptions _awsOptions;
    private readonly AmazonCognitoIdentityProviderClient _client;
    private readonly AmazonCognitoIdentityProviderClient _provider;
  
    public CognitoService(IOptions<AWS.Options.AWSOptions> awsOptions)
    {
        ArgumentNullException.ThrowIfNull(awsOptions);
        _awsOptions = awsOptions.Value;

        _provider = new AmazonCognitoIdentityProviderClient(RegionEndpoint.GetBySystemName(_awsOptions.Region));
        _client = new AmazonCognitoIdentityProviderClient();
    }

    public async Task<Resultado> SignUp(User user)
    {
        try
        {
            var adminUser = new AdminGetUserRequest()
            {
                Username = user.Cpf,
                UserPoolId = _awsOptions!.UserPoolId
            };

            var userCognito = await _client.AdminGetUserAsync(adminUser);
            if (user.Cpf.Equals(_awsOptions.UserTechLanches))
            {
                return Resultado.Ok();
            }
            return Resultado.Falha("Usuário já cadastrado. Por favor tente autenticar");
        }
        catch
        {
            var input = new SignUpRequest
            {
                ClientId = _awsOptions.UserPoolClientId,
                Username = user.Cpf,
                Password = _awsOptions.PasswordDefault,
                UserAttributes = new List<AttributeType>
                {
                    new AttributeType {Name = "email", Value = user.Email },
                    new AttributeType {Name = "name", Value = user.Nome }
                }
            };

            var signUpResponse = await _client.SignUpAsync(input);

            if (signUpResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                return Resultado.Falha("Houve algo de errado ao cadastrar o usuário");

            var confirmRequest = new AdminConfirmSignUpRequest
            {
                Username = user.Cpf,
                UserPoolId = _awsOptions.UserPoolId
            };

            await _client.AdminConfirmSignUpAsync(confirmRequest);
            return Resultado.Ok();
        }
    }

    public async Task<Resultado<TokenResult>> SignIn(string userName)
    {
        using var provider = _provider;
        var userPool = new CognitoUserPool(_awsOptions.UserPoolId, _awsOptions.UserPoolClientId, provider);
        var user = new CognitoUser(userName, _awsOptions.UserPoolClientId, userPool, provider);

        var authRequest = new InitiateAdminNoSrpAuthRequest
        {
            Password = _awsOptions.PasswordDefault
        };

        var authResponse = await user.StartWithAdminNoSrpAuthAsync(authRequest);

        if (authResponse.AuthenticationResult != null)
            return Resultado.Ok(new TokenResult { AccessToken = authResponse.AuthenticationResult.AccessToken, TokenId = authResponse.AuthenticationResult.IdToken });

        return Resultado.Falha<TokenResult>("Ocorreu um erro ao fazer login.");
    }

    public class TokenResult
    {
        public string TokenId { get; set; }
        public string AccessToken { get; set; }

    }
}