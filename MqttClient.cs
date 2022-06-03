using System;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Text;
using System.Threading.Tasks;

namespace MQTTDemo
{
    class MqttClient
    {
        private IMqttFactory factory = new MqttFactory();
        private IMqttClient? mqttClient;

        public async void Init(string host, int port, string clientId, string email)
        {
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(host, port)
                .WithCleanSession()
                .Build();

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine($"{DateTime.Now} " +
                    $"{e.ApplicationMessage.Topic}; " +
                    $"{Encoding.UTF8.GetString(e.ApplicationMessage.Payload)};");

                Task.Run(() => mqttClient.PublishAsync("topic/*", "refresh"));

                if(!String.IsNullOrEmpty(email))
                {
                    // Send Email
                    SendEmail(email);
                }
            });

            mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                // Subscribe to a topic
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("topic/rechnung").Build());
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("topic/position").Build());

                Console.WriteLine("### SUBSCRIBED ###");
            });

            await mqttClient.ConnectAsync(options, CancellationToken.None);
        }

        private void SendEmail(string destination)
        {

        }
    }
}