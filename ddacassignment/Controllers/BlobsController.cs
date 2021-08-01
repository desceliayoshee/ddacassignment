using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Table;
using ddacassignment.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using ddacassignment.Models;

namespace ddacassignment.Controllers
{
    public class BlobsController : Controller
    {
        private CloudBlobContainer getCloudContainerInformation()
        {


            //step1: read content from appsettings.json
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build();

            //step2: get access key from appsettings.json and put inside the code
            CloudStorageAccount accountdetails = CloudStorageAccount.Parse(configure["ConnectionStrings:blobstorageconnection"]);

            //step3: how to refer to an existing/new container in the blob storage account
            CloudBlobClient clientagent = accountdetails.CreateCloudBlobClient();
            CloudBlobContainer container = clientagent.GetContainerReference("storageblob");
            
            return container;
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
            CloudTable table = tableClient.GetTableReference("Review");
            return table;
        }

        //2
        public ActionResult CreateContainer()
        {
            // link to the correct storage account first and container name first
            CloudBlobContainer container = getCloudContainerInformation();

            // see whether need to create a new container or not
            ViewBag.result = container.CreateIfNotExistsAsync().Result;

            // create container name to display the name in the view
            // to tell user success or not
            ViewBag.containerName = container.Name;

            // return the result to the front end
            return View();
        }

        // 3 
        public String UploadImage()
        {
            CloudBlobContainer container = getCloudContainerInformation();
            CloudBlockBlob blobItem = null;

            try
            {
                var filestream = System.IO.File.OpenRead(@"");
                blobItem = container.GetBlockBlobReference("Mytextfile" + Path.GetExtension(filestream.Name));
                blobItem.UploadFromStreamAsync(filestream).Wait();
            }
            catch(Exception ex)
            {
                return "Technical issue: " + ex.ToString() + "Please upload again";
            }
            return blobItem.Name + " is successfully uploaded to Blob Storage. The URL is: " + blobItem.Uri;
        }

        // 4
        public ActionResult UploadFiles(string Message = null)
        {
            ViewBag.msg = Message;
            return View();
        }

        [HttpPost]
        public ActionResult UploadFiles(List<IFormFile> files)
        {
            string message = null;

            CloudBlobContainer container = getCloudContainerInformation();
            CloudBlockBlob blobItem = null;

            foreach(var item in files)
            {
                try
                {
                    blobItem = container.GetBlockBlobReference(item.FileName);
                    var stream = item.OpenReadStream();
                    blobItem.UploadFromStreamAsync(stream).Wait();
                    message = message + blobItem.Name + " is uploaded to Blob storage ";
                }
                catch (Exception ex)
                {
                    message = message + blobItem.Name + "Unable to upload to blob storage";
                    message = message + "Technical issue: " + ex.ToString() + "Please upload again";
                }
            }
            
            return RedirectToAction("UploadFiles", "Blobs", new { Message = message });
        }

        public ActionResult AddReview()
        {
            return View();
        }
        public ActionResult AddNewReview()
        {
            return View();
        }

        // 5
        public ActionResult DisplayImage()
        {
            return null;
        }

        public ActionResult CreateReviewTable()
        {

            //refer to the container
            CloudTable table = getTableStorageInformation();
            ViewBag.Success = table.CreateIfNotExistsAsync().Result;
            ViewBag.TableName = table.Name;
            return View();
        }

        private UserManager<ddacassignmentUser> userManager;

        public BlobsController(UserManager<ddacassignmentUser> userMan)
        {
            userManager = userMan;
        }

        public ActionResult test(string PartitionKey, string Rowkey, string rating, string imageFileName, IFormFile imageFile)
        {
            CloudTable table = getTableStorageInformation();
            CloudBlobContainer container = getCloudContainerInformation();
            /*CreateContainer();
            CreateReviewTable();*/

            var myusername = this.userManager.GetUserName(HttpContext.User);
            
            if(imageFile != null && imageFile.Length > 0)
            {
                CloudBlockBlob item = container.GetBlockBlobReference(imageFile.FileName);
                item.Properties.ContentType = imageFile.ContentType;
                item.UploadFromStreamAsync(imageFile.OpenReadStream()).Wait();

                imageFileName = imageFile.FileName;
                
                
            }
            return View();
        }
        [HttpPost]
        public ActionResult AddSingleEntity2(string PartitionKey, string RowKey, string Content, string Rating, string ImageFileName, IFormFile img)
        {

            //refer to the table 
            CloudTable table = getTableStorageInformation();
            /*CloudBlobContainer container = getCloudContainerInformation();*/

            /*CreateContainer();
            CreateReviewTable();*/

            // error nya disini ternyata
            /*var myusername = this.userManager.GetUserName(HttpContext.User);

            PartitionKey = myusername;*/


                /*CloudBlockBlob blobItem = container.GetBlockBlobReference(img.FileName);
                var stream = img.OpenReadStream();
                blobItem.UploadFromStreamAsync(stream).Wait();

                ImageFileName = img.FileName;*/

                ReviewEntity review = new ReviewEntity(PartitionKey, RowKey);

                review.Content = Content;
                review.Rating = Rating;
                
                try
                {
                    TableOperation insertOperation = TableOperation.Insert(review);
                    TableResult result = table.ExecuteAsync(insertOperation).Result;

                    ViewBag.TableName = table.Name;
                    ViewBag.SuccessCode = result.HttpStatusCode;
                    /*return View(result);*/
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Errornya di: " + ex.ToString();
                }

            return View();
        }

        public ActionResult ViewAllReview(string dialogmsg = null)
        {
            CloudBlobContainer container = getCloudContainerInformation();
            CloudTable table = getTableStorageInformation();
            CreateContainer();
            CreateReviewTable();

            List<ReviewEntity> reviews = new List<ReviewEntity>();
            TableContinuationToken token = null;

            BlobResultSegment result = container.ListBlobsSegmentedAsync(null).Result;
            foreach(IListBlobItem item in result.Results)
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob singleblob = (CloudBlockBlob)item;
                    try
                    {
                        List<ReviewEntity> filReviews = new List<ReviewEntity>();

                        TableQuery<ReviewEntity> query = new TableQuery<ReviewEntity>()
                            .Where(TableQuery.GenerateFilterCondition("imageFilename", QueryComparisons.Equal, singleblob.Name));
                        TableQuerySegment<ReviewEntity> reviewResult = table.ExecuteQuerySegmentedAsync(query, token).Result;
                        token = reviewResult.ContinuationToken;
                        
                        foreach(ReviewEntity review in reviewResult.Results)
                        {
                            filReviews.Add(review);
                        }
                        ReviewEntity rev = filReviews.First();
                        rev.ImageFilename = singleblob.Name + "#" + singleblob.Uri.ToString();
                        reviews.Add(rev);
                    }
                    catch(Exception ex)
                    {
                        ViewBag.msg = "Technical Error: " + ex.ToString();
                        string errorMessage = "No reviews added";
                        return RedirectToAction("", "Review", new { dialogmsg = errorMessage });  
                    }

                }
            }

            if (reviews.Count != 0)
            {
                return View(reviews); // back to display
            }
            else
            {
                return RedirectToAction("", "Review");
            }
        }
    }
}

/*try
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
}*/