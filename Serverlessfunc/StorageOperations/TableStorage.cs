﻿using Serverlessfunc.Models;


namespace Serverlessfunc.StorageOperations
{
    public static class TableStorage
    {
        public static TodoTableEntity ToTableEntity(this Todo todo)
        {
            return new TodoTableEntity()
            {
                PartitionKey = "TODO",
                RowKey = todo.Id,
                CreatedTime = todo.CreatedTime,
                IsCompleted = todo.IsCompleted,
                TaskDescription = todo.TaskDescription
            };
        }

        public static Todo ToTodo(this TodoTableEntity todo)
        {
            return new Todo()
            {
                Id = todo.RowKey,
                CreatedTime = todo.CreatedTime,
                IsCompleted = todo.IsCompleted,
                TaskDescription = todo.TaskDescription
            };
        }
    }
}
