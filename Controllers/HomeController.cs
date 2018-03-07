using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FileTracking.Services.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UserAPI()
        {
            return View();
        }

        public IActionResult DirectoryFileAPI()
        {
            return View();
        }
    }
}