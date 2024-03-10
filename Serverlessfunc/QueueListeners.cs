using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Serverlessfunc.Models;

namespace Serverlessfunc
{
    public static class QueueListeners
    {
        [FunctionName("QueueListeners")]
        public static async Task Run([QueueTrigger("todo", Connection = "AzureWebJobsStorage")] Todo todo,
            [Blob("todo", Connection = "AzureWebJobsStorage")] BlobContainerClient container,
            ILogger log)
        {
            log.LogInformation($"Queue Listener trigger function processed Started.");
            await container.CreateIfNotExistsAsync();
            var blob = container.GetBlobClient($"{todo.Id}.txt");
            await blob.UploadAsync(GetStreamWithStreamWriter($"Created a new task: {todo.TaskDescription}", null));
            log.LogInformation($"C# Queue trigger function processed: {todo.TaskDescription}");
        }

        public static Stream GetStreamWithStreamWriter(string sampleString, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var stream = new MemoryStream(encoding.GetByteCount(sampleString));
            using var writer = new StreamWriter(stream, encoding, -1, true);
            writer.Write(sampleString);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
