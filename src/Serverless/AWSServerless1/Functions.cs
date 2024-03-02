using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
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

        var userTechLanches = Environment.GetEnvironmentVariable("UserTechLanches");
        context.Logger.LogInformation("Handling the 'GetAuth' Request");

        try
        {
            var awsOptions = configuration.GetSection("AWS")
               .Get<Options.AWSOptions>();

            ArgumentNullException.ThrowIfNull(awsOptions);
            string cpf;
            bool cpfFoiInformado = request.QueryStringParameters.Any(x => x.Key == "cpf" && !string.IsNullOrEmpty(x.Value) && !string.IsNullOrWhiteSpace(x.Value));
            if (cpfFoiInformado)
            {
                if (!ValidatorCPF.Validar(request.QueryStringParameters["cpf"])) throw new Exception("CPF Invalido");
                cpf = ValidatorCPF.LimparCpf(request.QueryStringParameters["cpf"]);
            }
            else
            {
                cpf = awsOptions.UserTechLanches;
            }

            if (await cognitoService.SignUp(cpf))
            {
                var token = await cognitoService.SignIn(cpf);

                //gravar cliente no DB

                return new APIGatewayProxyResponse
                {
                    StatusCode = !string.IsNullOrEmpty(token) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(token),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = JsonConvert.SerializeObject("An error occurred while signing up."),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Auth Lambda response error: " + ex.Message);
            throw new Exception(ex.Message);
        }
    }
}
