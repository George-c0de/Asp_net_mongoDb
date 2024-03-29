﻿using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Models
{
    [BsonIgnoreExtraElements]
    public class Question
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [Display(Name = "Текст")]
        public string Text { get; set; }

        [Display(Name = "Ответ")]
        public string Answer { get; set; }

        [Display(Name = "id Категории")]
        public string id_category { get; set; }
        [Display(Name = "Приоритет")]
        public string Complexity { get; set; }

        [Display(Name = "Примечание")]
        public string Note { get; set; }

    }
}
