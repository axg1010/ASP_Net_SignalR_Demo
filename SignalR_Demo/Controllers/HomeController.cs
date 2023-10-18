using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using SignalR_Demo.Hubs;

namespace SignalR_Demo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //var hubContext = GlobalHost.ConnectionManager.GetHubContext<PhoneNumberHub>();
            //hubContext.Clients.All.UpdatePhoneNumber();
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}