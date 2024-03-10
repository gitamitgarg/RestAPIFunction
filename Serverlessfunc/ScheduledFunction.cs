using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Serverlessfunc.Models;

namespace Serverlessfunc
{
    public class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer,
            [Table("todo", Connection = "AzureWebJobsStorage")] TableClient client,
            ILogger log)
        {
            var segment = client.Query<TodoTableEntity>().ToList();
            var deleted = 0;
            foreach (var todo in segment)
            {
                if (todo.IsCompleted)
                {
                    await client.DeleteEntityAsync(todo.PartitionKey, todo.RowKey, ETag.All);
                    deleted++;
                }
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        }
    }
}
