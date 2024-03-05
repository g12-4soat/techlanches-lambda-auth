using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.Extensions.Configuration;
using TechLanchesLambda.Utils;

namespace TechLanchesLambda.Service;

public interface ICognitoService
{
    Task<Resultado> SignUp(string userName);
    Task<Resultado<string>> SignIn(string userName);
}

public class CognitoService : ICognitoService
{
    private readonly Options.AWSOptions _awsOptions;
    private readonly AmazonCognitoIdentityProviderClient _client;
    private readonly AmazonCognitoIdentityProviderClient _provider;
  
    public CognitoService(IConfiguration configuration)
    {
        var awsOptions = configuration.GetSection("AWS")
            .Get<Options.AWSOptions>();

        ArgumentNullException.ThrowIfNull(awsOptions);
        _awsOptions = awsOptions;

        _provider = new AmazonCognitoIdentityProviderClient(RegionEndpoint.GetBySystemName(_awsOptions.Region));
        _client = new AmazonCognitoIdentityProviderClient();
    }

    public async Task<Resultado> SignUp(string userName)
    {
        try
        {
            var adminUser = new AdminGetUserRequest()
            {
                Username = userName,
                UserPoolId = _awsOptions!.UserPoolId
            };

            var user = await _client.AdminGetUserAsync(adminUser);
            return Resultado.Ok();
        }
        catch
        {
            var input = new SignUpRequest
            {
                ClientId = _awsOptions.UserPoolClientId,
                Username = userName,
                Password = _awsOptions.PasswordDefault,
                UserAttributes = new List<AttributeType>
                {
                    new AttributeType {Name = "email", Value = _awsOptions.EmailDefault }
                }
            };

            var signUpResponse = await _client.SignUpAsync(input);

            if (signUpResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                return Resultado.Falha("Houve algo de errado ao cadastrar o usuário");

            var confirmRequest = new AdminConfirmSignUpRequest
            {
                Username = userName,
                UserPoolId = _awsOptions.UserPoolId
            };

            await _client.AdminConfirmSignUpAsync(confirmRequest);
            return Resultado.Ok();
        }
    }

    public async Task<Resultado<string>> SignIn(string userName)
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
            return Resultado.Ok(authResponse.AuthenticationResult.AccessToken);

        return Resultado.Falha<string>("Ocorreu um erro ao fazer login.");
    }
}