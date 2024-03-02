using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.Extensions.Configuration;
using TechLanchesLambda.Utils;

namespace TechLanchesLambda.Service
{
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

        public async Task<bool> SignUp(string cpf)
        {
            try
            {
                var adminUser = new AdminGetUserRequest()
                {
                    Username = cpf,
                    UserPoolId = _awsOptions!.UserPoolId
                };

                var user = await _client.AdminGetUserAsync(adminUser);
                return true;
            }
            catch(Exception ex) 
            {
                var input = new SignUpRequest
                {
                    ClientId = _awsOptions.UserPoolClientId,
                    Username = cpf,
                    Password = _awsOptions.PasswordDefault,
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType {Name = "email", Value = _awsOptions.EmailDefault }
                    }
                };

                var signUpResponse = await _client.SignUpAsync(input);

                // Optionally, you can auto-confirm the user
                if (signUpResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var confirmRequest = new AdminConfirmSignUpRequest
                    {
                        Username = cpf,
                        UserPoolId = _awsOptions.UserPoolId
                    };

                    await _client.AdminConfirmSignUpAsync(confirmRequest);
                    return true;
                }

                return false;
            }
        }

        public async Task<string> SignIn(string cpf)
        {
            using (var provider = _provider)
            {
                var userPool = new CognitoUserPool(_awsOptions.UserPoolId, _awsOptions.UserPoolClientId, provider);
                var user = new CognitoUser(cpf, _awsOptions.UserPoolClientId, userPool, provider);

                var authRequest = new InitiateAdminNoSrpAuthRequest
                {
                    Password = _awsOptions.PasswordDefault
                };

                var authResponse = await user.StartWithAdminNoSrpAuthAsync(authRequest);

                if (authResponse.AuthenticationResult != null)
                    return authResponse.AuthenticationResult.AccessToken;
            }

            return "An error occurred while signing in.";
        }
    }
}