using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
    public CustomResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        return new CustomResponse
        {
            statusCode = 200,
            message = "Hello from Lambda"
        };
    }

    public class CustomResponse
    {
        public int statusCode { get; set; }
        public string message { get; set; }
    }
}
