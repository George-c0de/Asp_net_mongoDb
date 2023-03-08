using WebApplication1.Controllers;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Moq;
using WebApplication1.ViewModels;

namespace TestProject
{
    public class UnitTestController
    {
        [Fact]
        public async void TestViewResultNotNull()
        {
            // Arrange
            var categoryMock = new Mock<ICategoryServices>();

            var questionMock = new Mock<IQuestionsServices>();

            var userMock = new Mock<IUsersService>();
            userMock.Setup(s => s.GetAsync())
                .ReturnsAsync(() => new List<Users>());

            var testMock = new Mock<ITestServices>();
            testMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new Test());

            var resultMock = new Mock<IResultServices>();
            resultMock.Setup(s => s.GetAsync())
                .ReturnsAsync(() => new List<Result>
                {
                    new(),
                    new()
                });

            resultMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new Result());

            var emailMessageMock = new Mock<IEmailMessageSender>();

            // Act
            var controller = new TestController(testMock.Object, categoryMock.Object,
                resultMock.Object, questionMock.Object, emailMessageMock.Object, userMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new ClaimsIdentity()))
                }
            };

            var a = await resultMock.Object.GetAsync();
            var b = a.Last();
            var result = await controller.ShowResult(b.Id) as ViewResult;
            // Assert
            Assert.IsType<Result>(result.Model);
        }

        [Fact]
        public async void GetAnalysis()
        {
            // Arrange
            var categoryMock = new Mock<ICategoryServices>();
            categoryMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new Category());

            var questionMock = new Mock<IQuestionsServices>();
            questionMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new Question());


            var userMock = new Mock<IUsersService>();
            userMock.Setup(s => s.GetAsync())
                .ReturnsAsync(() => new List<Users>());

            var testMock = new Mock<ITestServices>();
            testMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new Test());

            var resultMock = new Mock<IResultServices>();
            resultMock.Setup(s => s.GetAsync())
                .ReturnsAsync(() => new List<Result>
                {
                    new(),
                    new()
                });

            resultMock.Setup(s => s.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new Result());

            var emailMessageMock = new Mock<IEmailMessageSender>();
            // Act
            TestController controller = new TestController(testMock.Object, categoryMock.Object,
                resultMock.Object, questionMock.Object, emailMessageMock.Object, userMock.Object);

            var result = await controller.GetAnalysis(new List<TestController.SaveResultClass>
            {
                new()
                {
                    answer = "answer",
                    id = "12345678910111",
                    id_category = "12345678910111"
                }
            });
            // Assert
            Assert.Single(result);
            Assert.Equal(0, result.First().GetRight());
            Assert.Null(result.First().GetName());
        }


        [Fact]
        public async void CreateCategory()
        {
            // Arrange
            var categoryMock = new Mock<ICategoryServices>();

            var questionMock = new Mock<IQuestionsServices>();

            var userMock = new Mock<IUsersService>();

            var controller = new CategoryController(categoryMock.Object, questionMock.Object, userMock.Object);
            var model = new CategoryModel
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
            var Questions = new List<TestController.Questions>();
            var Question = new TestController.Questions
            {
                Category = "MyTest",
                Quantity = "MyAnswer"
            };
            Questions.Add(Question);
            var result = new Test()
            {
                Name = "MyTest",
                Time = "300",
                Questions = Questions,
            };
            var testsServiceMock = new Mock<ITestServices>();
            testsServiceMock.Setup(s => s.CreateAsync(It.IsAny<Test>()));
            testsServiceMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(() => result);
            var testsService = testsServiceMock.Object;


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
            var question = new Question()
            {
                Text = "MyTest",
                Answer = "MyAnswer",
                id_category = "14characters144",
                Note = "not",
            };
            var questionServicesMock = new Mock<IQuestionsServices>();
            questionServicesMock.Setup(s => s.CreateAsync(It.IsAny<Question>()));
            questionServicesMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(() => question);
            var questionServices = questionServicesMock.Object;

            await questionServices.CreateAsync(question);
            // Act
            var result = await questionServices.GetAsync(question.Id);
            // Assert
            Assert.NotNull(result);
        }
    }
}