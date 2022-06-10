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
        public async Task<JsonResult> Get_q(string id)
        {
            var a = await _resultatservices.GetAsync(id);
            return Json(a);
        }

        public async Task<string> get_cat_q(string id)
        {
            var a = await _questionServices.GetAsync(id);
            if (a == null)
            {
                return "error";
            }
            return a.id_category;
        }

        public async Task<Dictionary<string, double>> analis2(List<Dictionary<string, string>> result)
        {
            Dictionary <string, double> cat = new Dictionary<string, double>();
            int col = result.Count;
            foreach (var el in result)
            {
                double right = 0;
                bool flag = false;
                var name_ = await _categoryServices.GetAsync(el["id_category"]);
                var name = name_.Name;
                var q = await _questionServices.GetAsync(el["id"]);
                if (el["answer"] == q.Answer)
                {
                    right = 1;
                }
                foreach (var l in cat)
                {
                    var id_c_ = await _categoryServices.GetAsync(el["id_category"]);
                    var id_c = id_c_.Name;
                    if (id_c == l.Key)
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    cat.Add(name,right);
                }
                else
                {
                    cat[name] = cat[name] + right;
                }
            }
            return cat;
        }

        public async Task<int> get_r(List<Dictionary<string, string>> result)
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
        public async Task<double> get_Percent(List<Dictionary<string, string>> result)
        {
            double col = result.Count();
            double percent = 0;
            double right = 0;
            foreach (var el in result)
            {
                var v = await _questionServices.GetAsync(el["id"]);
                if (v.Answer == el["answer"])
                {
                    right++;
                }
            }
            percent = (right / col) * 100;
            return percent;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Send_mess()
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
        public async Task<IActionResult> Result_save()
        {
            int col = Convert.ToInt16(Request.Form["col_answer"]);
            string id_result = Request.Form["id_result"].ToString();
            Dictionary<string, string> res = new Dictionary<string, string>();
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            for (int i = 0; i < col; i++)
            {
                string name_id = "id" + i;
                string name_an = "q" + i;
                string id_a = Request.Form[name_id].ToString();
                string answer = Request.Form[name_an].ToString();
                Dictionary<string, string> res_temp = new Dictionary<string, string>();
                res_temp.Add("id", id_a);
                res_temp.Add("answer", answer);
                var val = await get_cat_q(id_a);
                res_temp.Add("id_category", val);
                result.Add(res_temp);
            }
            var res_ = await _resultatservices.GetAsync(id_result);
            var analis = await analis2(result);
            analis.OrderBy(pair => pair.Value);
            string rec = "";
            if (analis.First().ToString() != analis.Last().ToString())
            {
                rec = "Ваша самая лучшая категория: " + analis.First().Key
                                                             + "\n" + "Ваша самая худшая категория " + analis.Last().Key;
            }
            else if(await get_Percent(result)==0)
            {
                rec = "Тест завален";
            }
            else
            {
                rec = "Тест пройден на " + await get_Percent(result) + "%";
            }
            var newResult = new Result { Id = res_.Id, Value = "true", Answers = result, Id_test = res_.Id_test,Percent = await get_Percent (result), Percentage_category = analis, recommendations = rec};
            await _resultatservices.RemoveAsync(res_.Id);
            await _resultatservices.CreateAsync(newResult);
            return Redirect(@Url.Action("Send", "Test", new { id = res_.Id }));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Send(string id)
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
        public async Task<IActionResult> Passage_test(string id)
        {
                var res = await _resultatservices.GetAsync(id);
                var test = await _testsService.GetAsync(res.Id_test);
                ViewData["id_r"] = id;
                return View(test);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Get_test(string id)
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
        public IActionResult Go_test()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> Get_url(string id)
        {
            var newRes = new Result { Value = "false", Id_test = id};
            await _resultatservices.CreateAsync(newRes);
            var a = await _resultatservices.GetAsync(newRes.Id);
            return Json(a);
        }
        [HttpPost]
        public async Task<IActionResult> Create_test()
        {
            List<Dictionary<string, string>> questions = new List<Dictionary<string, string>>();
            Dictionary<string, string> quest = new Dictionary<string,string>();
            string col = "col";
            string cat = "cat";
            for (int i = 0; i < Convert.ToInt16(Request.Form["h_col"][0]);i++)
            {
                quest.Clear();
                quest["Quantity"] = Request.Form[col + i][0];
                quest["Category"] = Request.Form[cat + i][0];
                Dictionary<string, string> temp = new Dictionary<string, string>(quest);
                questions.Add(temp);
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
            var newUser = new Test { Name = Request.Form["name"][0], Questions = questions,Time = Request.Form["time"][0]};
            await _testsService.CreateAsync(newUser);
            return Redirect(@Url.Action("Get","Test"));
        }

        public async Task<IActionResult> Get()
        {
                var a = await _testsService.GetAsync();
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
        public async Task<ActionResult<string>> Test(string id)
        {
            var test = await _testsService.GetAsync(id);
            foreach (var cat in test.Questions)
            {
                foreach (var e in cat)
                {
                    if (e.Key == "Category")
                    {
                        ActionResult<string> log = await Category_get(e.Value);
                        string name = log.Value;
                        ViewData[(e.Value).ToString()] = name;
                    }
                }
            }
            return View(test);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<ActionResult<string>> Category_get(string id)
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
