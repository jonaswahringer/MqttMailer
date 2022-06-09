using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTDemo.Model;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTDemo
{
    class MqttClient
    {
        private IMqttFactory factory = new MqttFactory();
        private IMqttClient? mqttClient;

        private Invoice InvoiceForEmail { get; set; }

        public async void Init(string host, int port, string clientId, string email)
        {
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(host, port)
                .WithCleanSession()
                .Build();

            mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                Console.WriteLine($"{DateTime.Now} " +
                    $"{e.ApplicationMessage.Topic}; " +
                    $"{Encoding.UTF8.GetString(e.ApplicationMessage.Payload)};");

                //Task.Run(() => mqttClient.PublishAsync("topic/*", "refresh"));

                if(!String.IsNullOrEmpty(email))
                {
                    // Wait 3 seconds for associated positions
                    if (e.ApplicationMessage.Topic.Equals("topic/invoice"))
                    {
                        // set invoice properties for mail
                        InvoiceForEmail = JsonConvert.DeserializeObject<Invoice>(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                    }

                    if (e.ApplicationMessage.Topic.Equals("topic/position"))
                    {
                           // set position properties for mail
                    }

                    // Send Email
                    SendEmail(email, payload);
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

        private async Task BuildMailAsync(Invoice invoice)
        {
            await Task.Delay(3000);
        }

        private void SendEmail(string destination, byte[] payload)
        {
            var rechnung = JsonConvert.DeserializeObject<Invoice>(Encoding.UTF8.GetString(payload));
            Console.WriteLine(rechnung);
        }
    }
}