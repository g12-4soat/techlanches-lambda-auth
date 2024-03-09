using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Options;
using TechLanchesLambda.AWS.Options;
using TechLanchesLambda.DTOs;
using TechLanchesLambda.Service;
using TechLanchesLambda.Utils;
using TechLanchesLambda.Validations;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TechLanchesLambda;

public class Functions
{
    private const string CPF_QUERY_STRING = "cpf";
    private const string NOME_QUERY_STRING = "nome";
    private const string EMAIL_QUERY_STRING = "email";

    public Functions()
    {
    }

    [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole", MemorySize = 512, Timeout = 30)]
    [RestApi(LambdaHttpMethod.Post, "/auth")]
    public async Task<APIGatewayProxyResponse> LambdaAuth(APIGatewayProxyRequest request,
                                                  ILambdaContext context,
                                                  [FromServices] ICognitoService cognitoService,
                                                  [FromServices] IOptions<AWSOptions> awsOptions)
    {
        try
        {
            context.Logger.LogInformation("Handling the 'LambdaAuth' Request");
            ArgumentNullException.ThrowIfNull(awsOptions);

            var usuario = ObterUsuario(request, awsOptions.Value, ehCadastro: false);
            var usuarioEhPadrao = usuario.Cpf.Equals(awsOptions.Value.UserTechLanches);
            if (usuarioEhPadrao)
            {
                var resultadoCadastroUsuario = await cognitoService.SignUp(usuario);
                if (!resultadoCadastroUsuario.Sucesso)
                {
                    return Response.BadRequest(resultadoCadastroUsuario.Notificacoes);
                }
            }

            var resultadoLogin = await cognitoService.SignIn(usuario.Cpf);
            if (!resultadoLogin.Sucesso)
            {
                return Response.BadRequest(resultadoLogin.Notificacoes);
            }

            var tokenResult = resultadoLogin.Value;
            return !string.IsNullOrEmpty(tokenResult.AccessToken) ? Response.Ok(tokenResult) : Response.BadRequest("Não possui token"); 
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
                                                  [FromServices] IOptions<AWSOptions> awsOptions)
    {
        try
        {
            context.Logger.LogInformation("Handling the 'LambdaCadastro' Request");

            ArgumentNullException.ThrowIfNull(awsOptions);

            var usuario = ObterUsuario(request, awsOptions.Value, ehCadastro: true);

            var resultadoValidacaoUsuario = new UsuarioCadastroValidation().Validate(usuario);
            if(!resultadoValidacaoUsuario.IsValid)
            {
                return Response.BadRequest(resultadoValidacaoUsuario.Errors.Select(x => new NotificacaoDto(x.ErrorMessage)).ToList());
            }
            
            var resultadoCadastroUsuario = await cognitoService.SignUp(usuario);
            if (!resultadoCadastroUsuario.Sucesso)
            {
                return Response.BadRequest(resultadoCadastroUsuario.Notificacoes);
            }

            var resultadoLogin = await cognitoService.SignIn(usuario.Cpf);
            if (!resultadoLogin.Sucesso)
            {
                return Response.BadRequest(resultadoLogin.Notificacoes);
            }

            var tokenResult = resultadoLogin.Value;
            return !string.IsNullOrEmpty(tokenResult.AccessToken) ? Response.Ok(tokenResult) : Response.BadRequest("Não possui token");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cadastro Lambda response error: " + ex.Message);
            throw new Exception(ex.Message);
        }
    }

    private UsuarioDto ObterUsuario(APIGatewayProxyRequest request, AWSOptions awsOptions, bool ehCadastro)
    {
        if (!CpfFoiInformado(request) && !ehCadastro)
            return new UsuarioDto(awsOptions.UserTechLanches, awsOptions.EmailDefault, awsOptions.UserTechLanches);

        string cpf = request.QueryStringParameters[CPF_QUERY_STRING];
        string cpfLimpo = ValidatorCPF.LimparCpf(request.QueryStringParameters[CPF_QUERY_STRING]);
        string email = ObterValorQueryString(request, EMAIL_QUERY_STRING);
        string nome = ObterValorQueryString(request, NOME_QUERY_STRING);

        var user = new UsuarioDto(string.IsNullOrEmpty(cpfLimpo) ? cpf : cpfLimpo, email, nome);
        return user;
    }

    private bool CpfFoiInformado(APIGatewayProxyRequest request)
    {
        return request.QueryStringParameters.Any(x => x.Key == CPF_QUERY_STRING &&
                                                                 !string.IsNullOrEmpty(x.Value) &&
                                                                 !string.IsNullOrWhiteSpace(x.Value));
    }

    private string ObterValorQueryString(APIGatewayProxyRequest request, string key)
    {
        return request.QueryStringParameters.Any(x => x.Key == key &&
                                                     !string.IsNullOrEmpty(x.Value) &&
                                                     !string.IsNullOrWhiteSpace(x.Value)) ? request.QueryStringParameters[key] : string.Empty;
    }
}
