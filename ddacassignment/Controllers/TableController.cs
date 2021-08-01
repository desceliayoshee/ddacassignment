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
using Microsoft.AspNetCore.Identity;
using ddacassignment.Areas.Identity.Data;

namespace ddacassignment.Controllers
{
    public class TableController : Controller
    {
        public IActionResult AddSingleEntity(string PartitionKey, string RowKey, DateTime Schedule, double price, String customerUsername, bool isbooked, bool isconfirmed)
        {

            //refer to the table 
            CloudTable table = getTableStorageInformation();

            ServicesEntity service1 = new ServicesEntity(PartitionKey, RowKey);
            service1.Schedule = Schedule;
            service1.Price = price;
            service1.customerUsername = "something";
            service1.isBooked= false;
            service1.isConfirmed = false;


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

        //to table
        public ActionResult AddService()
        {
            CloudTable table = getTableStorageInformation();
            CreateTable();

            string errormessage = null;

            //display all info 
            try
            {
                //create query
                TableQuery<ServicesEntity> query = new TableQuery<ServicesEntity>();

                List<ServicesEntity> services = new List<ServicesEntity>();
                TableContinuationToken token = null; //to identify if there is still more data
                do
                {
                    TableQuerySegment<ServicesEntity> result = table.ExecuteQuerySegmentedAsync(query, token).Result;
                    token = result.ContinuationToken;

                    foreach (ServicesEntity service in result.Results)
                    {
                        services.Add(service);
                    }
                }
                while (token != null); //token not empty; continue read data
                if (services.Count != 0)
                {
                    return View(services); //back to display
                }
                else
                {
                    errormessage = "Data not Found";
                    return RedirectToAction("AddService", "Table", new { dialogmsg = errormessage });
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = "Technical error: " + ex.ToString();
            }
            return View();
        }

        //delete data 
        public ActionResult deletedata(string pkey, string rkey)
        {
            string message = null;
            CloudTable table = getTableStorageInformation();
            try
            {
                ServicesEntity services1 = new ServicesEntity(pkey, rkey) { ETag = "*" };
                TableOperation deletemethod = TableOperation.Delete(services1);
                table.ExecuteAsync(deletemethod);
                message = "The service" + pkey + "of" + rkey + "has been deleted";
                ViewBag.message = message;
            }
            catch(Exception ex)
            {
                message = "Technical errors" + ex.ToString();
            }
            return RedirectToAction("AddService", "Table", new { Message = message });
        }

        //edit data
        public ActionResult editdata(string pkey, string rkey)
        {
            string message = null;
            CloudTable table = getTableStorageInformation();

            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<ServicesEntity>(pkey, rkey);
                TableResult tableResult = table.ExecuteAsync(retrieveOperation).Result; //come together ETag
                if (tableResult.Etag != null)
                {
                    var customer = tableResult.Result as ServicesEntity;
                    return View(customer);
                }
                else
                {
                    message = "No such service in the list";
                }
            }
            catch (Exception e)
            {
                message = "Technical error:" + e.ToString();
            }

            return RedirectToAction("AddService", "Table", new { Message = message });
        }

        //edit data from the save button
        [HttpPost]
        public ActionResult editdata([Bind("PartitionKey", "RowKey", "Schedule", "Price")] ServicesEntity serv)
        {
            ServicesEntity service3 = new ServicesEntity();
            DateTime Schedule = service3.Schedule;
            double Price = service3.Price;

            string message = null;
            CloudTable table = getTableStorageInformation();
            if (ModelState.IsValid)
            {
                serv.ETag = "*";
                try
                {

                    TableOperation updateoperation = TableOperation.Replace(serv);
                    table.ExecuteAsync(updateoperation);
                    message = "The Service Information " + serv.PartitionKey + "" + serv.RowKey + " has been updated";

                }
                catch (Exception ex)
                {
                    message = "Unable to update the data. Error: " + ex.ToString();
                }

            }
            else
            {
                return View(serv);
            }

            return RedirectToAction("AddService", "Table", new { Message = message });
            //return View();
        }
        // create form to insert service details from AddSingleEntity
        public ActionResult InsertForm()
        {

            return View();
        }
        //search page 
        public ActionResult SearchPage(String Message = null)
        {
            ViewBag.msg = Message;
            return View();
        }

        //how to search customer information from table
        public ActionResult getsingleentity(string PartitionKey, string RowKey)
        {
            string message = null;
            CloudTable table = getTableStorageInformation();

            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<ServicesEntity>(PartitionKey, RowKey);
                TableResult tableResult = table.ExecuteAsync(retrieveOperation).Result; //come together ETag
                if(tableResult.Etag != null )
                {
                    var customer = tableResult.Result as ServicesEntity;
                    return View(customer);
                }else
                {
                    message = "No such service in the list";
                }
            } catch (Exception e)
            {
                message = "Technical error:" + e.ToString();
            }

            return RedirectToAction("SearchPage", "Table", new { Message = message});
        }

        private UserManager<ddacassignmentUser> userManager;

        public TableController(UserManager<ddacassignmentUser> usrMan)
        {
            userManager = usrMan;
        }
        //book 
        public ActionResult bookdata(string PartitionKey, string RowKey)
        {
            CloudTable table = getTableStorageInformation();
            string errormessage = null;

            //get current username 
            var myusername = this.userManager.GetUserName(HttpContext.User);
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<ServicesEntity>(PartitionKey, RowKey);

                //Execute the operation
                TableResult result = table.ExecuteAsync(retrieveOperation).Result;

                //asign the result to item objct
                ServicesEntity updateEntity = (ServicesEntity)result.Result;

                //change the description 
                updateEntity.isBooked = true;
                updateEntity.customerUsername = myusername;
                 
                //create the inssertorreplace tableoperation
                TableOperation insertorReplaceOperation = TableOperation.InsertOrReplace(updateEntity);
                var service = result.Result as ServicesEntity;
                //execute the operation
                TableResult resultof = table.ExecuteAsync(insertorReplaceOperation).Result;
                ViewBag.Result = result.HttpStatusCode;
                return View(resultof);
            }
            catch (Exception ex)
            {
                ViewBag.msg = "Technical Error: " + ex.ToString();
            }
            ViewBag.msg = errormessage;

            return View();
        }

        //displaybooking 
        public ActionResult displayresult(string dialogmsg = null)
        {
            CloudTable table = getTableStorageInformation();
            CreateTable();
            string errormessage = null;

            //get current username
            var myusername = this.userManager.GetUserName(HttpContext.User);
            try
            {
                TableQuery<ServicesEntity> query = new TableQuery<ServicesEntity>()
                    .Where(TableQuery.GenerateFilterCondition("customerUsername", QueryComparisons.Equal, myusername));
                List<ServicesEntity> services = new List<ServicesEntity>();
                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<ServicesEntity> result = table.ExecuteQuerySegmentedAsync(query, token).Result;
                    token = result.ContinuationToken;
                    foreach (ServicesEntity service in result.Results)
                    {
                        services.Add(service);
                    }
                }
                while (token != null);

                if (services.Count != 0)
                {
                    return View(services);
                } 
                else
                {
                    errormessage = "Data not found";
                    return View(new { dialogmsg = errormessage });
                }

            }
            catch (Exception ex)
            {
                ViewBag.msg = "Technical Error: " + ex.ToString();
            }
            ViewBag.msg = errormessage;

            return View();
           
        }

        // to display all service (customer)
        public ActionResult ViewService(string PartitionKey, string RowKey)
        {
            CloudTable table = getTableStorageInformation();
            CreateTable();

            string errormessage = null;

            //display all info 
            try
            {
                //create query
                TableQuery<ServicesEntity> query = new TableQuery<ServicesEntity>();

                List<ServicesEntity> services = new List<ServicesEntity>();
                TableContinuationToken token = null; //to identify if there is still more data
                do
                {
                    TableQuerySegment<ServicesEntity> result = table.ExecuteQuerySegmentedAsync(query, token).Result;
                    token = result.ContinuationToken;

                    foreach (ServicesEntity service in result.Results)
                    {
                        services.Add(service);
                    }
                }
                while (token != null); 
                if (services.Count != 0)
                {
                    return View(services);
                }
                else
                {
                    errormessage = "Data not Found";
                    return RedirectToAction("AddService", "Table", new { dialogmsg = errormessage });
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = "Technical error: " + ex.ToString();
            }
            return View();
        }

        // to display all service (driver)
        public ActionResult ViewServiceStaff(string PartitionKey, string RowKey)
        {
            CloudTable table = getTableStorageInformation();
            CreateTable();

            string errormessage = null;

            //display all info 
            try
            {
                //create query
                TableQuery<ServicesEntity> query = new TableQuery<ServicesEntity>();

                List<ServicesEntity> services = new List<ServicesEntity>();
                TableContinuationToken token = null; //to identify if there is still more data
                do
                {
                    TableQuerySegment<ServicesEntity> result = table.ExecuteQuerySegmentedAsync(query, token).Result;
                    token = result.ContinuationToken;

                    foreach (ServicesEntity service in result.Results)
                    {
                        services.Add(service);
                    }
                }
                while (token != null); //token not empty; continue read data
                if (services.Count != 0)
                {
                    return View(services); //back to display
                }
                else
                {
                    errormessage = "Data not Found";
                    return RedirectToAction("AddService", "Table", new { dialogmsg = errormessage });
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = "Technical error: " + ex.ToString();
            }
            return View();
        }


    }
}
