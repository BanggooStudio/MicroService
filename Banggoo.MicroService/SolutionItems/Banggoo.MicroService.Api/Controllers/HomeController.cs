using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Banggoo.MicroService.Api.Controllers
{
    public class HomeController
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}
