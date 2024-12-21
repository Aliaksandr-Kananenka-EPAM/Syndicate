using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
    public async Task FunctionHandler(SNSEvent request, ILambdaContext context)
    {
        var logger = context.Logger;

        foreach (var record in request.Records)
        {
            if (record == null) continue;

            logger.LogInformation($"{JsonSerializer.Serialize(record.Sns.Message)}");
        }

        await Task.CompletedTask;
    }
}
