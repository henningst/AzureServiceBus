using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;


namespace FrontendWebRole
{
    public static class QueueConnector
    {
        // Thread-safe. Recommended that you cache rather than recreating it
        // on every request.
        public static TopicClient OrdersTopicClient;


        // Obtain these values from the Management Portal
        public const string Namespace = "[yournamespace]";
        public const string IssuerName = "[yourissuer]";
        public const string IssuerKey = "[yoursecretkey]";


        // The name of your queue
        public const string TopicName = "OrdersTopic";


        public static NamespaceManager CreateNamespaceManager()
        {
            // Create the namespace manager which gives you access to
            // management operations
            var uri = ServiceBusEnvironment.CreateServiceUri(
                "sb", Namespace, String.Empty);
            var tP = TokenProvider.CreateSharedSecretTokenProvider(
                IssuerName, IssuerKey);
            return new NamespaceManager(uri, tP);
        }


        public static void Initialize()
        {
            // Using Http to be friendly with outbound firewalls
            ServiceBusEnvironment.SystemConnectivity.Mode =
                ConnectivityMode.Http;


            // Create the namespace manager which gives you access to 
            // management operations
            var namespaceManager = CreateNamespaceManager();


            // Create the topic if it does not exist already
            if (!namespaceManager.TopicExists(TopicName))
            {
                namespaceManager.CreateTopic(TopicName);
            }

            var topic = namespaceManager.GetTopic(TopicName);

            // Create subscriptions
            if (!namespaceManager.SubscriptionExists(topic.Path, "OrderProcessingSubscription"))
            {
                SubscriptionDescription orderSubscription = namespaceManager.CreateSubscription(topic.Path,
                                                                                                "OrderProcessingSubscription");
            }

            if (!namespaceManager.SubscriptionExists(topic.Path, "AuditSubscription"))
            {
                SubscriptionDescription auditSubscription = namespaceManager.CreateSubscription(topic.Path,
                                                                                                "AuditSubscription");
            }



            // Get a client to the queue
            var messagingFactory = MessagingFactory.Create(
                namespaceManager.Address,
                namespaceManager.Settings.TokenProvider);
            OrdersTopicClient = messagingFactory.CreateTopicClient(
                "OrdersTopic");
        }
    }


}