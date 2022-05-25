using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Models
{
    public class Test
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Вопросы")]
        public List<Dictionary<string,string>> Questions { get; set; }

        [Display(Name = "Время")]
        public string Time { get; set; }

    }
}
