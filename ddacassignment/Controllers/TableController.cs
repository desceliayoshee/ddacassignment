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
using Microsoft.WindowsAzure.Storage.Blob;

namespace ddacassignment.Controllers
{
    public class TableController : Controller
    {
        private UserManager<ddacassignmentUser> userManager;
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

        public ActionResult CreateJobTable()
        {

            //refer to the container
            CloudTable table = getTableStorageInformation();
            ViewBag.Success = table.CreateIfNotExistsAsync().Result;
            ViewBag.TableName = table.Name;
            return View();
        }

        //public void CreateJobContainer()
        //{
        //    //step 1:call the getBlobStroageInforemation() method  to link with the account
        //    //refer to the container
        //    CloudBlobContainer container = getBlobStorageInformation();

        //    //Step 2 : create the contaier if the system found that the name doest exist
        //    var Success = container.CreateIfNotExistsAsync().Result;

        //    //step 3: collect the container name to display in the frontend
        //    var BlobContainerName = container.Name;

        //    //4: moodifty view and display the result to the user
        //    //return View();
        //}

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

        public TableController(UserManager<ddacassignmentUser> usrMan)
        {
            userManager = usrMan;
        }
        //book 
        public ActionResult bookdata(string Pkey, string RKey)
        {
            CloudTable table = getTableStorageInformation();
            string errormessage = null;

            //get current username 
            var myusername = this.userManager.GetUserName(HttpContext.User);
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<ServicesEntity>(Pkey, RKey);

                //Execute the operation
                TableResult result = table.ExecuteAsync(retrieveOperation).Result;
                if (result.Etag != null)
                {
                    //asign the result to item objct
                    ServicesEntity updateEntity = (ServicesEntity)result.Result;

                    //change the description 
                    updateEntity.isBooked = true;
                    updateEntity.customerUsername = myusername;
                    updateEntity.isConfirmed = true;

                    //create the inssertorreplace tableoperation
                    TableOperation insertorReplaceOperation = TableOperation.InsertOrReplace(updateEntity);
                    var service = result.Result as ServicesEntity;
                    //execute the operation
                    TableResult resultof = table.ExecuteAsync(insertorReplaceOperation).Result;
                    // ViewBag.Result = result.HttpStatusCode;
                    return View(resultof);
                }
                else//no message found
                {
                    errormessage = "Error message";
                }
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

        public ActionResult SearchPageManager(String Message = null)
        {
            ViewBag.msg = Message;
            return View();
        }

        //how to search customer information from table
        public ActionResult getSearchManager(string PartitionKey, string RowKey)
        {
            string message = null;
            CloudTable table = getTableStorageInformation();

            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<ServicesEntity>(PartitionKey, RowKey);
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

            return RedirectToAction("SearchPage", "Table", new { Message = message });
        }

        public ActionResult viewBooked(string dialogmsg = null)
        {
            ViewBag.msg = dialogmsg;

            CloudTable table = getTableStorageInformation();
            CreateTable();

            string errormessage = null;

            //get current username
            var myusername = this.userManager.GetUserName(HttpContext.User);

            //diaplay all  the data information in a table
            try
            {
                //create query
                TableQuery<ServicesEntity> query = new TableQuery<ServicesEntity>()
                    .Where(TableQuery.GenerateFilterCondition("customerUsername", QueryComparisons.Equal, myusername));

                List<ServicesEntity> booklist = new List<ServicesEntity>();
                TableContinuationToken token = null; //to identify if there is still more data
                do
                {
                    TableQuerySegment<ServicesEntity> result = table.ExecuteQuerySegmentedAsync(query, token).Result;
                    token = result.ContinuationToken;

                    foreach (ServicesEntity slot in result.Results)
                    {
                        booklist.Add(slot);
                    }
                }
                while (token != null); //token not emplty; continue read data

                if (booklist.Count != 0)
                {
                    return View(booklist); //back to display

                }
                else
                {
                    //ViewBag.msg = "You have no bookingd yet";
                    errormessage = "No  booked slots ";
                    return RedirectToAction("Index", "Home", new { dialogmsg = errormessage });

                }

            }
            catch (Exception ex)
            {
                ViewBag.msg = "Technical Error: " + ex.ToString();
                errormessage = "No  booked slots ";
                return RedirectToAction("Index", "Home", new { dialogmsg = errormessage });
            }

            ViewBag.msg = dialogmsg;
            errormessage = "No  booked slots ";
            return RedirectToAction("Index", "Home", new { dialogmsg = errormessage });

        }

        public ActionResult viewAllBooked(string dialogmsg = null)
        {
            ViewBag.msg = dialogmsg;

            CloudTable table = getTableStorageInformation();
            CreateTable();

            string errormessage = null;

            //get current username
            var myusername = this.userManager.GetUserName(HttpContext.User);

            //diaplay all  the data information in a table
            try
            {
                //create query
                TableQuery<ServicesEntity> query = new TableQuery<ServicesEntity>()
                    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterConditionForBool("isBooked", QueryComparisons.Equal, true), TableOperators.And, TableQuery.GenerateFilterConditionForBool("isConfirmed", QueryComparisons.Equal, true)));

                List<ServicesEntity> booklist = new List<ServicesEntity>();
                TableContinuationToken token = null; //to identify if there is still more data
                do
                {
                    TableQuerySegment<ServicesEntity> result = table.ExecuteQuerySegmentedAsync(query, token).Result;
                    token = result.ContinuationToken;

                    foreach (ServicesEntity slot in result.Results)
                    {
                        booklist.Add(slot);
                    }
                }
                while (token != null); //token not emplty; continue read data

                if (booklist.Count != 0)
                {
                    return View(booklist); //back to display

                }
                else
                {
                    //ViewBag.msg = "You have no bookingd yet";
                    errormessage = "No  booked slots ";
                    return RedirectToAction("Index", "Home", new { dialogmsg = errormessage });

                }

            }
            catch (Exception ex)
            {
                ViewBag.msg = "Technical Error: " + ex.ToString();
                errormessage = "No  booked slots ";
                return RedirectToAction("Index", "Home", new { dialogmsg = errormessage });
            }

            ViewBag.msg = dialogmsg;
            errormessage = "No  booked slots ";
            return RedirectToAction("Index", "Home", new { dialogmsg = errormessage });

        }

        //private CloudBlobContainer getBlobStorageInformation()
        //{
        //    //step 1: read json
        //    var builder = new ConfigurationBuilder()
        //    .SetBasePath(Directory.GetCurrentDirectory())
        //    .AddJsonFile("appsettings.json");
        //    IConfigurationRoot configure = builder.Build();

        //    //to get key access
        //    //once link, time to read the content to get the connectionstring
        //    CloudStorageAccount objectaccount = CloudStorageAccount.Parse(configure["ConnectionStrings:storageconnection"]);

        //    //give info about the appsetting.json and read connection string
        //    CloudBlobClient blobclient = objectaccount.CreateCloudBlobClient();


        //    //step 2: how to create a new container in the blob storage account.
        //    CloudBlobContainer container = blobclient.GetContainerReference("jonreport");

        //    return container;

        //}
        //public void CreateJobContainer()
        //{
        //    //step 1:call the getBlobStroageInforemation() method  to link with the account
        //    //refer to the container
        //    CloudBlobContainer container = getBlobStorageInformation();

        //    //Step 2 : create the contaier if the system found that the name doest exist
        //    var Success = container.CreateIfNotExistsAsync().Result;

        //    //step 3: collect the container name to display in the frontend
        //    var BlobContainerName = container.Name;

        //    //4: moodifty view and display the result to the user
        //    //return View();
        //}

        public ActionResult viewAllJobs(string dialogmsg = null)
        {
            var userid = this.userManager.GetUserName(HttpContext.User);
            ddacassignmentUser user = userManager.FindByEmailAsync(userid).Result;
            string useremail = user.Email;
            ViewBag.msg = dialogmsg;
            //CloudBlobContainer container = getBlobStorageInformation();
            //table information
            CloudTable table = getTableStorageInformation();

            //CreateJobContainer();
            CreateTable();

            //create listing for your blob
            List<string> blobs = new List<string>();
            List<ServicesEntity> jobs = new List<ServicesEntity>();
            TableContinuationToken token = null; //to identify if there is still more data

            //start reading  the contents
            //BlobResultSegment result = container.ListBlobsSegmentedAsync(null).Result;

            //foreach (IListBlobItem item in result.Results)
            //{
            //    //step 4.1. check the type of the blob : block blob or directory or page block
            //    if (item.GetType() == typeof(CloudBlockBlob))
            //    {
                    //CloudBlockBlob singleblob = (CloudBlockBlob)item;
                    try
                    {
                        List<ServicesEntity> alljobs = new List<ServicesEntity>();
                TableQuery<ServicesEntity> query = new TableQuery<ServicesEntity>();
                            //.Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, useremail)
                            //,
                            //TableOperators.And,
                            //      TableQuery.GenerateFilterCondition("imagefilename", QueryComparisons.Equal, singleblob.Name)));

                        TableQuerySegment<ServicesEntity> jobResult = table.ExecuteQuerySegmentedAsync(query, token).Result;
                        token = jobResult.ContinuationToken;




                        foreach (ServicesEntity job in jobResult.Results)
                        {
                            alljobs.Add(job);
                        }
                        ServicesEntity rev = alljobs.First();

                        //rev.imagefilename = singleblob.Name + "#" + singleblob.Uri.ToString();

                        jobs.Add(rev);

                    }
                    catch (Exception ex)
                    {

                        ViewBag.msg = "Technical Error: " + ex.ToString();
                    }
                //}
            //}
            if (jobs.Count != 0)
            {
                return View(jobs); //back to display

            }
            else
            {
                dialogmsg = "Data not found!";
                return RedirectToAction("AddJobEntity", "JobReport");

            }

        }

    }
}
