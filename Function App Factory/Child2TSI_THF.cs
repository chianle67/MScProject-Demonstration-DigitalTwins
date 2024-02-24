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

namespace AzureFunctionApp
{
    public static class Child_THF_2_TSI
    {
        [FunctionName("Child_THF_2_TSI")]
        public static async Task Run(
            [EventHubTrigger("evh-rg-az220", Connection = "ADT_HUB_CONNECTIONSTRING")] EventData[] events,
            [EventHub("evh-tsi-rg-az220", Connection = "TSI_HUB_CONNECTIONSTRING")] IAsyncCollector<string> outputEvents,
            ILogger log)
        {

            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    if ((string)eventData.Properties["cloudEvents:type"] == "microsoft.iot.telemetry" &&
                        (string)eventData.Properties["cloudEvents:dataschema"] == "dtmi:com:rg_az220:thf_sensor;1")
                    {
                        string messageBody =
                            Encoding.UTF8.GetString(
                                eventData.Body.Array,
                                eventData.Body.Offset,
                                eventData.Body.Count);
                        JObject message = (JObject)JsonConvert.DeserializeObject(messageBody);

                        var tsiUpdate = new Dictionary<string, object>();
                        tsiUpdate.Add("$dtId", eventData.Properties["cloudEvents:source"]);

                        tsiUpdate.Add("temperature", message["temperature"]);
                        tsiUpdate.Add("desired_temperature", message["desired_temperature"]);
                        tsiUpdate.Add("humidity", message["humidity"]);
                        tsiUpdate.Add("desired_humidity", message["desired_humidity"]);

                        var tsiUpdateMessage = JsonConvert.SerializeObject(tsiUpdate);
                        log.LogInformation($"TSI event: {tsiUpdateMessage}");

                        await outputEvents.AddAsync(tsiUpdateMessage);
                    }
                    else
                    {
                        log.LogInformation($"Not THF Device telemetry");
                        await Task.Yield();
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
