using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ddacassignment.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ddacassignment.Controllers
{
    public class TableController : Controller
    {
        public IActionResult AddSingleEntity(string PartitionKey, string RowKey, DateTime Schedule, double price)
        {

            //refer to the table 
            CloudTable table = getTableStorageInformation();

            ServicesEntity service1 = new ServicesEntity(PartitionKey, RowKey);
            service1.Schedule = Schedule;
            service1.Price = price;


            try
            {
                TableOperation insertOperation = TableOperation.Insert(service1);
                TableResult insertresult = table.ExecuteAsync(insertOperation).Result;

                ViewBag.SuccessCode = insertresult.HttpStatusCode;
                ViewBag.TableName = table.Name;
            }
            catch(Exception ex)
            {
                ViewBag.Message = ex.ToString();
            }

            return View();
        }
        private CloudTable getTableStorageInformation()
        {
            //step 1: read json 
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build();

            //to get key access 
            //once link, time to read the content to get the connectionstring 
            CloudStorageAccount storageaccount =

            CloudStorageAccount.Parse(configure["ConnectionStrings:TableStorageConnection"]);
            CloudTableClient tableClient = storageaccount.CreateCloudTableClient();

            //step 2: how to create a new table in the storage. 
            CloudTable table = tableClient.GetTableReference("Services");
            return table;
        }

        public ActionResult CreateTable()
        {

            //refer to the container
            CloudTable table = getTableStorageInformation();
            ViewBag.Success = table.CreateIfNotExistsAsync().Result;
            ViewBag.TableName = table.Name;
            return View();
        }

        // create form to insert service details from AddSingleEntity
        public ActionResult InsertForm()
        {

            return View();
        }

    }
}
