using System.Net.Http.Headers;

namespace Aria2Mqtt;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        string FitbitUrl = Environment.GetEnvironmentVariable("FITBIT_URL") ?? "";
        string mqttServer = Environment.GetEnvironmentVariable("MQTT_SERVER") ?? "";
        string mqttUsername = Environment.GetEnvironmentVariable("MQTT_USER") ?? "";
        string mqttPassword = Environment.GetEnvironmentVariable("MQTT_PASSWORD") ?? "";



        await MqttFunctions.ConnectToMqttAsync(mqttServer, mqttUsername, mqttPassword);
        await MqttFunctions.PublishDiscoveryAsync();

        app.MapPost("/scale/upload", async (HttpRequest request) =>
        {
            request.EnableBuffering();

            using var memoryStream = new MemoryStream();
            await request.Body.CopyToAsync(memoryStream);
            var body = memoryStream.ToArray();
            var data = DataInterpreter.InterpretData(body);

            await MqttFunctions.PublishDataAsync(data);

            var targetUrl = FitbitUrl;
            var forwardRequest = new HttpRequestMessage(new HttpMethod(request.Method), targetUrl + request.Path + request.QueryString);

            if (request.ContentLength > 0)
            {
                forwardRequest.Content = new StreamContent(new MemoryStream(body));

                foreach (var header in request.Headers)
                {
                    forwardRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }

                forwardRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType ?? "application/octet-stream");
                forwardRequest.Content.Headers.ContentLength = request.ContentLength;
                forwardRequest.Headers.Host = request.Host.ToString();
            }

            using var client = new HttpClient();
            var response = await client.SendAsync(forwardRequest);

            foreach (var header in response.Headers)
            {
                request.HttpContext.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in response.Content.Headers)
            {
                request.HttpContext.Response.Headers[header.Key] = header.Value.ToArray();
            }

            request.HttpContext.Response.StatusCode = (int)response.StatusCode;

            var responseBody = await response.Content.ReadAsStreamAsync();
            await responseBody.CopyToAsync(request.HttpContext.Response.Body);

        });

        app.Run();
    }
}
