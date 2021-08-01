using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Required(ErrorMessage = "Must Enter the text fields")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Must Enter the text fields")]
        public string Rating { get; set; }

        public IFormFile ImageFilee { get; set; }

        public string ImageFilename { get; set; }


    }
}
