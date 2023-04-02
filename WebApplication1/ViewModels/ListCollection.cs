using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ListCollection
    {
        public List<Test> Tests { get; set; }
        public string Field { get; set; }

    }
}
