using System;
using System.Collections.Generic;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
    private readonly ILogger<Function> _logger;

    public Function()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });
        _logger = loggerFactory.CreateLogger<Function>();
    }

    public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        _logger.LogInformation($"Request: {JsonSerializer.Serialize(request)}");

        var path = request.RequestContext.Http.Path;
        var method = request.RequestContext.Http.Method;

        if (path == "/hello" && string.Equals(method, "GET", StringComparison.CurrentCultureIgnoreCase) )
        {
            _logger.LogInformation($"Incoming url /hello and GET method type.");

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = "{\"statusCode\": 200, \"message\": \"Hello from Lambda\"}",
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        _logger.LogInformation($"Bad request.");

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 400,
            Body = $"{{\"statusCode\": 400, \"message\": \"Bad request syntax or unsupported method. Request path: {path}. HTTP method: {method}\"}}",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
