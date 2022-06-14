using System.IO;
using WebApplication1.Controllers;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Core.Configuration;
using WebApplication1.Models;
using WebApplication1.Services;

namespace TestProject
{
    //public class DatabaseSettings
    //{
    //    public string ConnectionString { get; set; } = "mongodb+srv://admin:admin@cluster0.c37dj.mongodb.net";

    //    public string DatabaseName { get; set; } = "Project20";

    //    public string UsersCollectionName { get; set; } = "Users";
    //}
    public class UnitTestController
    {
        private IOptions<DatabaseSettings> _config;
        public IOptions<DatabaseSettings> connect()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            _config = Options.Create(configuration.GetSection("Project20Database").Get<DatabaseSettings>());
            return _config;
        }
        [Fact]
        public void IndexViewResultNotNull()
        {
           
            TestServices testsService = new TestServices(connect());
            CategoryServices categoryServices = new CategoryServices(connect());
            ResultServices resultServices = new ResultServices(connect());
            QuestionsServices questionServices = new QuestionsServices(connect());

            TestController controller = new TestController(testsService, categoryServices,
                resultServices, questionServices);

            ViewResult result = controller.Index() as ViewResult;

            Assert.NotNull(result);
        }

    }
}