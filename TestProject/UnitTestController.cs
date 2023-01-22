using System;
using System.IO;
using WebApplication1.Controllers;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Core.Configuration;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Moq;
using System.Threading.Tasks;
using WebApplication1.ViewModels;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace TestProject
{
    public class UnitTestController 
    {
        private IOptions<DatabaseSettings>? _config = null;

        public IOptions<DatabaseSettings> Connect()
        {   
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            _config = Options.Create(configuration.GetSection("ProjectDb").Get<DatabaseSettings>());
            return _config;
        }
        //
        [Fact]
        public async void TestViewResultNotNull()
        {
            // Arrange
            TestServices testsService = new TestServices(Connect());
            CategoryServices categoryServices = new CategoryServices(Connect());
            ResultServices resultServices = new ResultServices(Connect());
            QuestionsServices questionServices = new QuestionsServices(Connect());
            UsersService userService = new UsersService(Connect());
            EmailMessageSender emailServices = new EmailMessageSender();
            // Act
            TestController controller = new TestController(testsService, categoryServices,
                resultServices, questionServices, emailServices, userService);
            var a = await resultServices.GetAsync();
            var b = a.Last();
            var result = await controller.ShowResult(b.Id) as ViewResult;
			// Assert
#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
			Assert.IsType<Result>(result.Model);
#pragma warning restore CS8602 // Разыменование вероятной пустой ссылки.
		}

        [Fact]
        public async void GetAnalysis()
        {
            // Arrange
            TestServices testsService = new TestServices(Connect());
            CategoryServices categoryServices = new CategoryServices(Connect());
            ResultServices resultServices = new ResultServices(Connect());
            QuestionsServices questionServices = new QuestionsServices(Connect());
            UsersService userService = new UsersService(Connect());
            EmailMessageSender emailServices = new EmailMessageSender();
            // Act
            TestController controller = new TestController(testsService, categoryServices,
                resultServices, questionServices, emailServices, userService);
            List<TestController.SaveResultClass> result1 = new List<TestController.SaveResultClass>();
            Dictionary<string, string> res_temp = new Dictionary<string, string>();
            res_temp.Add("id", "12345678910111");
            res_temp.Add("answer", "answer");
            res_temp.Add("id_category", "12345678910111");
            var result = await controller.GetAnalysis(result1);
            // Assert
            Assert.IsType<List<TestController.TestResult>>(result);
        }

        [Fact]
        public async void CreateCategory()
        {
           // Arrange
           CategoryServices categoryServices = new CategoryServices(Connect());
           QuestionsServices questionServices = new QuestionsServices(Connect());
           UsersService userService = new UsersService(Connect());
           CategoryController controller = new CategoryController(categoryServices, questionServices, userService);
           CategoryModel model = new CategoryModel();
           model.Name = "1";
           // Act
           var result = await controller.Create(model) as RedirectResult;
           // Assert
           Assert.Equal("/category/Index", result?.Url);
        }

        [Fact]
        public async void CreateTest()
        {
            //Arrange
            TestServices testsService = new TestServices(Connect());
            List<TestController.Questions> Questions = new List<TestController.Questions>();

            TestController.Questions Question = new TestController.Questions();
            Question.Category = "1";
            Question.Quantity = "1"; 
            Questions.Add(Question);
            Test result = new Test()
            {
                Id= "6456476",
                Name = "1",
                Time = "1",
                Questions = Questions,
            };
            // Act
            await testsService.CreateAsync(result);
#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
			var result2 = await testsService.GetAsync(result.Id);
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
							  // Assert
			Assert.NotNull(result2);
        }
        [Fact]
        public async void CreateQuestion()
        {
            //Arrange
            QuestionsServices questionServices = new QuestionsServices(Connect());
            Question question = new Question()
            {
                Id = "63c18cf3dc1ddeab9fd540bc",
                Text = "Что такое переменная ( 1 - тип, 2- область памяти, 3- ячейка",
                Answer = "2",
                id_category = "63c03507639d4ba9f809e910",
                Complexity = "низкий",
                Note = "Область памяти",
            };
            await questionServices.CreateAsync(question);
            // Act
            var result = await questionServices.GetAsync(question.Id);
            // Assert
            Assert.NotNull(result);
        }
    }
}
