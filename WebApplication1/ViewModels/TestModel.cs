using System.ComponentModel.DataAnnotations;
namespace WebApplication1.ViewModels
{
    public class TestModel
    {
        [Required(ErrorMessage = "Не указан Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указаны вопросы")]
        public string Questions { get; set; }
        [Required(ErrorMessage = "Не указано время")]
        public string Time { get; set; }

    }
}
