using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System.Net.Http;

namespace AzureFunctionApp
{
    public static class Prop_Values_From_THF_2_Factory
    {
        //Your Digital Twins URL is stored in an application setting in Azure Functions.
        private static readonly string adtServiceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");

        // The code also follows a best practice of using a single, static,
        // instance of the HttpClient
        private static readonly HttpClient httpClient = new HttpClient();

        // USES CONSUMERGROUP 'graph'
        [FunctionName("Prop_Values_From_THF_2_Factory")]
        public static async Task Run(
            [EventHubTrigger("evh-rg-az220", ConsumerGroup = "$Default", Connection = "ADT_HUB_CONNECTIONSTRING")] EventData[] events,
            ILogger log)
        {
            log.LogInformation($"Executing: {events.Length} events...");

            if (adtServiceUrl == null)
            {
                log.LogError("Application setting \"ADT_SERVICE_URL\" not set");
                return;
            }

            // As this function is processing a batch of events, a way to handle
            // errors is to create a collection to hold exceptions. The function
            // will then iterate through each event in the batch, catching
            // exceptions and adding them to the collection. At the end of the
            // function, if there are multiple exceptions, an AggregateException
            // is created with the collection, if a single exception is generated,
            // then the single exception is thrown.
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                log.LogInformation("***************");
                foreach (var p in eventData.Properties)
                {
                    log.LogInformation($"{p.Key} - {p.Value}");
                }
                log.LogInformation("***************");

                var cloudEventsType = "microsoft.iot.telemetry";
                var cloudEventsDataSchema = "dtmi:com:rg_az220:thf_sensor;1";

                if ((string)eventData.Properties["cloudEvents:type"] != cloudEventsType
                    || (string)eventData.Properties["cloudEvents:dataschema"] != cloudEventsDataSchema)
                {
                    log.LogWarning($"This function only supports cloudEvents type '{cloudEventsType}' for devices with modelId '{cloudEventsDataSchema}'");

                    await Task.Yield();
                    continue;
                }

                try
                {
                    try
                    {
                        //// Authenticate with Digital Twins

                        var cred = new DefaultAzureCredential();
                        var client = new DigitalTwinsClient(new Uri(adtServiceUrl), cred);

                        if (client != null)
                        {
                            log.LogInformation("ADT service client connection created.");

                            string twinId = eventData.Properties["cloudEvents:source"].ToString();

                            string messageBody =
                                Encoding.UTF8.GetString(
                                    eventData.Body.Array,
                                    eventData.Body.Offset,
                                    eventData.Body.Count);
                            JObject body = (JObject)JsonConvert.DeserializeObject(messageBody);

                            log.LogInformation($"Body received: {messageBody}");

                            //Find and update parent Twin
                            
                            string parentId = await AdtUtilities.FindParentAsync(client, twinId, "fac_thf", log);

                            if (parentId != null)
                            {
                                log.LogInformation($"PARENT {parentId} FOUND");

                                var temperature = body["temperature"].Value<double>();
                                var humidity = body["humidity"].Value<double>();

                                var patch = new Azure.JsonPatchDocument();

                                patch.AppendAdd<double>("/temperature", temperature); // convert the JToken value to bool
                                patch.AppendAdd<double>("/humidity", humidity); // convert the JToken value to bool

                                try
                                {
                                    log.LogInformation($"PATCHING: {patch}");
                                }
                                catch (System.Exception ex)
                                {
                                    log.LogError($"patch unavailable: {ex.Message}");
                                }

                                // deviceid IS the twinid in this case!
                                await client.UpdateDigitalTwinAsync(parentId, patch);
                            }
                            else
                            {
                                log.LogInformation($"NO PARENT FOUND");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError($"ADT service client connection failed. {e}");
                        return;
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
