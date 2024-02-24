using System;
using System.Net.Http;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Azure.Messaging.EventGrid;
using System.Text;

namespace AzureFunctionApp
{
    public class IoTHub_2_Child_GPS
    {
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("IoTHub_2_Child_GPS")]
        // While async void should generally be used with caution, it's not uncommon for Azure function apps, since the function app isn't awaiting the task.
#pragma warning disable AZF0001 // Suppress async void error
        public async void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
#pragma warning restore AZF0001 // Suppress async void error
        {
            if (adtInstanceUrl == null) log.LogError("Application setting \"ADT_SERVICE_URL\" not set");

            try
            {
                var cred = new DefaultAzureCredential();
                var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), cred);

                log.LogInformation($"");
                log.LogInformation($"Azure digital twins service client connection created.");

                if (eventGridEvent != null && eventGridEvent.Data != null)
                {
                    JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];

                    var bodyJson = Encoding.ASCII.GetString((byte[])deviceMessage["body"]);
                    JObject body = (JObject)JsonConvert.DeserializeObject(bodyJson);

                    var latitude = body["latitude"].Value<double>();
                    var longitude = body["longitude"].Value<double>();
                    var desired_latitude = body["desired_latitude"].Value<double>();
                    var desired_longitude = body["desired_longitude"].Value<double>();
                    var is_arrival = body["is_arrival"].Value<bool>();
                    var date_time = body["date_time"].Value<string>(); //{/gps_date_time:2023-10-21_10:44:39}

                    log.LogInformation($"Device: {deviceId}");
                    log.LogInformation($"");
                    log.LogInformation($"latitude: {latitude}, longitude: {longitude}, desired_latitude: {desired_latitude}," +
                        $"desired_longitude: {desired_longitude}, is_arrival: {is_arrival}, date_time: {date_time}");
                    log.LogInformation($"");

                    var patch = new Azure.JsonPatchDocument();

                    // convert the JToken value to 
                    patch.AppendAdd<double>("/latitude", latitude); 
                    patch.AppendAdd<double>("/longitude", longitude);
                    patch.AppendAdd<double>("/desired_latitude", desired_latitude);
                    patch.AppendAdd<double>("/desired_longitude", desired_longitude);
                    patch.AppendAdd<bool>("/is_arrival", is_arrival);
                    patch.AppendAdd<string>("/date_time", date_time);

                    try
                    {
                        log.LogInformation($"PATCHING: {patch.ToString()}");
                        log.LogInformation($"");
                    }
                    catch (System.Exception ex)
                    {
                        log.LogError($"patch unavailable: {ex.Message}");
                        log.LogInformation($"");
                    }

                    await client.UpdateDigitalTwinAsync(deviceId, patch);
                    await client.PublishTelemetryAsync(deviceId, null, bodyJson);
                }
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
            }
        }
    }
}