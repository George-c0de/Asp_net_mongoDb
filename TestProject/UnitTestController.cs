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
		private IOptions<DatabaseSettings> _config;

		public IOptions<DatabaseSettings> Connect()
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", false)
				.Build();

			_config = Options.Create(configuration.GetSection("ProjectDb").Get<DatabaseSettings>());
			return _config;
		}

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
			Assert.IsType<Result>(result.Model);
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
			Question.Category = "MyTest";
			Question.Quantity = "MyAnswer";
			Questions.Add(Question);
			Test result = new Test()
			{
				Name = "MyTest",
				Time = "300",
				Questions = Questions,
			};
			// Act
			await testsService.CreateAsync(result);
			var result2 = await testsService.GetAsync(result.Id);
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
				Text = "MyTest",
				Answer = "MyAnswer",
				id_category = "14characters144",
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