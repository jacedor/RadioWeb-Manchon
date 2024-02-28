using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using RadioWeb.Repositories;

namespace RadioWeb.Controllers
{
    [Authorize]
    public class RemesasController : Controller
    {
        private RadioDBContext db = new RadioDBContext();

        public RemesasController()
        {
        }


        public ActionResult Index(int oid)
        {

            return View();
        }


       


    }

}
