using Azure;
using Azure.Data.Tables;
using System;


namespace Serverlessfunc.Models
{
    public class BaseTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
       
    }
}
