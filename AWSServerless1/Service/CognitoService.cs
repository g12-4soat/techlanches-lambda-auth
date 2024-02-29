using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Http;
using TechLanchesLambda.Utils;

namespace TechLanchesLambda.Service
{
    public class CognitoService : ICognitoService
    {
        //string region = Environment.GetEnvironmentVariable("region");
        //string userPasswordDefalut = Environment.GetEnvironmentVariable("PasswordDefault");
        //string emailDefault = Environment.GetEnvironmentVariable("EmailDefault");
        //string cognitoPoolId = Environment.GetEnvironmentVariable("UserPoolId");
        //string cognitoClientId = Environment.GetEnvironmentVariable("UserPoolClientId");

        const string region = "us-east-1";
        const string userPasswordDefalut = "Test1234@";
        const string emailDefault = "g12.4soat.fiap@outlook.com";
        const string cognitoPoolId = "us-east-1_FI9vWiND3";
        const string cognitoClientId = "50vqcv01qh0cflkthrvs1jal30";

        //private readonly UserManager<CognitoUser> _userManager;
        //private readonly SignInManager<CognitoUser> _signInManager;
        private readonly AmazonCognitoIdentityProviderClient _client;
        private readonly AmazonCognitoIdentityProviderClient _provider;

        public CognitoService()
        {
            //_userManager = userManager;
            //_signInManager = signInManager;
            _provider = new AmazonCognitoIdentityProviderClient(RegionEndpoint.GetBySystemName(region));
            _client = new AmazonCognitoIdentityProviderClient();
        }

        public async Task<bool> SignUp(string cpf)
        {
            #region
            //var credentials = new RefreshingAWSCredentials()
            //{
            //    PreemptExpiryTime
            //};

            //var accessKey = "ASIAYS2NQCBYDBXENQN2";
            //var secretKey = "HFlm5TcPe5MDgbTJTVHM+SNiaX/o2uv6Vb6bvaDz";
            //var tokenAws = "FwoGZXIvYXdzEAoaDMXhmn6zGCpXCLoBwSLOAa6CqVGzmIKWbqlgXmCrLfFi7KFKtmFoXoAM##t4o9wIYeoesvgMW5IjMaMkmmI8lyLHRMv0C9G1y+mH+XXFvQtTg85UnJLLSEwgCD+oiysLj4MyNyK+7DwlW71lA3qKP3NgPaHfGXdZw7oltpiia4AI4FHAGyT1SKmrqb+QgDb++N8QU5tx/QSuD2dpUzcEWuy4iwGdwEJR4P27qmfRvnkDi8IGNupLF9artQvpm/WK5erOdiWVM1u34TA0RkNPXFUZddjnnqv4cdsl4iv1dnKKT1+a4GMi35+lukv/r7jgdXyrRtrj2N+HHgKIHPhfpPNapPwqw60ltpp7jIdU1ZuFVGEjM=";

            ////AccessKey = awsAccessKeyId;
            ////SecretKey = awsSecretAccessKey;
            ////Token = token ?? string.Empty;


            //var xpto = new ImmutableCredentials(accessKey, secretKey, tokenAws);


            //var teste = new CredentialsRefreshState(xpto, DateTime.Now.AddHours(1));
            ////GetCredentialsAsync


            //var aa = new AWSCredentials();
            ////var teste2 = teste.Upda

            ////UpdateToGeneratedCredentials

            //var bb = RefreshingAWSCredentials {
            //    CredentialsRefreshState = teste;
            //}
            #endregion

            try
            {
                var adminUser = new AdminGetUserRequest()
                {
                    Username = cpf,
                    UserPoolId = cognitoPoolId
                };

                var user = await _client.AdminGetUserAsync(adminUser);
                return true;
            }
            catch(Exception ex) 
            {
                if (!cpf.Contains("techlanches") && !ValidatorCPF.Validar(cpf)) return false;

                else
                    cpf = cpf.Contains("techlanches") ? cpf : ValidatorCPF.LimparCpf(cpf);

                var input = new SignUpRequest
                {
                    ClientId = cognitoClientId,
                    Username = cpf,
                    Password = userPasswordDefalut,
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType {Name = "email", Value = emailDefault }
                    }
                };

                var signUpResponse = await _client.SignUpAsync(input);

                // Optionally, you can auto-confirm the user
                if (signUpResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var confirmRequest = new AdminConfirmSignUpRequest
                    {
                        Username = cpf,
                        UserPoolId = cognitoPoolId
                    };

                    await _client.AdminConfirmSignUpAsync(confirmRequest);
                    return true;
                }

                return false;
            }
        }

        public async Task<string> SignIn(string cpf)
        {
            if (!cpf.Contains("techlanches")) cpf = ValidatorCPF.LimparCpf(cpf);
                
            using (var provider = _provider)
            {
                var userPool = new CognitoUserPool(cognitoPoolId, cognitoClientId, provider);
                var user = new CognitoUser(cpf, cognitoClientId, userPool, provider);

                var authRequest = new InitiateAdminNoSrpAuthRequest
                {
                    Password = userPasswordDefalut
                };

                var authResponse = await user.StartWithAdminNoSrpAuthAsync(authRequest);

                if (authResponse.AuthenticationResult != null)
                    return authResponse.AuthenticationResult.AccessToken;
            }

            return "An error occurred while signing in.";
        }

        //public async Task<bool> SignUp(string cpf)
        //{
        //    if (!ValidatorCPF.Validar(cpf)) return false;

        //    cpf = ValidatorCPF.LimparCpf(cpf);

        //    var request = new AdminCreateUserRequest
        //    {
        //        UserPoolId = cognitoPoolId,
        //        TemporaryPassword = userPasswordDefalut,
        //        Username = cpf,
        //        MessageAction = MessageActionType.SUPPRESS.ToString(),
        //    };

        //    var response = await _client.AdminCreateUserAsync(request);

        //    if (response.User?.Username != null)
        //    {
        //        var statusCode = await AuthenticateCognitoUser(cpf);

        //        return statusCode.ToLower() == "ok";
        //    }

        //    return false;
        //}

        //public async Task<bool> SignUp2(string cpf)
        //{
        //    if (!ValidatorCPF.Validar(cpf)) return false;

        //    cpf = ValidatorCPF.LimparCpf(cpf);

        //    var cognitoUserPool = new CognitoUserPool(cognitoPoolId, cognitoClientId, _provider, cognitoClientSecret);

        //    var attr = new Dictionary<string, string>
        //    {
        //        { CognitoAttribute.Email.AttributeName, emailDefault }
        //    };

        //    var newUser = new CognitoUser(
        //        cpf,
        //        cognitoClientId,
        //        cognitoUserPool,
        //        _provider,
        //        cognitoClientSecret,
        //        attributes: attr);

        //    var result = await _userManager.CreateAsync(newUser, userPasswordDefalut);

        //    if(result.Succeeded)
        //    {
        //        //var teste = await ((CognitoUserManager<CognitoUser>)_userManager).SendEmailConfirmationTokenAsync(newUser);
        //        //var teste = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

        //        //var codeToken = "609464";

        //        //var confirmUser = await ((CognitoUserManager<CognitoUser>)_userManager).ConfirmSignUpAsync(newUser, codeToken, true);

        //        newUser = await _userManager.FindByIdAsync(cpf);

        //        return true;
        //    }

        //    return false;
        //}

        //public async Task<string> SignIn(string cpf)
        //{
        //    cpf = ValidatorCPF.LimparCpf(cpf);

        //    var accessToken = await AuthenticateCognitoUser(cpf);

        //    return accessToken;

        //    #region
        //    //var user = await _userManager.FindByIdAsync(cpf);

        //    //var result = await _signInManager.PasswordSignInAsync(user, userPassword, false, false);

        //    //if (result.Succeeded) return user.SessionTokens.AccessToken;

        //    //return "An error occurred while signing in."
        //    #endregion
        //}

        //public async Task<string> SignIn2(string cpf)
        //{
        //    //cpf = ValidatorCPF.LimparCpf(cpf);

        //    try
        //    {
        //        var user = await _userManager.FindByIdAsync(cpf);

        //        var result = await _signInManager.PasswordSignInAsync(user, userPasswordDefalut, false, false);

        //        if (result.Succeeded) return user.SessionTokens.AccessToken;

        //        return "An error occurred while signing in.";
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        throw;
        //    }
        //}

        //public async Task<string> AuthenticateCognitoUser(string username)
        //{
        //    var userPool = new CognitoUserPool(cognitoPoolId, cognitoClientId, _provider);
        //    //var user = new CognitoUser(username, cognitoClientId, userPool, _provider);

        //    var secretHash = CreateSecretHash(username, cognitoClientId, cognitoClientSecret);

        //    var authRequest = new InitiateAuthRequest
        //    {
        //        AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
        //        ClientId = cognitoClientId,
        //        AuthParameters = new Dictionary<string, string>
        //        {
        //            { "USERNAME", username },
        //            { "PASSWORD", userPasswordDefalut ?? EncryptionHelper.EncryptPassword(username) },
        //            { "SECRET_HASH", secretHash }
        //        }
        //    };

        //    try
        //    {
        //        var authResponse = await _provider.InitiateAuthAsync(authRequest);

        //        if (authResponse.AuthenticationResult != null)
        //        {
        //            // Assuming getToken is a method that retrieves a token using the result, identityPoolId, and userPoolId
        //            return await GetToken(authResponse.AuthenticationResult, cognitoIdentity, cognitoPoolId);
        //        }
        //        else if (authResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
        //        {
        //            var userPassword = new AdminSetUserPasswordRequest()
        //            {
        //                Password = userPasswordDefalut,
        //                Username = username,
        //                UserPoolId = cognitoPoolId,
        //                Permanent = true
        //            };

        //            var result = await _provider.AdminSetUserPasswordAsync(userPassword);

        //            return result.HttpStatusCode.ToString();
        //        }
        //        else
        //        {
        //            throw new Exception("Authentication failed with unknown challenge.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Error.WriteLine("Authentication failed", ex.Message);
        //        throw;
        //    }
        //}

        //public async Task<string> GetToken(AuthenticationResultType result, string identityPoolId, string userPoolId)
        //{
        //    var credentials = new CognitoAWSCredentials(identityPoolId, RegionEndpoint.GetBySystemName(region));
        //    credentials.AddLogin($"cognito-idp.{region}.amazonaws.com/{userPoolId}", result.IdToken);

        //    //var s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(AWS_REGION));

        //    // Assuming you want to refresh the credentials
        //    await credentials.GetCredentialsAsync();

        //    string accessToken = result.AccessToken;
        //    return accessToken;
        //}

        //private string CreateSecretHash(string username, string userPoolClientId, string userPoolClientSecret)
        //{
        //    var encoding = new ASCIIEncoding();
        //    var clientIdBytes = encoding.GetBytes(username + userPoolClientId);
        //    var clientSecretBytes = encoding.GetBytes(userPoolClientSecret);
        //    var base64Str = "";
        //    using (var hmacsha256 = new HMACSHA256(clientSecretBytes))
        //    {
        //        var hash = hmacsha256.ComputeHash(clientIdBytes);
        //        base64Str = Convert.ToBase64String(hash);
        //    }
        //    return base64Str;
        //}
        
        #region Pode ser util para user admin
        //var provider = new AmazonCognitoIdentityProviderClient();

        //var cognitoUserPool = new CognitoUserPool(cognitoPoolId, cognitoClientId, provider, cognitoClientSecret);

        //var attr = new Dictionary<string, string>
        //{
        //    { CognitoAttribute.Email.AttributeName, emailDefault }
        //};

        //var newUser = new CognitoUser(
        //    cpf,
        //    cognitoClientId,
        //    cognitoUserPool,
        //    provider,
        //    cognitoClientSecret,
        //    attributes: attr);

        //var result = await _userManager.CreateAsync(newUser, userPassword);

        //await ((CognitoUserManager<CognitoUser>)_userManager).SendEmailConfirmationTokenAsync();

        //var confirmUser = await ((CognitoUserManager<CognitoUser>)_userManager).ConfirmSignUpAsync(newUser, codeToken, true);

        //precisa adicionar um gatilho lambda de pre signup para que seja possivel criar o usuário já confirmado
        //https://docs.aws.amazon.com/cognito/latest/developerguide/user-pool-lambda-pre-sign-up.html
        #endregion
    }
}