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

            _config = Options.Create(configuration.GetSection("Project20Database").Get<DatabaseSettings>());
            return _config;
        }
        //
        [Fact]
        public async void TestViewResultNotNull()
        {
            // Arrange
            TestServices testsService = new(Connect());
            CategoryServices categoryServices = new(Connect());
            ResultServices resultServices = new(Connect());
            QuestionsServices questionServices = new(Connect());
            UsersService userService = new(Connect());
            EmailMessageSender emailServices = new();
            // Act
            TestController controller = new(testsService, categoryServices,
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
            TestServices testsService = new(Connect());
            CategoryServices categoryServices = new(Connect());
            ResultServices resultServices = new(Connect());
            QuestionsServices questionServices = new(Connect());
            UsersService userService = new(Connect());
            EmailMessageSender emailServices = new();
            // Act
            TestController controller = new(testsService, categoryServices,
                resultServices, questionServices, emailServices, userService);
			List<TestController.SaveResultClass> saveResultClasses = new();
			List<TestController.SaveResultClass> result1 = saveResultClasses;
#pragma warning disable IDE0059 // Ненужное присваивание значения
			Dictionary<string, string> res_temp = new()
			{
				{ "id", "12345678910111" },
				{ "answer", "answer" },
				{ "id_category", "12345678910111" }
			};
#pragma warning restore IDE0059 // Ненужное присваивание значения
			var result = await controller.GetAnalysis(result1);
            // Assert
            Assert.IsType<List<TestController.TestResult>>(result);
        }

        [Fact]
        public async void CreateCategory()
        {
           // Arrange
           CategoryServices categoryServices = new(Connect());
           QuestionsServices questionServices = new(Connect());
           UsersService userService = new(Connect());
           CategoryController controller = new(categoryServices, questionServices, userService);
			CategoryModel model = new()
			{
				Name = "1"
			};
			// Act
			var result = await controller.Create(model) as RedirectResult;
           // Assert
           Assert.Equal("/category/Index", result?.Url);
        }

        [Fact]
        public async void CreateTest()
        {
            //Arrange
            TestServices testsService = new(Connect());
            List<TestController.Questions> Questions = new();

			TestController.Questions Question = new()
			{
				Category = "MyTest",
				Quantity = "MyAnser"
			};
			Questions.Add(Question);
			Test test = new()
			{
				Name = "MyTest",
				Time = "300",
				Questions = Questions,
			};
			Test result = test;
            // Act
            await testsService.CreateAsync(result);
			var result2 = await testsService.GetAsync(result.Id);
							  // Assert
			Assert.NotNull(result2);
        }
        [Fact]
        //
        public async void CreateQuestion()
        {
            //Arrange
            QuestionsServices questionServices = new(Connect());
            Question question = new()
            {
                Id= "63e40696ed107ec6cf3e58ed",
                Text = "MyTest",
                Answer = "MyAnswer",
                Complexity = "1",
                id_category = "63e406154f19383915b3422a",
                Note = "not",
            };
            await questionServices.CreateAsync(question);
            // Act
            var result = await questionServices.GetAsync(question.Id);
            // Assert
            Assert.NotNull(result);
        }
    }
}