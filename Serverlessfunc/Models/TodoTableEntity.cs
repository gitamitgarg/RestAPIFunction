using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Serverlessfunc.Models
{
    public class TodoTableEntity : BaseTableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}
