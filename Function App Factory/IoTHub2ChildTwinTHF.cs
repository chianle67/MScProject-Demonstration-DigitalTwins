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
    public class IoTHub_2_Child_THF
    {
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("IoTHub_2_Child_THF")]
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

                    var temperature = body["temperature"].Value<double>();
                    var humidity = body["humidity"].Value<int>();
                    var desired_temperature = body["desired_temperature"].Value<double>();
                    var desired_humidity = body["desired_humidity"].Value<int>();
                    var temperature_alert = body["temperature_alert"].Value<bool>();
                    var humidity_alert = body["humidity_alert"].Value<bool>();
                    var fan_alert = body["fan_alert"].Value<bool>();

                    log.LogInformation($"Device: {deviceId}");
                    log.LogInformation($"");
                    log.LogInformation($"temperature: {temperature}, humidity: {humidity}, desired_temperature: {desired_temperature}," +
                        $"desired_humidity: {desired_humidity}, temperature_alert: {temperature_alert}, humidity_alert: {humidity_alert}" +
                        $"fan_alert: {fan_alert}");
                    log.LogInformation($"");

                    var patch = new Azure.JsonPatchDocument();

                    // convert the JToken value to 
                    patch.AppendAdd<double>("/temperature", temperature); 
                    patch.AppendAdd<int>("/humidity", humidity);
                    patch.AppendAdd<double>("/desired_temperature", desired_temperature);
                    patch.AppendAdd<int>("/desired_humidity", desired_humidity);
                    patch.AppendAdd<bool>("/temperature_alert", temperature_alert);
                    patch.AppendAdd<bool>("/humidity_alert", humidity_alert);
                    patch.AppendAdd<bool>("/fan_alert", fan_alert);

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