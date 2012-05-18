using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using Microsoft.ServiceBus.Messaging;

namespace WindowsAzure.ServiceBus
{
    /// <summary>
    /// Interaction logic for DeviceWindow.xaml
    /// </summary>
    public partial class DeviceWindow : Window
    {
        static string baseAddress;
        static string serviceNamespace = "<YourNamespace>";
        const string sbHostName = "servicebus.windows.net";
        static string _token;
        private string _topicName;
        private string _subscriptionName;
        private DeviceVM _deviceVm = new DeviceVM();
        private BackgroundWorker worker;

        public DeviceWindow(string topicName, string subscriptionName, string token)
        {
            InitializeComponent();
            baseAddress = "https://" + serviceNamespace + "." + sbHostName + "/";
            _token = token;
            _topicName = topicName;
            _subscriptionName = subscriptionName;
            DataContext = _deviceVm;
            worker = new BackgroundWorker();
            worker.RunWorkerAsync();
            worker.WorkerReportsProgress = true;
            worker.DoWork += bw_DoWork;
            worker.ProgressChanged += backgroundWorker_ProgressChanged;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                var msg = GetMessage(_topicName + "/Subscriptions/" + _subscriptionName);
                if (msg != null)
                {
                    try
                    {
                        worker.ReportProgress(0, msg);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {

                    }
                }
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _deviceVm.Messages.Add(e.UserState.ToString());
        }

        private static string GetMessage(string relativeAddress){
            string fullAddress = baseAddress + relativeAddress + "/messages/head" + "?timeout=60";
            Console.WriteLine("\nRetrieving message from {0}", fullAddress);
            WebClient webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Authorization] = _token;

            byte[] response = webClient.UploadData(fullAddress, "POST", new byte[0]);
            string responseStr = Encoding.UTF8.GetString(response);

            Console.WriteLine(responseStr);
            return responseStr;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
