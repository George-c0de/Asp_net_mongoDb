using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Authorize]
    [Route("category/[action]")]
    public class CategoryController : Controller
    {
        private readonly CategoryServices _categoryServices;
        private readonly QuestionsServices _questionServices;

        public CategoryController(CategoryServices categoryServices, QuestionsServices questionServices)
        {
            _categoryServices = categoryServices;
            _questionServices = questionServices;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create([FromForm] CategoryModel model)
        {
            Request.ContentType = "application/json";
            if (ModelState.IsValid)
            {
                var category = _categoryServices.GetAsync();
                var newCategory = new Category { Name = model.Name};
                _categoryServices.CreateAsync(newCategory);
            }
            string a = @Url.Action("Index", "Category");
            return Redirect(a);
        }
        [HttpGet]
        public IActionResult Index()
        {
            var a = _categoryServices.GetAsync();
            var b = a.Result;
            return View(b);
        }
        [HttpGet]
        public IActionResult Get(string id)
        {
            var a = _categoryServices.GetAsync(id);
            var b = a.Result;
            Task<List<Question>> c = _questionServices.GetAsync_id_cat(b.Id);
            ViewData["Title"] = b.Name;
            ViewData["id"] = b.Id;
            return View(c.Result);
        }
    }
}
