using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System.Net.Http;
using AzureFunctionApp;

namespace AzureFunctionApp
{
    public static class Prop_Alert_From_THF_2_Warehouse
    {
        //Your Digital Twins URL is stored in an application setting in Azure Functions.
        private static readonly string adtServiceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");

        // The code also follows a best practice of using a single, static,
        // instance of the HttpClient
        private static readonly HttpClient httpClient = new HttpClient();

        // USES CONSUMERGROUP 'graph'
        [FunctionName("Prop_Alert_From_THF_2_Warehouse")]
        public static async Task Run(
            [EventHubTrigger("evh-rg-az220-prop-alert", ConsumerGroup = "$Default", Connection = "ADT_HUB_PROP_CONNECTIONSTRING")] EventData[] events,
            ILogger log)
        {
            log.LogInformation($"Executing: {events.Length} events...");

            if (adtServiceUrl == null)
            {
                log.LogError("Application setting \"ADT_SERVICE_URL\" not set");
                return;
            }

            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                log.LogInformation("***************");
                foreach (var p in eventData.Properties)
                {
                    log.LogInformation($"{p.Key} - {p.Value}");
                }
                log.LogInformation("***************");

                var cloudEventsType = "Microsoft.DigitalTwins.Twin.Update";
                var cloudEventsDataSchema = "dtmi:com:rg_az220:thf_sensor;1";

                if ((string)eventData.Properties["cloudEvents:type"] != cloudEventsType)
                {
                    log.LogWarning($"This function only supports cloudEvents type '{cloudEventsType}'");
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

                            string twinId = eventData.Properties["cloudEvents:subject"].ToString();

                            string messageBody =
                                Encoding.UTF8.GetString(
                                    eventData.Body.Array,
                                    eventData.Body.Offset,
                                    eventData.Body.Count);

                            log.LogInformation($"'{twinId}'     Body received: {messageBody}");

                            var convertedPatch = PatchConverter.GetPatch(messageBody);

                            if (convertedPatch.modelId != cloudEventsDataSchema)
                            {
                                log.LogWarning($"This function only supports cloudevents dataschema '{cloudEventsDataSchema}'");

                                await Task.Yield();
                                continue;
                            }

                            //Find and update parent Twin
                            string parentId = await AdtUtilities.FindParentAsync(client, twinId, "ware_thf", log);

                            if (parentId != null)
                            {
                                log.LogInformation($"PARENT {parentId} FOUND");

                                var patch = new Azure.JsonPatchDocument();

                                patch.AppendAdd("/fan_alert", Convert.ToBoolean(convertedPatch.PatchItems.First(x => x.path == "/fan_alert").value));
                                patch.AppendAdd("/temperature_alert", Convert.ToBoolean(convertedPatch.PatchItems.First(x => x.path == "/temperature_alert").value));
                                patch.AppendAdd("/humidity_alert", Convert.ToBoolean(convertedPatch.PatchItems.First(x => x.path == "/humidity_alert").value));

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
