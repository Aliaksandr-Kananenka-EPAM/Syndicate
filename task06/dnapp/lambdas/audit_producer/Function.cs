using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
    private static readonly string ConfigurationTableName = Environment.GetEnvironmentVariable("SOURCE_TABLE");
    private static readonly string AuditTableName = Environment.GetEnvironmentVariable("TARGET_TABLE");
    private static readonly AmazonDynamoDBClient _dbClient = new AmazonDynamoDBClient(RegionEndpoint.EUCentral1);
    private static ILambdaLogger _logger;

    public async Task FunctionHandler(DynamoDBEvent request, ILambdaContext context)
    {
        _logger = context.Logger;
        _logger.LogInformation($"Start process to handle event: {JsonSerializer.Serialize(request)}");

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        foreach (var record in request.Records)
        {
            _logger.LogInformation($"Start process record: {JsonSerializer.Serialize(record)}");

            if (IsCreateOrModifyEvent(record.EventName))
            {
                var newImageJson = record.Dynamodb.NewImage.ToJson();
                var newImage = Document.FromJson(newImageJson);

                var oldImageJson = record.Dynamodb.OldImage?.ToJson();
                var oldImage = oldImageJson != null
                    ? Document.FromJson(oldImageJson)
                    : null;

                var key = newImage["key"].AsString();
                var newValue = newImage["value"].AsInt();

                if (oldImage == null)
                {
                    await CreateAuditRecord(key, newValue);
                }
                else
                {
                    var oldValue = oldImage["value"].AsInt();
                    await CreateAuditRecord(key, newValue, oldValue);
                }
            }
        }

        _logger.LogInformation($"Finish process to handle event success.");
    }

    private bool IsCreateOrModifyEvent(string eventName) =>
        eventName == "INSERT" || eventName == "MODIFY";

    private async Task CreateAuditRecord(
        string key,
        int newValue,
        int? oldValue = null)
    {
        var auditTable = Table.LoadTable(_dbClient, AuditTableName);
        var auditRecord = new Document
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["itemKey"] = key,
            ["modificationTime"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            ["newValue"] = new Document
            {
                ["key"] = key,
                ["value"] = newValue
            }
        };

        if (oldValue.HasValue)
        {
            auditRecord["updatedAttribute"] = "value";
            auditRecord["oldValue"] = oldValue.Value;
            auditRecord["newValue"] = newValue;
        }

        await auditTable.PutItemAsync(auditRecord);
        _logger.LogInformation($"Audit document for key: {key}");
    }
}
