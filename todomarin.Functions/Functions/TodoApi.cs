using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using todomarin.common.Models;
using todomarin.common.Responses;
using todomarin.Functions.Entities;

namespace todomarin.Functions.Functions
{
    public static class TodoApi
    {
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todotable,
            ILogger log)
        {
            log.LogInformation("Recived a new todo.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);
            //If object have not description
            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSucess = false,
                    Message = "The request must have a TaskDescrption."
                });
            }
            //If object have description
            TodoEntity todoEntity = new TodoEntity
            {
                CreatedTime = DateTime.UtcNow,
                ETag = "*",
                IsCompleted = false,
                PartitionKey = "Todo",
                RowKey = Guid.NewGuid().ToString(),
                TaskDescription = todo.TaskDescription
            };

            TableOperation addOperation = TableOperation.Insert(todoEntity);
            await todotable.ExecuteAsync(addOperation);

            string message = "new todo stored in table";
            log.LogInformation(message);



            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity
            });
        }

        [FunctionName(nameof(UpdateTodo))]
        public static async Task<IActionResult> UpdateTodo(
                [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
                [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todotable,
                string id,
                ILogger log)
        {
            log.LogInformation($"Update for todo: {id}, received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            //Validate todo id 
            TableOperation findOperation = TableOperation.Retrieve<TodoEntity>("Todo", id);
            TableResult findResult = await todotable.ExecuteAsync(findOperation);
            if (findResult.Result == null) 
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSucess = false,
                    Message = "Todo not found"
                });
            }
            // Update todo
            TodoEntity todoEntity = (TodoEntity)findResult.Result;
            todoEntity.IsCompleted = todo.IsCompleted;

            if (!string.IsNullOrEmpty(todo.TaskDescription))
            {
                todoEntity.TaskDescription = todo.TaskDescription;
            }

            TableOperation addOperation = TableOperation.Replace(todoEntity);
            await todotable.ExecuteAsync(addOperation);

            string message = $" Todo: {id}, uddate in table.";
            log.LogInformation(message);



            return new OkObjectResult(new Response
            {
                IsSucess = true,
                Message = message,
                Result = todoEntity
            });
        }
    }
}
