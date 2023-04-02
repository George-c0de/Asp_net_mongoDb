using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WebApplication1.Controllers;

namespace WebApplication1.Models
{
    [BsonIgnoreExtraElements]
    public class Test
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Вопросы")]
        public List<TestController.Questions> Questions { get; set; }
        

        [Display(Name = "Время")]
        public string Time { get; set; }

    }
}
