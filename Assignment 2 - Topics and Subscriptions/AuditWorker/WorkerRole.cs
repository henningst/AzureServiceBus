using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using FrontEnd.Models;

namespace AuditWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string TopicName = "OrdersTopic";

        // TopicClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        SubscriptionClient Client;
        bool IsStopped;

        public override void Run()
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            Client = SubscriptionClient.CreateFromConnectionString(connectionString, TopicName, "AuditSubscription");
            BrokeredMessage message = null;


            while (!IsStopped)
            {
                try
                {
                    // Receive the message
                    BrokeredMessage receivedMessage = null;
                    receivedMessage = Client.Receive();

                    if (receivedMessage != null)
                    {
                        // Process the message
                        Trace.WriteLine("Processing", receivedMessage.SequenceNumber.ToString());

                        OnlineOrder order = receivedMessage.GetBody<OnlineOrder>();
                        Trace.WriteLine(order.Customer + ": " + order.Product, "ProcessingMessage");

                        receivedMessage.Complete();
                    }
                }
                catch (MessagingException e)
                {
                    if (!e.IsTransient)
                    {
                        Trace.WriteLine(e.Message);
                        throw;
                    }

                    Thread.Sleep(10000);
                }
                catch (OperationCanceledException e)
                {
                    if (!IsStopped)
                    {
                        Trace.WriteLine(e.Message);
                        throw;
                    }
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.TopicExists(TopicName))
            {
                namespaceManager.CreateTopic(TopicName);
            }

            // Initialize the connection to Service Bus Queue
            //Client = QueueClient.CreateFromConnectionString(connectionString, TopicName);
            Client = SubscriptionClient.CreateFromConnectionString(connectionString, TopicName, "OrderProcessingSubscription");
            IsStopped = false;
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            IsStopped = true;
            Client.Close();
            base.OnStop();
        }
    }
}
