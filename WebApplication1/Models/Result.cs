using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Models
{
    public class Result
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [Display(Name = "Значение")]
        public string Value { get; set; }
        [Display(Name = "Тест")]
        public string Id_test { get; set; }

        [Display(Name = "Ответы")]
        public List<Dictionary<string, string>> Answers { get; set; }

        [Display(Name = "Процент решений по категориям")]
        public Dictionary<string, double> Percentage_category { get; set; }

        [Display(Name = "Общий процент")]
        public double Percent { get; set; }

        [Display(Name = "рекомендации")]
        public string recommendations { get; set; }
    }
}
