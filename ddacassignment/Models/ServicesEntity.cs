using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ddacassignment.Models
{
    public class ServicesEntity : TableEntity
    {
        public ServicesEntity() { }
        public ServicesEntity(String service , string company)
        {
            this.PartitionKey = service;
            this.RowKey = company;
        }

        
       
        public DateTime Schedule { get; set; }

        public double Price { get; set; }

        public string customerUsername { get; set; }

        public bool isBooked { get; set; }

        public bool isConfirmed { get; set; }
    }
}
