using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using FrontendWebRole;

namespace FrontEnd.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // GET: /Home/Submit
        // Controller method for a view you will create for the submission
        // form
        public ActionResult Submit()
        {
            // Will put code for displaying queue message count here.
            // Get a NamespaceManager which allows you to perform management and
            // diagnostic operations on your Service Bus Queues.
            var namespaceManager = QueueConnector.CreateNamespaceManager();


            // Get the queue, and obtain the message count.
            var queue = namespaceManager.GetTopic(QueueConnector.TopicName);
            ViewBag.MessageCount = queue.MessageCountDetails.ActiveMessageCount;

            return View();
        }


        // POST: /Home/Submit
        // Controler method for handling submissions from the submission
        // form 
        [HttpPost]
        public ActionResult Submit(OnlineOrder order)
        {
            if (ModelState.IsValid)
            {
                // Create a message from the order
                var message = new BrokeredMessage(order);


                // Submit the order
                QueueConnector.OrdersTopicClient.Send(message);
                return RedirectToAction("Submit");
            }
            else
            {
                return View(order);
            }
        }
    }
}
