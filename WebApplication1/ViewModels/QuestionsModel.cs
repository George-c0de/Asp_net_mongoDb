using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class QuestionsModel
    {
        [Required(ErrorMessage = "Не указано условие")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Не указан ответ")]
        public string Answer { get; set; }
        public string Complexity { get; set; }
        public string id_category { get; set; }

        [Required(ErrorMessage = "Не указано примечание, если его нет установите '-'")]
        public string Note { get; set; }
    }
}
