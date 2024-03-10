using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serverlessfunc.Models;
using System.Collections.Generic;
using System.Linq;
using Serverlessfunc.DTO;
using Microsoft.WindowsAzure.Storage;
using Serverlessfunc.StorageOperations;
using Azure.Data.Tables;
using Azure;

namespace Serverlessfunc
{
    public static class APIFunction
    {
        static List<Todo> items = new List<Todo>();
        
        //[FunctionName("APIFunction")]
        //public static async Task<IActionResult> Run(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        //    ILogger log)
        //{
        //    log.LogInformation("C# HTTP trigger function processed a request.");

        //    string name = req.Query["name"];

        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    dynamic data = JsonConvert.DeserializeObject(requestBody);
        //    name = name ?? data?.name;

        //    string responseMessage = string.IsNullOrEmpty(name)
        //        ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
        //        : $"Hello, {name}. This HTTP triggered function executed successfully.";

        //    return new OkObjectResult(responseMessage);
        //}

         [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoTableEntity> todoTable,
            [Queue("todo", Connection = "AzureWebJobsStorage")] IAsyncCollector<Todo> todoQueue,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item.");

           string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

        var todo = new Todo() { TaskDescription = input.TaskDescription };
            //items.Add(todo);
            await todoTable.AddAsync(todo.ToTableEntity());
            await todoQueue.AddAsync(todo);
            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "todo")] HttpRequest req,
           [Table("todo", Connection = "AzureWebJobsStorage")] TableClient client,
           ILogger log)
        {
            log.LogInformation("Getting todo list items");
            var result = client.Query<Azure.Data.Tables.TableEntity>().ToList();
            return new OkObjectResult(result);
            
        }

        [FunctionName("GetTodoById")]
        public static async Task<IActionResult> GetTodoById(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "todo/{id}")] HttpRequest req,
           [Table("todo", Connection = "AzureWebJobsStorage")] TableClient client,
           ILogger log, string id)
        {
            log.LogInformation("Creating a new todo list item.");

            var result = client.Query<Azure.Data.Tables.TableEntity>().ToList();
            var todo = result.FirstOrDefault(t => t.RowKey == id);
            if (todo == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(todo);
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "todo/{id}")] HttpRequest req,
           [Table("todo", Connection = "AzureWebJobsStorage")] TableClient todoTable,
           ILogger log, string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);
            var findResult = await todoTable.GetEntityAsync<TodoTableEntity>("TODO",id, null);
            if (findResult == null)
            {
                return new NotFoundResult();
            }
            var existingRow = findResult.Value;
            existingRow.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TaskDescription))
            {
                existingRow.TaskDescription = updated.TaskDescription;
            }

            await todoTable.UpdateEntityAsync(existingRow, ETag.All, TableUpdateMode.Replace);
            return new OkObjectResult(existingRow.ToTodo());
        }

        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "todo/{id}")] HttpRequest req,
           [Table("todo", Connection = "AzureWebJobsStorage")] TableClient todoTable,
           ILogger log, string id)
        {
            try
            {
               await todoTable.DeleteEntityAsync("TODO", id, ETag.All);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult("Record has been deleted.");
        }

    }
}
