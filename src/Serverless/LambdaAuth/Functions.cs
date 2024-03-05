using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
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
    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Functions()
    {
    }

    /// <summary>
    /// A Lambda function to respond to HTTP Get methods from API Gateway
    /// </summary>
    /// <remarks>
    /// This uses the <see href="https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.Annotations/README.md">Lambda Annotations</see> 
    /// programming model to bridge the gap between the Lambda programming model and a more idiomatic .NET model.
    /// 
    /// This automatically handles reading parameters from an APIGatewayProxyRequest
    /// as well as syncing the function definitions to serverless.template each time you build.
    /// 
    /// If you do not wish to use this model and need to manipulate the API Gateway 
    /// objects directly, see the accompanying Readme.md for instructions.
    /// </remarks>
    /// <param name="context">Information about the invocation, function, and execution environment</param>
    /// <returns>The response as an implicit <see cref="APIGatewayProxyResponse"/></returns>
    [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole", MemorySize = 512, Timeout = 30)]
    [RestApi(LambdaHttpMethod.Post, "/Auth")]
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
            var resultadoValidacaoUsuario = ObterNomeUsuario(request, awsOptions);
            if(resultadoValidacaoUsuario.Falhou)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(resultadoValidacaoUsuario.Erros.First()),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var nomeUsuario = resultadoValidacaoUsuario.Value;
            var resultadoCadastroUsuario = await cognitoService.SignUp(nomeUsuario);
            if (!resultadoCadastroUsuario.Sucesso)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(resultadoCadastroUsuario.Erros.First()),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var resultadoLogin = await cognitoService.SignIn(nomeUsuario);
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

            var token = resultadoLogin.Value;
            return new APIGatewayProxyResponse
            {
                StatusCode = !string.IsNullOrEmpty(token) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                Body = JsonConvert.SerializeObject(token),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Auth Lambda response error: " + ex.Message);
            throw new Exception(ex.Message);
        }
    }

    private static Resultado<string> ObterNomeUsuario(APIGatewayProxyRequest request, AWSOptions awsOptions)
    {
        const string NOME_QUERY_STRING = "cpf";

        bool cpfFoiInformado = request.QueryStringParameters.Any(x => x.Key == NOME_QUERY_STRING && !string.IsNullOrEmpty(x.Value) && !string.IsNullOrWhiteSpace(x.Value));
        if (!cpfFoiInformado) return Resultado.Ok(awsOptions.UserTechLanches);

        if (!ValidatorCPF.Validar(request.QueryStringParameters[NOME_QUERY_STRING]))
            return Resultado.Falha<string>("CPF Inválido");

        string cpfLimpo = ValidatorCPF.LimparCpf(request.QueryStringParameters[NOME_QUERY_STRING]);
        return Resultado.Ok(cpfLimpo);
    }
}
