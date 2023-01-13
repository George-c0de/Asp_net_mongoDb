using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Principal;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Authorize]
    [Route("category/[action]")]
    public class CategoryController : Controller
    {
        private readonly CategoryServices _categoryServices;
        private readonly QuestionsServices _questionServices;
        private readonly UsersService _usersService;

        public CategoryController(
            CategoryServices categoryServices, QuestionsServices questionServices, 
            UsersService usersService)
        {
            _usersService = usersService;
            _categoryServices = categoryServices;
            _questionServices = questionServices;
        }
        public async Task<bool> CheckUser()
        {
            if (User is null)
                return false;
            var user_name = User.Identity?.Name;
            var users = await _usersService.GetAsync();
            foreach (var el in users)
            {
                if (el.Name == user_name && el.Role != "admin")
                {
                    return true;
                }
            }
            return false;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (await CheckUser())
            {
                return StatusCode(403);
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CategoryModel model)
        {
            if (await CheckUser())
            {
                return StatusCode(403);
            }
            if (Request != null)
                Request.ContentType = "application/json";
            if (ModelState.IsValid)
            {
                var category = await _categoryServices.GetAsync();
                var newCategory = new Category { Name = model.Name};
                await _categoryServices.CreateAsync(newCategory);
            }
            string a = "/category/Index";
            if (@Url != null)
            {
                a = @Url.Action("Index", "Category");
            }
            return Redirect(a);
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (await CheckUser())
            {
                return StatusCode(403);
            }
            var a = await _categoryServices.GetAsync();
            var b = a;
            return View(b);
        }
        [HttpGet]
        public async Task<IActionResult> GetQuestionByCategory(string id)
        {
            var a = await _categoryServices.GetAsync(id);
            var b = a;
            List<Question> c = await _questionServices.GetAsync_id_cat(b.Id);
            ViewData["Title"] = b.Name;
            ViewData["id"] = b.Id;
            return View(c);
        }
    }
}
