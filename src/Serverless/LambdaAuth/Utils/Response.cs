using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using System.Net;
using TechLanchesLambda.DTOs;

namespace TechLanchesLambda.Utils;

public static class Response
{
    public static APIGatewayProxyResponse BadRequest(List<NotificacaoDto> notificacoes)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonConvert.SerializeObject(notificacoes),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public static APIGatewayProxyResponse BadRequest(string mensagem)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonConvert.SerializeObject(new NotificacaoDto(mensagem)),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    public static APIGatewayProxyResponse Ok(object body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = JsonConvert.SerializeObject(body),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}