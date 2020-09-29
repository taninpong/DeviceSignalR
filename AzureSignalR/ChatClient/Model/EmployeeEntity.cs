
//using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatClient.Model
{
    public class EmployeeEntity : TableEntity
    {
        public EmployeeEntity(string Name, string connectionID)
        {
            this.PartitionKey = Name; this.RowKey = connectionID;
        }
        public EmployeeEntity() { }
        //public string Message { get; set; }
    }
}
