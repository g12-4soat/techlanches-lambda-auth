using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net;
using TechLanchesLambda.Options;
using TechLanchesLambda.Service;
using TechLanchesLambda.Utils;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TechLanchesLambda;

public class Functions
{
    public Functions()
    {
    }

    [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole", MemorySize = 512, Timeout = 30)]
    [RestApi(LambdaHttpMethod.Post, "/auth")]
    public async Task<APIGatewayProxyResponse> LambdaAuth(APIGatewayProxyRequest request,
                                                  ILambdaContext context,
                                                  [FromServices] ICognitoService cognitoService,
                                                  [FromServices] IConfiguration configuration)
    {
        try
        {
            context.Logger.LogInformation("Handling the 'GetAuth' Request");

            var awsOptions = configuration.GetSection("AWS")
               .Get<Options.AWSOptions>();

            ArgumentNullException.ThrowIfNull(awsOptions);
            var resultadoValidacaoUsuario = ObterNomeUsuario(request, awsOptions, false);
            if (resultadoValidacaoUsuario.Falhou)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(resultadoValidacaoUsuario.Erros.First()),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var user = resultadoValidacaoUsuario.Value;
            var resultadoLogin = await cognitoService.SignIn(user.Cpf);

            if (!resultadoLogin.Sucesso)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(resultadoLogin.Erros.First()),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            //gravar cliente no DB

            var tokenResult = resultadoLogin.Value;
            return new APIGatewayProxyResponse
            {
                StatusCode = !string.IsNullOrEmpty(tokenResult.AccessToken) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                Body = JsonConvert.SerializeObject(tokenResult),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Auth Lambda response error: " + ex.Message);
            throw new Exception(ex.Message);
        }
    }

    [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole", MemorySize = 512, Timeout = 30)]
    [RestApi(LambdaHttpMethod.Post, "/cadastro")]
    public async Task<APIGatewayProxyResponse> LambdaCadastro(APIGatewayProxyRequest request,
                                                  ILambdaContext context,
                                                  [FromServices] ICognitoService cognitoService, 
                                                  [FromServices] IConfiguration configuration)
    {
        try
        {
            context.Logger.LogInformation("Handling the 'GetAuth' Request");

            var awsOptions = configuration.GetSection("AWS")
               .Get<Options.AWSOptions>();

            ArgumentNullException.ThrowIfNull(awsOptions);

            var resultadoValidacaoUsuario = ObterNomeUsuario(request, awsOptions, true);
            if(resultadoValidacaoUsuario.Falhou)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(resultadoValidacaoUsuario.Erros.First()),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var user = resultadoValidacaoUsuario.Value;
            var userValido = user.Validar();
            if(!userValido)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject("Houve erro de validação"),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            var resultadoCadastroUsuario = await cognitoService.SignUp(user);
            if (!resultadoCadastroUsuario.Sucesso)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(resultadoCadastroUsuario.Erros.First()),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var resultadoLogin = await cognitoService.SignIn(user.Cpf);
            if (!resultadoLogin.Sucesso)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(resultadoLogin.Erros.First()),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            //gravar cliente no DB

            var tokenResult = resultadoLogin.Value;
            return new APIGatewayProxyResponse
            {
                StatusCode = !string.IsNullOrEmpty(tokenResult.AccessToken) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                Body = JsonConvert.SerializeObject(tokenResult),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Auth Lambda response error: " + ex.Message);
            throw new Exception(ex.Message);
        }
    }

    private static Resultado<User> ObterNomeUsuario(APIGatewayProxyRequest request, AWSOptions awsOptions, bool ehCadastro)
    {
        const string NOME_QUERY_STRING = "cpf";

        bool cpfFoiInformado = request.QueryStringParameters.Any(x => x.Key == NOME_QUERY_STRING && !string.IsNullOrEmpty(x.Value) && !string.IsNullOrWhiteSpace(x.Value));
        if (!cpfFoiInformado && !ehCadastro) return Resultado.Ok(new User { Nome = awsOptions.UserTechLanches, Email = awsOptions.EmailDefault, Cpf = awsOptions.UserTechLanches});

        if (ehCadastro && !cpfFoiInformado)
            return Resultado.Falha<User>("O CPF não foi informado para cadastro.");

        if (!ValidatorCPF.Validar(request.QueryStringParameters[NOME_QUERY_STRING]))
            return Resultado.Falha<User>("O CPF informado está inválido");

        string cpfLimpo = ValidatorCPF.LimparCpf(request.QueryStringParameters[NOME_QUERY_STRING]);

        var user = new User
        {
            Cpf = cpfLimpo,
            Email = request.QueryStringParameters["email"],
            Nome = request.QueryStringParameters["nome"]
        };

        return Resultado.Ok(user);
    }
}
