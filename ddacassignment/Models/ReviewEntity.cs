using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Table;

namespace ddacassignment.Models
{
    public class ReviewEntity : TableEntity
    {
        public ReviewEntity(string userName, string serviceCompany)
        {
            this.PartitionKey = userName;
            this.RowKey = serviceCompany;
        }

        public ReviewEntity() { }

        public string Content { get; set; }

        public string Rating { get; set; }

        public IFormFile ImageFilee { get; set; }

        public string ImageFilename { get; set; }


    }
}
