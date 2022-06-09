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
        private readonly QuestionsServices _questionsService;
        private readonly CategoryServices _categoryService;

        public QuestionsController(QuestionsServices questionsService, CategoryServices categoryService)
        {
            _categoryService = categoryService;
            _questionsService = questionsService;
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
        public async Task<JsonResult> Get_ques(string id)
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

        [HttpGet]
        public IActionResult Get(string id)
        {
            ViewData["id"] = id;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] QuestionsModel model)
        {
            Request.ContentType = "application/json";
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
                var newQuestion = new Question { Text = model.Text, Answer = model.Answer, Note = note_, id_category = model.id_category };
                await _questionsService.CreateAsync(newQuestion);
            }
            string a = @Url.Action("Get", "Category", new { id = model.id_category });
            return Redirect(a);
        }
        public async Task<ActionResult> Details()
        {
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
