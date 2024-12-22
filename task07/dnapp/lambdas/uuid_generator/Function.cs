using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using PutRequest = Amazon.S3.Model.PutObjectRequest;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambdaFunction;

public class Function
{
    private static readonly string bucketName = Environment.GetEnvironmentVariable("TARGET_BUCKET");
    private static readonly IAmazonS3 s3Client = new AmazonS3Client();

    public async Task FunctionHandler(ILambdaContext context)
    {
        var uuids = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            uuids.Add(Guid.NewGuid().ToString());
        }

        var fileName = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var content = new { ids = uuids };
        var jsonContent = JsonSerializer.Serialize(content);

        var putRequest = new PutRequest
        {
            BucketName = bucketName,
            Key = fileName,
            ContentBody = jsonContent
        };

        await s3Client.PutObjectAsync(putRequest);
    }
}
