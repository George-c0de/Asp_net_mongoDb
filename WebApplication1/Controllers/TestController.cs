using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using JsonResult = Microsoft.AspNetCore.Mvc.JsonResult;
using Microsoft.AspNetCore.Authorization;

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

        public class SaveResultClass
        {
            public string id { get; set; }
            public string id_category { get; set; }
            public string answer { get; set; }
        }
        private readonly TestServices _testsService;
        private readonly CategoryServices _categoryServices;
        private readonly ResultServices _resultatservices;
        private readonly QuestionsServices _questionServices;
        public TestController(TestServices testsService, CategoryServices categoryServices,
            ResultServices resultServices, QuestionsServices questionServices)
        {
            _resultatservices = resultServices;
            _testsService = testsService;
            _categoryServices = categoryServices;
            _questionServices = questionServices;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
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
                    a.SetRight(1);
                    right = 1;
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
            double percent = 0;
            int right = 0;
            foreach (var el in result)
            {
                var v = await _questionServices.GetAsync(el["id"]);
                if (v.Answer == el["answer"])
                {
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
            MailAddress from = new MailAddress("Test_Company@gmail.com", "Test_Company");
            //komy
            MailAddress to = new MailAddress(Request.Form["email"]);
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = "Тест";
            // текст письма
            string body;
            var t = await _resultatservices.GetAsync(Request.Form["id"]);
            var test_ = await _testsService.GetAsync(t.Id_test);
            var test = test_.Name;
            body = "<h5>" + test + "</h5>";
            body += "<p>" + "Количество правильных ответов по категориям" + "</p>";
            foreach (var el in t.Percentage_category)
            {
                body +="<p>"+"Категория: " + el.Key + " : " +el.Value + "</p>";
            }
            body+= "<p>" + "Доля правильных ответов" + t.Percent + "</p>";
            body+= "<p>" + "Ваши рекомендации" + "</p>";
            body+= "<p>" + t.recommendations + "</p>";
            m.Body = body;
            // письмо представляет код html
            m.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            // логин и пароль
            smtp.Credentials = new NetworkCredential("Test_Company@gmail.com", "mypassword");
            smtp.EnableSsl = true;
            smtp.Send(m);
            return Redirect("");
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

            string rec = "";
            if (keyValuePairs.First().ToString() != keyValuePairs.Last().ToString())
            {
                
                rec = "Ваша самая лучшая категория: " + keyValuePairs.Last().GetName()
                                                      + "\n" + "Ваша самая худшая категория " + keyValuePairs.First().GetName();
            }
            else if(await GetPercent(result)==0)
            {
                rec = "Тест завален";
            }
            else
            {
                rec = "Тест пройден на " + await GetPercent(result) + "%";
            }

            Dictionary<string, double> a = new Dictionary<string, double>();
            foreach (var el in analis)
            {
                a[el.GetName()] = el.GetRight();
            }
            var newResult = new Result { Id = res_.Id, Value = "true", Answers = result, Id_test = res_.Id_test,Percent = await GetPercent(result), Percentage_category = a, recommendations = rec};
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
            var newRes = new Result { Value = "false", Id_test = id};
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
            List<Dictionary<string, string>> questions = new List<Dictionary<string, string>>();
            List<Questions> questions2 = new List<Questions>();
            Dictionary<string, string> quest = new Dictionary<string,string>();
            string col = "col";
            string cat = "cat";
            for (int i = 0; i < Convert.ToInt16(Request.Form["h_col"][0]);i++)
            {
                quest.Clear();
                Questions quest2 = new Questions();
                quest2.Quantity = Request.Form[col + i][0];
                quest2.Category =Request.Form[cat + i][0];
                quest["Quantity"] = Request.Form[col + i][0];
                quest["Category"] = Request.Form[cat + i][0];
                Dictionary<string, string> temp = new Dictionary<string, string>(quest);
                questions.Add(temp);
                questions2.Add(quest2);
            }
            for (int i = 0; i < questions.Count-1; i++)
            {
                if (questions[i]["Category"] == questions[i+1]["Category"])
                {
                    questions[i]["Quantity"] = (Convert.ToInt16(questions[i]["Quantity"]) +
                                                                Convert.ToInt16(questions[i + 1]["Quantity"])).ToString();
                    questions.RemoveAt(i + 1);
                    i = -1;
                }
            }

            for (int i = 0; i < questions2.Count-1; i++)
            {
                if (questions2[i].Category == questions2[i + 1].Category)
                {
                    questions2[i].Quantity = (Convert.ToInt16(questions2[i].Quantity) +
                                                Convert.ToInt16(questions2[i + 1].Quantity)).ToString();
                    questions2.RemoveAt(i + 1);
                    i = -1;
                }
            }
            var newUser = new Test { Name = Request.Form["name"][0], Questions = questions2, Time = Request.Form["time"][0]};
            await _testsService.CreateAsync(newUser);
            
            return Redirect(@Url.Action("OpenTestById", "Test"));
        }

        public async Task<IActionResult> OpenTestById()
        {
                List<Test> a = await _testsService.GetAsync();
                var b = a;
                return View(b);
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
    }
}
