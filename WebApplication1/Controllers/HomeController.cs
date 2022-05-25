using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return Redirect("test/Get");
        }

        //// GET: HomeController
        //public ActionResult Index()
        //{
        //    return View();
        //}


        // GET: HomeController/Create
        public ActionResult Create()
        {
            return View();
        }
    }
}
