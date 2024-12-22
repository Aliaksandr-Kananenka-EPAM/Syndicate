using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Function.Entities;
using Function.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
    private static readonly string TableName = Environment.GetEnvironmentVariable("TARGET_TABLE");
    private static readonly AmazonDynamoDBClient _dbClient = new AmazonDynamoDBClient(RegionEndpoint.EUCentral1);

    public async Task<APIGatewayProxyResponse> FunctionHandler(Request request, ILambdaContext context)
    {
        var logger = context.Logger;
        logger.LogInformation($"Incoming request: {JsonSerializer.Serialize(request)}");

        var eventId = Guid.NewGuid().ToString();
        var createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        var contentMap = new Dictionary<string, AttributeValue>
        {
            { "name", GetStringAttributeValue(request.Content.Name) },
            { "surname", GetStringAttributeValue(request.Content.Surname) }
        };

        var eventEntityToPut = new Dictionary<string, AttributeValue>
        {
            { "id", new AttributeValue(eventId) },
            { "principalId", new AttributeValue { N = request.PrincipalId.ToString() } },
            { "createdAt", new AttributeValue(createdAt) },
            { "body", new AttributeValue { M = contentMap } }
        };

        var putItemRequest = new PutItemRequest
        {
            TableName = TableName,
            Item = eventEntityToPut
        };

        await _dbClient.PutItemAsync(putItemRequest);

        var responseBody = new Event
        {
            Id = eventId,
            PrincipalId = request.PrincipalId,
            CreatedAt = createdAt,
            Body = request.Content
        };

        return new APIGatewayProxyResponse
        {
            StatusCode = 201,
            Body = JsonSerializer.Serialize(responseBody),
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };
    }

    private AttributeValue GetStringAttributeValue(string value)
    {
        if (value == null)
        {
            return new AttributeValue { NULL = true };
        }

        return new AttributeValue { S = value };
    }
}
