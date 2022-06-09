using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;


namespace MQTTDemo
{
    class Program
    {
        private static string MqttHost { get; set; } = string.Empty;
        private static int MqttPort { get; set; } = 0;
        private static string MqttClientId { get; set; } = string.Empty;
        private static string MqttDestinationEmail { get; set; } = string.Empty;

        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
            [Option('o', "host", Required = true, HelpText = "Set Host for MQTT Client")]
            public string Host { get; set; }
            [Option('p', "port", Required = true, HelpText = "Specify Port number for MQTT Client")]
            public int Port { get; set; }
            [Option('c', "client", Required = true, HelpText = "Set Client ID for MQTT Client")]
            public string Client{ get; set; }
            [Option('e', "email", Required = false, HelpText = "Set Destination Address to recieve MQTT Data over Email")]
            public string Email { get; set; }
        }

        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.Verbose)
                       {
                           Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                       }
                       if(o.Host.Length != 0)
                       {
                           Console.WriteLine(o.Host);
                           MqttHost = o.Host;
                       }
                       if (o.Port != 0)
                       {
                           Console.WriteLine(o.Port);
                           MqttPort = o.Port;
                       }
                       if (o.Client.Length != 0)
                       {
                           Console.WriteLine(o.Client);
                           MqttClientId = o.Client;
                       }
                       if(!String.IsNullOrEmpty(o.Email))
                       {
                           Console.WriteLine(o.Email);
                           MqttDestinationEmail = o.Email;
                       }
                       else
                       {
                           Console.WriteLine($"Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example!");
                       }
                   });

            MqttClient mqttClient = new MqttClient();
            mqttClient.Init(MqttHost, MqttPort, MqttClientId, MqttDestinationEmail);

            Console.Read();

            var publisher = new MqttPublisher();
            publisher.PublishMessages();

            Console.Read();
        }

    }
}