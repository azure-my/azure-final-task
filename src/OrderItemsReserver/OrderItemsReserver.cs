using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Models;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace OrderItemsReserver
{
    public static class OrderItemsReserver
    {
        // hardcoded due to lack of C# knowlage
        private const string URL = "https://eshop-error-handler.azurewebsites.net:443/api/final-task/triggers/manual/invoke?api-version=2022-05-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=75RNE4bnkV31LokuE-eL1OlOPU1iFJDdIzFGQipPOsE";

        [FunctionName("OrderItemsReserver")]
        public static void Run(
            [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string myQueueItem,
            Int32 deliveryCount,
            DateTime enqueuedTimeUtc,
            string messageId,
            [Blob("orders", Connection = "BlobConnection")] BlobContainerClient container,
            ILogger log)
        {
            log.LogInformation("eShop new order!!!");


            try
            {
                container.CreateIfNotExistsAsync();
                var blobName = Guid.NewGuid().ToString();
                var appendBlob = container.GetAppendBlobClient(blobName);
                appendBlob.CreateIfNotExists();
                appendBlob.SetHttpHeaders(new BlobHttpHeaders() { ContentType = "application/json" });


                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(myQueueItem)))
                {
                    appendBlob.AppendBlock(ms);
                }

            }
            catch (Exception e)
            {
                log.LogInformation("Error!!!" + e.Message);

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var result = client.PostAsync(URL, new StringContent(myQueueItem, Encoding.UTF8, "application/json"));
            }
        }
    }
}
