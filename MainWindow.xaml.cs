
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Threading;

namespace WindowsAzure.ServiceBus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //these are the key settings from Eddie's Azure developer portal
        //by default the azure servicebus uses port 9354 so I had to use a REST-based approach

        static string serviceNamespace = "<YourNamespace>";
        static string baseAddress;
        static string token;
        const string issuer = "owner";
        const string key = "<YourKey>";
        const string sbHostName = "servicebus.windows.net";
        const string acsHostName = "accesscontrol.windows.net";
        private ServiceBusVM serviceBusVm = new ServiceBusVM();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = serviceBusVm;
            baseAddress = "https://" + serviceNamespace + "." + sbHostName + "/";
            token = GetToken();
        }

        private static string GetToken()
        {
            var acsEndpoint = "https://" + serviceNamespace + "-sb." + acsHostName + "/WRAPv0.9/";

            // Note that the realm used when requesting a token uses the HTTP scheme, even though
            // calls to the service are always issued over HTTPS
            var realm = "http://" + serviceNamespace + "." + sbHostName + "/";

            var values = new NameValueCollection();
            values.Add("wrap_name", issuer);
            values.Add("wrap_password", key);
            values.Add("wrap_scope", realm);

            var webClient = new WebClient();
            byte[] response = webClient.UploadValues(acsEndpoint, values);

            string responseString = Encoding.UTF8.GetString(response);

            var responseProperties = responseString.Split('&');
            var tokenProperty = responseProperties[0].Split('=');
            var token = Uri.UnescapeDataString(tokenProperty[1]);

            return "WRAP access_token=\"" + token + "\"";
        }

        private void QueueMessages(object sender, RoutedEventArgs e)
        {
            CreateRESTfulQueue();
            string fullAddress = baseAddress + "TestQueue" + "/messages" + "?timeout=60";
            for (int i = 0; i < 5; i++)
            {
                SendMessage(fullAddress, "Message #:" + i.ToString(CultureInfo.InvariantCulture));
                serviceBusVm.Messages.Add("Message #:" + i.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static void SendMessage(string relativeAddress, string body)
        {
            Console.WriteLine("\nSending message {0} - to address {1}", body, relativeAddress);
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = token;
            webClient.UploadData(relativeAddress, "POST", Encoding.UTF8.GetBytes(body));
        }

        private void CreateTopicAndSubScription(object sender, RoutedEventArgs e)
        {
            string topicName = "Topic" + Guid.NewGuid().ToString();
            string subscriptionName = "Subscription" + Guid.NewGuid().ToString();
            CreateTopic(topicName);
            serviceBusVm.Topics.Add(topicName);
            CreateSubscription(topicName, subscriptionName);
            serviceBusVm.Subscriptions.Add(subscriptionName);
            MessageBox.Show("Successfully created the Topic (DeviceId) of " + topicName + "with the subscription of " +subscriptionName);
        }

        private static string CreateTopic(string topicName)
        {
            var topicAddress = baseAddress + topicName;
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = token;

            Console.WriteLine("\nCreating topic {0}", topicAddress);
            // Prepare the body of the create queue request
             var putData = @"<entry xmlns=""http://www.w3.org/2005/Atom"">
                                <title type=""text"">" + topicName + @"</title>
                                <content type=""application/xml"">
                                    <TopicDescription xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.microsoft.com/netservices/2010/10/servicebus/connect"" />
                                </content>
                                </entry>";

            byte[] response = webClient.UploadData(topicAddress, "PUT", Encoding.UTF8.GetBytes(putData));
            return Encoding.UTF8.GetString(response);
        }

        private static string CreateSubscription(string topicName, string subScriptionName)
        {
            var subscriptionAddress = baseAddress + topicName + "/Subscriptions/" + subScriptionName;
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = token;

            Console.WriteLine("\nCreating subscription {0}", subscriptionAddress);
            // Prepare the body of the create queue request
            var putData = @"<entry xmlns=""http://www.w3.org/2005/Atom"">
                                <title type=""text"">" + subScriptionName + @"</title>
                                <content type=""application/xml"">
                                <SubscriptionDescription xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.microsoft.com/netservices/2010/10/servicebus/connect"" />
                                </content>
                            </entry>";

            byte[] response = webClient.UploadData(subscriptionAddress, "PUT", Encoding.UTF8.GetBytes(putData));
            return Encoding.UTF8.GetString(response);
        }

        private static void CreateRESTfulQueue()
        {
            // Restful creation of the Queue
            // Create the URI of the new Queue, note that this uses the HTTPS scheme
            var queueAddress = baseAddress + "TestQueue";
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = token;

            var putData = @"<entry xmlns=""http://www.w3.org/2005/Atom"">
                                <title type=""text"">"+"TestQueue"+@"</title>
                                <content type=""application/xml"">
                                <QueueDescription xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.microsoft.com/netservices/2010/10/servicebus/connect"" />
                                </content>
                            </entry>";
            try
            {
                byte[] response = webClient.UploadData(queueAddress, "PUT", Encoding.UTF8.GetBytes(putData));
            }
            catch (Exception ex)
            {
                //eating for now, prob means we already have the queue out there - a 409 conflict means it was created already
            }
        }

        private void ReceiveAndDeleteMessage(object sender, RoutedEventArgs e)
        {
            string fullAddress = baseAddress + "TestQueue" + "/messages/head" + "?timeout=60";
            Console.WriteLine("\nRetrieving message from {0}", fullAddress);
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = token;

            // this will just remove one message from the queue
            string response = webClient.UploadString(fullAddress, "DELETE", String.Empty);
        }

        private void ReceiveAndShowMessage(object sender, RoutedEventArgs e)
        {
            string fullAddress = baseAddress + "TestQueue" + "/messages/head" + "?timeout=60";
            Console.WriteLine("\nRetrieving message from {0}", fullAddress);
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = token;

            // this will show the message
            MessageBox.Show(webClient.UploadString(fullAddress, "POST", String.Empty), "Message in Queue");
        }

        private void DeleteQueue(object sender, RoutedEventArgs e)
        {
            string fullAddress = baseAddress + "TestQueue";
            Console.WriteLine("\nRetrieving message from {0}", fullAddress);
            var webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = token;

            // this will delete the entire queue
            string response = webClient.UploadString(fullAddress, "DELETE", String.Empty);
            MessageBox.Show("Queue " + response + " Deleted");
        }

        private void SendToTopic(object sender, RoutedEventArgs e)
        {
            if (Topics.SelectedItem != null)
            {
                string fullAddress = baseAddress + Topics.SelectedItem + "/messages" + "?timeout=60";
                SendMessage(fullAddress, "Broadcasted Message" + Guid.NewGuid().ToString());
            }
        }

        private void NewDevice(object sender, RoutedEventArgs e)
        {
            if (Topics.SelectedItem != null)
            {
                var device = new DeviceWindow(Topics.SelectedItem.ToString(), Subscriptions.SelectedItem.ToString(), token);
                device.Show();
            }
        }
    }
}
