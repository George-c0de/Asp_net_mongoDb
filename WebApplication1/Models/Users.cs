using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Models
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Фамилия")]
        public string Surname { get; set; }

        [Display(Name = "Пароль")]
        public string Password { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Token")]
        public string? Token { get; set; }
        [Display(Name = "Роль")]
        public string? Role { get; set; }
    }
}
