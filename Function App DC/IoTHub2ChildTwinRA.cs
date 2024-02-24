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
    public class IoTHub_2_Child_RArm
    {
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("IoTHub_2_Child_RArm")]
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

                    var desired_power = body["desired_power"].Value<double>();
                    var desired_hydraulic_pressure = body["desired_hydraulic_pressure"].Value<double>();
                    var power = body["power"].Value<double>();
                    var hydraulic_pressure = body["hydraulic_pressure"].Value<double>();

                    var power_alert = body["power_alert"].Value<bool>();
                    var hydraulic_pressure_alert = body["hydraulic_pressure_alert"].Value<bool>();
                    var overload_alert = body["overload_alert"].Value<bool>();

                    log.LogInformation($"Device: {deviceId}");
                    log.LogInformation($"");
                    log.LogInformation($"desired_power: {desired_power}, desired_hydraulic_pressure: {desired_hydraulic_pressure}, power: {power}," +
                        $"hydraulic_pressure: {hydraulic_pressure}, power_alert: {power_alert}, hydraulic_pressure_alert: {hydraulic_pressure_alert}," +
                        $"overload_alert: {overload_alert}");
                    log.LogInformation($"");

                    var patch = new Azure.JsonPatchDocument();

                    // convert the JToken value to 
                    patch.AppendAdd<double>("/desired_power", desired_power); 
                    patch.AppendAdd<double>("/desired_hydraulic_pressure", desired_hydraulic_pressure);
                    patch.AppendAdd<double>("/power", power);
                    patch.AppendAdd<double>("/hydraulic_pressure", hydraulic_pressure);

                    patch.AppendAdd<bool>("/power_alert", power_alert);
                    patch.AppendAdd<bool>("/hydraulic_pressure_alert", hydraulic_pressure_alert);
                    patch.AppendAdd<bool>("/overload_alert", overload_alert);

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