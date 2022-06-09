using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTDemo.Model;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Mail;
using CommandLine;
using System.Net;
using System.Linq;
using System.Security.Cryptography;

namespace MQTTDemo
{
    class MqttClient
    {
        private IMqttFactory factory = new MqttFactory();
        private IMqttClient? mqttClient;

        private Invoice InvoiceForEmail { get; set; }
        private List<InvoicePosition> PositionsForEmail { get; set; } = new List<InvoicePosition>();

        private const string SourceMailAddress = "jonas.wahringer@sz-ybbs.ac.at";
        private const string SourceMailPassword = "Jy67LfOo";

        public async void Init(string host, int port, string clientId, string email) 
        {
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(host, port)
                .WithWillMessage(new MqttApplicationMessage
                {
                    Topic = "invoice/rechnung",
                    Payload = Encoding.UTF8.GetBytes(clientId + " offline"),
                    QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce,
                    Retain = false
                })
                .WithCleanSession()
                .Build();

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                if(!String.IsNullOrEmpty(email))
                {
                    Console.WriteLine($"{DateTime.Now} " + $"{e.ApplicationMessage.Topic}; " +
                            $"{Encoding.UTF8.GetString(e.ApplicationMessage.Payload)};");

                    if (e.ApplicationMessage.Topic.Equals("invoice/rechnung"))
                    {
                        // set invoice properties for mail
                        try
                        {
                            InvoiceForEmail = JsonConvert.DeserializeObject<Invoice>(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }
                        Console.WriteLine("Sending email ...");
                        SendEmail(email);
                    }

                    if (e.ApplicationMessage.Topic.Equals("invoice/position"))
                    {
                        // set position properties for mail
                        InvoicePosition position = null;
                        try
                        {
                            position = JsonConvert.DeserializeObject<InvoicePosition>(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }

                        if (position.InvoiceId == InvoiceForEmail.Id)
                        {
                            PositionsForEmail.Add(position);
                        }
                    }
                }
            });

            mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("invoice/rechnung").Build());
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("invoice/position").Build());
                Console.WriteLine("### SUBSCRIBED ###");
            });

            await mqttClient.ConnectAsync(options, CancellationToken.None);
        }

        private async Task SendEmail(string DestinationMailAddress)
        {
            Console.WriteLine(DestinationMailAddress);
            Console.WriteLine(InvoiceForEmail.Id);

            var mailClient = createClient();
            Console.WriteLine("Mail Client Created");
            
            await Task.Delay(3000);
            
            Console.WriteLine(PositionsForEmail.Count);

            try
            {
                mailClient.Send(createMessage(DestinationMailAddress));
            }
            catch(Exception mailx)
            {
                Console.WriteLine(mailx.Message);
            }
            
            Console.WriteLine("Email sent.");
        }

        private SmtpClient createClient()
        {
            Console.WriteLine("In Create Client");
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(SourceMailAddress, SourceMailPassword);
            return smtp;
        }

        private MailMessage createMessage(string DestinationMailAddress)
        {
            Console.WriteLine("Creating Mail Message ...");
            MailMessage mail = new MailMessage(SourceMailAddress, DestinationMailAddress);
            
            mail.From = new MailAddress(SourceMailAddress);
            mail.Subject = "New Invoices";
            mail.Body = createMessageBody(); //Invoices and Positions
            mail.IsBodyHtml = false;
            return mail;
        }

        private string createMessageBody()
        {
            string body = "Sie haben eine neue Rechnung vom " + InvoiceForEmail.InvoiceDate.ToLocalTime().ToLongDateString() + "\n";
            body += "   Rechnungsidentifikation: " + InvoiceForEmail.Id.ToString() + "\n";
            body += "   Betrag: " + InvoiceForEmail.Vat.ToString() + "\n";
            body += "\n";

            if (PositionsForEmail.Count != 0)
            {
                body += "Die zugehörigen Positionen sind folgende: \n";
                foreach(var pos in PositionsForEmail)
                {
                    body += "   Positionidentifikation: " + pos.Id + "\n";
                    body += "   Positionsnummer: " + pos.ItemNr + "\n";
                    body += "   Preis: " + pos.Price + "\n";
                    body += "   Anzahl: " + pos.Qty + "\n";
                    body += "   Gesamtpreis: " + pos.PriceOverall + "\n";
                }
                body += "\n\n";
            }
            return body;
        }

        /*
        private MqttApplicationMessage createLastWillMessage(string client)
        {
            var lastWill = new MqttApplicationMessage();

            lastWill.Payload = Encoding.ASCII.GetBytes("Client: " + client + "disconnected!");
            lastWill.QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce;
            lastWill.Topic = "invoice/*";
            lastWill.Retain = true;
            return lastWill;
        }
        */
    }
}