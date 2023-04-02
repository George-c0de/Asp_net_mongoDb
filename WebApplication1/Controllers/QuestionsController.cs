using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;
using Microsoft.AspNetCore.Authorization;
namespace WebApplication1.Controllers
{
    [ApiController]
    [Authorize]
    [Route("question/[action]")]
    public class QuestionsController : Controller
    {
        private readonly IQuestionsServices _questionsService;
        private readonly ICategoryServices _categoryService;
        private readonly IUsersService _usersService;

        public QuestionsController(IQuestionsServices questionsService, 
            ICategoryServices categoryService, IUsersService usersService)
        {
            _categoryService = categoryService;
            _questionsService = questionsService;
            _usersService = usersService;
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Question>> Details(string id)
        {
            var question = await _questionsService.GetAsync(id);

            if (question is null)
            {
                return NotFound();
            }
            return question;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> GetQuestions(string id)
        {
            if (id.Length != 24)
            {
                return Json("Error");
            }
            var a = await _questionsService.GetAsync_id_cat(id);
            if (a == null)
            {
                return Json("Error");
            }
            return Json(a);
        }
        public async Task<bool> CheckUser()
        {
            var user_name = User.Identity.Name;
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
        public async Task<IActionResult> Get(string id)
        {
            if (await CheckUser())
            {
                return StatusCode(403);
            }
            ViewData["id"] = id;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] QuestionsModel model)
        {
            if (await CheckUser())
            {
                return StatusCode(403);
            }
            if (Request != null)
            {
                Request.ContentType = "application/json";
            }
            string b = @Url.Action("Details", "Questions");
            Redirect(b);
            if (ModelState.IsValid)
            {
                string note_= " ";
                var user = await _questionsService.GetAsync();
                var category = await _categoryService.GetAsync(model.id_category);
                if (category != null)
                {
                    ViewData["id"] = category.Id;
                    ViewData["Title"] = category.Name;
                }

                // добавляем пользователя в бд
                if (model.Note != "-")
                {
                    note_ = model.Note;
                }
                var newQuestion = new Question { Text = model.Text, Answer = model.Answer, Note = note_, id_category = model.id_category};
                await _questionsService.CreateAsync(newQuestion);
            }

            string a = "/category/GetQuestionByCategory";
            if(@Url!=null)
                a = @Url.Action("GetQuestionByCategory", "Category", new { id = model.id_category });
            
            return Redirect(a);
        }
        public async Task<ActionResult> Details()
        {
            if (await CheckUser())
            {
                return StatusCode(403);
            }
            var question = await _questionsService.GetAsync();
            foreach (var el in question)
            {
                var log = await _categoryService.GetAsync(el.id_category);
                string name = log.Name;
                ViewData[(el.id_category)] = name;
            }
            return View(question);
        }

        // POST: QuestionsController1/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
        
    }
}
