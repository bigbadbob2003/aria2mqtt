using System.Text.Json;
using System.Text.Json.Serialization;
using MQTTnet;
using MQTTnet.Client;

namespace Aria2Mqtt;

public static class MqttFunctions
{
    static MqttFactory mqttFactory = new();
    static IMqttClient mqttClient = mqttFactory.CreateMqttClient();

    public static async Task ConnectToMqttAsync(string mqttServer, string mqttUsername, string mqttPassword)
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttServer)
            .WithClientId("Aria")
            .WithCredentials(mqttUsername, mqttPassword)
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
    }

    public async static Task PublishDataAsync(DataInterpretation data)
    {
        if (data.Readings.Count == 0)
        {
            return;
        }

        var dr = data.Readings.OrderBy(x => x.Date).Last();
        var mqttState = new MqttState
        {
            WeightKG = dr.WeightKg,
            WeightLBS = dr.WeightLbs,
            WeightST = dr.WeightSt,
            Battery = data.Battery
        };

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("homeassistant/sensor/aria/state")
            .WithPayload(JsonSerializer.Serialize(mqttState))
            .Build();

        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }


    public static async Task PublishDiscoveryAsync()
    {
        var homeassistantDiscoveryLb = new HomeassistantDiscovery
        {
            Name = "Weight lb",
            UniqueId = "aria_lb",
            StateTopic = "homeassistant/sensor/aria/state",
            UnitOfMeasurement = "lb",
            ValueTemplate = "{{ value_json.weight_lb | round(2) }}",
            DeviceClass = "weight",
            Device = new HomeassistantDevice
            {
                Identifiers = ["aria"],
                Name = "Aria Scales",
                Manufacturer = "Fitbit",
                Model = "Aria"
            }
        };

        var homeassistantDiscoveryKg = new HomeassistantDiscovery
        {
            Name = "Weight kg",
            UniqueId = "aria_kg",
            StateTopic = "homeassistant/sensor/aria/state",
            UnitOfMeasurement = "kg",
            ValueTemplate = "{{ value_json.weight_kg | round(2)  }}",
            DeviceClass = "weight",
            Device = new HomeassistantDevice
            {
                Identifiers = ["aria"],
                Name = "Aria Scales",
                Manufacturer = "Fitbit",
                Model = "Aria"
            }
        };

        var homeassistantDiscoverySt = new HomeassistantDiscovery
        {
            Name = "Weight st",
            UniqueId = "aria_st",
            StateTopic = "homeassistant/sensor/aria/state",
            UnitOfMeasurement = "st",
            ValueTemplate = "{{ value_json.weight_st | round(2)  }}",
            DeviceClass = "weight",
            Device = new HomeassistantDevice
            {
                Identifiers = ["aria"],
                Name = "Aria Scales",
                Manufacturer = "Fitbit",
                Model = "Aria"
            }
        };

        var homeassistantDiscoveryBatt = new HomeassistantDiscovery
        {
            Name = "Battery",
            UniqueId = "aria_batt",
            StateTopic = "homeassistant/sensor/aria/state",
            UnitOfMeasurement = "%",
            ValueTemplate = "{{ value_json.battery }}",
            DeviceClass = "battery",
            Device = new HomeassistantDevice
            {
                Identifiers = ["aria"],
                Name = "Aria Scales",
                Manufacturer = "Fitbit",
                Model = "Aria"
            }
        };


        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("homeassistant/sensor/arialb/config")
            .WithPayload(JsonSerializer.Serialize(homeassistantDiscoveryLb))
            .Build();

        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

        applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("homeassistant/sensor/ariakg/config")
            .WithPayload(JsonSerializer.Serialize(homeassistantDiscoveryKg))
            .Build();

        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

        applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("homeassistant/sensor/ariast/config")
            .WithPayload(JsonSerializer.Serialize(homeassistantDiscoverySt))
            .Build();

        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

        applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("homeassistant/sensor/ariabatt/config")
            .WithPayload(JsonSerializer.Serialize(homeassistantDiscoveryBatt))
            .Build();

        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }

}


public class MqttState
{
    [JsonPropertyName("weight_kg")]
    public double WeightKG { get; set; }

    [JsonPropertyName("weight_lb")]
    public double WeightLBS { get; set; }

    [JsonPropertyName("weight_st")]
    public double WeightST { get; set; }

    [JsonPropertyName("battery")]
    public uint Battery { get; set; }
}

public class HomeassistantDiscovery
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("unique_id")]
    public string? UniqueId { get; set; }

    [JsonPropertyName("state_topic")]
    public string? StateTopic { get; set; }

    [JsonPropertyName("unit_of_measurement")]
    public string? UnitOfMeasurement { get; set; }

    [JsonPropertyName("value_template")]
    public string? ValueTemplate { get; set; }

    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; set; }

    [JsonPropertyName("device")]
    public HomeassistantDevice? Device { get; set; }

}

public class HomeassistantDevice
{
    [JsonPropertyName("identifiers")]
    public string[]? Identifiers { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }
}