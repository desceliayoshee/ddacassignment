using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ddacassignment.Models
{
    public class ServicesEntity : TableEntity
    {
        public ServicesEntity(String id, string services)
        {
            this.PartitionKey = id;
            this.RowKey = services;
        }

        public ServicesEntity() { }
       
        public DateTime Schedule { get; set; }
        public double Price { get; set; }

    }
}
