using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class CategoryModel
    {
        [Required(ErrorMessage = "Не указано Имя")]
        public string Name { get; set; }
    }
}
