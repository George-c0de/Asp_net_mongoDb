using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string Login { get; set; }
    }
}
