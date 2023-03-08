using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using JsonResult = Microsoft.AspNetCore.Mvc.JsonResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic;
using WebApplication1.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Authorize]
    [Route("test/[action]")]
    public class TestController : Controller
    {
        public class TestResult
        {
            private string Name;
            private double Right;

            public void SetRight(double n)
            {
                Right = n;
            }
            public double GetRight()
            {
                return Right;
            }
            public void SetName(string n)
            {
                Name = n;
            }
            public string GetName()
            {
                return Name;
            }
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

     

        public class SaveResultClass
        {
            public string id { get; set; }
            public string id_category { get; set; }
            public string answer { get; set; }
        }
        private readonly ITestServices _testsService;
        private readonly ICategoryServices _categoryServices;
        private readonly IResultServices _resultatservices;
        private readonly IQuestionsServices _questionServices;
        private readonly IEmailMessageSender _emailServices;
        private readonly IUsersService _usersService;
        //private readonly IRepository repo;

        public TestController(
            ITestServices testsService, ICategoryServices categoryServices,
            IResultServices resultServices, IQuestionsServices questionServices,
            IEmailMessageSender emailServices, IUsersService usersService)
        {
            _usersService = usersService;
            _resultatservices = resultServices;
            _testsService = testsService;
            _categoryServices = categoryServices;
            _questionServices = questionServices;
            _emailServices = emailServices;
            //repo = r;
        }


		[HttpGet]
        public async Task<IActionResult> Create()
        {
            if (await CheckUser())
            {
                return StatusCode(403);
            }
            var category = await _categoryServices.GetAsync();

            return View(category);
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetQuestion(string id)
        {
            var a = await _resultatservices.GetAsync(id);
            return Json(a);
        }

        public async Task<string> GetCategoryByQuestion(string id)
        {
            var a = await _questionServices.GetAsync(id);
            if (a == null)
            {
                return "error";
            }
            return a.id_category;
        }


        public async Task<List<TestResult>> GetAnalysis(List<SaveResultClass> result)
        {
            List<TestResult> cat2 = new List<TestResult>();
            int col = result.Count;
            foreach (var el in result)
            {
                TestResult a = new TestResult();
                a.SetRight(0);
                double right = 0;
                bool flag = false;
                var name_ = await _categoryServices.GetAsync(el.id_category);
                var name = name_.Name;
                a.SetName(name_.Name);
                var q = await _questionServices.GetAsync(el.id);
                if (el.answer == q.Answer)
                {

                    if (q.Complexity == "средний")
                    {
                        right = 2;
                    }
                    else if (q.Complexity == "высокий")
                    {
                        right = 3;
                    }
                    else
                        right = 1;
                    a.SetRight(right);
                }
                foreach (var l in cat2)
                {
                    var id_c_ = await _categoryServices.GetAsync(el.id_category);
                    var id_c = id_c_.Name;
                    if (id_c == l.GetName())
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    cat2.Add(a);
                }
                else
                {
                    for (int i = 0; i < cat2.Count; i++)
                    {
                        if (cat2[i].GetName() == name)
                        {
                            string new_name = cat2[i].GetName() + right;
                            cat2[i].SetName(new_name);
                        }
                    }
                }
            }
            return cat2;
        }


        public async Task<int> GetCorrectAnswers(List<Dictionary<string, string>> result)
        {
            int col = result.Count();
            int percent = 0;
            int right = 0;
            foreach (var el in result)
            {
                var v = await _questionServices.GetAsync(el["id"]);
                if (v.Answer == el["answer"])
                {
                    if (v.Complexity == "средний")
                    {
                        right *= 2;
                    }

                    if (v.Complexity == "высокий")
                    {

                    }
                    right++;
                }
            }
            return right;
        }
        public async Task<double> GetPercent(List<SaveResultClass> result)
        {
            double col = result.Count();
            double right = 0;
            foreach (var el in result)
            {
                var v = await _questionServices.GetAsync(el.id);
                if (v.Answer == el.answer)
                {
                    right++;
                }
            }
            double percent = right / col * 100;
            return percent;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SendMessage()
        {
            string email = Request.Form["email"];
            string id = Request.Form["id"];
            Result t = await _resultatservices.GetAsync(id);
            var test_ = await _testsService.GetAsync(t.Id_test);
            var test = test_.Name;
            _emailServices.Send(email, id, t, test);
            return Redirect("/test/OpenTestById");
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SaveResult()
        {
            int col = Convert.ToInt16(Request.Form["col_answer"]);
            string id_result = Request.Form["id_result"].ToString();
            List<SaveResultClass> result = new List<SaveResultClass>();
            for (int i = 0; i < col; i++)
            {
                string name_id = "id" + i;
                string name_an = "q" + i;
                string id_a = Request.Form[name_id].ToString();
                string answer = Request.Form[name_an].ToString();
                SaveResultClass res_temp2 = new SaveResultClass();
                res_temp2.id = id_a;
                res_temp2.answer = answer;
                var val = await GetCategoryByQuestion(id_a);
                res_temp2.id_category = val;
                result.Add(res_temp2);
            }
            var res_ = await _resultatservices.GetAsync(id_result);
            var analis = await GetAnalysis(result);

            var keyValuePairs = analis.OrderBy(pair => pair.GetRight());
            double temp_per = await GetPercent(result);
            double percent = Math.Round(temp_per, 2);
            string rec;
            if (keyValuePairs.Count() != 1 && percent != 0)
            {
                rec = "Мы рекомендуем вам поступать на " + keyValuePairs.Last().GetName();

            }
            else if (percent == 0)
            {
                rec = "Тест пройден на 0%\nТемы, которые стоит повторить:\n";
                foreach (var el in keyValuePairs)
                {
                    rec += el.GetName();
                    rec += ", ";
                }
            }
            else
            {
                rec = "Тест пройден на " + percent + "%";
                rec += ". Мы рекомендуем вам поступать на " + keyValuePairs.Last().GetName();

            }
            ViewBag.Re = analis;
            Dictionary<string, double> a = new Dictionary<string, double>();
            foreach (var el in analis)
            {
                a[el.GetName()] = el.GetRight();
            }
            var newResult = new Result { Id = res_.Id, Value = "true", Answers = result, Id_test = res_.Id_test, Percent = percent, Percentage_category = a, recommendations = rec };
            await _resultatservices.RemoveAsync(res_.Id);
            await _resultatservices.CreateAsync(newResult);
            return Redirect(@Url.Action("ShowResult", "Test", new { id = res_.Id }));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ShowResult(string id)
        {
            var analis = await _resultatservices.GetAsync(id);
            var name_ = await _testsService.GetAsync(analis.Id_test);
            var name = name_.Name;
			ViewData["userEm"] = CheckUserById().Result;
            ViewData["name_test"] = name;
            if (analis == null)
            {
                return Redirect("Home");
            }
            else
            {
                return View(analis);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPassage(string id)
        {
            var res = await _resultatservices.GetAsync(id);
            var test = await _testsService.GetAsync(res.Id_test);
            ViewData["id_r"] = id;
            return View(test);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> GetTest(string id)
        {
            if (id.Length != 24)
            {
                return Json("Error");
            }
            var a = await _resultatservices.GetAsync(id);
            if (a == null)
            {
                return Json("Error");
            }
            return Json(a);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetTestByCode()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetUrl(string id)
        {
            var newRes = new Result { Value = "false", Id_test = id };
            await _resultatservices.CreateAsync(newRes);
            var a = await _resultatservices.GetAsync(newRes.Id);
            return Json(a);
        }

        public class TestQuestions
        {
            public string Text;
            public string Answer;
            public string id_category;
            public string Note;
        }
        public class Questions
        {
            public string Quantity;
            public string Category;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTest()
        {
            if (await CheckUser())
            {
                return StatusCode(403);
            }
            List<Dictionary<string, string>> questions = new List<Dictionary<string, string>>();
            List<Questions> questions2 = new List<Questions>();
            Dictionary<string, string> quest = new Dictionary<string, string>();
            string col = "col";
            string cat = "cat";
            for (int i = 0; i < Convert.ToInt16(Request.Form["h_col"][0]); i++)
            {
                quest.Clear();
                Questions quest2 = new Questions();
                quest2.Quantity = Request.Form[col + i][0];
                quest2.Category = Request.Form[cat + i][0];
                quest["Quantity"] = Request.Form[col + i][0];
                quest["Category"] = Request.Form[cat + i][0];
                Dictionary<string, string> temp = new Dictionary<string, string>(quest);
                questions.Add(temp);
                questions2.Add(quest2);
            }
            for (int i = 0; i < questions.Count - 1; i++)
            {
                if (questions[i]["Category"] == questions[i + 1]["Category"])
                {
                    questions[i]["Quantity"] = (Convert.ToInt16(questions[i]["Quantity"]) +
                                                                Convert.ToInt16(questions[i + 1]["Quantity"])).ToString();
                    questions.RemoveAt(i + 1);
                    i = -1;
                }
            }

            for (int i = 0; i < questions2.Count - 1; i++)
            {
                if (questions2[i].Category == questions2[i + 1].Category)
                {
                    questions2[i].Quantity = (Convert.ToInt16(questions2[i].Quantity) +
                                                Convert.ToInt16(questions2[i + 1].Quantity)).ToString();
                    questions2.RemoveAt(i + 1);
                    i = -1;
                }
            }
            var newUser = new Test { Name = Request.Form["name"][0], Questions = questions2, Time = Request.Form["time"][0] };
            await _testsService.CreateAsync(newUser);

            return Redirect(@Url.Action("OpenTestById", "Test"));
        }

      

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<JsonResult> JsonSearch()
        {
            var a = await _categoryServices.GetAsync();
            List<Category> b = a;
            return Json(b);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet("{id:length(24)}")]
        public async Task<ActionResult<string>> CreateCodeForTest(string id)
        {
            if (await CheckUser())
            {
                return StatusCode(403);
            }
            var test = await _testsService.GetAsync(id);
            foreach (var cat in test.Questions)
            {
                ActionResult<string> log = await GetCategory(cat.Category);
                string name = log.Value;
                ViewData[(cat.Category)] = name;
            }
            return View(test);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<ActionResult<string>> GetCategory(string id)
        {
            if (await CheckUser())
            {
                return StatusCode(403);
            }
            var category = await _categoryServices.GetAsync(id);
            if (category is null)
            {
                return NotFound();
            }
            return category.Name;
        }

        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([FromForm] TestModel model)
        //{
        //    Request.ContentType = "application/json";
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _testsService.GetAsync();
        //        foreach (var el in user.AsReadOnly())
        //        {
        //            if (el.Name == model.Name)
        //            {
        //                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
        //                //вывод ошибки
        //            }
        //        }
        //        // добавляем пользователя в бд
        //        var newTest = new Test { Name = model.Name, Questions = model.Questions };
        //        await _testsService.CreateAsync(newTest);
        //        return CreatedAtAction(nameof(Get), new { id = newTest.Id }, newTest);
        //    }
        //}
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> OpenTestById()
        {
            var vm = new ListCollection();
            vm.Tests = await _testsService.GetAsync();
            vm.Users = await _usersService.GetAsync();
            //return View(repo.GetAll());
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SendMe()
        {
            var tests = await _testsService.GetAsync();
            string f = Request.Form["Field"];
            f = f.Trim();
            foreach (var i in tests.ToList().Where(x=>x.Name.Equals(f)))
            {
                if (i.Name == f)
                {
                   _emailServices.Send(CheckTestName(f).Result, CheckUserById().Result);
                }
                else
                {
                    return NotFound();
                }
            }
            return Redirect("/test/OpenTestById");
        }
        //ostanovka
        public async Task<string> CheckTestName(string f)
        {
            
            var tests = await _testsService.GetAsync();
            string nm = "";
            foreach (var el in tests)
            {
                if (el.Name == f)
                {
                    nm = el.Name;
                }
            }
            return nm;
        }

        public async Task<string> CheckUserById()
        {
            var user_name = User.Identity.Name;
            var users = await _usersService.GetAsync();
            string nm = "";
            foreach (var el in users)
            {
                if (el.Name == user_name)
                {
                    nm = el.Email;
                }
            }
            return nm;
        }


        //public async Task<string> CheckUserId(string id)
        //{
        //    var user_name = User.Identity.Name;
        //    var users = await _usersService.GetAsync();
        //    foreach (var el in users)
        //    {
        //        if (el.Id != null)
        //        {
        //            id = el.Id;
        //        }
        //    }
        //    return id;
        //}

    }
}